using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.GX;

namespace GCNLibrary.Pikmin1.Model
{
    public class MODMaterial : STGenericMaterial
    {
        public MODMaterial(MOD_Parser parser, Material mat)
        {
            this.DiffuseColor = mat.DiffuseColor;

            for (int i = 0; i < mat.TextureAttributeIndices.Length; i++) {
                short index = mat.TextureAttributeIndices[i];
                if (index != -1)
                {
                    var attribute = parser.TextureAttributes[index];
                    TextureMaps.Add(new STGenericTextureMap()
                    {
                        WrapU = ConvertWrapMode(attribute.WrapS),
                        WrapV = ConvertWrapMode(attribute.WrapT),
                        Name = $"Texture{attribute.TextureIndex}",
                        Type = STTextureType.Diffuse,
                    });
                }
            }
        }

        private static STTextureWrapMode ConvertWrapMode(byte value)
        {
            switch (value)
            {
                case 0:
                    return STTextureWrapMode.Clamp;
                case 1:
                    return STTextureWrapMode.Repeat;
                case 2:
                    return STTextureWrapMode.Mirror;
                default:
                    return STTextureWrapMode.Repeat;
            }
        }
    }
}
