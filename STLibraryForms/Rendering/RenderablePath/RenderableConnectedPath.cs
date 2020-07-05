using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework;
using Toolbox.Core;

namespace Toolbox.Core.Rendering
{
    public partial class RenderableConnectedPath : EditableObject
    {
        /// <summary>
        /// Allows the path object to be seen above other objects
        /// </summary>
        public static bool XRayMode = false;

        /// <summary>
        /// Automatically connects points when a point is moved near another point
        /// </summary>
        public static bool ConnectMovedHoveredPoints = true;

        /// <summary>
        /// Automatically connects points when a point is extruded near another point
        /// </summary>
        public static bool ConnectExtrudedHoveredPoints = true;

        /// <summary>
        /// Determines to scale points based on camera distance.
        /// </summary>
        public static bool ScaleByCamera = false;

        /// <summary>
        /// Determines the factor scale points based on camera distance.
        /// </summary>
        public static float CameraScaleFactor = 0.01f;

        /// <summary>
        /// Determines the thickness of the line connecting the points
        /// </summary>
        public virtual float LineWidth { get; set; } = 30.0f;

        /// <summary>
        /// The offset of the line in the Y axis when drawn. 
        /// Can prevent z fighting points depenting on how the point is drawn
        /// </summary>
        public virtual float LineOffset { get; set; } = 0.0f;

        /// <summary>
        /// Determines the arrow size
        /// </summary>
        public virtual float ArrowScale { get; set; } = 0.1f;

        /// <summary>
        /// Determines to draw the arrow in between points. Defaults to the child if disabled.
        /// </summary>
        public virtual bool IsArrowCentered { get; set; } = false;

        /// <summary>
        /// The total possible amount of parents that can parent a point
        /// </summary>
        public virtual int LimitParentCount => 100;

        /// <summary>
        /// The scale to default to when a point is created
        /// </summary>
        public virtual Vector3 DefaultScale { get; set; } = new Vector3(2);

        /// <summary>
        /// The color used for the points
        /// </summary>
        public Color SphereColor = Color.Red;

        /// <summary>
        /// The color used for the lines connecting the points.
        /// </summary>
        public Color LineColor = Color.Red;

        /// <summary>
        /// The color used for the arrows.
        /// </summary>
        public Color ArrowColor = Color.Yellow;

        public EventHandler PointAdded = null;
        public EventHandler PointRemoved = null;

        /// <summary>
        /// A list of all the points used in the path object.
        /// </summary>
        public List<RenderablePathPoint> PathPoints = new List<RenderablePathPoint>();

        public RenderableConnectedPath(Color color, Color lineColor, Color arrowColor)
        {
            SphereColor = color;
            LineColor = lineColor;
            ArrowColor = arrowColor;
        }

        public virtual RenderablePathPoint CreatePoint(Vector3 position)
        {
            var point = new RenderablePathPoint(this, position);
            point.Scale = DefaultScale;
            return point;
        }

        /// <summary>
        /// Gets all the selected points in the path object.
        /// </summary>
        /// <returns></returns>
        public List<RenderablePathPoint> GetSelectedPoints()
        {
            List<RenderablePathPoint> points = new List<RenderablePathPoint>();
            for (int i = 0; i < PathPoints.Count; i++)
            {
                if (PathPoints[i].IsSelected())
                    points.Add(PathPoints[i]);
            }
            return points;
        }

        /// <summary>
        /// Gets a list of all the points that are currently being hovered over in the scene.
        /// </summary>
        /// <param name="editorScene"></param>
        /// <returns></returns>
        public List<RenderablePathPoint> GetHoveredPoints(EditorSceneBase editorScene)
        {
            List<RenderablePathPoint> points = new List<RenderablePathPoint>();
            bool hovered = editorScene.Hovered == this;

            int part = 0;
            foreach (var drawable in PathPoints)
            {
                if (drawable.GetPickableSpan() == 0)
                    continue;

                if (hovered && (editorScene.HoveredPart == part))
                    points.Add(drawable);

                part++;
            }

            return points;
        }

        /// <summary>
        /// Determines if the given point can be connected based on the properties of the path object.
        /// The point must be connected to a point that is less that the parent limit.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool PointCanConnect(RenderablePathPoint point)
        {
            return true;

            foreach (var parent in point.Parents)
            {
                if (parent.Parents.Count >= LimitParentCount)
                    return false;
            }

            return true;
        }

