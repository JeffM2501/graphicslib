using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Drawables.Textures;
using Drawables.DisplayLists;

using GraphTest;

namespace P51Movie
{
    public class Clouds
    {
        Texture Cloud = null;
        double LastUpdate = 0;

        double CloudShift = 0;
        double CloudSpeed = 0.125;

        public Clouds ( Texture img )
        {
            Cloud = img;
        }

        public void Animate(double time)
        {
            double delta = 0;

            if (LastUpdate != 0)
                delta = time - LastUpdate;

            CloudShift += CloudSpeed * delta;

            LastUpdate = time;
        }

        public void Draw()
        {
            if (Cloud == null)
                return;

            GL.MatrixMode(MatrixMode.Texture);
            GL.Translate(CloudShift, 0, 0);
            GL.MatrixMode(MatrixMode.Modelview);

            Cloud.Bind();
            GL.Disable(EnableCap.Lighting);

            double innerSize = 50.0;
            double outerSize = 200.0;
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
    }

    public class Background
    {
        public static void Draw(double time, ViewAPI API)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);

            GL.DepthMask(false);

            GL.Begin(BeginMode.Quads);

            GL.Color4(Color.SkyBlue);
            GL.Normal3(0, 0, 1);
            GL.Vertex2(0, 0);
            GL.Vertex2(API.Width, 0);

            GL.Color4(Color.DeepSkyBlue);
            GL.Vertex2(API.Width, API.Height);
            GL.Vertex2(0, API.Height);

            GL.End();
            GL.Color4(Color.White);

            GL.DepthMask(true);
        }
    }

    public class MoutainRenderer
    {
        public List<Texture> Tex = new List<Texture>();
        public ListableEvent geometry;

        public double RotIncrement = 0;
        public double Radius = 900;
        public double Height = 650;
        public double Width = 0;

        public MoutainRenderer(string path)
        {
            int i = 1;

            while (true)
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

        void list_Generate(int increment)
        {
            double fudge = 1.0;

            double degRad = Math.PI / 180.0;
            double thisAng = (RotIncrement * increment) * degRad;
            double halfRot = (RotIncrement * 0.5) * degRad;

            // Height = Math.Sin(halfRot + halfRot) * Radius * fudge;

            double X1 = Math.Cos(thisAng + halfRot) * Radius * fudge;
            double Y1 = Math.Sin(thisAng + halfRot) * Radius * fudge;

            double X2 = Math.Cos(thisAng - halfRot) * Radius * fudge;
            double Y2 = Math.Sin(thisAng - halfRot) * Radius * fudge;

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

        public void Draw()
        {
            GL.Disable(EnableCap.Lighting);
            GL.PushMatrix();
            GL.Color4(Color.White);

            GL.Translate(0, 0, -200.0);
            for (int i = 0; i < Tex.Count; i++)
            {
                Tex[i].Bind();
                list_Generate(i);
            }
            GL.Enable(EnableCap.Lighting);
            GL.PopMatrix();
        }
    }
}
