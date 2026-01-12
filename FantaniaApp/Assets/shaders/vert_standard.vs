#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec2 aUV;
layout (location = 5) in vec2 aUV2;

out vec4 vColor;
out vec3 vNormal;
out vec3 vTangent;
out vec2 vUV;
out vec2 vUV2;

uniform mat3 uView;
uniform vec4 uResolution;   // xy is resolution, zw is 1/resolution.

void main() {
    vec3 posVS = uView * vec3(aPos.xy, 1.0);
    vec2 posCS = posVS.xy * uResolution.zw * 2.0 - vec2(1.0);
    gl_Position = vec4(posCS.xy, aPos.z / 10000.0, 1.0);
    vColor = aColor;
    vNormal = aNormal;
    vTangent = aTangent;
    vUV = aUV;
    vUV2 = aUV2;
}