        public void AverageSelected(EditorSceneBase scene, uint axis)
        {
            var selected = GetSelectedPoints();
            if (selected.Count < 2)
                return;

            AddPointTransformsToUndo(scene, selected);

            var firstPoint = selected.FirstOrDefault();
            switch (axis)
            {
                case 0: //X
                    for (int i = 0; i < selected.Count; i++)
                        selected[i].SetPositionXAxis(firstPoint.GlobalPosition.X);
                    break;
                case 1: //Y
                    for (int i = 0; i < selected.Count; i++)
                        selected[i].SetPositionYAxis(firstPoint.GlobalPosition.Y);
                    break;
                case 2: //Z
                    for (int i = 0; i < selected.Count; i++)
                        selected[i].SetPositionZAxis(firstPoint.GlobalPosition.Z);
                    break;
            }
        }

        private void AddPointTransformsToUndo(EditorSceneBase scene, List<RenderablePathPoint> selected)
        {
            TransformChangeInfos transformChangeInfos = new TransformChangeInfos(new List<TransformChangeInfo>());
            for (int i = 0; i < selected.Count; i++)
                transformChangeInfos.Add(selected[i], i, selected[i].GlobalPosition, selected[i].Rotation, selected[i].GlobalScale);

            scene.AddTransformToUndo(transformChangeInfos);
        }

        public void FillSelectedPoints(EditorSceneBase scene)
        {
            var selected = GetSelectedPoints();
            if (!selected[1].Children.Contains(selected[0]))
            {
                selected[1].Children.Add(selected[0]);
            }
        }

        public void MergeSelectedPoints(EditorSceneBase scene)
        {
            var selected = GetSelectedPoints();
            if (selected.Count == 2)
            {
                selected[1].ConnectToPoint(selected[0]);
            }
        }

        public void ConnectToHoveredPoints(EditorSceneBase scene, bool alt)
        {
            //Only pass if points can be hover connected by movement or being extruded
            //If an action occurs then the point may be being extruded
            bool hasExclusiveAction = (scene.ExclusiveAction != null && scene.ExclusiveAction != EditorScene.NoAction);
            bool hasNormalAction = (scene.CurrentAction != null && scene.CurrentAction != EditorScene.NoAction);

            if ((!hasExclusiveAction && !hasNormalAction) && ConnectExtrudedHoveredPoints)
                return;

            //Get the selected points.
            //Make sure selected points cannot use the hovered pick pass
            var points = GetSelectedPoints();
            foreach (var point in points)
                point.isHoverPickPass = true;

            //For each point disable the current hovered point
            foreach (var point in PathPoints)
                point.IsPointOver = false;

            //Find any points hovered over while a point is selected
            int part = scene.FindPickablePart(scene.GL_Control, this);
            if (part != -1)
            {
                foreach (var point in points)
                {
                    int hoveredPart = part;
                    foreach (var pt in PathPoints)
                    {
                        //When a selected point's position is changed
                        //check all points and find a hovered point
                        //To determine a connection to be made when released
                        int span = pt.GetPickableSpan();
                        if (hoveredPart >= 0 && hoveredPart < span && !pt.IsSelected() && PointCanConnect(point))
                        {
                            pt.IsPointOver = true;
                        }
                        hoveredPart -= span;
                    }
                }
            }

            //Disable bool preventing hover pick pass when finished
            foreach (var point in points)
                point.isHoverPickPass = false;

            scene.GL_Control.Invalidate();
        }

        public void CopyPointsFromSelected(EditorSceneBase scene)
        {
            ExtrudePointsFromSelected(scene, true);
        }

        public void ExtrudePointsFromSelected(EditorSceneBase scene, bool copySelected = false)
        {
            if (scene.ExclusiveAction != null && scene.ExclusiveAction.GetType() == typeof(EditorSceneBase.TranslateAction))
                return;

            var selectedPoints = GetSelectedPoints();
            if (copySelected && selectedPoints.Count == 0)
                return;

            //Mouse position
            Vector3 position = Vector3.Zero;
            if (scene.Hovered != null)
                position = -scene.GL_Control.GetPointUnderMouse(scene.GL_Control.PickingDepth);
            else
                position = -scene.GL_Control.GetPointUnderMouse(30);

            //Create the new point
            var point = CreatePoint(position);

            //Find the first hovered point
            //Add the new point as the child of the previously selected point
            var parent = GetHoveredPoints(scene).FirstOrDefault();
            if (parent != null)
            {
                parent.AddChild(point);
                point.Position = parent.Position;
            }

            if (copySelected)
                selectedPoints[0].CopyPropertiesToPoint(point);

            //Add the new point
            AddPoint(point);

            DeselectAll(scene.GL_Control);

            //Select only the new point 
            scene.SelectedObjects.Clear();
            point.SelectDefault(scene.GL_Control);

            scene.GL_Control.Invalidate();

            scene.AddToUndo(new RevertableAddPointCollection(point));

            //Transform the point
            var transform = new EditableObject.LocalOrientation();
            scene.UndoTransform = false;
            scene.StartTransformAction(transform, EditorSceneBase.DragActionType.TRANSLATE, PathPoints.Count - 1, null, this, true);
        }

