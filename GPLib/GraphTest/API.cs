using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics;
using OpenTK;
//using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace GraphTest
{
    public class ViewAPI
    {
        internal Form1 form;
        internal ViewAPI (Form1 f)
        {
            form = f;

            form.glControl1.Resize += new EventHandler(glControl1_Resize);
        }

        void glControl1_Resize(object sender, EventArgs e)
        {
            if (Resize != null)
                Resize(e);
        }

        public delegate void EmptyHandler ( EventArgs args );
        public delegate void MenuHandler ( EventArgs args, string menu );

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
        public override string Name() { return string.Empty; }

        protected double FOV = 45;
        protected double Hither = 1;
        protected double Yon = 1000;
        protected double OrthoNear = 0;
        protected double OrthoFar = 100;

        protected Color ClearColor = Color.DarkGray;

        public override void Init(bool first)
        {
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
            SetPerspective();
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

            SetPerspective();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            SetCamera(time);
            Draw3D(time);

            SetOrthographic();
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

        public virtual void SetCamera(double time)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Translate(0, 0, -10);						// pull back on allong the zoom vector
            GL.Rotate(0, 1.0f, 0.0f, 0.0f);					// pops us to the tilt
            GL.Rotate(0, 0.0f, 1.0f, 0.0f);					// gets us on our rot
            GL.Translate(0, 0, 0);	                        // take us to the pos
            GL.Rotate(-90, 1.0f, 0.0f, 0.0f);				// gets us into XY
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

    public class MouseCameraModule : StandardGLViewModule
    {
        public override string Name() { return "MouseCameraModule"; }

        public virtual void Load(bool first)
        {
            ClearColor = Color.Black;
        }

        public virtual void SetCamera(double time)
        {
         
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
}
