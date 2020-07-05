using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core.Animations;
using Toolbox.Core.Rendering;
using STLibrary.Forms.MapEditor;
using STLibrary;
using STLibrary.Forms;
using Toolbox.Core;
using GL_EditorFramework.Interfaces;

namespace Toolbox.Winforms
{
    public partial class Viewport : STUserControl
    {
        public class DrawContainer
        {
            public string Name { get; set; }
            public List<AbstractGlDrawable> Drawables = new List<AbstractGlDrawable>();
        }

        public List<DrawContainer> Containers = new List<DrawContainer>();

        public List<IModelFormat> Formats = new List<IModelFormat>();

        MapEditor3D Viewport3D;
        STAnimationPanel AnimationPanel;

        public EventHandler ObjectSelected;

        public Viewport()
        {
            InitializeComponent();

            Viewport3D = new MapEditor3D();
            Viewport3D.Dock = DockStyle.Fill;
            stPanel1.Controls.Add(Viewport3D);

            AnimationPanel = new STAnimationPanel();
            AnimationPanel.Dock = DockStyle.Fill;
            AnimationPanel.SetViewport(Viewport3D.gl_Control);
            stPanel3.Controls.Add(AnimationPanel);

            activeModelCB.Items.Add("All Models");
            pickingModeCB.Items.Add("Normal");
            pickingModeCB.Items.Add("Object Selection");
            pickingModeCB.Items.Add("Mesh Selection");
            pickingModeCB.SelectedIndex = 0;
        }

        public void LoadAnimationFormat(STAnimation animFormat) {
            AnimationPanel.AddAnimation(animFormat);
        }

        public void LoadModelFormat(IModelSceneFormat modelFormat)
        {

        }

        public void LoadModelFormat(IModelFormat modelFormat)
        {
            if (Formats.Contains(modelFormat)) {
                activeModelCB.SelectedItem = modelFormat.Renderer.Model.Name;
                return;
            }

            Formats.Add(modelFormat);

            var container = new ModelContainer();
            container.AddModel(modelFormat);
            Runtime.ModelContainers.Add(container);

            var drawableContainer = new DrawContainer();
            drawableContainer.Name = modelFormat.Renderer.Model.Name;
            var drawable = new GenericModelRender(modelFormat.Renderer);
            var skeleton = new GenericSkeletonRenderer(modelFormat.Renderer.Model.Skeleton);
            drawableContainer.Drawables.Add(drawable);
            drawableContainer.Drawables.Add(skeleton);
            Containers.Add(drawableContainer);

            if (Viewport3D.Scene != null)
                ReloadScene();

            activeModelCB.Items.Add(modelFormat.Renderer.Model.Name);
            activeModelCB.SelectedItem = modelFormat.Renderer.Model.Name;
        }

        public void DeselectAllObjects() {
            Viewport3D.Scene.SelectedObjects.Clear();
        }

        public void UpdateViewport() {
            Viewport3D.UpdateViewport();
        }

        public List<STGenericMesh> GetSelectedMeshes()
        {
            List<STGenericMesh> meshes = new List<STGenericMesh>();
            foreach (var obj in Viewport3D.Scene.SelectedObjects)
            {
                if (obj is GenericRenderer)
                    meshes.AddRange(((GenericRenderer)obj).GetSelectedBaseMeshes());
            }
            return meshes;
        }

        private void ObjectSelectionChanged(object sender, EventArgs e) {
            ObjectSelected?.Invoke(sender, e);
        }

        private void Viewport_Load(object sender, EventArgs e) {
            ReloadScene();
            Viewport3D.UpdateScene();
            Viewport3D.Scene.SelectionChanged += ObjectSelectionChanged;
            Viewport3D.gl_Control.ActiveCamera = new GL_EditorFramework.StandardCameras.InspectCamera();
        }

        private void ReloadScene()
        {
            foreach (var container in Containers) {
                foreach (var drawable in container.Drawables)
                    Viewport3D.AddDrawable(drawable);
            }
        }

        private void activeModelCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = activeModelCB.SelectedIndex;
            if (index == 0)
            {
                for (int i = 0; i < Containers.Count; i++)
                    ToggleModel(Containers[i], true);
            }
            else
            {
                if (!ActiveDrawHasMeshes(index - 1))
                    return;

                for (int i = 0; i < Containers.Count; i++)
                {
                    if (i == index - 1)
                        ToggleModel(Containers[i], true);
                    else
                        ToggleModel(Containers[i], false);
                }
            }
            ReloadPickingModes();

