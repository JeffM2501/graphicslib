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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;

using OpenTK;
using Math3D;

namespace World
{
    [Serializable]
    public class CellVert
    {
        public Vector3 Bottom;
        public float Top;

        public float GetTopZ(bool incremental)
        {
            if (incremental)
                return Top + Bottom.Z;

            if (Top > Bottom.Z)
                return Top;

            return Bottom.Z;
        }

        public CellVert()
        {
            Bottom = new Vector3();
            Top = 10;
        }

        public CellVert(CellVert v)
        {
            Bottom = new Vector3(v.Bottom);
            Top = v.Top;
        }

        public CellVert(Vector3 b, float t)
        {
            Bottom = new Vector3(b);
            Top = t;
        }

        public CellVert(float x, float y, float z, float height)
        {
            Bottom = new Vector3(x,y,z);
            Top = z + height;
        }
    }

    [Serializable]
    public enum CellEdgeType
    {
        Unknown,
        Wall,
        Portal,
    }

    [Serializable]
    public class CellID
    {
        public static CellID Empty = new CellID(string.Empty,string.Empty);

        public string CellName = string.Empty;
        public string GroupName = string.Empty;

        public CellID() { }
        public CellID ( string cell, string group )
        {
            CellName = cell;
            GroupName = group;
        }
    }

    [Serializable]
    public class CellMaterialInfo
    {
        public static CellMaterialInfo Empty = new CellMaterialInfo();

        public float Rotation = 0f;

        public string Material = string.Empty;

        public Vector2 UVScale = Vector2.One;
        public Vector2 UVShift = Vector2.Zero;

        public CellMaterialInfo()
        {}

        public CellMaterialInfo(string material)
        {
            Material = material;
        }

        public Vector2 GetFinalUV(float u, float v)
        {
            return new Vector2(GetFinalU(u),GetFinalV(v));
        }

        public float GetFinalU(float u)
        {
            return (u + UVShift.X) * UVScale.X;
        }

        public float GetFinalV(float v)
        {
            return (v + UVShift.Y) * UVScale.Y;
        }
    }

    [Serializable]
    public class PortalDestination
    {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public Cell Cell = null;

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public CellGroup Group = null;

        public CellMaterialInfo Material = new CellMaterialInfo();

        public bool Visable = false;

        public CellID DestinationCell = new CellID();

        public PortalMapAttributes DestinationAttributes = new PortalMapAttributes();

        public Vector3 SPBottom = new Vector3();
        public Vector3 SPTop = new Vector3();
       
        public Vector3 EPBottom = new Vector3();
        public Vector3 EPTop = new Vector3();

        public PortalDestination() { }

        public PortalDestination(PortalDestination d)
        {
            DestinationCell.CellName = string.Copy(d.DestinationCell.CellName);
            DestinationCell.GroupName = string.Copy(d.DestinationCell.GroupName);
        }


        [System.Xml.Serialization.XmlIgnoreAttribute]
        public static bool CachePolygons = false;

        [System.Xml.Serialization.XmlIgnoreAttribute]
        List<Vector3> cachedVertList = null;

        public List<Vector3> GetPolygon ( )
        {
            if (!CachePolygons || cachedVertList == null)
            {
                cachedVertList = new List<Vector3>();

                cachedVertList.Add(EPBottom);
                cachedVertList.Add(SPBottom);
                cachedVertList.Add(SPTop);
                cachedVertList.Add(EPTop);
            }

            return cachedVertList;
        }
    }

    [Serializable]
    public class LightmapInfo
    {
        public float UnitSize = 8;

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public Image Map = null;

        public String ID = String.Empty;
    }

    [Serializable]
    public class CellWallGeometry
    {
        public bool Vizable = true;

        public float[] LowerZ = new float[2];
        public float[] UpperZ = new float[2];

        public CellMaterialInfo Material =  new CellMaterialInfo();

        public CellID Bottom = CellID.Empty;
        public CellID Top = CellID.Empty;

        public LightmapInfo Lightmap = new LightmapInfo();

        public CellWallGeometry()
        {}

        public CellWallGeometry(CellID top, CellID bottom, CellMaterialInfo mat)
        {
            Bottom = bottom;
            Top = top;
            Material = mat;
        }
    }

