using System;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using System.Collections;
using System.Collections.Generic;
using WinInut = System.Windows.Input;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework;

namespace Toolbox.Core.Rendering
{
    public class RenderablePathPoint : TransformableObject
    {
        public RenderableConnectedPath ParentPath;

        public List<RenderablePathPoint> Parents { get; private set; }
        public List<RenderablePathPoint> Children { get; private set; }

        public bool Hovered = false;

        public override bool IsSelected() => Selected;
        public override bool IsSelected(int partIndex) => Selected;

        public bool IsPointOver = false;

        public Vector4 Color = new Vector4(1f, 0f, 0f, 1f);
        public Vector3 Scale = new Vector3(2, 2, 2);

        public Vector3 Translate;

        public override Vector3 GlobalPosition
        {
            get { return base.Position; }
            set
            {
                Translate = value;
                base.Position = value;
            }
        }

        public RenderablePathPoint(RenderableConnectedPath path, Vector3 position)
            : base(position, Vector3.Zero, Vector3.One)
        {
            Translate = position;
            ParentPath = path;

            Parents = new List<RenderablePathPoint>();
            Children = new List<RenderablePathPoint>();
        }

        public RenderablePathPoint(RenderableConnectedPath path, Vector4 color, Vector3 pos,
            Vector3 rot, Vector3 sca) : base(pos, rot, sca)
        {
            Color = color;
            ParentPath = path;

            Parents = new List<RenderablePathPoint>();
            Children = new List<RenderablePathPoint>();
        }

        //Note only copy the transform properties and any other thing linked to the path
        //Do not copy children or parent data
        public virtual void CopyPropertiesToPoint(RenderablePathPoint point)
        {
            point.GlobalPosition = this.GlobalPosition;
            point.GlobalRotation = this.GlobalRotation;
            point.GlobalScale = this.GlobalScale;
        }

        public void RemoveChild(RenderablePathPoint point)
        {
            point.Parents.Remove(this);
            Children.Remove(point);
        }

        public void AddChild(RenderablePathPoint point)
        {
            point.Parents.Add(this);
            Children.Add(point);
        }

        public void SetPositionXAxis(float value)
        {
            GlobalPosition = new Vector3(value, GlobalPosition.Y, GlobalPosition.Z);
        }

        public void SetPositionYAxis(float value)
        {
            GlobalPosition = new Vector3(GlobalPosition.X, value, GlobalPosition.Z);
        }

        public void SetPositionZAxis(float value)
        {
            GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y, value);
        }

        public void SwapChildrenAndParents()
        {
            List<RenderablePathPoint> children = new List<RenderablePathPoint>();
            List<RenderablePathPoint> parents = new List<RenderablePathPoint>();
            foreach (var child in Children)
                if (child.IsSelected())
                    children.Add(child);
            foreach (var parent in Parents)
                if (parent.IsSelected())
                    parents.Add(parent);

            foreach (var parent in parents)
                parent.Children.Remove(this);
            foreach (var child in children)
                child.Parents.Remove(this);

            foreach (var child in children)
                Children.Remove(child);
            foreach (var parent in parents)
                Parents.Remove(parent);

            Parents.AddRange(children);
            Children.AddRange(parents);

            foreach (var parent in Parents)
                parent.Children.Add(this);
            foreach (var child in Children)
                child.Parents.Add(this);
        }

        public void ConnectToPoint(RenderablePathPoint point)
        {
            Console.WriteLine($"ConnectToPoint {ParentPath.PathPoints.IndexOf(point)}");

            List<RenderablePathPoint> parents = new List<RenderablePathPoint>();
            foreach (var pt in Parents)
                parents.Add(pt);

            foreach (var parent in parents)
                parent.AddChild(point);

            foreach (var parent in parents)
                parent.Children.Remove(this);

            ParentPath.RemovePoint(this);
        }

        public int[] GetChildIndices()
        {
            int[] indices = new int[Children.Count];
            for (int i = 0; i < Children.Count; i++)
                indices[i] = ParentPath.PathPoints.IndexOf(Children[i]);
            return indices;
        }

        public int[] GetParentIndices()
        {
            int[] indices = new int[Parents.Count];
            for (int i = 0; i < Parents.Count; i++)
                indices[i] = ParentPath.PathPoints.IndexOf(Parents[i]);
            return indices;
        }

        public override int GetPickableSpan()
        {
            //Disable pciking pass for selected objects that are hovering over another
            //So we can pick the hovered object instead
            if (isHoverPickPass && IsSelected())
                return 0;

            //Make sure the point can be drawn for picking
            if (ObjectRenderState.ShouldBeDrawn(this))
                return 1;
            else
                return 0;
        }

