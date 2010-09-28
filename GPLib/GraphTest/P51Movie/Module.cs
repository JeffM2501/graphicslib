using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using GraphTest;
using Drawables.Textures;
using Drawables.DisplayLists;
using Drawables.StaticModels;
using Drawables.StaticModels.OBJ;

namespace P51Movie
{
    public class Module : GraphTest.MouseInspectionCameraModule
    {
        protected List<Sceene> Sceenes = new List<Sceene>();

        protected Sceene CurrentSceene = null;
        protected int Index = 0;

        public override string Name() { return "P51Movie"; }

        public override void Load(bool first)
        {
            base.Load(first);

            Sceenes.Add(new BZFlagDanceView(this.API,this));

            Index = 0;
            CurrentSceene = Sceenes[Index];
        }

        public override void SetGLOptons()
        {
            base.SetGLOptons();
            CurrentSceene.Load();
        }

        public override void EndFrame(double time)
        {
            if (CurrentSceene != null && Index != Sceenes.Count)
            {
                if (CurrentSceene.Done(time))
                {
                    Index++;
                    if (Index < Sceenes.Count)
                    {
                        CurrentSceene = Sceenes[Index];
                        CurrentSceene.Load();
                    }
                }
            }
        }

        public override void Draw3D(double time)
        {
            if (CurrentSceene != null)
                CurrentSceene.Draw3D(time);
        }

        public override void DrawOverlay(double time)
        {
            if (CurrentSceene != null)
                CurrentSceene.DrawOverlay(time);
        }
    }

    public class Sceene
    {
        protected ViewAPI API;
        protected Module TheModule;

        public Sceene( ViewAPI api, Module m)
        {
            API = api;
            TheModule = m;
        }

        public virtual bool Done(double time)
        {
            return false;
        }

        public virtual void Load()
        {
        }

        public virtual void Draw3D(double time)
        {

        }

        public virtual void DrawOverlay(double time)
        {

        }
    }

    public class BZFlagDanceView : Sceene
    {
        Texture grass = null;
        Texture walls = null;

        Texture TankSkin = null;
        Texture TankShadow = null;

        ListableEvent GrassList;
        ListableEvent WallList;

        StaticModel TankMesh;

        TankModel Tank;

        Texture Clouds = null;
        MoutainRenderer Moutains;

        double LastUpdate = 0;

        double CloudShift = 0;
        double CloudSpeed = 0.125;

        public BZFlagDanceView(ViewAPI api, Module m) : base(api, m) { }

        public override void Load()
        {
            GL.ClearColor(Color.SkyBlue);

            grass = TextureSystem.system.GetTexture(API.GetFile("img/grass.png"));
            GrassList = new ListableEvent(new ListableEvent.GenerateEventHandler(GrassList_Generate));

            walls = TextureSystem.system.GetTexture(API.GetFile("img/wall.png"));
            WallList = new ListableEvent(new ListableEvent.GenerateEventHandler(WallList_Generate));
            
            TankSkin = TextureSystem.system.GetTexture(API.GetFile("model/blue.png"));
            TankMesh = OBJFile.Read(API.GetFile("model/tank.obj"));
            TankShadow = TextureSystem.system.GetTexture(API.GetFile("model/shadow.png"));

            Tank = new TankModel(TankMesh, TankSkin, TankShadow);

            Moutains = new MoutainRenderer(API.GetDirectory("img/moutain"));
            Clouds = TextureSystem.system.GetTexture(API.GetFile("img/clouds.png"));
            Clouds.mipmap = false;

            TheModule.ViewPosition = new Vector3(0, -1, 2);
            TheModule.Spin = 45;
            TheModule.Pullback = 10;
        }

        protected void DrawBackground(double time)
        {
            TheModule.SetCamera(time, false);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);

            GL.DepthMask(false);

            GL.Begin(BeginMode.Quads);

            GL.Color4(Color.MidnightBlue);
            GL.Normal3(0, 0, 1);
            GL.Vertex2(0, 0);
            GL.Vertex2(API.Width, 0);

            GL.Color4(Color.SkyBlue);
            GL.Vertex2(API.Width, API.Height);
            GL.Vertex2(0, API.Height);

            GL.End();
            GL.Color4(Color.White);

            GL.DepthMask(true);
        }