    [Serializable]
    public class CellEdge
    {
        public bool Vizable = true;
        public int Start, End;
        public CellEdgeType EdgeType = CellEdgeType.Unknown;

        public List<PortalDestination> Destinations = new List<PortalDestination>();
        public List<CellWallGeometry> Geometry = new List<CellWallGeometry>();

        public Vector2 Normal = new Vector2();
        public Vector2 Slope = new Vector2();

        public List<PortalMapAttribute> EdgeAttributes = new List<PortalMapAttribute>();

        public Plane EdgePlane = Plane.Empty;

        public CellEdge()
        {
            Start = -1;
            End = -1;
        }

        public CellEdge(CellEdge e)
        {
            Start = e.Start;
            End = e.End;
            EdgeType = e.EdgeType;
            foreach (PortalDestination dest in e.Destinations)
                Destinations.Add(new PortalDestination(dest));
            Normal = new Vector2(e.Normal.X, e.Normal.Y);
        }

        public CellEdge( int start, int end, CellWallGeometry geo )
        {
            Start = start;
            End = end;
            EdgeType = CellEdgeType.Wall;
            Geometry.Add(geo);
        }
    }

    [Serializable]
    public class Cell
    {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public CellGroup Group = null;

        public CellID ID = new CellID(new Random().Next().ToString(), string.Empty);
            
        public List<CellVert> Verts = new List<CellVert>();
        public List<CellEdge> Edges = new List<CellEdge>();

        public Vector3 FloorNormal = new Vector3(0, 0, 1);
        public Vector3 RoofNormal = new Vector3(0, 0, -1);

        public bool HeightIsIncremental = true;

        public bool RoofVizable = true;
        public bool FloorVizable = true;

        public CellMaterialInfo FloorMaterial =  new CellMaterialInfo();
        public CellMaterialInfo RoofMaterial =  new CellMaterialInfo();

        public LightmapInfo FloorLightmap = new LightmapInfo();
        public LightmapInfo RoofLightmap = new LightmapInfo();

        public PortalMapAttributes CellAttributes = new PortalMapAttributes();

        [System.Xml.Serialization.XmlIgnoreAttribute]
        BoundingBox Bounds = BoundingBox.Empty;
        [System.Xml.Serialization.XmlIgnoreAttribute]
        Plane FloorPlane = Plane.Empty;
        [System.Xml.Serialization.XmlIgnoreAttribute]
        Plane RoofPlane = Plane.Empty;
        
        public void Invaldate ()
        {
            Bounds = BoundingBox.Empty;
            FloorPlane = Plane.Empty;
            RoofPlane = Plane.Empty;
        }

        public override string ToString()
        {
            return ID.CellName;
        }

        public Cell()
        { }

        public Cell(Cell cell)
        {
            ID.CellName = cell.ID.CellName;
            ID.GroupName = cell.ID.GroupName;

            CellAttributes = cell.CellAttributes;

            Verts = cell.Verts;
            Edges = cell.Edges;
            HeightIsIncremental = cell.HeightIsIncremental;

            FloorNormal = cell.FloorNormal;
            RoofNormal = cell.RoofNormal;

            RoofVizable = cell.RoofVizable;
            FloorVizable = cell.FloorVizable;

            RoofMaterial = cell.RoofMaterial;
            FloorMaterial = cell.FloorMaterial;

            FloorLightmap = cell.FloorLightmap;
            RoofLightmap = cell.RoofLightmap;
        }

        public bool HasEdge(Vector2 e1, Vector2 e2)
        {
            foreach (CellEdge edge in Edges)
            {
                Vector2 p1 = new Vector2(Verts[edge.Start].Bottom.X, Verts[edge.Start].Bottom.Y);
                Vector2 p2 = new Vector2(Verts[edge.End].Bottom.X, Verts[edge.End].Bottom.Y);

                if (VectorHelper2.Equal(e1, p1) && VectorHelper2.Equal(e2, p2))
                    return true;

                if (VectorHelper2.Equal(e1, p2) && VectorHelper2.Equal(e2, p1))
                    return true;
            }
            return false;
        }

