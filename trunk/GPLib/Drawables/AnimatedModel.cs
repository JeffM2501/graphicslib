﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Math3D;

using System.Drawing;

using MilkhapeModel;

namespace Drawables.AnimateModels
{
    public class AnimationEvent
    {
        public double time = 0;
        public Vector3 value = Vector3.Zero;

        public AnimationEvent(double t, Vector3 v)
        {
            time = t;
            value = v;
        }
    }

    public class Bone
    {
        public Matrix4 matrix = Matrix4.Identity;

        public Vector3 translation = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;

        public List<AnimationEvent> FrameTranslations = new List<AnimationEvent>();
        public List<AnimationEvent> FrameRotations = new List<AnimationEvent>();

        public Matrix4 CachedWorldMatrix
        {
            get
            {
                if (worldMatrixCache == null)
                    CacheWorldMatrix();
                return worldMatrixCache;
            }
        }
        Matrix4 worldMatrixCache;

        public Matrix4 CachedWorldMatrixInv
        {
            get
            {
                if (worldMatrixCacheInv == null)
                    CacheWorldMatrix();
                return worldMatrixCacheInv;
            }
        }
        Matrix4 worldMatrixCacheInv;

        Bone Parent = null;
        public List<Bone> Children = new List<Bone>();

        public List<Vector3> Verts = new List<Vector3>();

        public Bone() { }

        public Bone(Matrix4 m)
            : this()
        {
            matrix = m;
        }

        public bool Oprhan()
        {
            return Parent == null;
        }

        public Bone Add(Bone child)
        {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public int Add(Vector3 vert)
        {
            for (int i = 0; i < Verts.Count; i++)
            {
                if (vert == Verts[i])
                    return i;
            }
            Verts.Add(vert);
            return Verts.Count - 1;
        }

        public Matrix4 WorldMatrix()
        {
            if (Parent == null)
                return matrix;

            return matrix * Parent.WorldMatrix();
        }

        public Matrix4 WorldMatrixInv()
        {
            Matrix4 mat = WorldMatrix();
            mat.Invert();
            return mat;
        }

        public void CacheWorldMatrix()
        {
            worldMatrixCache = WorldMatrix();
            worldMatrixCacheInv = WorldMatrixInv();

            foreach (Bone child in Children)
                child.CacheWorldMatrix();
        }

        public Vector3 GetVert(int index)
        {
            return Verts[index];
        }

        public Vector3 GetVertInvMatrix(int index)
        {
            return Vector3.Transform(Verts[index], worldMatrixCacheInv);
        }

        public Vector3 GetNormal(Vector3 normal)
        {
            return normal;
        }

        public Vector3 GetNormalInvMatrix(Vector3 normal)
        {
            return Vector3.TransformNormal(normal, worldMatrixCacheInv);
        }
    }

    public class AnimatedBoneMatrix
    {
        public Matrix4 CumulativeMatrix = Matrix4.Identity;
        public Matrix4 LocalMatrix = Matrix4.Identity;

        public int lastTransIndex = -1;
        public int lastRotIndex = -1;
    }

    public class AnimationHandler
    {
        public Dictionary<Bone, AnimatedBoneMatrix> BoneTransforms = new Dictionary<Bone, AnimatedBoneMatrix>();
        public AnimatedModel model;

        public AnimationHandler(AnimatedModel m)
        {
            model = m;

            // walk the tree and push the bones.
            PushBone(m.Root);
        }

        protected void PushBone(Bone bone)
        {
            BoneTransforms.Add(bone, new AnimatedBoneMatrix());
            foreach (Bone b in bone.Children)
                PushBone(b);
        }

        public AnimatedBoneMatrix GetMatrices(Bone bone)
        {
            if (BoneTransforms.ContainsKey(bone))
                return BoneTransforms[bone];

            return new AnimatedBoneMatrix();
        }

        public void SetTime(double time)
        {
            // walk the bone tree from root up
            // and compute the various matrices
            SetBoneMatrix(model.Root, time, Matrix4.Identity);
        }

        int FindIndexForTime(List<AnimationEvent> events, double time, int start)
        {
            if (events.Count < 1)
                return -1;

            if (time < events[0].time)
                return 0;

            int frame = start;
            if (frame >= 0 && frame < events.Count)
            {
                if (events[frame].time <= time)
                {
                    int nextFrame = frame + 1;
                    if (nextFrame < events.Count)
                    {
                        if (events[nextFrame].time > time)
                            return frame; // this is the easy out
                    }

                    frame++;
                    nextFrame++;

                    while (true)
                    {
                        if (nextFrame < events.Count)
                        {
                            if (events[frame].time <= time && events[nextFrame].time > time)
                                return frame;

                            if (events[nextFrame].time == time)
                                return nextFrame; // just in case this happens

                            // nope so move on
                            frame++;
                            nextFrame++;
                        }
                        else
                        {
                            // shit we went over so shift the time back to the start
                            time -= events[frame].time;

                            //handle the non 0 start case
                            if (time < events[0].time)
                                return 0;

                            frame = 0;
                            nextFrame = 1;
                        }
                    }
                }
            }
            return 0;
        }

