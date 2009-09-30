﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;

using OpenTK;

using Drawables.DisplayLists;
using Drawables;

namespace PortalEdit
{

    public enum ViewEditMode
    {
        Select,
        Paint,
    }

    class Editor
    {
        public PortalMap map;
        public MapRenderer mapRenderer;
        public MapViewRenderer viewRenderer;
        public EditFrame frame;

        public static Editor instance;

        public static float EditZFloor = 0;
        public static float EditZRoof = 2;
        public static bool EditZInc = true;

        public ViewEditMode viewEditMode = ViewEditMode.Select;

        public Editor(EditFrame _frame, Control mapctl, GLControl view)
        {
            frame = _frame;
            map = new PortalMap();

            mapRenderer = new MapRenderer(mapctl, map);
            viewRenderer = new MapViewRenderer(view, map);

            mapRenderer.NewPolygon += new NewPolygonHandler(mapRenderer_NewPolygon);
            mapRenderer.MouseStatusUpdate += new MouseStatusUpdateHandler(frame.mapRenderer_MouseStatusUpdate);
            mapRenderer.CellSelected += new CellSelectedHander(mapRenderer_CellSelected);

            viewRenderer.CellClicked += new MapViewRenderer.CellClickedEventHandler(viewRenderer_CellClicked);

            NewGroup(false);
        }

        void viewRenderer_CellClicked(object sender, MapViewRenderer.CellClickedEventArgs e)
        {
            if (viewEditMode == ViewEditMode.Select)
            {
                SelectObject(e.cell);
            }
        }

        TreeNode FindSelectedNode ( object tag, TreeNode node )
        {
            if (node.Tag == tag)
                return node;

            foreach (TreeNode child in node.Nodes)
            {
                TreeNode foundNode = FindSelectedNode(tag, child);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }

        void mapRenderer_CellSelected(object sender, Cell cell)
        {
            SelectObject(cell);
        }

        void SelectObject ( object item )
        {
            TreeNode selectedNode = null;

            foreach (TreeNode child in frame.MapTree.Nodes)
            {
                selectedNode = FindSelectedNode(item, child);
                if (selectedNode != null)
                    break;
            }

            frame.MapTree.SelectedNode = selectedNode;
            frame.Invalidate(true);
        }

        public void NewGroup ()
        {
            NewGroup(true);
        }

        public void NewGroup ( bool undo )
        {
            CellGroup group = new CellGroup();
            group.Name = map.NewGroupName();
            map.cellGroups.Add(group);

            if (undo)
            Undo.System.Add(new GroupAddUndo(group));
            ResetViews();
            SelectObject(group);
        }

        protected void ResetViews ()
        {
            DisplayListSystem.system.Invalidate();
            frame.populateCellList();
            mapRenderer.Redraw();
            viewRenderer.Render3dView();
        }

        public EditorCell GetSelectedCell ( )
        {
            if (frame.MapTree.SelectedNode == null)
                return null;

            object tag = frame.MapTree.SelectedNode.Tag;
            if (tag.GetType() == typeof(EditorCell))
                return (EditorCell)tag;

            return null;
        }

        public CellGroup GetSelectedGroup()
        {
            if (frame.MapTree.SelectedNode == null)
                return null;

            object tag = frame.MapTree.SelectedNode.Tag;
            if (tag.GetType() == typeof(CellGroup))
                return (CellGroup)tag;

            return null;
        }

        public CellVert GetSelectedVert()
        {
            Cell cell = GetSelectedCell();
            if (cell == null)
                return null;

            int index = GetSelectedVertIndex();
            if (index >= 0)
                return cell.Verts[index];
             return null;
        }

        public int GetSelectedVertIndex()
        {
            Cell cell = GetSelectedCell();
            if (cell == null)
                return -1;

            if (frame.CellVertList.SelectedRows.Count > 0)
            {
                int index = int.Parse(frame.CellVertList.SelectedRows[0].Cells[0].Value.ToString());
                return index;
            }
            return -1;
        }


        public void EditVert ()
        {
            CellVert vert = GetSelectedVert();
            if (vert == null)
                return;

            Undo.System.Add(new VertexDataEditUndo(GetSelectedCell(), GetSelectedVertIndex()));
            try
            {
                float.TryParse(frame.CellVertList.SelectedRows[0].Cells[1].Value.ToString(), out vert.Bottom.Z);
                float.TryParse(frame.CellVertList.SelectedRows[0].Cells[2].Value.ToString(), out vert.Top);

                DisplayListSystem.system.Invalidate();
                ResetViews();
            }
            catch (System.Exception ex)
            {
            }
        }

        public void SetCellInZ ( bool inc )
        {
            EditorCell cell = GetSelectedCell();
            if (cell == null)
                return;

            Undo.System.Add(new IncrementalHeightsUndo(cell));

            cell.HeightIsIncremental = inc;
        }

        public bool Open ( FileInfo file )
        {
            PortalMap newMap = PortalMap.Read(file);
            if (newMap == null)
                return false;

            mapRenderer.ClearEditPolygon();

            DisplayListSystem.system.Invalidate();
            DrawablesSystem.system.removeAll();
            map.cellGroups.Clear();

            foreach (CellGroup group in newMap.cellGroups)
            {
                CellGroup newGroup = new CellGroup();
                newGroup.Name = group.Name;
                map.cellGroups.Add(newGroup);

                foreach (Cell cell in group.Cells)
                    newGroup.Cells.Add(new EditorCell(cell));
            }

            map.RebindCells();
            ResetViews();
            return true;
        }

        public bool Save(FileInfo file)
        {
            return map.Write(file);
        }

        public void RebuildMap ()
        {
            foreach (CellGroup group in map.cellGroups)
                foreach (EditorCell cell in group.Cells)
                    cell.CheckEdges(map);
        }

        public bool DeleteCell(EditorCell cell)
        {
            if (cell == null)
                return false;

            Undo.System.Add(new CellDeleteUndo(cell));

            cell.Dispose();
            map.RemoveCell(cell);
            RebuildMap();

            ResetViews();
            return true;
        }

        void mapRenderer_NewPolygon(object sender, Polygon polygon)
        {
            if (map.cellGroups.Count == 0)
                return;

            CellGroup group = GetSelectedGroup();
            if (group == null)
            {
                Cell selCel = GetSelectedCell();
                if (selCel != null)
                    group = selCel.Group;

                if (group == null && selCel != null)
                    group = map.FindGroup(selCel.GroupName);

                if (group == null)
                    group = map.cellGroups[map.cellGroups.Count - 1];
            }

            EditorCell cell = new EditorCell(polygon, map, group);

            group.Cells.Add(cell);

            foreach(CellGroup g in map.cellGroups)
            {
                foreach (Cell c in g.Cells )
                {
                    EditorCell eCell = (EditorCell)c;
                    eCell.CheckEdges(map);
                }
            }

            Undo.System.Add(new CellAddUndo(cell));

            DisplayListSystem.system.Invalidate();
            ResetViews();
        }
    }
}