        public int MatchingIndex(Vector3 inVert)
        {
            for (int i = 0; i < Verts.Count; i++)
            {
                if (FloatHelper.Equals(inVert.X, Verts[i].Bottom.X) && FloatHelper.Equals(inVert.Y, Verts[i].Bottom.Y))
                    return i;
            }
            return -1;
        }

        public CellVert MatchingVert(CellVert inVert)
        {
            return MatchingVert(inVert.Bottom);
        }

        public CellVert MatchingVert(Vector3 inVert)
        {
            for (int i = 0; i < Verts.Count; i++)
            {
                if (FloatHelper.Equals(inVert.X, Verts[i].Bottom.X) && FloatHelper.Equals(inVert.Y, Verts[i].Bottom.Y))
                    return Verts[i];
            }
            return null;
        }

        public float EdgeDistance(CellEdge edge)
        {
            CellVert sp = Verts[edge.Start];
            CellVert ep = Verts[edge.End];

            return (float)Math.Sqrt((ep.Bottom.X - sp.Bottom.X) * (ep.Bottom.X - sp.Bottom.X) + (ep.Bottom.Y - sp.Bottom.Y) * (ep.Bottom.Y - sp.Bottom.Y));
        }

        public float EdgeDistance(int edge)
        {
            return EdgeDistance(Edges[edge]);
        }

        public Vector3 FloorPoint(int index)
        {
            return Verts[index].Bottom;
        }

        public Vector3 RoofPoint(int index)
        {
            return new Vector3(Verts[index].Bottom.X, Verts[index].Bottom.Y, RoofZ(index));
        }

        public Vector3 RoofPoint(CellVert vert)
        {
            return new Vector3(vert.Bottom.X, vert.Bottom.Y, vert.GetTopZ(HeightIsIncremental));
        }

        public float RoofZ(int index)
        {
            return Verts[index].GetTopZ(HeightIsIncremental);
        }

        public float RoofZ(CellVert vert)
        {
            return vert.GetTopZ(HeightIsIncremental);
        }

        public static float GetZInPlane(Plane plane, float x, float y)
        {
            return (plane.D - plane.Normal.X * x - plane.Normal.Y * y) / plane.Normal.Z;
        }

        public Plane GetFloorPlane()
        {
            if (FloorPlane == Plane.Empty)
                FloorPlane = new Plane(FloorNormal, Vector3.Dot(FloorNormal, Verts[0].Bottom));
            return FloorPlane;
        }

        public Plane GetRoofPlane()
        {
            if (RoofPlane == Plane.Empty)
                RoofPlane = new Plane(RoofNormal, Vector3.Dot(RoofNormal, RoofPoint(0)));
            return RoofPlane;
        }


        public Plane GetEdgePlane (CellEdge edge)
        {
            if (edge.EdgePlane == Plane.Empty)
                edge.EdgePlane = new Plane(new Vector3(edge.Normal), Vector3.Dot(new Vector3(edge.Normal), Verts[edge.Start].Bottom));
            return edge.EdgePlane;
        }

        public BoundingBox GetBoundingBox ()
        {
            if (Bounds == BoundingBox.Empty)
            {
                List<Vector3> verts = new List<Vector3>();

                foreach (CellVert vert in Verts)
                {
                    verts.Add(vert.Bottom);
                    verts.Add(RoofPoint(vert));
                }
                Bounds = BoundingBox.CreateFromPoints(verts);
            }
            return Bounds;
        }

        public bool PointIn2D( Vector3 point )
        {
            // check the XY see if we are outside
            foreach (CellEdge edge in Edges)
            {
                Vector2 v = new Vector2(Verts[edge.Start].Bottom.X - point.X, Verts[edge.Start].Bottom.Y - point.Y);
                float dot = Vector2.Dot(v, edge.Normal);
                if (dot > 0)
                    return false;
            }

            return true;
        }

        public bool PointIn ( Vector3 point )
        {
            if (GetBoundingBox().Contains(point) == ContainmentType.Disjoint)
                return false;

            if (!PointIn2D(point))
                return false;

            if (GetFloorPlane().Intersects(point) == PlaneIntersectionType.Back)
                return false;

            if (GetRoofPlane().IntersectsPoint(point) == PlaneIntersectionType.Back)
                return false;

            return true;
        }

