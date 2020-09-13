using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.OpenGL;
using OpenTK;

namespace GCNLibrary.LM.BIN
{
    public class BIN_Render : ModelRenderer
    {
        public BIN_Render(STGenericModel model) : base(model)
        {

        }

        public override void PrepareShaders()
        {
            if (ActiveShader != null)
                return;

            ActiveShader = new ShaderProgram(
                new VertexShader(VertexShaderBasic),
                new FragmentShader(FragmentShaderBasic));

            PrepareDebugShaders();
        }

        private static string FragmentShaderBasic = @"
            #version 330

            uniform vec4 highlight_color;

            //Samplers
            uniform sampler2D tex_Diffuse;

            uniform int hasDiffuse;
            uniform int renderVertColor;

            uniform vec4 tint_color;

            in vec2 f_texcoord0;
            in vec3 fragPosition;

            in vec4 vertexColor;
            in vec3 normal;
            in vec3 boneWeightsColored;

            out vec4 FragColor;

            void main(){
                vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                float hc_a   = highlight_color.w;

                vec4 color = vec4(0.8f);
                if (hasDiffuse == 1)
                    color = texture(tex_Diffuse,f_texcoord0);

                float halfLambert = max(displayNormal.y,0.5);
                vec4 colorComb = vec4(color.rgb * (1-hc_a) + highlight_color.rgb * hc_a, color.a);

	            vec3 lightDir = vec3(0, 0, 1);
	            float light = 0.6 + abs(dot(normal, lightDir)) * 0.8;

                FragColor = vec4(colorComb.rgb * light, colorComb.a);

                if (renderVertColor == 1)
                    FragColor *= min(vertexColor, vec4(1));

                FragColor *= min(tint_color, vec4(1));
                FragColor.rgb *= min(boneWeightsColored, vec3(1));
         }";

        private static string VertexShaderBasic = @"
            #version 330

            layout(location = 0) in vec3 vPosition;
            layout(location = 1) in vec3 vNormal;
            layout(location = 2) in vec2 vTexCoord;
            layout(location = 3) in vec4 vColor;
            layout(location = 4) in vec4 vBone;
            layout(location = 5) in vec4 vWeight;

            uniform mat4 mtxMdl;
            uniform mat4 mtxCam;

            // Skinning uniforms
            uniform mat4 bones[200];

            // Bone Weight Display
            uniform sampler2D weightRamp1;
            uniform sampler2D weightRamp2;
            uniform int selectedBoneIndex;
            uniform int debugOption;

            uniform int RigidSkinning;
            uniform int SingleBoneIndex;
            uniform int NoSkinning;

            out vec2 f_texcoord0;
            out vec4 vertexColor;
            out vec3 normal;
            out vec3 boneWeightsColored;

            vec4 skin(vec3 pos, ivec4 index)
            {
                vec4 newPosition = vec4(pos.xyz, 1.0);

                newPosition = bones[index.x] * vec4(pos, 1.0) * vWeight.x;
                newPosition += bones[index.y] * vec4(pos, 1.0) * vWeight.y;
                newPosition += bones[index.z] * vec4(pos, 1.0) * vWeight.z;
                if (vWeight.w < 1) //Necessary. Bones may scale weirdly without
		            newPosition += bones[index.w] * vec4(pos, 1.0) * vWeight.w;

                return newPosition;
            }

            vec3 skinNRM(vec3 nr, ivec4 index)
            {
                vec3 newNormal = vec3(0);

	            newNormal =  mat3(bones[index.x]) * nr * vWeight.x;
	            newNormal += mat3(bones[index.y]) * nr * vWeight.y;
	            newNormal += mat3(bones[index.z]) * nr * vWeight.z;
	            newNormal += mat3(bones[index.w]) * nr * vWeight.w;

                return newNormal;
            }

            vec3 BoneWeightColor(float weights)
            {
	            float rampInputLuminance = weights;
	            rampInputLuminance = clamp((rampInputLuminance), 0.001, 0.999);
                if (debugOption == 1) // Greyscale
                    return vec3(weights);
                else if (debugOption == 2) // Color 1
	               return texture(weightRamp1, vec2(1 - rampInputLuminance, 0.50)).rgb;
                else // Color 2
                    return texture(weightRamp2, vec2(1 - rampInputLuminance, 0.50)).rgb;
            }

            float BoneWeightDisplay(ivec4 index)
            {
                float weight = 0;
                if (selectedBoneIndex == index.x)
                    weight += vWeight.x;
                if (selectedBoneIndex == index.y)
                    weight += vWeight.y;
                if (selectedBoneIndex == index.z)
                    weight += vWeight.z;
                if (selectedBoneIndex == index.w)
                    weight += vWeight.w;

                if (selectedBoneIndex == index.x && RigidSkinning == 1)
                    weight = 1;
               if (selectedBoneIndex == SingleBoneIndex && NoSkinning == 1)
                    weight = 1;

                return weight;
            }

            void main(){
                f_texcoord0 = vTexCoord;
                vertexColor = vColor;
                normal = vNormal;

                ivec4 index = ivec4(vBone);
                normal = vNormal;

                vec4 objPos = mtxMdl * vec4(vPosition.xyz, 1.0);
	            if (vBone.x != -1.0)
		            objPos = skin(objPos.xyz, index);
	            if(vBone.x != -1.0)
		            normal = normalize((skinNRM(vNormal.xyz, index)).xyz);

                gl_Position = mtxCam*objPos;

                float totalWeight = BoneWeightDisplay(index);
                boneWeightsColored = BoneWeightColor(totalWeight).rgb;
            }";

        public override void SetMaterialUniforms(ShaderProgram shader, STGenericMaterial material, STGenericMesh mesh) {
            shader.SetVector4("tint_color", Vector4.One);

            var mat = (BIN.BIN_Material)material;

            shader.SetVector4("tint_color", new Vector4(
                mat.TintColor.R / 255F,
                mat.TintColor.G / 255F,
                mat.TintColor.B / 255F, 
                mat.TintColor.A / 255F));
        }
    }
}
