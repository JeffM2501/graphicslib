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


namespace Input
{
    public class KeyboardDevice
    {
        public delegate void KeyDownEvent(KeyboardDevice sender, Keys key);
        public delegate void KeyUpEvent(KeyboardDevice sender, Keys key);

        public event KeyUpEvent KeyUp;
        public event KeyDownEvent KeyDown;

        public virtual bool this[Keys key]
        {
            get { return false; }
        }

        protected void SendKeys(Keys k, bool up)
        {
            if (up && KeyUp != null)
                KeyUp(this, k);
            else if (!up && KeyDown != null)
                KeyDown(this, k);
        }
    }
}