        public float DistanceToCircle (CellEdge edge, Vector2 center, float radius)
        {
            Vector2 dir = VectorHelper2.Subtract(Verts[edge.End].Bottom, Verts[edge.Start].Bottom);
            Vector2 diff = VectorHelper2.Subtract(center, Verts[edge.Start].Bottom);

            float t = Vector2.Dot(diff, dir) / Vector2.Dot(dir, dir);

            if (t < 0.0f)
                t = 0.0f;
            if (t > 1.0f)
                t = 1.0f;

            Vector2 closest = new Vector2(Verts[edge.Start].Bottom.X,Verts[edge.Start].Bottom.Y) + t * dir;

            Vector2 d = center - closest;

            float distsqr = Vector2.Dot(d, d);
            return (float)Math.Sqrt(distsqr);
        }

        public bool CircleCrossEdge ( CellEdge edge, Vector2 center, float radius)
        {
            return DistanceToCircle(edge,center,radius) <= radius;
        }

        public bool CircleIn ( Vector2 center, float radius )
        {
            foreach (CellEdge edge in Edges)
            {
                if (CircleCrossEdge(edge, center, radius))
                    return false;
            }

            return true;
        }

        public Vector2 FindMinXY ( )
        {
            // find the lower left
            Vector2 v = new Vector2(FloorPoint(0).X, FloorPoint(0).Y);
            for (int i = 1; i < Verts.Count; i++)
            {
                if (Verts[i].Bottom.X < v.X)
                    v.X = Verts[i].Bottom.X;

                if (Verts[i].Bottom.Y < v.Y)
                    v.Y = Verts[i].Bottom.Y;

            }

            return v;
        }


        public Vector2 FindMaxXY()
        {
            // find the lower left
            Vector2 v = new Vector2(FloorPoint(0).X, FloorPoint(0).Y);
            for (int i = 1; i < Verts.Count; i++)
            {
                if (Verts[i].Bottom.X > v.X)
                    v.X = Verts[i].Bottom.X;

                if (Verts[i].Bottom.Y > v.Y)
                    v.Y = Verts[i].Bottom.Y;

            }

            return v;
        }
    }

    [Serializable]
    public class CellGroup
    {
        public string Name = string.Empty;
        public List<Cell> Cells = new List<Cell>();
        public PortalMapAttributes GroupAttributes = new PortalMapAttributes();

        public override string ToString()
        {
            return Name;
        }

        protected bool NameExists(string name)
        {
            foreach (Cell cell in Cells)
            {
                if (cell.ID.CellName == name)
                    return true;
            }
            return false;
        }

        public string NewCellName()
        {
            int count = Cells.Count;
            while (NameExists(count.ToString()))
                count++;

            return count.ToString();
        }

        public Cell HasEdge(Vector2 e1, Vector2 e2)
        {
            foreach (Cell cell in Cells)
            {
                if (cell.HasEdge(e1, e2))
                    return cell;
            }

            return null;
        }

        public Cell FindCell(string name)
        {
            foreach (Cell cell in Cells)
            {
                if (cell.ID.CellName == name)
                    return cell;
            }

            return null;
        }

        public Cell FindCell(CellID id)
        {
            foreach (Cell cell in Cells)
            {
                if (cell.ID.CellName == id.CellName)
                    return cell;
            }

            return null;
        }
    }

    [Serializable]
    public class PortalMapAttribute
    {
        public PortalMapAttribute() { }
        public PortalMapAttribute(string n, string v)
        {
            Name = n;
            Value = v;
        }

        public string Name = string.Empty;
        public string Value = string.Empty;
    }

    [Serializable]
    public class PortalMapAttributes
    {
        public List<PortalMapAttribute> AttributeList = new List<PortalMapAttribute>();

        public void Clear()
        {
            AttributeList.Clear();
        }
        public PortalMapAttribute[] Find(string name)
        {
            List<PortalMapAttribute> foundItems = new List<PortalMapAttribute>();

            foreach (PortalMapAttribute item in AttributeList)
            {
                if (item.Name == name)
                    foundItems.Add(item);
            }

            return foundItems.ToArray();
        }

        public bool FindAttributeWithValue ( string name, string val )
        {
            foreach (PortalMapAttribute item in AttributeList)
            {
                if (item.Name == name)
                    if (item.Value == val)
                        return true;
            }
            return false;
        }

