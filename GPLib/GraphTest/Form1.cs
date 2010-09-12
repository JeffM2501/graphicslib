using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

using Utilities.Paths;

namespace GraphTest
{
    internal partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        internal class ModuleMenu
        {
            public string Name;
            public ViewAPI.MenuHandler Handler;
            public ToolStripMenuItem Menu;
        }

        protected ViewAPI API;

        public Module CurrentModule = null;

        protected class ModuleDef
        {
            public Type t;
            public string path;
        }

        protected Dictionary<string, ModuleDef> Modules = new Dictionary<string, ModuleDef>();

        public Timer FPSTimer = new Timer();

        protected double Now = 0, StartTime = 0;
        protected Stopwatch RuntimeTimer = new Stopwatch();

        protected Int32 LastMenuID = 0;

        protected Dictionary<int, ModuleMenu> ModuleMenus = new Dictionary<int, ModuleMenu>();

        internal int AddMenu (string name, ViewAPI.MenuHandler handler, int parrent )
        {
            LastMenuID++;
            ModuleMenu menu = new ModuleMenu();
            menu.Name = name;
           
            ModuleMenus.Add(LastMenuID, menu);

            if (ModuleMenus.ContainsKey(parrent))
            {
                menu.Handler = handler;
                menu.Menu = new ToolStripMenuItem(name);
                menu.Menu.Click += new EventHandler(Menu_Click);
                menu.Menu.Tag = menu;
                ModuleMenus[parrent].Menu.DropDownItems.Add(menu.Menu);
            }
            else
                PluginMenuStrip.Items.Add(name);
            return LastMenuID;
        }

        void Menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem m = sender as ToolStripMenuItem;
            if (m != null)
            {
                ModuleMenu menu = m.Tag as ModuleMenu;
                if (menu != null)
                    menu.Handler(e, menu.Name);
            }
        }

        private void x600ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected void SetNow ()
        {
            Now = RuntimeTimer.ElapsedMilliseconds / 1000.0;
        }

        protected void LoadModules ( Assembly dll )
        {
            foreach(Type t in dll.GetTypes())
            {
                if (!t.IsAbstract && t.IsSubclassOf(typeof(Module)))
                {
                    Module m = (Module)Activator.CreateInstance(t);
                    if (m.Name() != string.Empty)
                    {
                        ModuleDef md = new ModuleDef();
                        md.t = t;
                        md.path = dll.Location;
                        Modules.Add(m.Name(), md);
                    }
                }
            }
        }

        protected void LoadModuleMenus ()
        {
            PluginMenuStrip.Items.Clear();
            foreach (KeyValuePair<string,ModuleDef> module in Modules)
            {
                ToolStripMenuItem menu = new ToolStripMenuItem(module.Key);
                menu.Click += new EventHandler(menu_Click);
                menu.Tag = module.Value;
                moduleToolStripMenuItem.DropDownItems.Add(menu);
            }
        }

        void UnloadModule ()
        {
            if (CurrentModule == null)
                return;

            ModuleMenus.Clear();
            PluginMenuStrip.Items.Clear();
            LastMenuID = 0;
        }

        void LoadModule ( ModuleDef md )
        {
            UnloadModule();

            // executable dir
            ResourceManager.KillPaths();
            ResourceManager.AddPath(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "data"));
            ResourceManager.AddPath(Path.GetDirectoryName(Application.CommonAppDataPath));

            CurrentModule = (Module)Activator.CreateInstance(md.t);
            DirectoryInfo appDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), CurrentModule.Name()));
            if (!appDir.Exists)
                appDir.Create();

            ResourceManager.AddPath(appDir.FullName);
            ResourceManager.AddPath(md.path);

            CurrentModule.Create(API);
            this.Text = CurrentModule.Name();
        }

        void menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu != null)
            {
                ModuleDef md = menu.Tag as ModuleDef;
                if (md != null)
                    LoadModule(md);
            }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            API = new ViewAPI(this);
            LoadModules(Assembly.GetExecutingAssembly());
            LoadModuleMenus();

            if (Modules.Count > 0)
            {
                foreach (KeyValuePair<string, ModuleDef> m in Modules)
                {
                    LoadModule(m.Value);
                    break;
                }
            }

            RuntimeTimer.Start();
            SetNow();
            StartTime = Now;
            
            FPSTimer.Tick += new EventHandler(FPSTimer_Tick);
            FPSTimer.Interval = (int)(1.0 / 30.0 * 1000.0);
            FPSTimer.Start();
        }

        void FPSTimer_Tick(object sender, EventArgs e)
        {
            SetNow();
            if (CurrentModule != null)
                CurrentModule.Update(Now-StartTime);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentModule != null)
                CurrentModule.Update(Now - StartTime);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CurrentModule != null)
                CurrentModule.Close();
        }
    }
}
