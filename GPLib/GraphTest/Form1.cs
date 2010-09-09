using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

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

        protected Module CurrentModule = null;

        protected Dictionary<string, Type> Modules = new Dictionary<string, Type>();

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
            menu.Handler = handler;
            menu.Menu = new ToolStripMenuItem(name);
            menu.Menu.Click +=new EventHandler(Menu_Click);
            menu.Menu.Tag = menu;

            ModuleMenus.Add(LastMenuID, menu);

            ToolStripMenuItem p = optionsToolStripMenuItem;
            if (ModuleMenus.ContainsKey(parrent))
                p = ModuleMenus[parrent].Menu;

            p.DropDownItems.Add(menu.Menu);
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
                        Modules.Add(m.Name(), t);
                }
            }
        }

        protected void LoadModuleMenus ()
        {
            moduleToolStripMenuItem.DropDownItems.Clear();

            foreach (KeyValuePair<string,Type> module in Modules)
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
            optionsToolStripMenuItem.DropDownItems.Clear();
            LastMenuID = 0;
        }

        void LoadModule ( Type t )
        {
            UnloadModule();
            CurrentModule = (Module)Activator.CreateInstance(t);
            CurrentModule.Create(API);
            this.Text = CurrentModule.Name();
        }

        void menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu != null)
            {
                Type t = menu.Tag as Type;
                if (t != null)
                    LoadModule(t);
            }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            API = new ViewAPI(this);
            LoadModules(Assembly.GetExecutingAssembly());
            LoadModuleMenus();

            if (Modules.Count > 0)
            {
                foreach(KeyValuePair<string,Type> m in Modules)
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
