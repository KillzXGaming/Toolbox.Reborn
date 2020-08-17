using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using Toolbox.Core;

namespace Toolbox.Core.Rendering
{
    public class GenericRenderer : TransformableObject
    {
        public new static Vector4 selectColor = new Vector4(EditableObject.selectColor.Xyz, 0.5f);
        public new static Vector4 hoverSelectColor = new Vector4(EditableObject.hoverSelectColor.Xyz, 0.5f);
        public new static Vector4 hoverColor = new Vector4(EditableObject.hoverColor.Xyz, 0.125f);

        //Note this is necessary to adjust if meshes are animated by shaders
        //For animated meshes use the normal vertex shader, then a picking color fragment shader
        //This is only for static meshes
        public virtual ShaderProgram PickingShader => Renderers.ColorBlockRenderer.SolidColorShaderProgram;

        public virtual List<GenericPickableMesh> PickableMeshes { get; set; } = new List<GenericPickableMesh>();

        [PropertyCapture.Undoable]
        public Vector3 ObjectTransform { get; set; }

        private PickingMode pickingSelection = PickingMode.Mesh;
        public PickingMode PickingSelection
        {
            get { return pickingSelection; }
            set
            {
                pickingSelection = value;
                DeselectAll(null);
            }
        }

        public override Vector3 GlobalPosition
        {
            get { return Position; }
            set
            {
                Position = value;
                DisplayTranslation = value;
            }
        }

        public override Vector3 GlobalScale
        {
            get { return Scale; }
            set
            {
                Scale = value;
                DisplayScale = value;
            }
        }

        public enum PickingMode
        {
            Object, //Selects entire model
            MeshGroup, //Selects a group of meshes
            Mesh, //Selects per mesh
            Material, //Selects per material
        }

        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayRotation { get; set; }
        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayTranslation { get; set; }
        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayScale { get; set; } = new Vector3(1, 1, 1);

        public virtual bool CanPick { get; set; } = true;

        public virtual void FrameCamera(GL_ControlBase control) { }

        public string Name { get; set; }

        public ObjAnimController AnimController = null;

        public void ResetAnim()
        {
            AnimController = null;
        }

        public GenericRenderer(Vector3 position, Vector3 rotationEuler, Vector3 scale) :
            base(position, rotationEuler, scale)
        {
        }

        public GenericRenderer() :
            base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
        }

        public List<STGenericMesh> GetSelectedBaseMeshes()
        {
            List<STGenericMesh> meshes = new List<STGenericMesh>();
            for (int i = 0; i < PickableMeshes.Count; i++)
            {
                if (PickableMeshes[i].IsSelected())
                    meshes.Add(PickableMeshes[i].Mesh);
            }
            return meshes;
        }

        public List<GenericPickableMesh> GetSelectedMeshes()
        {
            List<GenericPickableMesh> meshes = new List<GenericPickableMesh>();
            for (int i = 0; i < PickableMeshes.Count; i++) {
                if (PickableMeshes[i].IsSelected())
                    meshes.Add(PickableMeshes[i]);
            }
            return meshes;
        }

