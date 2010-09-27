using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

            TheModule.ViewPosition = new Vector3(0, -1, 2);
            TheModule.Spin = 45;
            TheModule.Pullback = 10;
        }

        public override void Draw3D(double time)
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            float bobble = 10.0f;

            Tank.Update(new Vector3(0f, 0f, 1.0f + ((float)Math.Sin(time) * bobble)+bobble), (float)time * 20.0f);
            DrawFlatGround();

            Tank.Draw();
        }

        protected void DrawFlatGround()
        {
            GL.Color4(Color.White);
            grass.Bind();
            GrassList.Call();
            walls.Bind();
            WallList.Call();
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
