using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

using OpenTK.Graphics;
using OpenTK;

using Drawables.Cameras;

//using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Utilities.Paths;

namespace GraphTest
{
    public class ViewAPI
    {
        internal Form1 form;
        internal ViewAPI (Form1 f)
        {
            form = f;

            form.glControl1.Resize += new EventHandler(glControl1_Resize);
            form.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(form_MouseDown);
            form.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(form_MouseUp);
            form.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(form_MouseMove);
            form.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(form_MouseWheel);
        }

        void form_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            double thisPos = e.Delta / 120;
            LastMoveArgs.WheelDelta = e.Delta / 120;
            LastMoveArgs.Wheel += LastMoveArgs.WheelDelta;

            if (MouseMove != null)
                MouseMove(LastMoveArgs);

            LastMoveArgs.WheelDelta = 0;
        }

        void form_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Vector2 v = new Vector2(e.X,e.Y);

            LastMoveArgs.PosDelta = v - LastMoveArgs.Position;
            LastMoveArgs.Position = v;
            if (MouseMove != null)
                MouseMove(LastMoveArgs);

            LastMoveArgs.PosDelta = Vector2.Zero;
        }

        void form_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LastMoveArgs.Buttons[0] = false;
                if (MouseUp != null)
                    MouseUp(EventArgs.Empty, 0);
            }

            if (e.Button == MouseButtons.Right)
            {
                LastMoveArgs.Buttons[1] = false;
                if (MouseUp != null)
                    MouseUp(EventArgs.Empty, 1);
            }

            if (e.Button == MouseButtons.Middle)
            {
                LastMoveArgs.Buttons[2] = false;
                if (MouseUp != null)
                    MouseUp(EventArgs.Empty, 2);
            }

            if (e.Button == MouseButtons.XButton1)
            {
                LastMoveArgs.Buttons[3] = false;
                if (MouseUp != null)
                    MouseUp(EventArgs.Empty, 3);
            }

            if (e.Button == MouseButtons.XButton2)
            {
                LastMoveArgs.Buttons[4] = false;
                if (MouseUp != null)
                    MouseUp(EventArgs.Empty,4);
            }
       }

        void form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LastMoveArgs.Buttons[0] = true;
                if (MouseDown != null)
                    MouseDown(EventArgs.Empty, 0);
            }

            if (e.Button == MouseButtons.Right)
            {
                LastMoveArgs.Buttons[1] = true;
                if (MouseDown != null)
                    MouseDown(EventArgs.Empty, 1);
            }

            if (e.Button == MouseButtons.Middle)
            {
                LastMoveArgs.Buttons[2] = true;
                if (MouseDown != null)
                    MouseDown(EventArgs.Empty, 2);
            }

            if (e.Button == MouseButtons.XButton1)
            {
                LastMoveArgs.Buttons[3] = true;
                if (MouseDown != null)
                    MouseDown(EventArgs.Empty, 3);
            }

            if (e.Button == MouseButtons.XButton2)
            {
                LastMoveArgs.Buttons[4] = true;
                if (MouseDown != null)
                    MouseDown(EventArgs.Empty, 4);
            }
        }

        void glControl1_Resize(object sender, EventArgs e)
        {
            if (Resize != null)
                Resize(e);
        }

        public delegate void EmptyHandler ( EventArgs args );
        public delegate void MenuHandler ( EventArgs args, string menu );
        public delegate void MouseButtonHandler( EventArgs args, int button);
        public delegate void MouseMoveHandler(MouseMoveArgs args);

        public class MouseMoveArgs : EventArgs
        {
            public Vector2 Position = Vector2.Zero;
            public Vector2 PosDelta = Vector2.Zero;
            public double Wheel = 0;
            public double WheelDelta = 0;
            public bool[] Buttons = new bool[5];
        }

        private MouseMoveArgs LastMoveArgs = new MouseMoveArgs();

        public Vector2 MousePosition
        {
            get { return LastMoveArgs.Position; }
        }

        public double MouseWheelPosition
        {
            get { return LastMoveArgs.Wheel; }
        }

        public event MouseMoveHandler MouseMove;

        public event MouseButtonHandler MouseDown, MouseUp;

        public event EmptyHandler Resize;

        public int Height
        {
            get { return form.glControl1.Height; }
        }

        public int Width
        {
            get { return form.glControl1.Width; }
        }

        public void SetFPS ( int fps )
        {
            form.FPSTimer.Stop();
            form.FPSTimer.Interval = (int)(1.0 / fps * 1000.0);
            form.FPSTimer.Start();
        }

        public void SetCurrent ()
        {
            form.glControl1.MakeCurrent();
        }

        public void SwapBuffers()
        {
            form.glControl1.SwapBuffers();
        }

        public int AddMenu (string name, MenuHandler handler )
        {
            return AddMenu(name, handler, -1);
        }

        public int AddMenu (string name, MenuHandler handler, int parrent )
        {
            return form.AddMenu(name, handler, parrent);
        }

        public void Invalidate ()
        {
            form.glControl1.Invalidate();
        }

        public void AddPath ( string path )
        {
            ResourceManager.AddPath(path);
        }

        public FileInfo GetModuleDataFile( string name )
        {
            return new FileInfo(Path.Combine(Path.Combine(Application.CommonAppDataPath, form.CurrentModule.Name()),name));
        }

        public string GetFile(string name)
        {
            return ResourceManager.FindFile(name);
        }

        public  List<string> GetFiles(string path)
        {
            return ResourceManager.FindFiles(path, string.Empty);
        }

        public List<string> GetFiles(string path, string filter)
        {
            return ResourceManager.FindFiles(path, filter);
        }

        public string GetDirectory(string name)
        {
            return ResourceManager.FindDirectory(name);
        }
    }

    public class Module
    {
        public virtual string Name() { return string.Empty; }

        protected ViewAPI API;

        public void Create (ViewAPI v)
        {
            API = v;
            Init(true);
        }

        public virtual void Init ( bool first )
        {
        }

        public virtual void Update(double time)
        {
        }

        public virtual void Close ()
        {

        }
    }

    public class StandardGLViewModule : Module
    {
        public override string Name() { return String.Empty; }

        protected double FOV = 45;
        protected double Hither = 1;
        protected double Yon = 1000;
        protected double OrthoNear = 0;
        protected double OrthoFar = 100;

        protected Color ClearColor = Color.DarkGray;

        public override void Init(bool first)
        {
            ClearColor = Color.DarkGray;
            API.SetCurrent();
            Load(first);

            API.Resize += new ViewAPI.EmptyHandler(API_Resize);

            GL.ClearColor(ClearColor);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.DepthTest);

            // setup light 0
            Vector4 lightInfo = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Light(LightName.Light0, LightParameter.Ambient, lightInfo);

            lightInfo = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lightInfo);
            GL.Light(LightName.Light0, LightParameter.Specular, lightInfo);

            API_Resize(EventArgs.Empty);

            SetGLOptons();
        }

        void API_Resize(EventArgs args)
        {
            GL.Viewport(0, 0, API.Width, API.Height); // Use all of the glControl painting area  
        }

        protected void SetPerspective()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float aspect = (float)API.Width / (float)API.Height;

            Glu.Perspective(FOV, aspect, Hither, Yon);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        protected void SetOrthographic()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, API.Width, 0, API.Height, OrthoNear, OrthoFar);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public override void Update(double time)
        {
            API.SetCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SetCamera(time,true);
            Draw3D(time);

            SetCamera(time, false);
            DrawOverlay(time);

            API.SwapBuffers();
        }

        public override void Close()
        {
            UnLoad();
        }

        public virtual void Load ( bool first )
        {
        }

        public virtual void SetGLOptons()
        {
        }

        public virtual void UnLoad()
        {
        }

        public virtual void SetCamera(double time, bool perspective)
        {
            if (perspective)
            {
                SetPerspective();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                GL.Translate(0, 0, -10);						// pull back on allong the zoom vector
                GL.Rotate(0, 1.0f, 0.0f, 0.0f);					// pops us to the tilt
                GL.Rotate(0, 0.0f, 1.0f, 0.0f);					// gets us on our rot
                GL.Translate(0, 0, 0);	                        // take us to the pos
                GL.Rotate(-90, 1.0f, 0.0f, 0.0f);				// gets us into XY
            }
            else
                SetOrthographic();
        }

        public virtual void Draw3D(double time)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.LineSmooth);

            GL.LineWidth(2);
            GL.Begin(BeginMode.Lines);

            GL.Color3(Color.Blue);
            GL.Vertex3(-1, 0, 0);
            GL.Vertex3(2, 0, 0);

            GL.Color3(Color.Red);
            GL.Vertex3(0, -1, 0);
            GL.Vertex3(0, 2, 0);

            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, -1);
            GL.Vertex3(0, 0, 4);
            GL.End();

            GL.Enable(EnableCap.LineSmooth);
            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);
            GL.DepthMask(true);
        }

        public virtual void DrawOverlay(double time)
        {
        }
    }

    public class MouseInspectionCameraModule : StandardGLViewModule
    {
        public override string Name() { return "MouseInspectionCamera"; }

        protected bool UseMouse = true;

        protected Vector3 ViewPosition = Vector3.Zero;
        protected double Spin = 0;
        protected double Tilt = 0;
        protected double Pullback = 10;

        protected bool DrawGrid = true;
        protected double GridSize = 1.0;
        protected double GridBounds = 25.0;

        protected double WheelIncrement = 0.25;
        public override void Load(bool first)
        {
            ClearColor = Color.WhiteSmoke;
            API.MouseMove += new ViewAPI.MouseMoveHandler(API_MouseMove);
        }

        void API_MouseMove(ViewAPI.MouseMoveArgs args)
        {
            if (!UseMouse)
                return;

            if (args.Buttons[1])
            {
                Spin -= args.PosDelta.X;
                Tilt += args.PosDelta.Y;
            }
            Pullback += args.WheelDelta * WheelIncrement;
            if (Pullback < 0)
                Pullback = 0;

            API.Invalidate();
        }

        public override void SetCamera(double time, bool perspective)
        {
            if (perspective)
            {
                base.SetPerspective();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                GL.Translate(0, 0, -Pullback);						// pull back on allong the zoom vector
                GL.Rotate(Tilt, 1.0f, 0.0f, 0.0f);					// pops us to the tilt
                GL.Rotate(-Spin, 0.0f, 1.0f, 0.0f);					// gets us on our rot
                GL.Translate(-ViewPosition.X, -ViewPosition.Z, ViewPosition.Y);	                        // take us to the pos
                GL.Rotate(-90, 1.0f, 0.0f, 0.0f);				    // gets us into XY
            }
            else
                base.SetOrthographic();
        }

        public override void Draw3D(double time)
        {
            if (!DrawGrid)
                return;

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.LineSmooth);

            GL.LineWidth(2);
            GL.Begin(BeginMode.Lines);

                GL.Color3(Color.DarkGray);
                for (double i = -GridBounds; i <= GridBounds; i += GridSize)
                {
                    if (i != 0)
                    {
                        GL.Vertex3(-GridBounds, i, 0);
                        GL.Vertex3(GridBounds, i, 0);
                        GL.Vertex3(i, -GridBounds, 0);
                        GL.Vertex3(i, GridBounds, 0);
                    }
                    else
                    {
                        GL.Vertex3(-GridBounds, i, 0);
                        GL.Vertex3(-GridSize, i, 0);
                        GL.Vertex3(GridBounds, i, 0);
                        GL.Vertex3(GridSize, i, 0);
                        GL.Vertex3(i, -GridBounds, 0);
                        GL.Vertex3(i, -GridSize, 0);
                        GL.Vertex3(i, GridBounds, 0);
                        GL.Vertex3(i, GridSize, 0);
                    }
                }

                GL.Color3(Color.Red);
                GL.Vertex3(-GridSize, 0, 0);
                GL.Vertex3(GridSize, 0, 0);
                GL.Vertex3(GridSize, 0, 0);
                GL.Vertex3(GridSize * 0.75, GridSize * 0.125, 0);

                GL.Color3(Color.Green);
                GL.Vertex3(0, -GridSize, 0);
                GL.Vertex3(0, GridSize, 0);
                GL.Vertex3(0, GridSize, 0);
                GL.Vertex3(GridSize * 0.125, GridSize * 0.75, 0);

                GL.Color3(Color.Blue);
                GL.Vertex3(0, 0, -GridSize);
                GL.Vertex3(0, 0, GridSize*2);

            GL.End();

            GL.Enable(EnableCap.LineSmooth);
            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);
            GL.DepthMask(true);
        }
    }

    public class MouseCameraModule : StandardGLViewModule
    {
        public override string Name() { return "MouseFrustumCameraModule"; }

        protected Camera TheCamera = new Camera();

        public override void Load(bool first)
        {
            API.Resize += new ViewAPI.EmptyHandler(API_Resize);
            ClearColor = Color.Blue;
        }

        public override void SetGLOptons()
        {
            base.SetGLOptons();
            TheCamera.set(new Vector3(0, 0, 0), 0, 0);
            TheCamera.Resize(API.Width, API.Height);
        }

        void API_Resize(EventArgs args)
        {
            TheCamera.Resize(API.Width, API.Height);
        }

        public override void SetCamera(double time, bool perspective)
        {
            if (perspective)
            {
                TheCamera.SetPersective();
                TheCamera.Execute();
            }
            else
                TheCamera.SetOrthographic();
        }

        public override void Draw3D(double time)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.LineSmooth);

            GL.LineWidth(2);
            GL.Begin(BeginMode.Lines);

            GL.Color3(Color.Blue);
            GL.Vertex3(-1, 0, 0);
            GL.Vertex3(2, 0, 0);

            GL.Color3(Color.Red);
            GL.Vertex3(0, -1, 0);
            GL.Vertex3(0, 2, 0);

            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, -1);
            GL.Vertex3(0, 0, 4);
            GL.End();

            GL.Enable(EnableCap.LineSmooth);
            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);
            GL.DepthMask(true);
        }

        public override void DrawOverlay(double time)
        {
        }
    }
}
