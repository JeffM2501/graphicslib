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
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace Drawables.DisplayLists
{
    public class DisplayList
    {
        int listID = -1;

        public Object Tag = null;

        bool generating = false;

        public bool Valid ()
        {
            return listID != -1;
        }

        public void Invalidate()
        {
            if (generating)
                End();

            if (listID != -1)
                GL.DeleteLists(listID,1);

            listID = -1;
        }

        public void Start ()
        {
            Start(false);
        }

        public void Start( bool execute )
        {
            Invalidate();
            listID = GL.GenLists(1);
            if (execute)
                GL.NewList(listID, ListMode.CompileAndExecute);
            else
                GL.NewList(listID, ListMode.Compile);

            generating = true;
        }

        public void End( )
        {
            if (generating)
                GL.EndList();

            generating = false;
        }

        public bool Call()
        {
            if (generating || !Valid())
                return false;

            GL.CallList(listID);
            return true;
        }
    }

    public class DisplayListSystem
    {
        public static DisplayListSystem system = new DisplayListSystem();

        List<DisplayList> displayLists = new List<DisplayList>();

        public void Invalidate()
        {
            foreach (DisplayList d in displayLists)
                d.Invalidate();
        }

        public void Flush()
        {
            foreach (DisplayList d in displayLists)
                d.Invalidate();

            displayLists.Clear();
        }

        public DisplayList newList ()
        {
            DisplayList d = new DisplayList();
            displayLists.Add(d);
            return d;
        }

        public void deleteList( DisplayList d )
        {
            displayLists.Remove(d);
        }
    }

    public class ListableItem : IDisposable
    {
        protected DisplayList list = new DisplayList();

        public void Dispose()
        {
            if (list != null)
            {
                DisplayListSystem.system.deleteList(list);
                list = null;
            }
        }

        public void Invalidate()
        {
            list.Invalidate();
        }

        protected void Rebuild()
        {
            list.Start();
            GenerateList();
            list.End();
        }

        protected virtual void GenerateList()
        {

        }

        public virtual void Execute()
        {
            if (!list.Valid())
                Rebuild();

            list.Call();
        }
    }

    public class ListableEvent : IDisposable
    {
        public delegate void GenerateEventHandler(object sender, DisplayList list);
        public event GenerateEventHandler Generate;

        public object Tag
        {
            get { if (list != null) return list.Tag; else return null; }
            set { if (list != null) list.Tag = value; }
        }

        protected DisplayList list;

        DisplayListSystem system = null;

        public ListableEvent()
        {
            list = DisplayListSystem.system.newList();
        }

        public ListableEvent(DisplayListSystem s)
        {
            system = s;
            list = system.newList();
        }

        public ListableEvent ( GenerateEventHandler handler )
        {
            list = DisplayListSystem.system.newList();
            Generate += handler;
        }

        public ListableEvent(GenerateEventHandler handler, DisplayListSystem s)
        {
            system = s;
            list = system.newList();
            Generate += handler;
        }

        protected DisplayListSystem GetSystem ()
        {
            if (system != null)
                return system;
            return DisplayListSystem.system;
        }

        public void Dispose()
        {
            Invalidate();

            if (list != null)
            {
                GetSystem().deleteList(list);
                list = null;
            }
        }

        public void Invalidate ()
        {
            if (list != null)
                list.Invalidate();
        }

        public void Draw ()
        {
            if (Generate == null)
                return;

            Generate(this, list);
        }

        public void Call ()
        {
            if (Generate == null)
                return;

            if (!list.Valid())
            {
                list.Start(true);
                Generate(this, list);
                list.End();
            }
            else
                list.Call();
        }
    }
}
