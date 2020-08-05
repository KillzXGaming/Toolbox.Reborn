using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrawlLib.Modeling.Triangle_Converter
{
    //From https://github.com/Sage-of-Mirrors/SuperBMD/blob/76d94508ea402e239139ac4707f5abf90b4efe37/SuperBMDLib/source/Rigging/Weight.cs
    public class Weight
    {
        public int WeightCount { get; private set; }
        public List<float> Weights { get; private set; }
        public List<int> BoneIndices { get; private set; }

        public Weight() {
            Weights = new List<float>();
            BoneIndices = new List<int>();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Weight))
                return false;

            WeightEqualityComparer comp = new WeightEqualityComparer();
            return comp.Equals(this, obj as Weight);
        }

        public override int GetHashCode()
        {
            WeightEqualityComparer comp = new WeightEqualityComparer();
            return comp.GetHashCode(this);
        }

        public void AddWeight(float weight, int boneIndex)
        {
            Weights.Add(weight);
            BoneIndices.Add(boneIndex);
            WeightCount++;
        }

        // Reorder the weights by their bone indices.
        // This is so that two weights that are identical except for having the bones in a different order are properly considered duplicates.
        public void ReorderBones()
        {
            if (WeightCount < 2) return;

            // Use Array.Sort to simultaneously sort the bone indices and the weights by the same order as the bone indices.
            var weightsArray = Weights.ToArray();
            var boneIndicesArray = BoneIndices.ToArray();
            Array.Sort(boneIndicesArray, weightsArray);

            Weights = weightsArray.ToList();
            BoneIndices = boneIndicesArray.ToList();
        }
    }

    class WeightEqualityComparer : IEqualityComparer<Weight>
    {
        public bool Equals(Weight x, Weight y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x.WeightCount != y.WeightCount)
                return false;

            for (int i = 0; i < x.WeightCount; i++)
            {
                if (x.BoneIndices[i] != y.BoneIndices[i] || x.Weights[i] != y.Weights[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(Weight obj)
        {
            int hash = 0;

            for (int i = 0; i < obj.WeightCount; i++)
            {
                float weightHash = (obj.Weights[i] * 10) * obj.BoneIndices[i];
                hash ^= (int)weightHash;
            }

            return hash;
        }
    }
}