        public void RedirectSelected(EditorSceneBase scene)
        {
            List<RenderablePathPoint> swappedPoints = new List<RenderablePathPoint>();

            var selected = GetSelectedPoints();
            foreach (var point in selected)
            {
                foreach (var child in point.Children)
                    if (swappedPoints.Contains(child))
                        continue;
                foreach (var parent in point.Parents)
                    if (swappedPoints.Contains(parent))
                        continue;

                point.SwapChildrenAndParents();
                swappedPoints.Add(point);
            }
        }

        public void Subdivide(EditorSceneBase scene)
        {
            List<RenderablePathPoint> addedPoints = new List<RenderablePathPoint>();

            var selected = GetSelectedPoints();
            foreach (var point in selected)
            {
                if (point.Children.Count == 0)
                    continue;

                List<RenderablePathPoint> children = new List<RenderablePathPoint>();
                foreach (var child in point.Children)
                    if (child.IsSelected())
                        children.Add(child);

                foreach (var child in children)
                {
                    var dir = child.Translate - point.Translate;
                    var center = (point.Translate + (dir / 2f));
                    //Add a centered point
                    var newpoint = CreatePoint(center);
                    AddPoint(newpoint);

                    point.Children.Remove(child);
                    point.AddChild(newpoint);
                    addedPoints.Add(newpoint);

                    newpoint.AddChild(child);
                }

            }
            if (addedPoints.Count > 0)
                scene.AddToUndo(new RevertableAddPointCollection(addedPoints));
        }

        public void RemoveSelected(EditorSceneBase scene)
        {
            List<RenderablePathPoint> points = new List<RenderablePathPoint>();
            foreach (RenderablePathPoint point in PathPoints)
                if (point.IsSelected())
                    points.Add(point);
            if (points.Count == 0)
                return;

            scene.AddToUndo(new RevertableDelPointCollection(points));
            RemovePoints(points);
        }

        private void RemovePointReferences(RenderablePathPoint removePoint)
        {
            bool isDivision = removePoint.Parents.Count == 1 && removePoint.Children.Count == 1;
            if (isDivision)
            {
                var parent = removePoint.Parents[0];
                var child = removePoint.Children[0];

                foreach (var point in PathPoints)
                {
                    if (point.Parents.Contains(removePoint))
                        point.Parents.Remove(removePoint);
                    if (point.Children.Contains(removePoint))
                        point.Children.Remove(removePoint);
                }

                parent.AddChild(child);
            }
            else
            {
                foreach (var point in PathPoints)
                {
                    if (point.Parents.Contains(removePoint))
                        point.Parents.Remove(removePoint);
                    if (point.Children.Contains(removePoint))
                        point.Children.Remove(removePoint);
                }
            }

            removePoint.Children.Clear();
            removePoint.Parents.Clear();
        }

        public void RemovePoints(List<RenderablePathPoint> removePoints)
        {
            foreach (var obj in removePoints)
                RemovePointReferences(obj);
            foreach (var obj in removePoints)
                RemovePoint(obj, true);

            PointRemoved?.Invoke(removePoints, EventArgs.Empty);
        }

        public void RemovePoint(RenderablePathPoint removePoint, bool isCollection = false)
        {
            RemovePointReferences(removePoint);
            PathPoints.Remove(removePoint);

            if (!isCollection)
                PointRemoved?.Invoke(removePoint, EventArgs.Empty);
        }

        public void AddPoint(RenderablePathPoint point)
        {
            DefaultScale = point.Scale;
            PathPoints.Add(point);

            PointAdded?.Invoke(point, EventArgs.Empty);
        }

        public override string ToString() => "path";

        LineRenderer LineRenderer;

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (!ObjectRenderState.ShouldBeDrawn(this))
                return;

            bool hovered = editorScene.Hovered == this;

            if (XRayMode && pass != Pass.PICKING)
                GL.Disable(EnableCap.DepthTest);