        public override void Prepare(GL_ControlModern control)
        {
            if (Renderers.ColorBlockRenderer.SolidColorShaderProgram != null)
                Renderers.ColorBlockRenderer.Initialize(control);
            else
                Renderers.ColorBlockRenderer.SolidColorShaderProgram.Link(control);

            base.Prepare(control);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (!Runtime.RenderModels || PickableMeshes.Count == 0)
                return;

            bool hovered = editorScene.Hovered == this;

            if (!CanPick)
            {
                Selected = false;
                hovered = false;
                if (pass == Pass.PICKING)
                    return;
            }

            Matrix3 rotMtx = GlobalRotation;

            Vector4 highlightColor = Vector4.Zero;
            if (IsSelected() && hovered)
                highlightColor = hoverSelectColor;
            else if (IsSelected())
                highlightColor = selectColor;
            else if (hovered)
                highlightColor = hoverColor;

            bool positionChanged = false;
            bool scaleChanged = false;
            bool rotationChanged = false;

            //Set transformations based on the drag action and output changes
            Vector3 position = !Selected ? GlobalPosition :
              editorScene.CurrentAction.NewPos(GlobalPosition, out positionChanged);
            rotMtx = !Selected ? rotMtx :
               editorScene.CurrentAction.NewRot(rotMtx, out rotationChanged);
            Vector3 scale = !Selected ? GlobalScale :
              editorScene.CurrentAction.NewScale(GlobalScale, rotMtx, out scaleChanged);

            //Auto calculate Y axis
            if (positionChanged && editorScene.CurrentAction is TranslateAction)
            {
                var newPosition = OnPositionChanged(position);
                //Limit axis to move on the X and Z axis
                if (newPosition != position)
                    ((TranslateAction)editorScene.CurrentAction).SetAxisXZ();
                position = newPosition;
            }

            //Method to automatically update values when either value is changed
            if (positionChanged || scaleChanged || rotationChanged)
                OnTransformChanged(position, scale, rotMtx.ExtractDegreeEulerAngles());

            //Transform with anim controller if present, else use the normal transform
            if (AnimController != null)
            {
                control.UpdateModelMatrix(
              Matrix4.CreateScale(AnimController.Scale) *
                 (Matrix4.CreateRotationX(AnimController.Rotate.X) *
                  Matrix4.CreateRotationX(AnimController.Rotate.Y) *
                  Matrix4.CreateRotationX(AnimController.Rotate.Z)) *
                  Matrix4.CreateTranslation(AnimController.Translate));
            }
            else
            {
                control.UpdateModelMatrix(
                    Matrix4.CreateScale(scale) *
                    new Matrix4(rotMtx) *
                    Matrix4.CreateTranslation(position));
            }

            DrawModel(control, editorScene, pass, highlightColor);

            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        public virtual void OnTransformChanged(Vector3 position, Vector3 scale, Vector3 rotation)
        {

        }

        private Vector3 OnPositionChanged(Vector3 position)
        {
            return position;
        }

        public virtual void DrawModel(GL_ControlModern control, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
            control.CurrentShader = PickingShader;
            DrawModels(control, control.CurrentShader, editorScene, pass, highlightColor);
        }

        public virtual void OnRender(GL_ControlModern control, ShaderProgram shaderProgram, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {

        }

        public virtual void DrawModels(GL_ControlModern control, ShaderProgram shaderProgram, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
            if (!Runtime.RenderModels) return;

            if (pass == Pass.PICKING && PickingSelection == PickingMode.Object)
            {
                SetPickingShader(control);
                control.CurrentShader.SetVector4("color", control.NextPickingColor());
            }
            else
            {
                if (pass == Pass.TRANSPARENT)
                {
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.Blend);
                }

                control.CurrentShader = shaderProgram;
                control.CurrentShader.SetVector4("highlight_color", highlightColor);
                GL.ActiveTexture(TextureUnit.Texture0);
            }   
            OnRender(control, shaderProgram, editorScene, pass, highlightColor);

            if (pass == Pass.OPAQUE && highlightColor.W != 0)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.Clear(ClearBufferMask.StencilBufferBit);
                GL.ClearStencil(0);
                GL.StencilFunc(StencilFunction.Always, 0x1, 0x1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
            }

            bool hovered = editorScene.Hovered == this;

            //    Matrix4 modelMatrix = control.ModelMatrix;
            Matrix4 modelMatrix = Matrix4.Identity;

            int part = 0;
            foreach (var mesh in PickableMeshes)
            {
                bool canDraw = CanDrawMesh(mesh);
                if (!canDraw)
                    continue;

                if (PickingSelection == PickingMode.Mesh)
                {
                    if (mesh.GetPickableSpan() == 0)
                        continue;

                    mesh.Hovered = hovered && (editorScene.HoveredPart == part);
                    part++;
                }

                bool MeshSelected = PickingSelection == PickingMode.Object;
                if (PickingSelection == PickingMode.Mesh && mesh.IsSelected() ||
                    PickingSelection == PickingMode.Mesh && mesh.Hovered)
                    MeshSelected = true;

                if (PickingSelection == PickingMode.Mesh && pass == Pass.PICKING)
                {
                    SetPickingShader(control);
                    control.CurrentShader.SetVector4("color", control.NextPickingColor());
                }

                Vector4 meshHighlightColor = Vector4.Zero;
                if (mesh.IsSelected())
                    meshHighlightColor = selectColor;
                else if (mesh.Hovered)
                    meshHighlightColor = hoverColor;
                else if (PickingSelection != PickingMode.Mesh)
                    meshHighlightColor = highlightColor;

                if (pass == Pass.PICKING)
                    control.CurrentShader.SetVector4("highlight_color", meshHighlightColor);

                RenderMaterial(control, pass, mesh, shaderProgram, meshHighlightColor);

                UpdateMeshMatrix(control, editorScene, modelMatrix, mesh);
                if (pass == mesh.Pass || pass == Pass.PICKING)
                    DrawMesh(control, mesh);
                else if (PickingSelection == PickingMode.Mesh && MeshSelected)
                {
                    if (pass == Pass.OPAQUE && highlightColor.W != 0)
                    {
                        GL.ColorMask(false, false, false, false);
                        GL.DepthMask(false);
                        DrawMesh(control, mesh);
                        GL.ColorMask(true, true, true, true);
                        GL.DepthMask(true);
                    }
                }
                else if (pass == Pass.OPAQUE && highlightColor.W != 0)
                {
                    GL.ColorMask(false, false, false, false);
                    GL.DepthMask(false);
                    DrawMesh(control, mesh);
                    GL.ColorMask(true, true, true, true);
                    GL.DepthMask(true);
                }
            }

            GL.Disable(EnableCap.Blend);
            if (pass == Pass.OPAQUE && highlightColor.W != 0)
            {
                SetPickingShader(control);
                control.CurrentShader.SetVector4("color", new Vector4(highlightColor.Xyz, 1));

                GL.LineWidth(3.0f);
                GL.StencilFunc(StencilFunction.Equal, 0x0, 0x1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

                //GL.DepthFunc(DepthFunction.Always);

                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

                foreach (var mesh in PickableMeshes)
                {
                    if (PickingSelection == PickingMode.Mesh && !mesh.IsSelected() && !mesh.Hovered)
                        continue;

                    UpdateMeshMatrix(control, editorScene, modelMatrix, mesh);
                    DrawMesh(control, mesh);
                }

                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

                //GL.DepthFunc(DepthFunction.Lequal);

                GL.Disable(EnableCap.StencilTest);
                GL.LineWidth(2);
            }
            GL.UseProgram(0);
        }

        public virtual void UpdateMeshMatrix(GL_ControlModern control,
            EditorSceneBase editorScene, Matrix4 modelMatrix, GenericPickableMesh mesh)
        {

        }

        public virtual void SetPickingShader(GL_ControlModern control)
        {
            control.CurrentShader = PickingShader;
            ReloadUniforms(control, PickingShader);
        }

        public virtual void ReloadUniforms(GL_ControlBase control, ShaderProgram shader)
        {

        }

        public virtual void RenderMaterial(GL_ControlBase control, Pass pass, GenericPickableMesh mesh, ShaderProgram shader, Vector4 highlight_color)
        {

        }

        public virtual bool CanDrawMesh(GenericPickableMesh mesh)
        {
            return true;
        }

        public virtual void DrawMesh(GL_ControlBase control, GenericPickableMesh mesh)
        {
        }

        public override void ApplyTransformActionToSelection(EditorSceneBase scene, AbstractTransformAction transformAction, ref TransformChangeInfos infos)
        { 
            if (PickingSelection == PickingMode.Mesh)
            {
                foreach (GenericPickableMesh point in PickableMeshes)
                    point.ApplyTransformActionToSelection(scene, transformAction, ref infos);
            }
            else if (Selected)
            {
                Vector3 pp = Position, pr = Rotation, ps = Scale;

                GlobalPosition = OnPositionChanged(transformAction.NewPos(GlobalPosition, out bool posHasChanged));

                Matrix3 rotMtx = GlobalRotation;

                GlobalRotation = transformAction.NewRot(GlobalRotation, out bool rotHasChanged);
                GlobalScale = transformAction.NewScale(GlobalScale, rotMtx, out bool scaleHasChanged);

                infos.Add(this, 0,
                    posHasChanged ? new Vector3?(pp) : new Vector3?(),
                    rotHasChanged ? new Vector3?(pr) : new Vector3?(),
                    scaleHasChanged ? new Vector3?(ps) : new Vector3?());
            }
        }

        public override int GetPickableSpan()
        {
            if (!Visible || !CanPick)
                return 0;

            if (PickingSelection == PickingMode.Mesh)
            {
                // if (!ObjectRenderState.ShouldBeDrawn(this) || !Visible)
                //    return 0;

                int picking = 0;
                for (int i = 0; i < PickableMeshes.Count; i++)
                    picking += PickableMeshes[i].GetPickableSpan();
                return picking;
            }
            return 1;
        }

        public override uint SelectDefault(GL_ControlBase control)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                for (int i = 0; i < PickableMeshes.Count; i++)
                    PickableMeshes[i].SelectDefault(control);
            }

            return base.SelectDefault(control);
        }

