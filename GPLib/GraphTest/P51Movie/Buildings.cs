using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Drawables.Textures;

namespace P51Movie
{
    public class BuildingRenderer
    {
        protected Texture Brick, Roof;

        double BrickScale = 2;
        double RoofScale = 0.5;

        public BuildingRenderer(Texture b, Texture r)
        {
            Brick = b;
            Roof = r;
        }

        public void Draw(Vector3 pos, Vector3 size, Double rot)
        {
            GL.PushMatrix();

            GL.Translate(pos);
            GL.Rotate(rot, 0, 0, 1);

            Roof.Bind();

            GL.Begin(BeginMode.Quads);

                GL.Normal3(0, 0, 1);
                GL.TexCoord2(-size.X / RoofScale, -size.Y / RoofScale);
                GL.Vertex3(-size.X, -size.Y, size.Z);
                GL.TexCoord2(size.X / RoofScale, -size.Y / RoofScale);
                GL.Vertex3(size.X, -size.Y, size.Z);
                GL.TexCoord2(size.X / RoofScale, size.Y / RoofScale);
                GL.Vertex3(size.X, size.Y, size.Z);
                GL.TexCoord2(-size.X / RoofScale, size.Y / RoofScale);
                GL.Vertex3(-size.X, size.Y, size.Z);

                GL.Normal3(0, 0, -1);

                GL.TexCoord2(-size.X / RoofScale, size.Y / RoofScale);
                GL.Vertex3(-size.X, size.Y, 0);
                GL.TexCoord2(size.X / RoofScale, size.Y / RoofScale);
                GL.Vertex3(size.X, size.Y, 0);
                GL.TexCoord2(size.X / RoofScale, -size.Y / RoofScale);
                GL.Vertex3(size.X, -size.Y, 0);
                GL.TexCoord2(-size.X / RoofScale, -size.Y / RoofScale);
                GL.Vertex3(-size.X, -size.Y, 0);
           
            GL.End();

            Brick.Bind();

            GL.Begin(BeginMode.Quads);

                // X+
                GL.Normal3(1.0, 0, 0);

                GL.TexCoord2(0, 0);
                GL.Vertex3(size.X, -size.Y, 0);

                GL.TexCoord2( size.Y * 2 / BrickScale,0);
                GL.Vertex3(size.X, size.Y, 0);

                GL.TexCoord2(size.Y * 2 /BrickScale, size.Z / BrickScale);
                GL.Vertex3(size.X, size.Y, size.Z);

                GL.TexCoord2(0,size.Z / BrickScale);
                GL.Vertex3(size.X, -size.Y, size.Z);

                // X-
                GL.Normal3(-1, 0.0, 0);

                GL.TexCoord2(0, 0);
                GL.Vertex3(-size.X, size.Y, 0);

                GL.TexCoord2(size.Y * 2 / BrickScale, 0);
                GL.Vertex3(-size.X, -size.Y, 0);

                GL.TexCoord2(size.Y * 2 / BrickScale, size.Z / BrickScale);
                GL.Vertex3(-size.X, -size.Y, size.Z);

                GL.TexCoord2(0, size.Z / BrickScale);
                GL.Vertex3(-size.X, size.Y, size.Z);

                // Y-
                GL.Normal3(0, -1.0, 0);
                GL.TexCoord2(0, 0);
                GL.Vertex3(-size.X, -size.Y, 0);

                GL.TexCoord2(size.X * 2 / BrickScale, 0);
                GL.Vertex3(size.X, -size.Y, 0);

                GL.TexCoord2(size.X * 2 / BrickScale, size.Z / BrickScale);
                GL.Vertex3(size.X, -size.Y, size.Z);

                GL.TexCoord2(0, size.Z / BrickScale);
                GL.Vertex3(-size.X, -size.Y, size.Z);

                // Y+
                GL.Normal3(0, 1.0, 0);
                GL.TexCoord2(0, 0);
                GL.Vertex3(size.X, size.Y, 0);

                GL.TexCoord2(size.X * 2 / BrickScale, 0);
                GL.Vertex3(-size.X, size.Y, 0);

                GL.TexCoord2(size.X * 2 / BrickScale, size.Z / BrickScale);
                GL.Vertex3(-size.X, size.Y, size.Z);

                GL.TexCoord2(0, size.Z / BrickScale);
                GL.Vertex3(size.X, size.Y, size.Z);

            GL.End();

            GL.PopMatrix();
        }
    }
}
