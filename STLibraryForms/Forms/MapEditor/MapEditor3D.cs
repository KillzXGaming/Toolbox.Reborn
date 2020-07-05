using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.StandardCameras;
using Toolbox.Core;
using Toolbox.Core.Rendering;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace STLibrary.Forms.MapEditor
{
    public class MapEditor3D : STUserControl
    {
        public EventHandler PointAdded;

        public EventHandler ObjectMoved;

        public GL_ControlBase gl_Control;
        public EditorScene Scene;

        public STAnimationPanel TimeLine;
        private bool loaded = false;

        public MapEditor3D()
        {
            if (Runtime.UseLegacyGL)
                gl_Control = new GL_ControlLegacy();
            else
                gl_Control = new GL_ControlModern();

            gl_Control.Dock = System.Windows.Forms.DockStyle.Fill;
            gl_Control.MouseClick += OnMouseClick;
            gl_Control.MouseMove += gl_Control_MouseMove;
            gl_Control.KeyDown += gl_Control_KeyDown;
            gl_Control.DragEnter += gl_Control1_DragEnter;
            Controls.Add(gl_Control);
        }

        public override void OnControlClosing()
        {
            Scene.objects.Clear();
            Scene.StaticObjects.Clear();
            gl_Control.MainDrawable = null;
            gl_Control.Dispose();
            base.OnControlClosing();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Scene = new EditorScene();
            Scene.RenderDistance = 10000000;
            Scene.ObjectsMoved += OnObjectsMoved;
            LoadBaseDrawables();

            WalkaroundCamera.CameraSpeed = 40;
            GL_EditorFramework.EditorDrawables.Path.CubeScale = 20;
            GL_EditorFramework.EditorDrawables.Path.ControlCubeScale = 15;
            gl_Control.CameraDistance = 30;
            gl_Control.AllowDrop = true;

            if (gl_Control is GL_ControlModern)
                GL_EditorFramework.Renderers.ColorBlockRenderer.Initialize((GL_ControlModern)gl_Control);

            loaded = true;
        }

        public void AddDrawable(AbstractGlDrawable drawable)
        {
            if (drawable is IEditableObject)
                Scene.objects.Add((IEditableObject)drawable);
            else
                Scene.StaticObjects.Add(drawable);

            if (gl_Control.MainDrawable != null)
                drawable.Prepare((GL_ControlModern)gl_Control);
        }

        public void TransformAction(IEditableObject drawable)
        {

        }

        public void UpdateScene()
        {
            gl_Control.MainDrawable = Scene;
        }

        public void UpdateViewport()
        {
            gl_Control.Invalidate();
        }

        //This is used to update the intro camera path
        //Paths can use control points, however intro cameras 
        //those are always at the same point (basically unused)
        //Only point and lookat target is necessary for these
        public void UpdateCamera(Vector3 point, Vector3 target,
            int fov = 0, int fov2 = 0, int fovSpeed = 1)
        {
            gl_Control.CameraLookat = target;
            gl_Control.CameraTarget = point;
            if (fov != 0)
                gl_Control.Fov = MathHelper.DegreesToRadians(fov);
        }

        public void UpdateCamera(Vector3 point,
            float roll, float pitch, float yaw,
            int fov = 0, int fov2 = 0, int fovSpeed = 1)
        {
            gl_Control.SetRotation(roll, pitch, yaw);
            gl_Control.CameraTarget = point;
            if (fov != 0)
                gl_Control.Fov = MathHelper.DegreesToRadians(fov);
        }

        public void UpdateObject(AbstractGlDrawable mapObject)
        {
            if (gl_Control is GL_ControlModern)
                mapObject.Prepare((GL_ControlModern)gl_Control);
            else
                mapObject.Prepare((GL_ControlLegacy)gl_Control);
        }


        private void OnObjectsMoved(object sender, EventArgs e)
        {
            ObjectMoved?.Invoke(sender, e);
        }

        private void OnMouseClick(object sender, EventArgs e)
        {
            if (TimeLine == null)
                return;

            //Reset lookat
            if (!TimeLine.IsPlaying && gl_Control.CameraLookat != Vector3.Zero)
                gl_Control.CameraLookat = Vector3.Zero;
        }


        private void gl_Control1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void gl_Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (gl_Control.SelectionTool != null)
                gl_Control.Invalidate();

            if (Scene != null && Scene.SelectedObjects.Count == 1)
            {
                //Check for selected paths
                var connectedPath = Scene.SelectedObjects.FirstOrDefault() as RenderableConnectedPath;
                if (connectedPath != null)
                    connectedPath.ConnectToHoveredPoints(Scene, Control.ModifierKeys.HasFlag(Keys.Alt));
            }
        }

        private void gl_Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D && Control.ModifierKeys.HasFlag(Keys.Control))
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                        ((RenderableConnectedPath)obj).CopyPointsFromSelected(Scene);
                }
                gl_Control.Invalidate();
            }
            if (e.KeyCode == Keys.M)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                        ((RenderableConnectedPath)obj).MergeSelectedPoints(Scene);
                }
                gl_Control.Invalidate();
            }
            if (e.KeyCode == Keys.F)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                        ((RenderableConnectedPath)obj).FillSelectedPoints(Scene);
                }
                gl_Control.Invalidate();
            }
            if (e.KeyCode == Keys.Delete)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                        ((RenderableConnectedPath)obj).RemoveSelected(Scene);
                }
                gl_Control.Invalidate();
            }
            if (e.KeyCode == Keys.S)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                    {
                        ((RenderableConnectedPath)obj).Subdivide(Scene);
                        gl_Control.Invalidate();
                    }
                }
            }
            if (e.KeyCode == Keys.R)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                    {
                        ((RenderableConnectedPath)obj).RedirectSelected(Scene);
                        gl_Control.Invalidate();
                    }
                }
            }
            if (e.KeyCode == Keys.E)
            {
                foreach (var obj in Scene.SelectedObjects)
                {
                    if (obj is RenderableConnectedPath)
                        ((RenderableConnectedPath)obj).ExtrudePointsFromSelected(Scene);
                }
            }
        }

        private void LoadBaseDrawables()
        {
            Runtime.OpenTKInitialized = true;

            var floor = new DrawableFloor();
            var xyzLnes = new DrawableXyzLines();
            var skybox = new DrawableSkybox();
            var background = new DrawableBackground();

            Scene.StaticObjects.Add(floor);
            Scene.StaticObjects.Add(background);
            Scene.StaticObjects.Add(xyzLnes);

            return;

            Scene.StaticObjects.Add(skybox);
        }

        private class CameraFrame
        {
            public Vector3 CameraTarget;

            public float CameraDistance;
            public float CamRotX;
            public float CamRotY;
            public float Fov = 0.5f;
            public float zNear = 0.01f;
            public float zFar = 1000000;
        }

        private CameraFrame perspectiveFrame = new CameraFrame();
        private CameraFrame orthoFrame = new CameraFrame();

        public void ToggleOrthographic(bool toggle)
        {
            if (toggle)
            {
                perspectiveFrame.CameraDistance = gl_Control.CameraDistance;
                perspectiveFrame.CameraTarget = gl_Control.CameraTarget;
                perspectiveFrame.CamRotX = gl_Control.CamRotX;
                perspectiveFrame.CamRotY = gl_Control.CamRotY;
                perspectiveFrame.Fov = gl_Control.Fov;
                perspectiveFrame.zNear = gl_Control.ZNear;
                perspectiveFrame.zFar = gl_Control.ZFar;

                gl_Control.OrthographicFactor = 0.5f;
                gl_Control.LockRotationX = true;
                gl_Control.LockRotationY = true;

                gl_Control.OrthographicCamera = true;

                orthoFrame.zNear = -100000;
                orthoFrame.zFar = 100000;
                orthoFrame.CameraTarget = new Vector3(3.964717f, 14.44976f, 5.03942f);
                orthoFrame.CamRotX = 0;
                orthoFrame.CamRotY = GL_EditorFramework.Framework.HALF_PI;
                ApplyCameraFrame(orthoFrame);
            }
            else
            {
                gl_Control.LockRotationX = false;
                gl_Control.LockRotationY = false;
                orthoFrame.CameraDistance = gl_Control.CameraDistance;
                orthoFrame.CameraTarget = gl_Control.CameraTarget;
                orthoFrame.CamRotX = gl_Control.CamRotX;
                orthoFrame.CamRotY = gl_Control.CamRotY;
                orthoFrame.Fov = gl_Control.Fov;
                orthoFrame.zNear = gl_Control.ZNear;
                orthoFrame.zFar = gl_Control.ZFar;

                ApplyCameraFrame(perspectiveFrame);

                gl_Control.OrthographicCamera = false;
                gl_Control.UpdateProjectionMatrix(gl_Control.Width / (float)gl_Control.Height);
            }

            UpdateViewport();
        }

        private void ApplyCameraFrame(CameraFrame frame)
        {
            gl_Control.CameraDistance = frame.CameraDistance;
            gl_Control.CameraTarget = frame.CameraTarget;
            gl_Control.CamRotX = frame.CamRotX;
            gl_Control.CamRotY = frame.CamRotY;
            gl_Control.Fov = frame.Fov;
            gl_Control.ZNear = frame.zNear;
            gl_Control.ZFar = frame.zFar;
        }
    }
}