        Vector3 InterpEvents(ref int lastIndex, List<AnimationEvent> events, Bone bone, double time, bool angles)
        {
            int lastFrame = FindIndexForTime(events, time, lastIndex);
            lastIndex = lastFrame;

            if (lastFrame < 0) // if there are no events, screw it we use 0 for the angles;
                return new Vector3(0, 0, 0);

            int nextFrame = lastFrame + 1;
            if (nextFrame >= events.Count)
                return events[lastFrame].value;

            double timeDelta = events[nextFrame].time - events[lastFrame].time;
            if (timeDelta == 0)
                return events[lastFrame].value;

            double interpTime = time - events[lastFrame].time;
            if (interpTime < 0)
                interpTime = 0;

            double param = interpTime / timeDelta;

            if (angles)
            {
                // TODO make them into quats, slurp them, then give em back
            }

            return events[lastFrame].value + (VectorHelper3.Subtract(events[nextFrame].value, events[lastFrame].value) * (float)param);
        }

        protected void SetBoneMatrix(Bone bone, double time, Matrix4 parrentMatrix)
        {
            AnimatedBoneMatrix instance = GetMatrices(bone);

            instance.LocalMatrix = bone.matrix;
            if (time > 0)
            {
                // check the rots
                Matrix4 rotMat = Matrix4.Identity;
                Vector3 angles = InterpEvents(ref instance.lastRotIndex, bone.FrameRotations, bone, time, true);
                rotMat = Matrix4.CreateRotationX(angles.X) * Matrix4.CreateRotationY(angles.Y) * Matrix4.CreateRotationZ(angles.Z);

                Matrix4 transMat = Matrix4.Identity;
                transMat = Matrix4.CreateTranslation(InterpEvents(ref instance.lastTransIndex, bone.FrameTranslations, bone, time, false));

                instance.LocalMatrix = transMat * rotMat * instance.LocalMatrix;// *rotMat * transMat;
            }

            instance.CumulativeMatrix = instance.LocalMatrix * parrentMatrix;

            foreach (Bone child in bone.Children)
                SetBoneMatrix(child, time, instance.CumulativeMatrix);
        }
    }

    public class Polygon
    {
        public List<int> Verts = new List<int>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector2> UVs = new List<Vector2>();

        public void Add(int v, Vector3 n, Vector2 t)
        {
            Verts.Add(v);
            Normals.Add(n);
            UVs.Add(t);
        }
    }

    public class BoneMesh
    {
        public Color Material = Color.White;

        public List<KeyValuePair<Bone, int>> Verts = new List<KeyValuePair<Bone, int>>();
        public List<Polygon> Faces = new List<Polygon>();
        public bool Show = true;

        public int AddVert(Bone bone, int index)
        {
            Verts.Add(new KeyValuePair<Bone, int>(bone, index));
            return Verts.Count - 1;
        }

        public void Draw(AnimationHandler anim)
        {
            if (!Show)
                return;

            GL.Color3(Material);
            foreach (Polygon face in Faces)
            {
                GL.Begin(BeginMode.Polygon);
                for (int i = 0; i < face.Verts.Count; i++)
                {
                    Bone bone = Verts[face.Verts[i]].Key;
                    int boneVert = Verts[face.Verts[i]].Value;

                    if (anim == null)
                    {
                        GL.Normal3(bone.GetNormal(face.Normals[i]));
                        GL.Vertex3(bone.GetVert(boneVert));
                    }
                    else
                    {
                        AnimatedBoneMatrix matrix = anim.GetMatrices(bone);
                        GL.Normal3(Vector3.TransformNormal(bone.GetNormal(face.Normals[i]), matrix.CumulativeMatrix));
                        GL.Vertex3(Vector3.Transform(bone.GetVert(boneVert), matrix.CumulativeMatrix));
                    }
                }
                GL.End();
            }
        }
    }

    public class AnimatedModel
    {
        public Bone Root = new Bone();
        public List<BoneMesh> Meshes = new List<BoneMesh>();
       
        public static Color BoneColor = Color.Blue;
        public static Color JointColor = Color.CornflowerBlue;

        public void Draw()
        {
            Draw(null);
        }