        public void Add(string name, string value)
        {
            foreach (PortalMapAttribute item in AttributeList)
            {
                if (item.Name == name && item.Value == value)
                    return;
            }
            AttributeList.Add(new PortalMapAttribute(name, value));
        }

        public void Remove(string name)
        {
            List<PortalMapAttribute> foundItems = new List<PortalMapAttribute>();

            foreach (PortalMapAttribute item in AttributeList)
            {
                if (item.Name == name)
                    foundItems.Add(item);
            }

            foreach (PortalMapAttribute item in foundItems)
                AttributeList.Remove(item);
        }

        public void Remove(string name, string value)
        {
            List<PortalMapAttribute> foundItems = new List<PortalMapAttribute>();

            foreach (PortalMapAttribute item in AttributeList)
            {
                if (item.Name == name && item.Value == value)
                    foundItems.Add(item);
            }

            foreach (PortalMapAttribute item in foundItems)
                AttributeList.Remove(item);
        }
    }

    [Serializable]
    public class ObjectInstance
    {
        public String ObjectType = String.Empty;
        public String Name = string.Empty;
        public Vector3 Postion = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;

        public List<CellID> cells = new List<CellID>();
        public PortalMapAttributes ObjectAttributes = new PortalMapAttributes();

        public override string ToString()
        {
            return Name + "(" + ObjectType+")";
        }
    }

    [Serializable]
    public enum LightType
    {
        PointLight,
        Spotlight,
        VectorLight,
    }

    [Serializable]
    public class LightInstance
    {
        public Vector3 Position = new Vector3(0, 0, 1);
        public LightType Type = LightType.PointLight;
        public Vector3 Direction = new Vector3(0, 0, -1);
        public float cone = 45.0f;
        public float Inensity = 1.0f;
        public float MinRadius = 1.0f;

        public override string ToString()
        {
            string name = "Point";
            if (Type == LightType.Spotlight)
                name = "Spot";
            if (Type == LightType.VectorLight)
                name = "Vector";
            return name + " (" + Position.ToString() + ")";
        }
    }

    [Serializable]
    public class LightmapBitmap
    {
        public string ID = string.Empty;
        public Byte[] buffer;
    }

    [Serializable]
    public class PortalWorld
    {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public static float LightmapUnitSize =8;

        public List<CellGroup> CellGroups = new List<CellGroup>();
        public PortalMapAttributes MapAttributes = new PortalMapAttributes();
        public List<ObjectInstance> MapObjects = new List<ObjectInstance>();

        public List<LightInstance> Lights = new List<LightInstance>();
        public float AmbientLight = 0.25f;

        public List<LightmapBitmap> Lightmaps = new List<LightmapBitmap>();

        protected void StoreLightmap (ref LightmapInfo info )
        {
            if (info.Map == null)
            {
                info.ID = string.Empty;
                return;
            }
            LightmapBitmap bitmap = new LightmapBitmap();
            bitmap.ID = info.Map.GetHashCode().ToString();
            info.ID = bitmap.ID;

            MemoryStream stream = new MemoryStream(1000000);
            info.Map.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.buffer = new Byte[stream.Position];
            stream.Position = 0;
            stream.Read(bitmap.buffer, 0, bitmap.buffer.Length);
            Lightmaps.Add(bitmap);
        }

        public void StoreLightmaps ( )
        {
            Lightmaps.Clear();
            foreach ( CellGroup group in CellGroups)
            {
                foreach (Cell cell in group.Cells)
                {
                    StoreLightmap(ref cell.FloorLightmap);
                    StoreLightmap(ref cell.RoofLightmap);

                    foreach(CellEdge edge in cell.Edges)
                    {
                        foreach (CellWallGeometry geo in edge.Geometry)
                            StoreLightmap(ref geo.Lightmap);
                    }
                }
            }
        }

        LightmapBitmap FindBitmap ( string ID )
        {
            foreach(LightmapBitmap lightmap in Lightmaps)
            {
                if (lightmap.ID == ID)
                    return lightmap;
            }

            return null;
        }

