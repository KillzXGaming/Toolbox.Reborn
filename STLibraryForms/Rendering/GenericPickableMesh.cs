using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GL_EditorFramework.GL_Core;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using Toolbox.Core.OpenGL;

namespace Toolbox.Core.Rendering
{
    public class GenericPickableMesh : TransformableObject
    {
        public string Name;

        public Pass Pass = Pass.OPAQUE;

        public Vector4 BoundingSphere = Vector4.Zero;
        public bool Hovered;

        public MeshRender Render { get; set; }

        public STGenericMesh Mesh { get; set; }

        private Matrix4 transform = Matrix4.Identity;
        public bool updateMatrix = false;
        public override Vector3 GlobalPosition
        {
            get { return base.GlobalPosition; }
            set
            {
                base.GlobalPosition = value;
                updateMatrix = true;
            }
        }

        public override Vector3 GlobalScale
        {
            get { return base.GlobalScale; }
            set
            {
                base.GlobalScale = value;
                updateMatrix = true;
            }
        }

        public override Matrix3 GlobalRotation
        {
            get { return base.GlobalRotation; }
            set
            {
                base.GlobalRotation = value;
                updateMatrix = true;
            }
        }

        public GenericPickableMesh(STGenericMesh mesh) : base(Vector3.Zero, Vector3.Zero, Vector3.One) {
            Mesh = mesh;

            mesh.SelectMesh += ForceSelection;
        }

        private void ForceSelection(object sender, EventArgs e) {
            Selected = true;
        }

        public override int GetPickableSpan()
        {
            return 1;
        }

        public override bool IsSelected() => Selected;
        public override bool IsSelected(int partIndex) => Selected;

        public Matrix4 GetMatrix(EditorSceneBase editorScene)
        {
            if (Selected)
            {
                bool posChanged = false;
                bool scaleChanged = false;
                bool rotChanged = false;

                editorScene.CurrentAction.NewPos(GlobalPosition, out posChanged);
                editorScene.CurrentAction.NewScale(GlobalScale, GlobalRotation, out scaleChanged);
                editorScene.CurrentAction.NewRot(GlobalRotation, out rotChanged);
                if (posChanged || scaleChanged || rotChanged)
                {
                    updateMatrix = true;
                }
                Console.WriteLine($"updateMatrix {updateMatrix} GlobalPosition {GlobalPosition}");
            }

            if (!updateMatrix)
                return transform;
            else
            {
                updateMatrix = false;
                transform = UpdateMatrix(editorScene);
                return transform;
            }
        }

        public virtual Matrix4 UpdateMatrix(EditorSceneBase editorScene)
        {
            return Matrix4.CreateScale((Selected ? editorScene.CurrentAction.NewScale(GlobalScale, GlobalRotation) : GlobalScale)) *
              new Matrix4(Selected ? editorScene.CurrentAction.NewRot(GlobalRotation) : GlobalRotation) *
              Matrix4.CreateTranslation(Selected ? editorScene.CurrentAction.NewPos(GlobalPosition) : GlobalPosition);
        }

        public override void StartDragging(DragActionType actionType, int hoveredPart, EditorSceneBase scene) {
            if (Selected)
                scene.StartTransformAction(new LocalOrientation(GlobalPosition, GlobalRotation), actionType);
        }

        public override void ApplyTransformActionToPart(EditorSceneBase scene, AbstractTransformAction transformAction, int part, ref TransformChangeInfos transformChangeInfos) {
            ApplyTransform(transformAction, ref transformChangeInfos);
        }

        public override void ApplyTransformActionToSelection(EditorSceneBase scene, AbstractTransformAction transformAction, ref TransformChangeInfos transformChangeInfos) {
            ApplyTransform(transformAction, ref transformChangeInfos);
        }

        public override void SetTransform(Vector3? pos, Vector3? rot, Vector3? scale, int part, out Vector3? prevPos, out Vector3? prevRot, out Vector3? prevScale)
        {
            updateMatrix = true;
            base.SetTransform(pos, rot, scale, part, out prevPos, out prevRot, out prevScale);
        }

        private void ApplyTransform(AbstractTransformAction transformAction, ref TransformChangeInfos transformChangeInfos)
        {
            if (!Selected)
                return;

            Vector3 pp = Position, pr = Rotation, ps = Scale;

            GlobalPosition = transformAction.NewPos(GlobalPosition, out bool posHasChanged);

            Matrix3 rotMtx = GlobalRotation;

            GlobalRotation = transformAction.NewRot(GlobalRotation, out bool rotHasChanged);
            GlobalScale = transformAction.NewScale(GlobalScale, rotMtx, out bool scaleHasChanged);

            Mesh.Transform.Position = GlobalPosition;
            Mesh.Transform.Scale = GlobalScale;
            Mesh.Transform.Rotation = GlobalRotation.ExtractRotation();

            transformChangeInfos.Add(this, 0,
                posHasChanged ? new Vector3?(pp) : new Vector3?(),
                rotHasChanged ? new Vector3?(pr) : new Vector3?(),
                scaleHasChanged ? new Vector3?(ps) : new Vector3?());
        }

        public override void DeleteSelected(EditorSceneBase scene, DeletionManager manager, IList list)
        {
            if (Selected)
                manager.Add(list, this);
        }

        public override uint SelectAll(GL_ControlBase control)
        {
            Selected = true;
            if (Mesh != null) Mesh.AferSelect();
            return REDRAW;
        }

        public override uint SelectDefault(GL_ControlBase control)
        {
            Selected = true;
            if (Mesh != null) Mesh.AferSelect();
            return REDRAW;
        }

        public override uint Select(int partIndex, GL_ControlBase control)
        {
            Selected = true;
            if (Mesh != null) Mesh.AferSelect();
            return REDRAW;
        }

        public override uint Deselect(int partIndex, GL_ControlBase control)
        {
            Selected = false;
            if (Mesh != null) Mesh.AferDeselect();
            return REDRAW;
        }

        public override uint DeselectAll(GL_ControlBase control)
        {
            Selected = false;
            if (Mesh != null) Mesh.AferDeselect();
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