            Viewport3D.UpdateViewport();
        }

        bool ActiveDrawHasMeshes(int index)
        {
            if (index >= Containers.Count)
                return false;

            foreach (var draw in Containers[index].Drawables)
            {
                if (draw is GenericModelRender)
                {
                    var modelRenderer = ((GenericModelRender)draw).Render;
                    return modelRenderer.Meshes.Count > 0;
                }
            }

            return false;
        }

        private bool ToggleModel(DrawContainer container, bool toggle) {
            foreach (var drawable in container.Drawables) {
                if (drawable is GenericModelRender)
                {
                    var modelRenderer = ((GenericModelRender)drawable).Render;
                    modelRenderer.SkeletonRender.Visibile = toggle;

                    if (toggle) {
                        string[] values = Enum.GetNames(typeof(Runtime.DebugRender));
                        UpdateDebugModes(values.ToList());
                    }

                    //Hide automatically if there is no meshes to display to
                    if (modelRenderer.Meshes.Count == 0 && toggle)
                        toggle = false;
                }
                drawable.Visible = toggle;
            }
            return toggle;
        }

        public override void OnControlClosing() {
            foreach (var format in Formats)
            {
                var container = Runtime.ModelContainers.FirstOrDefault(x => x.ModelList.Contains(format));
                if (container != null && Runtime.ModelContainers.Contains(container))
                    Runtime.ModelContainers.Remove(container);
            }

            Viewport3D.OnControlClosing();
            Viewport3D.Dispose();

            if (AnimationPanel != null)
                AnimationPanel.OnControlClosing();
        }

        private void UpdateDebugModes(List<string> debugModes) {
            shadingToolStripMenuItem.DropDownItems.Clear();
            foreach (var mode in debugModes)
                shadingToolStripMenuItem.DropDownItems.Add(
                    new STToolStipMenuItem(mode, null, debugMode_SelectedChanged)
                    {
                        //Check first item
                        Checked = shadingToolStripMenuItem.DropDownItems.Count == 0,
                    });
        }

        private void debugMode_SelectedChanged(object sender, EventArgs e)
        {
            var menu = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem item in shadingToolStripMenuItem.DropDownItems)
                item.Checked = false;
            menu.Checked = true;

            int index = shadingToolStripMenuItem.DropDownItems.IndexOf(menu);
            Runtime.DebugRendering = (Runtime.DebugRender)index;

            shadingToolStripMenuItem.Text = $"Shading [{menu.Text}]";

            Viewport3D.UpdateViewport();
        }

        private void pickingModeCB_SelectedIndexChanged(object sender, EventArgs e) {
            ReloadPickingModes();
            Viewport3D.UpdateViewport();
        }

        private void ReloadPickingModes()
        {
            foreach (var container in Containers) {
                foreach (var drawable in container.Drawables) {
                    if (drawable is GenericModelRender)
                        SetModelPicker((GenericModelRender)drawable);
                }
            }
        }

        private void SetModelPicker(GenericModelRender modelRender)
        {
            if (!modelRender.Visible)
                return;

            if (pickingModeCB.SelectedIndex != 0)
                modelRender.CanPick = true;
            else
                modelRender.CanPick = false;

            switch (pickingModeCB.SelectedIndex)
            {
                case 0:
                case 1:
                    modelRender.PickingSelection = GenericRenderer.PickingMode.Object;
                    break;
                case 2:
                    modelRender.PickingSelection = GenericRenderer.PickingMode.Mesh;
                    break;
                case 3: //Todo picking CB does not include materials yet as it is not functional atm
                    modelRender.PickingSelection = GenericRenderer.PickingMode.Material;
                    break;
            }
        }

        private void resetPoseToolStripMenuItem_Click(object sender, EventArgs e) {
            if (AnimationPanel != null)
            {
                AnimationPanel.ResetModels();
                AnimationPanel.Frame = AnimationPanel.StartFrame;
                AnimationPanel.AnimationPlayerState = AnimPlayerState.Stop;
            }
            UpdateViewport();
        }

        private void toOriginToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Viewport3D == null) return;

            Viewport3D.gl_Control.ResetCamera(false);
            UpdateViewport();
        }
    }
}
