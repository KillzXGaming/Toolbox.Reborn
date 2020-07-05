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

uniform int NoSkinning;
uniform int RigidSkinning;
uniform int SingleBoneIndex;

out vec3 normal;
out vec4 vertexColor;
out vec3 position;
out vec2 f_texcoord0;
out vec2 f_texcoord1;
out vec2 f_texcoord2;

out vec3 boneWeightsColored;

uniform int textureCount;

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

    ivec4 index = ivec4(vBone);
    normal = vNormal;

    vec4 objPos = mtxMdl * vec4(vPosition.xyz, 1.0);
	if (vBone.x != -1.0)
		objPos = skin(objPos.xyz, index);
	if(vBone.x != -1.0)
		normal = normalize((skinNRM(vNormal.xyz, index)).xyz);

    vertexColor = vColor;
	position = objPos.xyz;
    f_texcoord0 = vTexCoord;
    f_texcoord1 = vTexCoord;
    f_texcoord2 = vTexCoord;

    for (int i = 0; i < textureCount; i++) {
         TextureAttribute attr = aTextureAttribute[i];
         vec2 shift = attr.texPositionStart;
         //Scale from center
         shift = vec2(shift.x / 1 - 0.5, shift.y / 1 - 0.5);
         vec2 scale = vec2(1) / attr.texScaleStart;
         vec2 transform = (vec2(0.5) + (vTexCoord.xy + shift )) * scale;

         if (i == 0)
             f_texcoord0 = transform;
         if (i == 1)     
            f_texcoord1 = transform;
         if (i == 2)     
            f_texcoord2 = transform;
    }

    gl_Position = mtxCam*objPos;

    float totalWeight = BoneWeightDisplay(index);
    boneWeightsColored = BoneWeightColor(totalWeight).rgb;
}