        protected void RestoreLightmap(ref LightmapInfo info)
        {
            LightmapBitmap bitmap = FindBitmap(info.ID);
            if (bitmap == null)
            {
                info.ID = string.Empty;
                info.Map = null;
            }
            else
            {
                MemoryStream stream = new MemoryStream(bitmap.buffer);
                info.Map = new Bitmap(stream);
            }
        }

        public void RestoreLightamps ()
        {
            foreach (CellGroup group in CellGroups)
            {
                foreach (Cell cell in group.Cells)
                {
                    RestoreLightmap(ref cell.FloorLightmap);
                    RestoreLightmap(ref cell.RoofLightmap);

                    foreach (CellEdge edge in cell.Edges)
                    {
                        foreach (CellWallGeometry geo in edge.Geometry)
                            RestoreLightmap(ref geo.Lightmap);
                    }
                }
            }

            Lightmaps.Clear();
        }

        public int CountCells ( )
        {
            int count = 0;
            foreach (CellGroup group in CellGroups)
                foreach (Cell cell in group.Cells)
                    count++;

            return count;
        }

        public int CountEdges()
        {
            int count = 0;
            foreach (CellGroup group in CellGroups)
                foreach (Cell cell in group.Cells)
                    foreach (CellEdge edge in cell.Edges)
                        count++;

            return count;
        }

        public int CountFaces()
        {
            int count = 0;
            foreach (CellGroup group in CellGroups)
                foreach (Cell cell in group.Cells)
                {
                    count += 2;
                    foreach (CellEdge edge in cell.Edges)
                        foreach(CellWallGeometry geo in edge.Geometry)
                            count++;
                }

            return count;
        }

        public List<ObjectInstance> FindObjects ( string type )
        {
            List<ObjectInstance> objs = new List<ObjectInstance>();
            foreach (ObjectInstance obj in MapObjects)
            {
                if (obj.ObjectType == type)
                    objs.Add(obj);
            }

            return objs;
        }

        public List<ObjectInstance> FindObjects(string type, string name)
        {
            List<ObjectInstance> objs = new List<ObjectInstance>();
            foreach (ObjectInstance obj in MapObjects)
            {
                if (obj.ObjectType == type && obj.Name == name)
                    objs.Add(obj);
            }

            return objs;
        }

        public bool Valid()
        {
            foreach (CellGroup group in CellGroups)
            {
                if (group.Cells.Count > 0)
                    return true;
            }
            return false;
        }

        protected bool NameExists(string name)
        {
            foreach (CellGroup group in CellGroups)
            {
                if (group.Name == name)
                    return true;
            }
            return false;
        }

        public string NewGroupName()
        {
            int count = CellGroups.Count;
            while (NameExists(count.ToString()))
                count++;

            return count.ToString();
        }

        public void AddCell(Cell cell, CellGroup group)
        {
            cell.Group = group;
            cell.ID.GroupName = group.Name;
            group.Cells.Add(cell);
        }

        public List<Cell> CellsThatContainEdge(Vector2 p1, Vector2 p2)
        {
            return CellsThatContainEdge(p1, p2, null);
        }

        public List<Cell> CellsThatContainEdge(Vector2 p1, Vector2 p2, Cell ignoreCell)
        {
            List<Cell> foundCells = new List<Cell>();
            foreach (CellGroup group in CellGroups)
            {
                foreach (Cell cell in group.Cells)
                {
                    if (cell != ignoreCell)
                    {
                        if (cell.HasEdge(p1, p2))
                            foundCells.Add(cell);
                    }
                }
            }

            return foundCells;
        }

        public void RemoveCell(Cell cell)
        {
            CellGroup group = cell.Group;
            if (group == null)
                group = FindGroup(cell.ID);

            if (group != null)
                group.Cells.Remove(cell);
        }

        public CellGroup FindGroup(CellID id)
        {
            foreach (CellGroup group in CellGroups)
            {
                if (group.Name == id.GroupName)
                    return group;
            }
            return null;
        }

        public CellGroup FindGroup(string name)
        {
            foreach (CellGroup group in CellGroups)
            {
                if (group.Name == name)
                    return group;
            }
            return null;
        }

        public Cell FindCell ( CellID id )
        {
            return FindCell(id.CellName, id.GroupName);
        }

        public Cell FindCell(string name, string groupName)
        {
            CellGroup group = FindGroup(groupName);
            if (group != null)
                return group.FindCell(name);

            return null;
        }

