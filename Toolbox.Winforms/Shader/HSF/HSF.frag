#version 330
in vec3 position;
in vec3 normal;
in vec4 vertexColor;

in vec2 f_texcoord0;
in vec2 f_texcoord1;
in vec2 f_texcoord2;

in vec3 boneWeightsColored;

uniform vec4 color;
uniform int picking;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D UVTestPattern;

uniform vec4 highlight_color;

uniform vec3 ambient_lit_color;
uniform vec3 ambient_color;
uniform vec3 shadow_color;

uniform float transparency;

uniform int vertex_mode;

uniform int textureCount;
uniform int renderType;
uniform int renderVertColor;

uniform int pass_flags;
uniform int alpha_flags;

uniform float brightness;

out vec4 FragColor;

const int MAX_TEXTURES = 3; 

struct TextureAttribute
{
  vec2 texScaleStart;
  vec2 texScaleEnd;
  vec2 texPositionStart;
  vec2 texPositionEnd;
};

layout (std140) uniform AttributeBlock {
    TextureAttribute aTextureAttribute[MAX_TEXTURES];
};


void main(){
    if (picking == 1)
    {
        FragColor = color;
        return;
    }

    float hc_a   = highlight_color.w;
    vec4 textureOutput0 = vec4(1);
    vec4 textureOutput1 = vec4(1);
    vec4 textureOutput3 = vec4(1);

    int blend = 1;
    for (int i = 0; i < textureCount; i++) 
    {
        TextureAttribute attr = aTextureAttribute[i];

        if (i == 0) {
            textureOutput0 = texture(texture0,f_texcoord0);
        }
        if (i == 1) {
            textureOutput1 = texture(texture1,f_texcoord1);
        //   if (attr.blendingFlag == 0)
         //       blend = 1;
        }
    }

    vec4 colorOutput = textureOutput0;
    if (textureCount > 1 && blend == 1) {
        colorOutput = textureOutput1;
        colorOutput.rgb = textureOutput1.rgb + textureOutput0.rgb * (1 - textureOutput1.a);
    }
    if (vertex_mode == 0 && alpha_flags == 0) {
    //   colorOutput.a = (1 - colorOutput.r) * transparency;
    }
  //  if (vertex_mode == 5 && alpha_flags == 0 && pass_flags == 0)
  //      colorOutput.a *= vertexColor.r;

    colorOutput.a *= transparency;

    colorOutput.rgb *= ambient_color;
    colorOutput.rgb *= brightness;
    vec4 colorComb = vec4(colorOutput.rgb * (1-hc_a) + highlight_color.rgb * hc_a, colorOutput.a);

    vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
    float halfLambert = max(displayNormal.y,0.5);

	vec3 lightDir = vec3(0, 1, 0.5f);
	float light = 0.6 + abs(dot(normal, lightDir)) * 0.5;

    FragColor = vec4(colorComb.rgb * light, colorComb.a);

    if (renderVertColor == 1)
        FragColor *= min(vertexColor, vec4(1));

 //  FragColor.rgb *= min(boneWeightsColored, vec3(1));

  	 if (renderType == 1) //Display Normal
         FragColor = vec4(displayNormal, 1);
	 else if (renderType == 3) //DiffuseColor
        FragColor = colorOutput;
     else if (renderType == 5) // vertexColor
        FragColor = vertexColor;
    else if (renderType == 7) // uv coords
        FragColor = vec4(f_texcoord0.x, f_texcoord0.y, 1, 1);
    else if (renderType == 8) // uv test pattern
        FragColor = vec4(texture(UVTestPattern, f_texcoord0).rgb, 1);
     else if (renderType == 12)
        FragColor.rgb = boneWeightsColored;
}