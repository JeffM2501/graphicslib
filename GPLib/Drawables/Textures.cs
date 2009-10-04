﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Drawables.DisplayLists;

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

        Size imageSize = Size.Empty;

        public Texture( FileInfo info )
        {
            file = info;
        }

        public bool Valid ()
        {
            return listID.Valid();
        }

        public void Invalidate()
        {
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
                Image image = Image.FromFile(file.FullName);
                if (image != null)
                    imageSize = new Size(image.Width, image.Height);
            }
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

            if (file != null && file.Exists)
            {
                Bitmap bitmap = new Bitmap(file.FullName);

                GL.GenTextures(1, out boundID);
                GL.BindTexture(TextureTarget.Texture2D, boundID);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                if (mipmap)
                {
                    if (TextureSystem.UseAniso)
                        GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FF, 2f);
                    
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                bitmap.UnlockBits(data);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            listID.Start(true);
            if (boundID != -1)
                GL.BindTexture(TextureTarget.Texture2D, boundID);
            listID.End();
        }
    }

    public class TextureSystem
    {
        public static TextureSystem system = new TextureSystem();
        public static bool UseAniso = true;

        public DirectoryInfo rootDir;

        Dictionary<string, Texture> textures = new Dictionary<string,Texture>();

        public void Invalidate()
        {
            foreach(KeyValuePair<string,Texture> t in textures)
                t.Value.Invalidate();
        }

        public Texture GetTexture(string path)
        {
            if (textures.ContainsKey(path))
                return textures[path];

            Texture texture = null;
            if (textureIsValid(path))
            {
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                    texture = new Texture(file);
                else if (rootDir != null)
                {
                    file = new FileInfo(Path.Combine(rootDir.FullName, path));
                    if (file.Exists)
                        texture = new Texture(file);
                }
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
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".tiff")
                return false;

            return true;
        }
    }
}