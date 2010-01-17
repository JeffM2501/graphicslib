﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace MD3
{
    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }

    public class AnimationSequence
    {
        public string Name = string.Empty;
        public int StartFrame = -1;
        public int EndFrame = -1;
        public int LoopPoint = -1;
        public float FPS = 30f;
    }

    public class Skin
    {
        public string Name = string.Empty;
        public Dictionary<string, string> Surfaces = new Dictionary<string, string>();

        public Skin ( string n )
        {
            Name = n;
        }
    }

    internal class ConnectedComponent
    {
        internal Component Part;
        internal Dictionary<Tag, List<ConnectedComponent>> Children;
    }

    public class Character
    {
        public Component[] Componenets = null;

        public String Name = string.Empty;

        public Gender Gender = Gender.Unknown;
        public Vector3 HeadOffset = Vector3.Zero;
        public string Footsetps = string.Empty;

        public AnimationSequence[] Sequences;

        public List<Skin> Skins = new List<Skin>();

        protected Dictionary<string, AnimationSequence> SequenceCache;

        internal ConnectedComponent RootNode;

        public Skin GetSkin ( string name )
        {
            foreach (Skin skin in Skins)
            {
                if (skin.Name == name)
                    return skin;
            }

            Skin s = new Skin(name);
            Skins.Add(s);
            return s;
        }

        public bool SkinExists ( string name )
        {
            foreach (Skin skin in Skins)
            {
                if (skin.Name == name)
                    return true;
            }

            return false;
        }

        public Skin SkinFromSurfs ()
        {
            if (SkinExists("from_surfs"))
                return GetSkin("from_surfs");

            Skin skin = new Skin("from_surfs");

            foreach (Component component in Componenets)
            {
                foreach (Mesh mesh in component.Meshes)
                {
                    if (mesh.ShaderFiles.Length > 0)
                        skin.Surfaces.Add(mesh.Name, mesh.ShaderFiles[0]);
                    else
                        skin.Surfaces.Add(mesh.Name, mesh.Name + ".tga");
                }
            }

            Skins.Add(skin);
            return skin;
        }

        public AnimationSequence FindSequence ( string name )
        {
            if (SequenceCache == null)
            {
                SequenceCache = new Dictionary<string, AnimationSequence>();
                foreach (AnimationSequence seq in Sequences)
                {
                    if (!SequenceCache.ContainsKey(seq.Name))
                        SequenceCache.Add(seq.Name, seq);
                }
            }
            if (!SequenceCache.ContainsKey(name))
                return null;

            return SequenceCache[name];
        }
    }
}