        public void Draw(AnimationHandler anim)
        {
            Root.CacheWorldMatrix();
            foreach (BoneMesh mesh in Meshes)
                mesh.Draw(anim);
        }

        void DrawBone(Bone bone, AnimationHandler anim)
        {
            GL.PushMatrix();
            Matrix4 mat = bone.matrix;
            if (anim != null)
                mat = anim.GetMatrices(bone).LocalMatrix;

            GL.MultMatrix(ref mat);
            GL.Color4(JointColor);
            GL.LineWidth(1);

            float markerSize = 0.03f;

            GL.Begin(BeginMode.Lines);
            GL.Vertex3(markerSize, 0, 0);
            GL.Vertex3(-markerSize, 0, 0);

            GL.Vertex3(0, markerSize, 0);
            GL.Vertex3(0, -markerSize, 0);

            GL.Vertex3(0, 0, markerSize);
            GL.Vertex3(0, 0, -markerSize);
            GL.End();

            foreach (Bone child in bone.Children)
            {
                Matrix4 childMat = child.matrix;
                if (anim != null)
                    childMat = anim.GetMatrices(child).LocalMatrix;
                GL.Begin(BeginMode.Lines);

                GL.Color3(BoneColor);
                GL.Vertex3(0, 0, 0);

                GL.Vertex3(Vector3.Transform(new Vector3(0, 0, 0), childMat));
                GL.End();

                DrawBone(child, anim);
            }
            GL.LineWidth(1);
            GL.PopMatrix();
        }

        public void DrawSkeliton()
        {
            DrawSkeliton(null);
        }

        public void DrawSkeliton(AnimationHandler anim)
        {
            GL.DepthMask(false);
            GL.Disable(EnableCap.DepthTest);

            DrawBone(Root, anim);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }
    }

    public class MS3DReader
    {
        public static AnimatedModel Read(string file)
        {
            MilkshapeModel msModel = new MilkshapeModel();
            if (!msModel.Read(new FileInfo(file)))
                return null;

            AnimatedModel model = new AnimatedModel();

            Dictionary<String, Bone> milkbones = new Dictionary<string, Bone>();

            List<Bone> boneIndexes = new List<Bone>();

            foreach (MilkshapeJoint joint in msModel.Joints)
            {
                Bone bone = new Bone();
                bone.matrix = Matrix4.CreateRotationX(joint.Rotation.X) * Matrix4.CreateRotationY(joint.Rotation.Y) * Matrix4.CreateRotationZ(joint.Rotation.Z) * Matrix4.CreateTranslation(joint.Translation);
                bone.translation = joint.Translation;
                bone.rotation = joint.Rotation;

                foreach (MilkshapeKeyframe keyframe in joint.RotationFrames)
                    bone.FrameRotations.Add(new AnimationEvent(keyframe.time, keyframe.Paramater));

                foreach (MilkshapeKeyframe keyframe in joint.TranslationFrames)
                    bone.FrameTranslations.Add(new AnimationEvent(keyframe.time, keyframe.Paramater));

                boneIndexes.Add(bone);
                milkbones.Add(joint.Name, bone);
            }

            foreach (MilkshapeJoint joint in msModel.Joints)
            {
                if (milkbones.ContainsKey(joint.ParentName))
                    milkbones[joint.ParentName].Add(milkbones[joint.Name]);
            }

            foreach (KeyValuePair<String, Bone> bone in milkbones)
            {
                if (bone.Value.Oprhan())
                    model.Root.Add(bone.Value);
            }

            milkbones.Clear();

            foreach (MilkshapeGroup group in msModel.Groups)
            {
                BoneMesh mesh = new BoneMesh();
                if (group.MaterialIndex > msModel.Materials.Count)
                    mesh.Material = msModel.Materials[group.MaterialIndex].Diffuse;

                foreach (int index in group.Triangles)
                {
                    MilkshapeTriangle tri = msModel.Triangles[index];
                    Polygon poly = new Polygon();
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 vertex = msModel.Verts[tri.Verts[i]].Location;
                        // find bone
                        Bone bone = model.Root;
                        if (msModel.Verts[tri.Verts[i]].BoneID < boneIndexes.Count)
                            bone = boneIndexes[msModel.Verts[tri.Verts[i]].BoneID];

                        // transform each vert BACK into the bone
                        int vert = mesh.AddVert(bone, bone.Add(Vector3.Transform(vertex, bone.WorldMatrixInv())));
                        poly.Add(vert, Vector3.TransformNormal(tri.Normals[i], bone.WorldMatrixInv()), tri.UVs[i]);
                    }

                    mesh.Faces.Add(poly);
                }

                model.Meshes.Add(mesh);
            }
            boneIndexes.Clear();

            return model;
        }
    }
}
