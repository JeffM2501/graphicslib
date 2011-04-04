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

using Drawables.DisplayLists;
using Drawables.Materials;

namespace Drawables
{
    public delegate bool ExecuteCallback(Material mat, object tag);

    public class ExecuteItem
    {
        public ExecuteCallback callback = null;
        public object tag = null;

        public bool call ( Material mat )
        {
            if (callback != null)
                return callback(mat, tag);

            return false;
        }
    }

    public class SingleListDrawableItem : IDisposable
    {
        public ListableEvent list = null;
        static string defaultMatName = "SingleListDrawableItemWhite";
        Material mat = MaterialSystem.system.GetMaterial(defaultMatName);
        int pass = DrawablesSystem.FirstPass;

        public delegate void ShouldDrawItemHandler(object sender, ref bool draw);

        public event ShouldDrawItemHandler ShouldDrawItem;

        public SingleListDrawableItem ( Material _mat, ListableEvent.GenerateEventHandler handler, object tag, int _pass )
        {
            mat = _mat;
            pass = _pass;
            Install(handler, tag);
        }

        public SingleListDrawableItem(Material _mat, ListableEvent.GenerateEventHandler handler, object tag)
        {
            mat = _mat;
            Install(handler, tag);
        }

        public SingleListDrawableItem( ListableEvent.GenerateEventHandler handler, object tag)
        {
            Install(handler, tag);
        }

        public SingleListDrawableItem(ListableEvent.GenerateEventHandler handler, object tag, int _pass)
        {
            pass = _pass;
            Install(handler, tag);
        }

        public SingleListDrawableItem(ListableEvent.GenerateEventHandler handler)
        {
            Install(handler, null);
        }

        public SingleListDrawableItem(ListableEvent.GenerateEventHandler handler, int _pass)
        {
            pass = _pass;
            Install(handler, null);
        }

        public SingleListDrawableItem(Material _mat, ListableEvent.GenerateEventHandler handler, int _pass)
        {
            mat = _mat;
            pass = _pass;
            Install(handler, null);
        }

        protected void Install ( ListableEvent.GenerateEventHandler handler, object tag )
        {
            list = new ListableEvent();
            list.Generate += handler;
            list.Tag = tag;
            if (mat == null)
            {
                mat = MaterialSystem.system.NewMaterial();
                mat.name = defaultMatName;
            }

            Add();
        }

        public void Add ( )
        {
            DrawablesSystem.system.addItem(mat, ExecuteCallback, pass);
        }

        public void Dispose()
        {
            if (list != null)
            {
                list.Dispose();
                list = null;
                DrawablesSystem.system.removeItem(mat, ExecuteCallback, pass);
            }
        }

        public bool ExecuteCallback(Material mat, object tag)
        {
            bool draw = true;
            if (ShouldDrawItem != null)
                ShouldDrawItem(this, ref draw);
            if (!draw)
                return false;

            if (list != null)
                list.Call();
            return list != null;
        }
    }

    public class DrawablesSystem
    {
        public static bool RestateMaterial = false;

        public static DrawablesSystem system = new DrawablesSystem();

        public static int FirstPass = 0;
        public static int MiddlePass = 500;
        public static int LastPass = 1000;

        Dictionary<int, Dictionary<Material, List<ExecuteItem>>> passes = new Dictionary<int, Dictionary<Material, List<ExecuteItem>>>();

        public void addItem(Material mat, ExecuteCallback callback)
        {
            addItem(mat, callback, LastPass,null);
        }

        public void addItem(Material mat, ExecuteCallback callback, object tag)
        {
            addItem(mat, callback, LastPass,tag);
        }

        public void addItem (Material mat, ExecuteCallback callback, int pass)
        {
            addItem(mat, callback, pass, null);
        }

        public void addItem (Material mat, ExecuteCallback callback, int pass, object tag)
        {
            if (!passes.ContainsKey(pass))
                passes[pass] = new Dictionary<Material, List<ExecuteItem>>();

            Dictionary<Material, List<ExecuteItem>> passList = passes[pass];
            if (!passList.ContainsKey(mat))
                passList[mat] = new List<ExecuteItem>();

            ExecuteItem item = new ExecuteItem();
            item.callback = callback;
            item.tag = tag;
            passList[mat].Add(item);
        }

        public void removeItem(Material mat, ExecuteCallback callback)
        {
            removeItem(mat,callback,LastPass);
        }

        public void removeItem (Material mat, ExecuteCallback callback, int pass)
        {
            if (!passes.ContainsKey(pass))
                return;
            if (!passes[pass].ContainsKey(mat))
                return;
            foreach (ExecuteItem i in passes[pass][mat])
            {
                if (i.callback == callback)
                {
                    passes[pass][mat].Remove(i);
                    return;
                }
            }
        }

        public void removeAll ()
        {
            passes.Clear();
        }

        public void Execute ()
        {
            foreach(KeyValuePair<int,Dictionary<Material, List<ExecuteItem>>> pass in passes)
            {
                foreach(KeyValuePair<Material,List<ExecuteItem>> matList in pass.Value)
                {
                    matList.Key.Execute();
                    foreach (ExecuteItem item in matList.Value)
                    {
                        item.call(matList.Key);
                        if (RestateMaterial)
                            matList.Key.Execute();
                    }
                }
            }
        }
    }
}