        public List<CellID> FindCells ( Vector3 point )
        {
            List<CellID> cells = new List<CellID>();

            foreach (CellGroup group in CellGroups)
            {
                // link up all the portals
                foreach (Cell cell in group.Cells)
                {
                    if (cell.PointIn(point))
                        cells.Add(cell.ID);
                 }
            }
            return cells;
        }

        public void FlushLightmaps ()
        {
            Lightmaps.Clear();
            foreach (CellGroup group in CellGroups)
            {
                foreach (Cell cell in group.Cells)
                {
                    cell.FloorLightmap = null;
                    cell.RoofLightmap = null;

                    foreach (CellEdge edge in cell.Edges)
                    {
                        foreach (CellWallGeometry geo in edge.Geometry)
                            geo.Lightmap.Map = null;
                    }
                }
            }
        }

        public void RebindCells()
        {
            foreach (CellGroup group in CellGroups)
            {
                // link up all the portals
                foreach (Cell cell in group.Cells)
                {
                    cell.Group = group;
                    cell.ID.GroupName = group.Name;

                    foreach (CellEdge edge in cell.Edges)
                    {
                        if (edge.EdgeType == CellEdgeType.Portal)
                        {
                            foreach (PortalDestination dest in edge.Destinations)
                            {
                                dest.Cell = FindCell(dest.DestinationCell);
                                dest.Group = dest.Cell.Group;
                            }
                        }
                    }
                }
            }
        }

        public static PortalWorld Read(FileInfo file)
        {
            if (!file.Exists)
                return null;

            PortalWorld world = Read(file.OpenRead(), false);
            if (world == null)
                world = Read(file.OpenRead(), true);

            return world;
        }

        public static PortalWorld Read(Stream stream, bool compressed)
        {
            if (stream == null)
                return null;

            XmlSerializer XML = new XmlSerializer(typeof(PortalWorld));

            PortalWorld map = null;
            if (!compressed)
            {
                try
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    map = (PortalWorld)XML.Deserialize(reader);
                    reader.Close();
                }
                catch (System.Exception /*ex*/)
                {
                    stream.Close();
                    return null;
                }
            }
            else
            {
                try
                {
                    GZipStream compressionStream = new GZipStream(stream, CompressionMode.Decompress);
                    map = (PortalWorld)XML.Deserialize(compressionStream);
                    compressionStream.Close();
                }
                catch (System.Exception /*ex*/)
                {
                    stream.Close();
                    return null;
                }
            }

            stream.Close();

            if (map != null)
            {
                map.RebindCells();
                map.RestoreLightamps();
            }

            return map;
        }

        public bool Write(FileInfo file)
        {
            return Write(file, false);
        }

        public bool Write(FileInfo file, bool compress)
        {
            if (file.Exists)
            {
                try
                {
                    file.Delete();
                }
                catch (System.Exception /*ex*/)
                {

                }
            }
            FileStream fs = file.OpenWrite();
            bool ret = Write(fs, compress);
            fs.Close();
            return ret;
        }

        public bool Write(Stream stream, bool compress)
        {
            XmlSerializer XML = new XmlSerializer(typeof(PortalWorld));

            PortalWorld writeMap = new PortalWorld();
            writeMap.MapAttributes = MapAttributes;
            writeMap.MapObjects = MapObjects;
            writeMap.Lights = Lights;
            writeMap.AmbientLight = AmbientLight;

            foreach (CellGroup group in CellGroups)
            {
                CellGroup newGroup = new CellGroup();
                newGroup.Name = group.Name;
                writeMap.CellGroups.Add(newGroup);
                foreach (Cell cell in group.Cells)
                    newGroup.Cells.Add(new Cell(cell));
            }

            writeMap.StoreLightmaps();

            try
            {
                if (compress)
                {
                    GZipStream compression = new GZipStream(stream, CompressionMode.Compress);
                    XML.Serialize(compression, writeMap);
                    compression.Close();
                }
                else
                {
                    StreamWriter writer = new StreamWriter(stream);
                    XML.Serialize(writer, writeMap);
                    writer.Close();
                }
            }
            catch (System.Exception /*ex*/)
            {
                return false;
            }

            return true;
        }
    }
}
