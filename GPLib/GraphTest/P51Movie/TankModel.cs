using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Drawables.Textures;
using Drawables.StaticModels;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace P51Movie
{
    public class TankModel
    {
        public StaticModel mesh;
        Texture skin, shadow;

        float rotation = 0;
        Vector3 position;

        public TankModel(StaticModel m, Texture s, Texture sh)
        {
            mesh = m;
            skin = s;
            shadow = sh;
        }

        public void Update(Vector3 pos, float rot)
        {
            rotation = rot;
            position = pos;
        }

        public void Draw()
        {
            GL.PushMatrix();
            GL.Translate(position.X, position.Y, position.Z + 0.01f);
            GL.Rotate(90, 1, 0, 0);
            GL.Rotate(rotation, 0, 1, 0);
            skin.Bind();
            mesh.drawAll(false);
            GL.PopMatrix();

            GL.PushMatrix();

            GL.Translate(position.X + 0.5, position.Y - 0.35, 0);
            GL.Rotate(rotation, 0, 0, 1);

            float shadowScale = 1.0f + (position.Z / 40.0f);
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
