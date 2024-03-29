﻿/*
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
using System.Drawing;

using Drawables;
using Drawables.Materials;
using Drawables.StaticModels;
using Drawables.DisplayLists;
using World;
using Math3D;

using OpenTK;

namespace GraphicWorlds
{
    public class GraphicWorld
    {
        public Dictionary<string, Material> materials = new Dictionary<string,Material>();
        public Dictionary<string, StaticModel> models = new Dictionary<string, StaticModel>();
        public ObjectWorld world = new ObjectWorld();

        GroundRenderer ground = new GroundRenderer();
        ObjectRenderer objRender;

        public bool drawAll = false;

        public GraphicWorld ()
        {
            objRender = new ObjectRenderer(this);
        }

        public bool ObjcetVis ( WorldObject obj )
        {
            if (drawAll)
                return true;

            return world.visList.Contains(obj);
        }

        public void Flush()
        {
            ground = new GroundRenderer();
            world.Flush();
            if (materials != null)
                materials.Clear();
            if (models != null)
                models.Clear();
        }

        public void AttachMesh ( WorldObject o )
        {
            if (o.tag == null && o.objectName != string.Empty)
            {
                if (models.ContainsKey(o.objectName))
                    o.tag = models[o.objectName];
            }
        }

        public void AttatchMeshes ()
        {
            foreach (WorldObject o in world.objects)
                AttachMesh(o);
        }

        public void RebuildTree()
        {
            world.Flush();
            world.BuildTree(new BoundingBox(world.size, world.size));
        }

        public void AddDrawables()
        {
            // consolidate the materials in the system
            if (materials != null)
            {
                Dictionary<string, Material> newMats = new Dictionary<string, Material>();
                foreach (KeyValuePair<string, Material> mat in materials)
                {
                    mat.Value.Invalidate();
                    newMats.Add(mat.Key, MaterialSystem.system.GetMaterial(mat.Value));
                }
                materials = newMats;
            }

            AttatchMeshes();
            foreach (WorldObject o in world.objects)
                objRender.AddCallbacks(o);

            ground.Setup(world);
        }

        public void SetBounds ( WorldObject obj )
        {
            StaticModel model = obj.tag as StaticModel;
            if (model == null)
                return;

            obj.bounds = BoundingBox.Empty;

            Matrix4 mat = objRender.GetTransformMatrix(obj);

            foreach(Mesh m in model.meshes)
            {
                List<Vector3> l = new List<Vector3>();
                foreach(Vector3 v in m.verts)
                    l.Add(new Vector3(Vector3.Transform(v, mat)));
                if (obj.bounds == BoundingBox.Empty)
                    obj.bounds = BoundingBox.CreateFromPoints(l);
                else
                    obj.bounds = BoundingBox.CreateMerged(BoundingBox.CreateFromPoints(l),obj.bounds);
            }
        }

        public void SetBounds()
        {
            foreach (WorldObject o in world.objects)
                SetBounds(o);
        }

        public void AddObject( WorldObject obj )
        {
            AttachMesh(obj);
            SetBounds(obj);
            objRender.AddCallbacks(obj);
            world.objects.Add(obj);
        }

        public void ComputeVis ( VisibleFrustum frustum )
        {

        }
    }
}
