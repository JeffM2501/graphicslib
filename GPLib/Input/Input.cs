using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Input
{
    public class InputManager
    {
        protected KeyboardDevice Keyboard;
        protected MouseDevice Mouse;

        public class InputEventArgs
        {
            public int EventID = -1;
            public int Position = 0;
            public float Paramater = 0.0f;
        }

        public delegate void InputEvent(object sender, InputEventArgs args);

        protected class EventWraper
        {
            public event InputEvent Call;
        }

        protected Dictionary<int, EventWraper > EventCallbacks = new Dictionary<int, EventWraper >;
        protected Dictionary<Keys,List<int> > KeyToEventIDMap = new Dictionary<Keys,List<int> >;
        protected Dictionary<MouseButtons,List<int> > ButtonToEventIDMap = new Dictionary<MouseButtons,List<int> >;

        protected class EventDeviceInfo
        {
            public enum EventType
            {
                KeyPressEvent,
                MouseButtonEvent,
                MouseWheelEvent,
                MouseMovedEvent,
            }

            public EventType Type = EventType.KeyPressEvent;

            public Keys Key = Keys.Enter;
            public MouseButtons Buttons = MouseButtons.Left;
            public int Axis = 0;
        }

        protected Dictionary<int,List<EventDeviceInfo> > EventInfos = new Dictionary<int,List<EventDeviceInfo> >();

        protected int WheelCount = 0;

        public InputManager(KeyboardDevice k, MouseDevice m)
        {
            Keyboard = k;
            Mouse = m;

            Mouse.WheelChanged +=new MouseWheelEventHandler(Mouse_WheelChanged);
        }

        void  Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            WheelCount += e.Delta;
        }

        public void ProcessEvents()
        {

        }

        public Point GetCursorPos ()
        {
            return new Point(Mouse.X,Mouse.Y);
        }

        public InputEventArgs PollInput(int EventID)
        {
            InputEventArgs args = new InputEventArgs();
            args.EventID = EventID;

            if (EventInfos.ContainsKey(EventID))
            {
                foreach (EventDeviceInfo infos in EventInfos[EventID])
                {
                    int value = 0;
                    switch (infos.Type)
                    {
                        case EventDeviceInfo.EventType.KeyPressEvent:
                           if (Keyboard[infos.Key])
                               value = 1;
                            break;
                        case EventDeviceInfo.EventType.MouseButtonEvent:
                            if (Mouse[infos.Buttons])
                                value = 1;
                             break;
                        case EventDeviceInfo.EventType.MouseMovedEvent:
                            if (infos.Axis == 0)
                                value = Mouse.X;
                            else
                                value = Mouse.Y;
                            break;
                        case EventDeviceInfo.EventType.MouseWheelEvent:
                            value = WheelCount;
                    }
                }
            }

            return args;
        }

        public void AddEventCallback ( int EventID, InputEvent handler )
        {
            if (!EventCallbacks.ContainsKey(EventID))
                EventCallbacks.Add(EventID,new EventWraper());

            EventCallbacks[EventID].Call += handler;
        }

        public void RemoveEventCallback ( int EventID, InputEvent handler )
        {
            if (!EventCallbacks.ContainsKey(EventID))
                return;

            EventCallbacks[EventID].Call -= handler;
        }

        public void AddKeyEvent ( int EventID, Keys key )
        {
            if (!KeyToEventIDMap.ContainsKey(key))
                KeyToEventIDMap.Add(key,new List<int>);

            if (!KeyToEventIDMap[key].Contains(EventID))
                KeyToEventIDMap[key].Add(EventID);
        }

        public void AddKeyEvent ( int EventID, Keys key )
        {
            if (!KeyToEventIDMap.ContainsKey(key))
                KeyToEventIDMap.Add(key,new List<int>);

            if (!KeyToEventIDMap[key].Contains(EventID))
                KeyToEventIDMap[key].Add(EventID);
        }

    }
}
