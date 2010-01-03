﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simulation;
using Clients;
using Messages;
using Lidgren.Network;

namespace Project23Client
{
    public delegate void MessageHandler(MessageClass message);

    public delegate void AuthenticationCallback(ref string username, ref string token);
    public delegate void JoinInfoCallback(ref string callsign, ref string pilot);

    public delegate void ServerVersionHandler ( object sender, int version );
    public delegate void PlayerEventHandler(object sender, Player player);

    public delegate void ChatEventHandler ( object sender, string channel, string from, string message );

    public partial class GameClient
    {
        public Sim sim = new Sim();
        public Player ThisPlayer = null;

        public event ServerVersionHandler ServerVersionEvent;
        public event PlayerEventHandler LocalPlayerJoinedEvent;
        public event ChatEventHandler ChatSentEvent;
        public event ChatEventHandler ChatReceivedEvent;

        Client client;
        bool connected = false;

        public bool ConnectedToHost
        {
            get { return connected; }
        }

        public AuthenticationCallback GetAuthentication;
        public JoinInfoCallback GetJoinInfo;

        MessageMapper messageMapper = new MessageMapper();

        public GameClient ( string address, int port )
        {
            InitMessageHandlers();
            client = new Client(address, port);
        }

        public void Kill ()
        {
            client.Kill();
        }

        public bool Update (double time)
        {
            if (connected)
            {
                if (!client.IsConnected)
                    return false;

                NetBuffer buffer = client.GetPentMessage();
                while (buffer != null)
                {
                    if (buffer.LengthBytes >= sizeof(Int32))
                    {
                        int name = buffer.ReadInt32();
                        MessageClass msg = messageMapper.MessageFromID(name);
                        msg.Unpack(ref buffer);

                        if (messageHandlers.ContainsKey(msg.GetType()))
                            messageHandlers[msg.GetType()](msg);
                    }
                    buffer = client.GetPentMessage();
                }

                sim.Update(time);
            }
            else
                connected = client.IsConnected;
            return true;
        }

        public void SendChat ( string channel, string message )
        {
            if (message == string.Empty)
                return;

            if (ChatSentEvent != null)
                ChatSentEvent(this, channel, ThisPlayer.Callsign, message);

            ChatMessage msg = new ChatMessage();
            msg.Channel = channel;
            msg.Message = message;
            client.SendMessage(msg.Pack(), msg.Channel());
        }
    }
}