        public bool isHoverPickPass = false;

        public Matrix4 GetTransform(Pass pass, EditorSceneBase editorScene)
        {
            Matrix3 rotMtx = GlobalRotation;

            bool positionChanged = false;
            bool hovered = this.Hovered;

            var position = Position;
            var scale = Scale;
            if (editorScene.ExclusiveAction != NoAction && hovered)
            {
                position = editorScene.ExclusiveAction.NewPos(Position, out positionChanged);
                scale = editorScene.ExclusiveAction.NewScale(Scale, rotMtx);
            }
            else if (Selected && editorScene.CurrentAction != NoAction)
            {
                position = editorScene.CurrentAction.NewPos(Position, out positionChanged);
                scale = editorScene.CurrentAction.NewScale(Scale, rotMtx);
            }

            if (positionChanged && pass != Pass.PICKING)
                position = OnPositionChanged(position);

            if (position != Translate)
                Translate = position;

            if (hovered)
                scale *= 1.2f;

            return Matrix4.CreateScale(scale * BoxScale) *
                new Matrix4(Selected ? editorScene.CurrentAction.NewRot(rotMtx) : rotMtx) *
                Matrix4.CreateTranslation(position);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if ((isHoverPickPass && IsSelected()))
                return;

            if (!ObjectRenderState.ShouldBeDrawn(this))
                return;

            bool hovered = this.Hovered;

            Matrix3 rotMtx = GlobalRotation;

            bool positionChanged = false;

            var position = Position;
            var scale = Scale;
            if (editorScene.ExclusiveAction != NoAction && hovered)
            {
                position = editorScene.ExclusiveAction.NewPos(Position, out positionChanged);
                scale = editorScene.ExclusiveAction.NewScale(Scale, rotMtx);
            }
            else if (Selected && editorScene.CurrentAction != NoAction)
            {
                position = editorScene.CurrentAction.NewPos(Position, out positionChanged);
                scale = editorScene.CurrentAction.NewScale(Scale, rotMtx);
            }

            //Not necessary to update position on picking pass
            //That will only cause more slowdown
            if (positionChanged && pass != Pass.PICKING)
            {
                var newPosition = OnPositionChanged(position);
                if (newPosition != position)
                {
                    /*   if (editorScene.ExclusiveAction is TranslateAction)
                           ((TranslateAction)editorScene.ExclusiveAction).SetAxisXZ();
                       if (editorScene.CurrentAction is TranslateAction)
                           ((TranslateAction)editorScene.CurrentAction).SetAxisXZ();*/
                }
                position = newPosition;
            }

            if (position != Translate)
                Translate = position;

            if (hovered)
                scale *= 1.2f;

            if (position.Length != 0 && RenderableConnectedPath.ScaleByCamera)
                scale *= 1 + ((control.CameraTarget.Length / position.Length) * RenderableConnectedPath.CameraScaleFactor);

            control.UpdateModelMatrix(
                Matrix4.CreateScale(scale * BoxScale) *
                new Matrix4(Selected ? editorScene.CurrentAction.NewRot(rotMtx) : rotMtx) *
                Matrix4.CreateTranslation(position));

            Vector4 blockColor;
            Vector4 lineColor;

            if (hovered && Selected)
                lineColor = hoverSelectColor;
            else if (Selected)
                lineColor = selectColor;
            else if (hovered)
                lineColor = hoverColor;
            else
                lineColor = Color;

            if (hovered && Selected)
                blockColor = Color * 0.5f + hoverSelectColor * 0.5f;
            else if (Selected)
                blockColor = Color * 0.5f + selectColor * 0.5f;
            else if (hovered)
                blockColor = Color * 0.5f + hoverColor * 0.5f;
            else
                blockColor = Color;

            if (IsPointOver)
            {
                blockColor = Color * 0.5f + hoverSelectColor * 0.5f;
            }
            DrawModel(control, editorScene, pass, blockColor, lineColor);
        }

        public virtual void DrawModel(GL_ControlModern control, EditorSceneBase editorScene, Pass pass, Vector4 blockColor, Vector4 lineColor)
        {
            bool xray = RenderableConnectedPath.XRayMode;
            ColorSphereRenderer.Draw(control, pass, blockColor, lineColor, xray);
        }