            if (pass == Pass.OPAQUE && !XRayMode || pass == Pass.TRANSPARENT && XRayMode)
            {
                Vector3 offset = new Vector3(0, LineOffset, 0);

                List<Vector3> points = new List<Vector3>();
                foreach (var path in PathPoints)
                {
                    if (!path.Visible)
                        continue;

                    foreach (var nextPt in path.Children)
                    {
                        points.Add((path.Translate + offset));
                        points.Add((nextPt.Translate + offset));
                    }
                }

                if (LineRenderer == null)
                {
                    LineRenderer = new LineRenderer();
                    LineRenderer.Width = LineWidth;
                    LineRenderer.Prepare(control);
                }

                LineRenderer.UpdateVertexData(control, points, new Vector3(1, 1, 1));
                LineRenderer.Color = LineColor;
                LineRenderer.Draw(control, pass);

                foreach (var path in PathPoints)
                {
                    if (!path.Visible)
                        continue;

                    foreach (var nextPt in path.Children)
                    {
                        var dir = (nextPt.Translate - path.Translate);
                        Vector3 scale = new Vector3(ArrowScale);
                        if (path.Translate.Length != 0 && ScaleByCamera)
                            scale *= 1 + ((control.CameraTarget.Length / nextPt.Translate.Length) * CameraScaleFactor);

                        var rotation = RotationFromTo(new Vector3(0, 0, 1), dir.Normalized());
                        Matrix4 offsetMat = Matrix4.CreateTranslation(new Vector3(0, 0, -25));
                        Matrix4 translateMat = Matrix4.CreateTranslation(nextPt.Translate);

                        if (IsArrowCentered)
                        {
                            offsetMat = Matrix4.Identity;
                            translateMat = Matrix4.CreateTranslation((path.Translate + (dir / 2f)));
                        }

                        control.UpdateModelMatrix(
                          offsetMat *
                          Matrix4.CreateScale(scale) *
                          rotation *
                          translateMat);

                        ColorConeRenderer.Draw(control, pass, ColorUtility.ToVector4(ArrowColor),
                            new Vector4(1, 1, 1, 1), Vector4.Zero, XRayMode);
                    }
                }
            }

            int part = 0;
            foreach (var drawable in PathPoints)
            {
                if (drawable.GetPickableSpan() == 0)
                    continue;

                drawable.Hovered = hovered && (editorScene.HoveredPart == part);
                drawable.Draw(control, pass, editorScene);
                part++;
            }

            if (XRayMode && pass != Pass.PICKING)
                GL.Enable(EnableCap.DepthTest);
        }

