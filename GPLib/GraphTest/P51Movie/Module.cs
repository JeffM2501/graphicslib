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

        Clouds Cloud = null;
        MoutainRenderer Moutains;

        BuildingRenderer buildingRenderer;

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
            Cloud = new Clouds(TextureSystem.system.GetTexture(API.GetFile("img/clouds.png")));

            buildingRenderer = new BuildingRenderer(TextureSystem.system.GetTexture(API.GetFile("img/boxwall.png")), TextureSystem.system.GetTexture(API.GetFile("img/roof.png")));

            TheModule.ViewPosition = new Vector3(0, -1, 2);
            TheModule.Spin = 45;
            TheModule.Pullback = 10;
        }

        protected void DrawBackground(double time)
        {
            TheModule.SetCamera(time, false);
            Background.Draw(time,API);
        }

        void Animate(double time)
        {
            double delta = 0;

            if (LastUpdate != 0)
                delta = time - LastUpdate;

            Cloud.Animate(time);

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

            GL.Enable(EnableCap.Lighting);
            buildingRenderer.Draw(new Vector3(10, 10, 0), new Vector3(5, 5, 5), 0);

            Tank.Draw();
        }

        protected void DrawFlatGround()
        {
            GL.Color4(Color.White);
            grass.Bind();
            GrassList.Call();
            walls.Bind();
            WallList.Call();
            Moutains.Draw();
            Cloud.Draw();
        }

        double worldSize = 100;
        double wallHeight = 5;
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
            double texSize =5;
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
    }
}
