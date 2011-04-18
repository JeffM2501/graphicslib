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
using Input;

namespace GUIGameWindow
{
    public class GUIMouseDevice : MouseDevice
    {
        Dictionary<MouseButtons, bool> buttonMap = new Dictionary<MouseButtons,bool>();

        public override bool this[MouseButtons button]
        {
            get 
            {
                if (!buttonMap.ContainsKey(button))
                    return false;
                else
                    return buttonMap[button]; 
            }
        }

        public override int X
        {
            get { return lastPoint.X; }
        }
        public override int Y
        {
            get { return lastPoint.Y; }
        }
        public override int XDelta
        {
            get { return lastDelta.X; }
        }
        public override int YDelta
        {
            get { return lastDelta.Y; }
        }

        public override int NumberOfButtons
        {
            get { return 5; }
        }

        public override int NumberOfWheels
        {
            get { return 1; }
        }

        Point lastPoint = new Point(0, 0);
        Point lastDelta = new Point(0, 0);
        bool lastPointSet = false;

        Control control = null;

        int wheelCount = 0;
        int lastWheelDelta = 0;

        public GUIMouseDevice(Control ctl)
        {
            control = ctl;
            ctl.MouseDown += new MouseEventHandler(InternalDown);
            ctl.MouseUp += new MouseEventHandler(InternalUp);

            ctl.MouseMove += new MouseEventHandler(InternalMove);
            ctl.MouseWheel += new MouseEventHandler(InternalWheel);
        }

        public override void Capture(bool cap)
        {
            control.Capture = cap;
        }

        public override void Hide()
        {
            Cursor.Hide();
        }

        public override void Show()
        {
            Cursor.Show();
        }

        protected void InternalDown(Object sender, MouseEventArgs args)
        {
            buttonMap[args.Button] = true;
            base.SendButton(new MouseButtonEventArgs(args.Button, true, args.Location), false);
        }

        protected void InternalUp(Object sender, MouseEventArgs args)
        {
            buttonMap[args.Button] = false;
            base.SendButton(new MouseButtonEventArgs(args.Button, true, args.Location), true);
        }

        protected void InternalMove(Object sender, MouseEventArgs args)
        {
            if (lastPointSet)
            {
                lastDelta = new Point(args.Location.X - lastPoint.X,args.Location.Y - lastPoint.Y);
                base.SendMove(new MouseMoveEventArgs(args.Location.X, args.Location.Y, lastDelta.X, lastDelta.Y, args.Button));
            }

            lastPoint = new Point(args.Location.X,args.Location.Y);
            lastPointSet = true;
        }

        protected void InternalWheel(Object sender, MouseEventArgs args)
        {
            lastWheelDelta = args.Delta;
            wheelCount += args.Delta;

            lastDelta = new Point(args.Location.X - lastPoint.X, args.Location.Y - lastPoint.Y);
            lastPoint = new Point(args.Location.X, args.Location.Y);
            lastPointSet = true;

            base.SendWheel(new MouseWheelEventArgs(args.Location.X, args.Location.Y, wheelCount, args.Delta));
        }
    }
}
