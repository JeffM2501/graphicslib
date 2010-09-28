using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using GraphTest;
using Drawables.Textures;
using Drawables.DisplayLists;


namespace P51Movie
{
    public class BZGround
    {
        Texture grass = null;
        Texture walls = null;

        ListableEvent GrassList;
        ListableEvent WallList;

        Clouds Cloud = null;
        MoutainRenderer Moutains;

        BuildingRenderer buildingRenderer;

        double LastUpdate = 0;

        double worldSize = 100;
        double wallHeight = 5;
        double floorZ = -0.01;

        public class BuildingDef
        {
            public Vector3 Position = Vector3.Zero;
            public Vector3 Size = new Vector3(5, 5, 5);
            public double Rotation = 30;
        }

        public List<BuildingDef> Buildings = new List<BuildingDef>();

        public void Load()
        {
            grass = TextureSystem.system.GetTexture(Sceene.API.GetFile("img/grass.png"));
            GrassList = new ListableEvent(new ListableEvent.GenerateEventHandler(GrassList_Generate));

            walls = TextureSystem.system.GetTexture(Sceene.API.GetFile("img/wall.png"));
            WallList = new ListableEvent(new ListableEvent.GenerateEventHandler(WallList_Generate));
            
            Moutains = new MoutainRenderer(Sceene.API.GetDirectory("img/moutain"));
            Cloud = new Clouds(TextureSystem.system.GetTexture(Sceene.API.GetFile("img/clouds.png")));

            buildingRenderer = new BuildingRenderer(TextureSystem.system.GetTexture(Sceene.API.GetFile("img/boxwall.png")), TextureSystem.system.GetTexture(Sceene.API.GetFile("img/roof.png")));
        }

        protected void DrawBackground(double time)
        {
            Sceene.TheModule.SetCamera(time, false);
            Background.Draw(time,Sceene.API);
        }

        public void Animate(double time)
        {
            double delta = 0;

            if (LastUpdate != 0)
                delta = time - LastUpdate;

            Cloud.Animate(time);

            LastUpdate = time;
        }
        
        public void Draw(double time)
        {
            DrawBackground(time);

            Sceene.TheModule.SetCamera(time, true);
            GL.Enable(EnableCap.Texture2D);

            GL.Color4(Color.White);

            GL.Disable(EnableCap.Lighting);
            GL.DepthMask(false);
            Moutains.Draw();
            Cloud.Draw();
            GL.DepthMask(true);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            grass.Bind();
            GrassList.Call();

            DrawWalls();

            foreach (BuildingDef def in Buildings)
                buildingRenderer.Draw(def.Position, def.Size, def.Rotation);
        }

        protected void DrawWalls ()
        {
            walls.Bind();
            WallList.Call();
        }

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
