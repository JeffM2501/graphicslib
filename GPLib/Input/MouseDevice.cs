/*
    Open Combat/Projekt 2501
    Copyright (C) 2010  Jeffery Allen Myers

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Input
{
    public struct MouseMoveEventArgs
    {
        //
        // Summary:
        //     Initializes a new instance of the OpenTK.Input.MouseMoveEventArgs class.
        //
        // Parameters:
        //   x:
        //     The X position.
        //
        //   y:
        //     The Y position.
        //
        //   xDelta:
        //     The change in X position produced by this event.
        //
        //   yDelta:
        //     The change in Y position produced by this event.
        public MouseMoveEventArgs(int x, int y, int xDelta, int yDelta, MouseButtons buttons)
        {
            Position = new Point(x, y);
            X = x;
            Y = y;
            XDelta = xDelta;
            YDelta = yDelta;
            Buttons = buttons;
        }

        // Summary:
        //     Gets a System.Drawing.Points representing the location of the mouse for the
        //     event.
        public Point Position;
        //
        // Summary:
        //     Gets the X position of the mouse for the event.
        public int X;
        //
        // Summary:
        //     Gets the change in X position produced by this event.
        public int XDelta;
        //
        // Summary:
        //     Gets the Y position of the mouse for the event.
        public int Y;
        //
        // Summary:
        //     Gets the change in Y position produced by this event.
        public int YDelta;

        public MouseButtons Buttons;
    } 

    public struct MouseButtonEventArgs
    {
        public MouseButtonEventArgs(MouseButtons b, bool down, Point pos)
        {
            this.Button = b;
            this.IsPressed = down;
            this.Position = pos;
            this.X = pos.X;
            this.Y = pos.Y;
        }
        // Summary:
        //     The mouse button for the event.
        public MouseButtons Button;
        //
        // Summary:
        //     Gets a System.Boolean representing the state of the mouse button for the
        //     event.
        public bool IsPressed;
        //
        // Summary:
        //     Gets a System.Drawing.Points representing the location of the mouse for the
        //     event.
        public Point Position;
        //
        // Summary:
        //     Gets the X position of the mouse for the event.
        public int X;
        //
        // Summary:
        //     Gets the Y position of the mouse for the event.
        public int Y;
    }

    public struct MouseWheelEventArgs
    {
        //
        // Summary:
        //     Initializes a new instance of the OpenTK.Input.MouseWheelEventArgs class.
        //
        // Parameters:
        //   x:
        //     The X position.
        //
        //   y:
        //     The Y position.
        //
        //   value:
        //     The value of the wheel.
        //
        //   delta:
        //     The change in value of the wheel for this event.
        public MouseWheelEventArgs(int x, int y, int value, int delta)
        {
            Delta = delta;
            Position = new Point(x, y);
            Value = value;
            X = x;
            Y = y;
        }

        // Summary:
        //     The change in value of the wheel for this event.
        public int Delta;
        //
        // Summary:
        //     Gets a System.Drawing.Points representing the location of the mouse for the
        //     event.
        public Point Position;
        //
        // Summary:
        //     The value of the wheel.
        public int Value;
        //
        // Summary:
        //     Gets the X position of the mouse for the event.
        public int X;
        //
        // Summary:
        //     Gets the Y position of the mouse for the event.
        public int Y;
    }
   
    public delegate void MouseMoveEventHandler(MouseDevice sender, MouseMoveEventArgs e);
    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);

    public abstract class MouseDevice
    {
        public event MouseButtonEventHandler ButtonDown;
        public event MouseButtonEventHandler ButtonUp;
        public event MouseMoveEventHandler Move;
        public event MouseWheelEventHandler WheelChanged;

        public virtual bool this[MouseButtons button]
        {
            get { return false; }
        }

        public virtual int X
        {
            get { return 0; }
        }
        public virtual int Y
        {
            get { return 0; }
        }
        public virtual int XDelta
        {
            get { return 0; }
        }
        public virtual int YDelta
        {
            get { return 0; }
        }
        public virtual int NumberOfButtons
        {
            get { return 0; }
        }
        public virtual int NumberOfWheels
        {
            get { return 0; }
        }

        public virtual void Capture ( bool cap )
        {
        }

        public virtual void Hide()
        {
        }

        public virtual void Show()
        {
        }

        protected void SendButton(MouseButtonEventArgs args, bool up)
        {
            if (up && ButtonUp != null)
                ButtonUp(this,args);
            else if (!up && ButtonDown != null)
                ButtonDown(this,args);
        }

        protected void SendMove(MouseMoveEventArgs args)
        {
            if (Move != null)
                Move(this, args);
        }

        protected void SendWheel(MouseWheelEventArgs args)
        {
            if (WheelChanged != null)
                WheelChanged(this, args);
        }

      //  public event MouseMoveEventHandler Move;
      //  public event MouseWheelEventHandler WheelChanged;
    }
}
