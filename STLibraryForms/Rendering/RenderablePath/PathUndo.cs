using System;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using System.Collections.Generic;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework;

namespace Toolbox.Core.Rendering
{
    public class RevertableDelPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableDelPointCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
            {
                foreach (var parent in point.Parents)
                    Points.Add(new PointInfo(parent));
                foreach (var child in point.Children)
                    Points.Add(new PointInfo(child));

                Points.Add(new PointInfo(point));
            }
        }

        public IRevertable Revert(EditorSceneBase scene)
        {
            var infos = new List<RenderablePathPoint>();
            foreach (var info in Points)
            {
                info.Point.Parents.Clear();
                info.Point.Children.Clear();

                foreach (var parent in info.Parents)
                    info.Point.Parents.Add(parent);
                foreach (var child in info.Children)
                    info.Point.Children.Add(child);

                if (!info.ParentPath.PathPoints.Contains(info.Point))
                    info.ParentPath.AddPoint(info.Point);
                infos.Add(info.Point);
            }

            return new RevertableDelPointCollection(infos);
        }

        public class PointInfo
        {
            public RenderablePathPoint Point;
            public RenderableConnectedPath ParentPath;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public PointInfo(RenderablePathPoint point)
            {
                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }
        }
    }

    public class RevertableAddPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableAddPointCollection(RenderablePathPoint point)
        {
            Points.Add(new PointInfo(point));
        }

        public RevertableAddPointCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
                Points.Add(new PointInfo(point));
        }

        public IRevertable Revert(EditorSceneBase scene)
        {
            var infos = new List<RenderablePathPoint>();
            foreach (var info in Points)
            {
                info.ParentPath.RemovePoint(info.Point);
                infos.Add(info.Point);
            }

            return new RevertableDelPointCollection(infos);
        }

        public class PointInfo
        {
            public RenderablePathPoint Point;
            public RenderableConnectedPath ParentPath;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public PointInfo(RenderablePathPoint point)
            {
                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }
        }
    }
}