        public override bool IsSelected(int partIndex)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                for (int i = 0; i < PickableMeshes.Count; i++)
                {
                    int span = PickableMeshes[i].GetPickableSpan();
                    if (partIndex >= 0 && partIndex < span)
                    {
                        if (PickableMeshes[i].IsSelected(partIndex))
                            return true;
                    }
                    partIndex -= span;
                }
            }
            return base.IsSelected(partIndex);
        }

        public override void StartDragging(DragActionType actionType, int hoveredPart, EditorSceneBase scene)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                foreach (GenericPickableMesh mesh in PickableMeshes)
                {
                    int span = mesh.GetPickableSpan();
                    if (hoveredPart >= 0 && hoveredPart < span || mesh.IsSelected())
                    {
                        mesh.StartDragging(actionType, hoveredPart, scene);
                    }
                    hoveredPart -= span;
                }
            }
            else
                base.StartDragging(actionType, hoveredPart, scene);
        }

        public override void SetTransform(Vector3? pos, Vector3? rot, Vector3? scale, int _part, out Vector3? prevPos, out Vector3? prevRot, out Vector3? prevScale)
        {
            prevPos = null;
            prevRot = null;
            prevScale = null;

            if (PickingSelection == PickingMode.Mesh)
            {
                foreach (GenericPickableMesh mesh in PickableMeshes)
                {
                    int span = mesh.GetPickableSpan();
                    if (_part >= 0 && _part < span)
                    {
                        mesh.SetTransform(pos, rot, scale, _part, out prevPos, out prevRot, out prevScale);
                        return;
                    }
                    _part -= span;
                }
            }
            else
                base.SetTransform(pos, rot, scale, _part, out prevPos, out prevRot, out prevScale);
        }

        public override void ApplyTransformActionToPart(EditorSceneBase scene, AbstractTransformAction transformAction, int _part, ref TransformChangeInfos transformChangeInfos)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                foreach (GenericPickableMesh mesh in PickableMeshes)
                {
                    int span = mesh.GetPickableSpan();
                    if (_part >= 0 && _part < span)
                    {
                        mesh.ApplyTransformActionToPart(scene, transformAction, _part, ref transformChangeInfos);
                        return;
                    }
                    _part -= span;
                }
            }
        }

        public override bool IsSelectedAll()
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                bool all = false;

                for (int i = 0; i < PickableMeshes.Count; i++)
                    all &= PickableMeshes[i].IsSelected();

                return all;
            }
            return base.IsSelectedAll();
        }

        public override bool IsSelected()
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                bool any = false;

                for (int i = 0; i < PickableMeshes.Count; i++)
                    any |= PickableMeshes[i].IsSelected();

                return any;
            }
            return base.IsSelected();
        }

        public override uint Select(int partIndex, GL_ControlBase control)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                for (int i = 0; i < PickableMeshes.Count; i++)
                {
                    int span = PickableMeshes[i].GetPickableSpan();
                    if (partIndex >= 0 && partIndex < span)
                    {
                        PickableMeshes[i].Select(partIndex, control);
                    }
                    partIndex -= span;
                }

                return REDRAW;
            }
            return base.Select(partIndex, control);
        }

        public override uint Deselect(int partIndex, GL_ControlBase control)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                bool noPointsSelected = true;
                for (int i = 0; i < PickableMeshes.Count; i++)
                {
                    int span = PickableMeshes[i].GetPickableSpan();
                    if (partIndex >= 0 && partIndex < span)
                    {
                        PickableMeshes[i].Deselect(partIndex, control);
                    }
                    partIndex -= span;
                    noPointsSelected &= !PickableMeshes[i].IsSelected();
                }
                return REDRAW;
            }
            if (PickableMeshes != null)
            {
                //Force deselection on meshes if previously selected
                for (int i = 0; i < PickableMeshes.Count; i++)
                    PickableMeshes[i].DeselectAll(control);
            }

            return base.Deselect(partIndex, control);
        }

        public override uint SelectAll(GL_ControlBase control)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                for (int i = 0; i < PickableMeshes.Count; i++)
                    PickableMeshes[i].SelectAll(control);

                return REDRAW;
            }
            return base.SelectAll(control);
        }

        public override uint DeselectAll(GL_ControlBase control)
        {
            for (int i = 0; i < PickableMeshes.Count; i++)
                PickableMeshes[i].DeselectAll(control);

            return base.DeselectAll(control);
        }

        public override void GetSelectionBox(ref BoundingBox boundingBox)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                for (int i = 0; i < PickableMeshes.Count; i++)
                    PickableMeshes[i].GetSelectionBox(ref boundingBox);
            }
            else
                base.GetSelectionBox(ref boundingBox);
        }

        public override bool IsInRange(float range, float rangeSquared, Vector3 pos)
        {
            if (PickingSelection == PickingMode.Mesh)
            {
                if (PickableMeshes.Count == 1)
                    return (PickableMeshes[0].Position - pos).LengthSquared < rangeSquared;

                BoundingBox box;
                for (int i = 1; i < PickableMeshes.Count; i++)
                {
                    box = BoundingBox.Default;
                    box.Include(PickableMeshes[i - 1].Position);
                    box.Include(PickableMeshes[i].Position);

                    if (pos.X < box.maxX + range && pos.X > box.minX - range &&
                        pos.Y < box.maxY + range && pos.Y > box.minY - range &&
                        pos.Z < box.maxZ + range && pos.Z > box.minZ - range)
                        return true;
                }
                return false;
            }
            return base.IsInRange(range, rangeSquared, pos);
        }
    }
}