        static Matrix4 RotationFromTo(Vector3 start, Vector3 end)
        {
            Vector3 cross = Vector3.Cross(start, end);
            var axis = Vector3.Cross(start, end).Normalized();
            var angle = (float)Math.Acos(Vector3.Dot(start, end));
            var mat = Matrix4.CreateFromAxisAngle(axis, angle);

            if (start == end)
                return Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), angle);
            else if (start + end == Vector3.Zero)
                return Matrix4.CreateFromAxisAngle(new Vector3(-1f, -1f, 0), angle);

            return mat;
        }

        public override void Draw(GL_ControlLegacy control, Pass pass, EditorSceneBase editorScene)
        {
            if (pass == Pass.TRANSPARENT)
                return;

            if (!ObjectRenderState.ShouldBeDrawn(this))
                return;
        }

        public override void Prepare(GL_ControlModern control)
        {

        }

        public override void Prepare(GL_ControlLegacy control)
        {

        }

        public override int GetPickableSpan()
        {
            if (!ObjectRenderState.ShouldBeDrawn(this))
                return 0;
            int i = 0;
            foreach (RenderablePathPoint point in PathPoints)
                i += point.GetPickableSpan();

            return i;
        }

        public override void StartDragging(DragActionType actionType, int hoveredPart, EditorSceneBase scene)
        {
            //   if (!WinInput.Keyboard.IsKeyDown(WinInput.Key.LeftCtrl))
            //        DeselectAll(scene.GL_Control);

            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (hoveredPart >= 0 && hoveredPart < span || point.IsSelected())
                {
                    point.StartDragging(actionType, hoveredPart, scene);
                }
                hoveredPart -= span;
            }
        }

        public override uint SelectAll(GL_ControlBase control)
        {
            foreach (RenderablePathPoint point in PathPoints)
                point.SelectAll(control);

            return REDRAW;
        }

        public override uint SelectDefault(GL_ControlBase control)
        {
            foreach (RenderablePathPoint point in PathPoints)
                point.SelectDefault(control);

            return REDRAW;
        }

        public override bool IsSelected(int partIndex)
        {
            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (partIndex >= 0 && partIndex < span)
                {
                    if (point.IsSelected(partIndex))
                        return true;
                }
                partIndex -= span;
            }
            return false;
        }

        public override uint Select(int partIndex, GL_ControlBase control)
        {
            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (partIndex >= 0 && partIndex < span)
                {
                    point.Select(partIndex, control);
                }
                partIndex -= span;
            }

            return REDRAW;
        }

        public override uint Deselect(int partIndex, GL_ControlBase control)
        {
            bool noPointsSelected = true;
            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (partIndex >= 0 && partIndex < span)
                {
                    point.Deselect(partIndex, control);
                }
                partIndex -= span;
                noPointsSelected &= !point.IsSelected();
            }

            return REDRAW;
        }

        public override void SetTransform(Vector3? pos, Vector3? rot, Vector3? scale, int _part, out Vector3? prevPos, out Vector3? prevRot, out Vector3? prevScale)
        {
            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (_part >= 0 && _part < span)
                {
                    point.SetTransform(pos, rot, scale, _part, out prevPos, out prevRot, out prevScale);
                    return;
                }
                _part -= span;
            }
            throw new Exception("Invalid partIndex");
        }

        public override void ApplyTransformActionToSelection(EditorSceneBase scene, AbstractTransformAction transformAction, ref TransformChangeInfos transformChangeInfos)
        {
            foreach (RenderablePathPoint point in GetSelectedPoints())
                point.ApplyTransformActionToSelection(scene, transformAction, ref transformChangeInfos);
        }

        public override void ApplyTransformActionToPart(EditorSceneBase scene, AbstractTransformAction transformAction, int _part, ref TransformChangeInfos transformChangeInfos)
        {
            foreach (RenderablePathPoint point in PathPoints)
            {
                int span = point.GetPickableSpan();
                if (_part >= 0 && _part < span)
                {
                    point.ApplyTransformActionToPart(scene, transformAction, _part, ref transformChangeInfos);
                    return;
                }
                _part -= span;
            }
            throw new Exception("Invalid partIndex");
        }

        public override uint DeselectAll(GL_ControlBase control)
        {
            foreach (RenderablePathPoint point in PathPoints)
                point.DeselectAll(control);

            return REDRAW;
        }

        public override void Draw(GL_ControlModern control, Pass pass)
        {
            throw new NotImplementedException();
        }

        public override void Draw(GL_ControlLegacy control, Pass pass)
        {
            throw new NotImplementedException();
        }

        public override void GetSelectionBox(ref BoundingBox boundingBox)
        {
            foreach (RenderablePathPoint point in PathPoints)
                point.GetSelectionBox(ref boundingBox);
        }

        public override bool IsInRange(float range, float rangeSquared, Vector3 pos)
        {
            if (PathPoints.Count == 1)
                return (PathPoints[0].Position - pos).LengthSquared < rangeSquared;

            BoundingBox box;
            for (int i = 1; i < PathPoints.Count; i++)
            {
                box = BoundingBox.Default;
                box.Include(PathPoints[i - 1].Position);
                box.Include(PathPoints[i].Position);

                if (pos.X < box.maxX + range && pos.X > box.minX - range &&
                    pos.Y < box.maxY + range && pos.Y > box.minY - range &&
                    pos.Z < box.maxZ + range && pos.Z > box.minZ - range)
                    return true;
            }
            return false;
        }

        public override void DeleteSelected(EditorSceneBase scene, DeletionManager manager, IList list)
        {
            List<RenderablePathPoint> points = new List<RenderablePathPoint>();
            foreach (RenderablePathPoint point in PathPoints)
                if (point.IsSelected())
                    points.Add(point);

            scene.AddToUndo(new RevertableDelPointCollection(points));

            bool allPointsSelected = true;
            foreach (RenderablePathPoint point in PathPoints)
                allPointsSelected &= point.IsSelected();

            foreach (RenderablePathPoint point in PathPoints)
                BeforePointDeleted(point);

            if (allPointsSelected)
            {
                scene.InvalidateList(PathPoints);
                manager.Add(list, this);
            }
            else
            {
                foreach (RenderablePathPoint point in PathPoints)
                    point.DeleteSelected(scene, manager, PathPoints);
            }
        }

        public virtual void BeforePointDeleted(RenderablePathPoint point)
        {

        }

        public override bool IsSelectedAll()
        {
            bool all = false;

            foreach (RenderablePathPoint point in PathPoints)
                all &= point.IsSelected();

            return all;
        }

        public override bool IsSelected()
        {
            bool any = false;

            foreach (RenderablePathPoint point in PathPoints)
                any |= point.IsSelected();

            return any;
        }

        public override Vector3 GetFocusPoint()
        {
            return PathPoints[0].GlobalPosition;
        }
    }
}