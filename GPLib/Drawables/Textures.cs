/*
    Open Combat/Projekt 2501
    Copyright (C) 2010  Jeffery Allen Myers

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Drawables.DisplayLists;
using Drawables.TGA;

namespace Drawables.Textures
{
    public class Texture : IDisposable
    {
        DisplayList listID = new DisplayList();
        int boundID = -1;

        public int Width
        {
            get 
            {
                CheckSize();
                return imageSize.Width;
            }
        }

        public int Height
        {
            get
            {
                CheckSize();
                return imageSize.Height;
            }
        }

        public bool mipmap = true;

        FileInfo file = null;
        public Image image = null;

        public bool Clamp = false;

        Size imageSize = Size.Empty;

        public Texture( FileInfo info )
        {
            file = info;
        }

        public Texture(Image img)
        {
            image = img;
        }

        public Texture(Image img, bool clamp)
        {
            image = img;
            Clamp = clamp;
        }

        public bool Valid ()
        {
            return listID.Valid();
        }

        public void Invalidate()
        {
            if (listID.Valid())
                listID.Invalidate();

            if(boundID != -1)
                GL.DeleteTexture(boundID);
            boundID = -1;
        }

        public void Dispose ()
        {
            Invalidate();
        }

        void CheckSize ()
        {
            if (imageSize == Size.Empty)
            {
                if (image != null)
                    imageSize = new Size(image.Width, image.Height);
                else
                {
                    Image pic = Image.FromFile(file.FullName);
                    if (pic != null)
                        imageSize = new Size(pic.Width, pic.Height);
                }
            }
        }

        public void Load ()
        {
            GL.Enable(EnableCap.Texture2D);
            Bitmap bitmap;
            if (file != null && file.Exists)
            {
                if (file.Extension.ToLower() == ".tga")
                    bitmap = (Bitmap)TGAFile.Read(file);
                else
                    bitmap = new Bitmap(file.FullName);
            }
            else
                bitmap = new Bitmap(image);

            GL.GenTextures(1, out boundID);
            GL.BindTexture(TextureTarget.Texture2D, boundID);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            if (mipmap)
            {
                if (TextureSystem.UseAniso)
                    GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FF, 2f);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            if (mipmap)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            if (Clamp)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }

            GL.BindTexture(TextureTarget.Texture2D, boundID);
        }

        public void Bind ()
        {
            if (boundID == -1)
            {
                Invalidate();
                Load();
            }

            if (boundID != -1)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, boundID);
            }
            else
                GL.Disable(EnableCap.Texture2D);
        }

        public void Execute()
        {
            // easy out, do this most of the time
            if (listID.Valid())
            {
                if (boundID != -1)
                    GL.Enable(EnableCap.Texture2D);
                else
                    GL.Disable(EnableCap.Texture2D);
                listID.Call();
                return;
            }

            if (boundID == -1 || !listID.Valid())
                Invalidate(); // we know one is bad, so make sure all is free;

            Load();

            listID.Start(true);
            if (boundID != -1)
                GL.BindTexture(TextureTarget.Texture2D, boundID);
            listID.End();
        }

        public void Draw ( float scale )
        {
            Bind();
            GL.Begin(BeginMode.Quads);

            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex2(Width * scale, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex2(Width * scale, Height * scale);

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, Height * scale);

            GL.End();
        }


        public void Draw(float width, float height)
        {
            Bind();
            GL.Begin(BeginMode.Quads);

            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex2(width, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex2(width, height);

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, height);

            GL.End();
        }

        public void DrawCentered(float width, float height)
        {
            Bind();
            GL.Begin(BeginMode.Quads);

            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-width * 0.5, -height * 0.5);

            GL.TexCoord2(1, 1);
            GL.Vertex2(width * 0.5, -height * 0.5);

            GL.TexCoord2(1, 0);
            GL.Vertex2(width * 0.5, height * 0.5);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-width * 0.5, height * 0.5);

            GL.End();
        }

        public void DrawCenteredXZ(float width, float height)
        {
            Bind();
            GL.Begin(BeginMode.Quads);

            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, 1);
            GL.Vertex3(-width * 0.5, 0, -height * 0.5);

            GL.TexCoord2(1, 1);
            GL.Vertex3(width * 0.5, 0, -height * 0.5);

            GL.TexCoord2(1, 0);
            GL.Vertex3(width * 0.5, 0, height * 0.5);

            GL.TexCoord2(0, 0);
            GL.Vertex3(-width * 0.5, 0, height * 0.5);

            GL.End();
        }

        public void Draw(float width, float height, float ppu)
        {
            Bind();
            GL.Begin(BeginMode.Quads);

            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, (height*ppu)/Height);
            GL.Vertex2(0, 0);

            GL.TexCoord2((width * ppu) / Width, (height * ppu) / Height);
            GL.Vertex2(width, 0);

            GL.TexCoord2((width * ppu) / Width, 0);
            GL.Vertex2(width, height);

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, height);

            GL.End();
        }


        public void DrawAtWidth ( float newWidth )
        {
            Draw(newWidth / Width);
        }

        public void DrawAtHeight(float newHeight)
        {
            Draw(newHeight / Height);
        }
    }

    public class TextureSystem
    {
        public static TextureSystem system = new TextureSystem();
        public static bool UseAniso = false;

        public delegate FileInfo LocateFileHandler(string file);

        public LocateFileHandler LocateFile = null;     

        Dictionary<string, Texture> textures = new Dictionary<string,Texture>();

        public void Invalidate()
        {
            foreach(KeyValuePair<string,Texture> t in textures)
                t.Value.Invalidate();
        }

        public void FlushAllImageTextures ()
        {
            List<string> texturesToKill = new List<string>();
            foreach(KeyValuePair<string,Texture> texture in textures)
            {
                if (texture.Value.image != null)
                {
                    texturesToKill.Add(texture.Key);
                    texture.Value.Invalidate();
                }
            }

            foreach (String name in texturesToKill)
                textures.Remove(name);
        }

        public Texture FromImage ( Image image )
        {
            return FromImage(image, false);
        }

        public Texture FromImage ( Image image, bool clamp )
        {
            if (image == null)
                return null;

            String name = "Image:" + image.GetHashCode().ToString() + clamp.ToString();

            if (textures.ContainsKey(name))
                return textures[name];

            Texture texture = new Texture(image, clamp);
            textures.Add(name, texture);
            return texture;
        }

        public Texture GetTexture(string path)
        {
            if (textures.ContainsKey(path))
                return textures[path];

            Texture texture = null;
            if (textureIsValid(path))
            {
                FileInfo file;
                if (LocateFile != null)
                    file = LocateFile(path);
                else
                    file = new FileInfo(path);

                if (file.Exists)
                    texture = new Texture(file);
            }
            if (texture == null)
                return null;

            textures.Add(path, texture);

            return texture;
        }

        bool textureIsValid(string t)
        {
            if (t == string.Empty)
                return false;
            string extension = Path.GetExtension(t);
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".tiff" && extension != ".tga")
                return false;

            return true;
        }
    }
}
