using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.OpenGL;
using OpenTK;

namespace GCNLibrary.LM.MDL
{
    public class MDL_Render : ModelRenderer
    {
        public MDL_Render(STGenericModel model) : base(model)
        {

        }
/*
        public override void PrepareShaders()
        {
            if (ShaderProgram != null)
                return;

            ShaderProgram = new ShaderProgram(
                new VertexShader(VertexShaderBasic),
                new FragmentShader(FragmentShaderBasic));
        }

        private static string FragmentShaderBasic = @"
            #version 330

            uniform vec4 tint_color;

            uniform vec4 highlight_color;

            //Samplers
            uniform sampler2D tex_Diffuse;

            uniform int hasDiffuse;
            uniform int renderVertColor;

            in vec2 f_texcoord0;
            in vec3 fragPosition;

            in vec4 vertexColor;
            in vec3 normal;

            out vec4 FragColor;

            void main(){
                vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                float hc_a   = highlight_color.w;

                vec4 color = vec4(0.9f);
                if (hasDiffuse == 1)
                    color = texture(tex_Diffuse,f_texcoord0);

                color *= min(tint_color, vec4(1));

                float halfLambert = max(displayNormal.y,0.5);
                vec4 colorComb = vec4(color.rgb * (1-hc_a) + highlight_color.rgb * hc_a, color.a);

	            vec3 lightDir = vec3(0, 1, 0.5f);
	            float light = 0.6 + abs(dot(normal, lightDir)) * 0.5;

                FragColor = vec4(colorComb.rgb * light, colorComb.a) * vec4(vec3(0.6), 1);

                if (renderVertColor == 1)
                    FragColor *= min(vertexColor, vec4(1));
            }";

        public override void SetMaterialUniforms(ShaderProgram shader, STGenericMaterial material, STGenericMesh mesh) {
            shader.SetVector4("tint_color", Vector4.One);

            var mat = (MDL.MDL_Material)material;

            shader.SetVector4("tint_color", new Vector4(
                mat.TintColor.R / 255F,
                mat.TintColor.G / 255F,
                mat.TintColor.B / 255F, 
                mat.TintColor.A / 255F));
        }*/
    }
}
