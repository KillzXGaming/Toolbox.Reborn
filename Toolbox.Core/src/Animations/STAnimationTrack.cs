using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core.Animations
{
    /// <summary>
    /// Represents a animation track used to store key frames
    /// </summary>
    public class STAnimationTrack
    {
        /// <summary>
        /// The name of the track displayed in the timeline
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The interpolation type used to interpoalte key frames.
        /// </summary>
        public STInterpoaltionType InterpolationType { get; set; }

        /// <summary>
        /// A list of key frames used for the track.
        /// </summary>
        public List<STKeyFrame> KeyFrames = new List<STKeyFrame>();

        /// <summary>
        /// Determines if the track has key data used.
        /// </summary>
        public bool HasKeys => KeyFrames.Count > 0;

        public STAnimationTrack() { }

        public STAnimationTrack(STInterpoaltionType interpolation)
        {
            InterpolationType = interpolation;
        }

        /// <summary>
        /// Determines if a keyframe is keyed at the given frame value
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool IsKeyed(float frame)
        {
            var matches = KeyFrames.Where(p => p.Frame == frame);
            return matches != null && matches.Count() > 0;
        }

        /// <summary>
        /// Gets the left and right key frame given the frame value.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public STKeyFrame[] GetFrame(float frame)
        {
            if (KeyFrames.Count == 0) return null;
            STKeyFrame k1 = (STKeyFrame)KeyFrames[0], k2 = (STKeyFrame)KeyFrames[0];
            foreach (STKeyFrame k in KeyFrames)
            {
                if (k.Frame < frame)
                {
                    k1 = k;
                }
                else
                {
                    k2 = k;
                    break;
                }
            }

            return new STKeyFrame[] { k1, k2 };
        }

        //Key frame setup based on
        //https://github.com/gdkchan/SPICA/blob/42c4181e198b0fd34f0a567345ee7e75b54cb58b/SPICA/Formats/CtrH3D/Animation/H3DFloatKeyFrameGroup.cs

        /// <summary>
        /// Gets the current value at the given frame based on the interpolation the track uses and the keyframes.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="startFrame"></param>
        /// <returns></returns>
        public virtual float GetFrameValue(float frame, float startFrame = 0)
        {
            if (KeyFrames.Count == 0) return 0;
            if (KeyFrames.Count == 1) return KeyFrames[0].Value;

            STKeyFrame LK = KeyFrames.First();
            STKeyFrame RK = KeyFrames.Last();

            float Frame = frame - startFrame;

            foreach (STKeyFrame keyFrame in KeyFrames)
            {
                if (keyFrame.Frame <= Frame) LK = keyFrame;
                if (keyFrame.Frame >= Frame && keyFrame.Frame < RK.Frame) RK = keyFrame;
            }

            if (LK.Frame != RK.Frame)
            {
                float FrameDiff = Frame - LK.Frame;
                float Weight = FrameDiff / (RK.Frame - LK.Frame);

                switch (InterpolationType)
                {
                    case STInterpoaltionType.Constant: return LK.Value;
                    case STInterpoaltionType.Step: return LK.Value;
                    case STInterpoaltionType.Linear: return InterpolationHelper.Lerp(LK.Value, RK.Value, Weight);
                    case STInterpoaltionType.Bezier:
                        {
                            float length = RK.Frame - LK.Frame;
                            float inSlope = 0;
                            float outSlope = 0;
                            if (LK is STBezierKeyFrame)
                            {
                                inSlope = ((STBezierKeyFrame)LK).SlopeIn;
                                outSlope = ((STBezierKeyFrame)LK).SlopeOut;
                            }

                            return InterpolationHelper.HermiteInterpolate(frame,
                                LK.Frame, RK.Frame,
                                inSlope, outSlope,
                                LK.Value, RK.Value);
                        }
                    case STInterpoaltionType.Hermite:

                        if (LK is STHermiteKeyFrame)
                        {
                            STHermiteKeyFrame hermiteKeyLK = (STHermiteKeyFrame)LK;
                            STHermiteKeyFrame hermiteKeyRK = (STHermiteKeyFrame)RK;

                            float length = RK.Frame - LK.Frame;

                            return InterpolationHelper.HermiteInterpolate(frame,
                             hermiteKeyLK.Frame,
                             hermiteKeyRK.Frame,
                             hermiteKeyRK.TangentIn,
                             hermiteKeyLK.TangentOut,
                             hermiteKeyLK.Value,
                             hermiteKeyRK.Value);
                        }

                        return InterpolationHelper.Herp(
                            LK.Value, RK.Value,
                            LK.Slope, RK.Slope,
                            FrameDiff,
                            Weight);
                }
            }

            return LK.Value;
        }
    }
}
