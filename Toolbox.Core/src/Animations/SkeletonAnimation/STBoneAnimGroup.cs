using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.Animations
{
    public class STBoneAnimGroup : STAnimGroup
    {
        public STAnimationTrack TranslateX { get; set; }
        public STAnimationTrack TranslateY { get; set; }
        public STAnimationTrack TranslateZ { get; set; }

        public STAnimationTrack RotateX { get; set; }
        public STAnimationTrack RotateY { get; set; }
        public STAnimationTrack RotateZ { get; set; }

        //Used if the rotation mode is set to quat
        public STAnimationTrack RotateW { get; set; }

        public STAnimationTrack ScaleX { get; set; }
        public STAnimationTrack ScaleY { get; set; }
        public STAnimationTrack ScaleZ { get; set; }

        public bool UseQuaternion = false;

        public STBoneAnimGroup()
        {
            TranslateX = new STAnimationTrack();
            TranslateY = new STAnimationTrack();
            TranslateZ = new STAnimationTrack();
            RotateX = new STAnimationTrack();
            RotateY = new STAnimationTrack();
            RotateZ = new STAnimationTrack();
            RotateW = new STAnimationTrack();
            ScaleX = new STAnimationTrack();
            ScaleY = new STAnimationTrack();
            ScaleZ = new STAnimationTrack();   
        }

        public override List<STAnimationTrack> GetTracks()
        {
            List<STAnimationTrack> tracks = new List<STAnimationTrack>();
            tracks.Add(TranslateX);
            tracks.Add(TranslateY);
            tracks.Add(TranslateZ);
            tracks.Add(RotateX);
            tracks.Add(RotateY);
            tracks.Add(RotateZ);
            tracks.Add(RotateW);
            tracks.Add(ScaleX);
            tracks.Add(ScaleY);
            tracks.Add(ScaleZ);
            return tracks;
        }
    }
}