        private Vector3 OnPositionChanged(Vector3 position)
        {
            foreach (var point in ParentPath.PathPoints)
            {
                if (point.IsPointOver && !point.IsSelected())
                {
                    return point.GlobalPosition;
                }
            }

            return position;
        }

        private static bool IsBoundingHit(BoundingBox bb, Vector3 position)
        {
            return bb.minX <= position.X && position.X <= bb.maxX &&
                    bb.minY <= position.Y && position.Y <= bb.maxY &&
                    bb.minZ <= position.Z && position.Z <= bb.maxZ;
        }

        public override void StartDragging(DragActionType actionType, int hoveredPart, EditorSceneBase scene)
        {
            if (Selected)
                scene.StartTransformAction(new LocalOrientation(GlobalPosition, GlobalRotation), actionType);
        }

        private Vector4 DarkenColor(Vector4 color, float amount)
        {
            return new Vector4(
                Math.Max(color.X - amount, 0),
                Math.Max(color.Y - amount, 0),
                Math.Max(color.Z - amount, 0),
                color.W);
        }

        public override void ApplyTransformActionToPart(EditorSceneBase scene, AbstractTransformAction transformAction, int part, ref TransformChangeInfos transformChangeInfos)
        {
            ApplyTransform(transformAction, ref transformChangeInfos);
        }

        public override void ApplyTransformActionToSelection(EditorSceneBase scene, AbstractTransformAction transformAction, ref TransformChangeInfos transformChangeInfos)
        {
            ApplyTransform(transformAction, ref transformChangeInfos);
        }

        private void ApplyTransform(AbstractTransformAction transformAction, ref TransformChangeInfos transformChangeInfos)
        {
            if (Selected)
            {
                Vector3 pp = Position, pr = Rotation, ps = Scale;

                Vector3 newPos = Vector3.Zero;
                bool posHasChanged = false;

                //Go through all points first to see if any point is over the applyed transform one
                ///This will move it to the original location and connect
                foreach (var point in ParentPath.PathPoints)
                {
                    if (point.IsPointOver)
                    {
                        posHasChanged = true;
                        newPos = point.GlobalPosition;
                    }
                }

                if (!posHasChanged)
                {
                    newPos = OnPositionChanged(transformAction.NewPos(GlobalPosition, out posHasChanged));
                    Console.WriteLine($"newPos {newPos}");
                }

                if (posHasChanged)
                {
                    List<RenderablePathPoint> pointsConnected = new List<RenderablePathPoint>();
                    foreach (var point in ParentPath.PathPoints)
                    {
                        if (point.IsPointOver)
                        {
                            Console.WriteLine($"pointsConnected {pointsConnected.Count}");
                            pointsConnected.Add(point);
                        }
                    }

                    foreach (var point in ParentPath.PathPoints)
                        point.IsPointOver = false;

                    foreach (var point in pointsConnected)
                        ConnectToPoint(point);
                }

                GlobalPosition = newPos;
                Matrix3 rotMtx = GlobalRotation;

                GlobalRotation = transformAction.NewRot(GlobalRotation, out bool rotHasChanged);
                GlobalScale = transformAction.NewScale(GlobalScale, rotMtx, out bool scaleHasChanged);

                transformChangeInfos.Add(this, 0,
                    posHasChanged ? new Vector3?(pp) : new Vector3?(),
                    rotHasChanged ? new Vector3?(pr) : new Vector3?(),
                    scaleHasChanged ? new Vector3?(ps) : new Vector3?());
            }
        }

        public override void DeleteSelected(EditorSceneBase scene, DeletionManager manager, IList list)
        {
            if (Selected)
                manager.Add(list, this);
        }

        public override uint SelectAll(GL_ControlBase control)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint SelectDefault(GL_ControlBase control)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint Select(int partIndex, GL_ControlBase control)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint Deselect(int partIndex, GL_ControlBase control)
        {
            Selected = false;
            return REDRAW;
        }

        public override uint DeselectAll(GL_ControlBase control)
        {
            Selected = false;
            return REDRAW;
        }

        public override bool IsSelectedAll()
        {
            return Selected;
        }

        public override void GetSelectionBox(ref BoundingBox boundingBox)
        {
            if (!Selected)
                return;

            boundingBox.Include(new BoundingBox(
                GlobalPosition.X - Scale.X,
                GlobalPosition.X + Scale.X,
                GlobalPosition.Y - Scale.Y,
                GlobalPosition.Y + Scale.Y,
                GlobalPosition.Z - Scale.Z,
                GlobalPosition.Z + Scale.Z
            ));
        }
    }
}