        void Animate(double time)
        {
            double delta = 0;

            if (LastUpdate != 0)
                delta = time - LastUpdate;

            CloudShift += CloudSpeed * delta;

            LastUpdate = time;
        }

        public override void Draw3D(double time)
        {
            Animate(time);
            DrawBackground(time);

            TheModule.SetCamera(time, true);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            Tank.Update(new Vector3(0f, 0f, 0f), (float)time * 20.0f);
            DrawFlatGround();

            Tank.Draw();
        }

        protected void DrawClouds()
        {
            if (Clouds == null)
                return;

            GL.MatrixMode(MatrixMode.Texture);
            GL.Translate(CloudShift, 0, 0);
            GL.MatrixMode(MatrixMode.Modelview);

            Clouds.Bind();
            GL.Disable(EnableCap.Lighting);

            double innerSize = 50.0;
            double outerSize = 500.0;
            double height = 60.0;

            double UVSize = 60.0;

            double soldAlpha = 0.5;

            GL.Begin(BeginMode.Quads);

            GL.Color4(1.0, 1.0, 1.0, soldAlpha);

            // core
            GL.TexCoord2(-innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(-innerSize, innerSize, height);

            GL.TexCoord2(innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(innerSize, innerSize, height);

            GL.TexCoord2(innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(innerSize, -innerSize, height);

            GL.TexCoord2(-innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(-innerSize, -innerSize, height);

            // X+
            GL.Color4(1.0, 1.0, 1.0, soldAlpha);
            GL.TexCoord2(innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(innerSize, innerSize, height);

            GL.Color4(1.0, 1.0, 1.0, 0.0);
            GL.TexCoord2(outerSize / UVSize, outerSize / UVSize);
            GL.Vertex3(outerSize, outerSize, height);

            GL.TexCoord2(outerSize / UVSize, -outerSize / UVSize);
            GL.Vertex3(outerSize, -outerSize, height);

            GL.Color4(1.0, 1.0, 1.0, soldAlpha);
            GL.TexCoord2(innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(innerSize, -innerSize, height);

            // X-
            GL.Color4(1.0, 1.0, 1.0, 0);
            GL.TexCoord2(-outerSize / UVSize, outerSize / UVSize);
            GL.Vertex3(-outerSize, outerSize, height);

            GL.Color4(1.0, 1.0, 1.0, soldAlpha);
            GL.TexCoord2(-innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(-innerSize, innerSize, height);

            GL.TexCoord2(-innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(-innerSize, -innerSize, height);

            GL.Color4(1.0, 1.0, 1.0, 0);
            GL.TexCoord2(-outerSize / UVSize, -outerSize / UVSize);
            GL.Vertex3(-outerSize, -outerSize, height);

            // Y+
            GL.Color4(1.0, 1.0, 1.0, 0);
            GL.TexCoord2(-outerSize / UVSize, outerSize / UVSize);
            GL.Vertex3(-outerSize, outerSize, height);

            GL.TexCoord2(outerSize / UVSize, outerSize / UVSize);
            GL.Vertex3(outerSize, outerSize, height);

            GL.Color4(1.0, 1.0, 1.0, soldAlpha);
            GL.TexCoord2(innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(innerSize, innerSize, height);

            GL.TexCoord2(-innerSize / UVSize, innerSize / UVSize);
            GL.Vertex3(-innerSize, innerSize, height);

            // Y-
            GL.Color4(1.0, 1.0, 1.0, soldAlpha);
            GL.TexCoord2(-innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(-innerSize, -innerSize, height);

            GL.TexCoord2(innerSize / UVSize, -innerSize / UVSize);
            GL.Vertex3(innerSize, -innerSize, height);

            GL.Color4(1.0, 1.0, 1.0, 0);
            GL.TexCoord2(outerSize / UVSize, -outerSize / UVSize);
            GL.Vertex3(outerSize, -outerSize, height);
           
            GL.TexCoord2(-outerSize / UVSize, -outerSize / UVSize);
            GL.Vertex3(-outerSize, -outerSize, height);

            GL.End();
            GL.Enable(EnableCap.Lighting);

            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);

            GL.Color4(1, 1, 1, 1.0);

        }

        protected void DrawFlatGround()
        {
            GL.Color4(Color.White);
            grass.Bind();
            GrassList.Call();
            walls.Bind();
            WallList.Call();
            Moutains.Draw();
            DrawClouds();
        }

        double worldSize = 100;
        double wallHeight = 2.5;
        double floorZ = -0.01;

        void GrassList_Generate(object sender, DisplayList list)
        {
            GL.Begin(BeginMode.Quads);
            GL.Normal3(0, 0, 1.0);

            double worldSize = 100;
            double texSize = 8;

            GL.TexCoord2(-worldSize / texSize, -worldSize / texSize);
            GL.Vertex3(-worldSize, -worldSize, floorZ);

            GL.TexCoord2(worldSize / texSize, -worldSize / texSize);
            GL.Vertex3(worldSize, -worldSize, floorZ);

            GL.TexCoord2(worldSize / texSize, worldSize / texSize);
            GL.Vertex3(worldSize, worldSize, floorZ);

            GL.TexCoord2(-worldSize / texSize, worldSize / texSize);
            GL.Vertex3(-worldSize, worldSize, floorZ);

            // big ground
            double bigFactor = 4;
            GL.TexCoord2(-worldSize * bigFactor / texSize, -worldSize * bigFactor / texSize);
            GL.Vertex3(-worldSize * bigFactor, -worldSize * bigFactor, floorZ * 2);

            GL.TexCoord2(worldSize * bigFactor / texSize, -worldSize * bigFactor / texSize);
            GL.Vertex3(worldSize * bigFactor, -worldSize * bigFactor, floorZ * 2);

            GL.TexCoord2(worldSize * bigFactor / texSize, worldSize * bigFactor / texSize);
            GL.Vertex3(worldSize * bigFactor, worldSize * bigFactor, floorZ * 2);

            GL.TexCoord2(-worldSize * bigFactor / texSize, worldSize * bigFactor / texSize);
            GL.Vertex3(-worldSize * bigFactor, worldSize * bigFactor, floorZ*2);

            GL.End();
        }

        void WallList_Generate(object sender, DisplayList list)
        {
            double texSize = 2.5;
            GL.Begin(BeginMode.Quads);
                
                // X+
                GL.Normal3(-1, 0, 0);

                GL.TexCoord2(worldSize / texSize, 0);
                GL.Vertex3(worldSize, worldSize, floorZ);

                GL.TexCoord2(-worldSize / texSize, 0);
                GL.Vertex3(worldSize, -worldSize, floorZ);

                GL.TexCoord2(-worldSize / texSize, wallHeight/texSize);
                GL.Vertex3(worldSize, -worldSize, texSize);

                GL.TexCoord2(worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(worldSize, worldSize, texSize);

                //X-
                GL.Normal3(1, 0, 0);

                GL.TexCoord2(worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(-worldSize, worldSize, texSize);

                GL.TexCoord2(-worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(-worldSize, -worldSize, texSize);

                GL.TexCoord2(-worldSize / texSize, 0);
                GL.Vertex3(-worldSize, -worldSize, floorZ);
                
                GL.TexCoord2(worldSize / texSize, 0);
                GL.Vertex3(-worldSize, worldSize, floorZ);

                // Y+
                GL.Normal3(-1, 0, 0);

                GL.TexCoord2(worldSize / texSize, 0);
                GL.Vertex3(worldSize, worldSize, floorZ);

                GL.TexCoord2(worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(worldSize, worldSize, texSize);

                GL.TexCoord2(-worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(-worldSize, worldSize, texSize);

                GL.TexCoord2(-worldSize / texSize, 0);
                GL.Vertex3(-worldSize, worldSize, floorZ);

                // Y-
                GL.Normal3(1, 0, 0);

                GL.TexCoord2(-worldSize / texSize, 0);
                GL.Vertex3(-worldSize, -worldSize, floorZ);

                GL.TexCoord2(-worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(-worldSize, -worldSize, texSize);

                GL.TexCoord2(worldSize / texSize, wallHeight / texSize);
                GL.Vertex3(worldSize, -worldSize, texSize);

                GL.TexCoord2(worldSize / texSize, 0);
                GL.Vertex3(worldSize, -worldSize, floorZ);

            GL.End();
        }

        public class MoutainRenderer
        {
            public List<Texture> Tex = new List<Texture>();
            public ListableEvent geometry;

            public double RotIncrement = 0;
            public double Radius = 800;
            public double Height = 450;
            public double Width = 0;

            public MoutainRenderer(string path)
            {
                int i = 1;

                while(true)
                {
                    string name = Path.Combine(path, "mountain" + i.ToString() + ".png");
                    if (File.Exists(name))
                    {
                        Texture tex = TextureSystem.system.GetTexture(name);
                        tex.Clamp = true;
                        Tex.Add(tex);
                        i++;
                    }
                    else
                        break;
                }

                RotIncrement = 360.0 / (Tex.Count);
            }

            void list_Generate( int increment )
            {
                double fudge = 1.0;

                double degRad = Math.PI / 180.0;
                double thisAng = (RotIncrement * increment)*degRad;
                double halfRot = (RotIncrement*0.5)*degRad;


                double X1 = Math.Cos(thisAng+halfRot) * Radius * fudge;
                double Y1 = Math.Sin(thisAng+halfRot) * Radius * fudge;

                double X2 = Math.Cos(thisAng-halfRot) * Radius * fudge;
                double Y2 = Math.Sin(thisAng-halfRot) * Radius * fudge;

                GL.Begin(BeginMode.Quads);

                    GL.Normal3(-1, 0, 0);

                    GL.TexCoord2(0, 0);
                    GL.Vertex3(X2, Y2, Height);

                    GL.TexCoord2(1, 0);
                    GL.Vertex3(X1, Y1, Height);

                    GL.TexCoord2(1, 1);
                    GL.Vertex3(X1, Y1, 0);

                    GL.TexCoord2(0, 1);
                    GL.Vertex3(X2, Y2, 0);

                GL.End();
            }

            public void Draw ()
            {
                GL.Disable(EnableCap.Lighting);
                GL.PushMatrix();
                GL.Color4(Color.White);

                GL.Translate(0, 0, -100.0);
                for (int i = 0; i < Tex.Count; i++)
                {
                    Tex[i].Bind();
                    list_Generate(i);
                }
                GL.Enable(EnableCap.Lighting);
                GL.PopMatrix();
            }
        }

        public class TankModel
        {
            public StaticModel mesh;
            Texture skin, shadow;

            float rotation = 0;
            Vector3 position;

            public TankModel( StaticModel m, Texture s, Texture sh )
            {
                mesh = m;
                skin = s;
                shadow = sh;
            }

            public void Update ( Vector3 pos, float rot )
            {
                rotation = rot;
                position = pos;
            }

            public void Draw ()
            {
                GL.PushMatrix();
                    GL.Translate(position.X, position.Y, position.Z + 0.01f);
                    GL.Rotate(90, 1, 0, 0);
                    GL.Rotate(rotation, 0, 1, 0);
                    skin.Bind();
                    mesh.drawAll(false);
                GL.PopMatrix();

                GL.PushMatrix();

                    GL.Translate(position.X+0.5, position.Y-0.35, 0);
                    GL.Rotate(rotation, 0, 0, 1);

                    float shadowScale = 1.0f + (position.Z/40.0f);
                    if (shadowScale > 3.0f)
                        shadowScale = 3.0f;

                    Vector2 shadowSize = new Vector2(4.8f * shadowScale, 2.8f * shadowScale);

                    shadow.Bind();

                    GL.Disable(EnableCap.Lighting);

                        GL.Begin(BeginMode.Quads);
                            GL.Color4(1.0, 1.0, 1.0, 0.75);
                            GL.Normal3(0, 0, 1.0);

                            GL.TexCoord2(0, 1.0);
                            GL.Vertex2(-shadowSize.X, -shadowSize.Y);

                            GL.TexCoord2(1.0, 1.0);
                            GL.Vertex2(shadowSize.X, -shadowSize.Y);

                            GL.TexCoord2(1.0, 0);
                            GL.Vertex2(shadowSize.X, shadowSize.Y);

                            GL.TexCoord2(0, 0);
                            GL.Vertex2(-shadowSize.X, shadowSize.Y);

                        GL.End();
                    GL.Enable(EnableCap.Lighting);

                GL.PopMatrix();
                GL.Color4(1.0, 1.0, 1.0, 1.0);
            }
        }
    }
}
