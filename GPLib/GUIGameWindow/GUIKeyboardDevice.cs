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
using System.Text;
using Input;

namespace GUIGameWindow
{
    public class GUIKeyboardDevice : KeyboardDevice
    {
        protected Dictionary<Keys, bool> keymap = new Dictionary<Keys,bool>();

        public override bool this[Keys key]
        {
            get 
            {
                if (!keymap.ContainsKey(key))
                    return false;
                else
                    return keymap[key]; 
            }
        }

        public GUIKeyboardDevice(Control ctl)
        {
            ctl.KeyDown += new KeyEventHandler(InternalDown);
            ctl.KeyUp += new KeyEventHandler(InternalUp);
        }

        protected void InternalDown(Object sender, KeyEventArgs args)
        {
            keymap[args.KeyCode] = true;
            base.SendKeys(args.KeyCode, false);
        }

        protected void InternalUp(Object sender, KeyEventArgs args)
        {
            keymap[args.KeyCode] = false;
            base.SendKeys(args.KeyCode, true);
        }
    }

}
