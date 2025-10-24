#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aUV;
layout (location = 2) in vec4 aColor;
layout (location = 3) in vec4 aTiling;
layout (location = 4) in vec4 aCustom;
layout (location = 5) in vec4 aCustom2;

out vec2 vUV;
out vec4 vColor;
out vec4 vTiling;
out vec4 vCustom;
out vec4 vCustom2;

uniform mat3 uView;
uniform vec4 uResolution;   // xy is resolution, zw is 1/resolution.

void main() {
    vec3 posVS = uView * vec3(aPos.xy, 1.0);
    vec2 posCS = posVS.xy * uResolution.zw * 2.0 - 1.0;
    gl_Position = vec4(posCS.xy, aPos.z / 10000.0, 1.0);
    vUV = aUV;
    vColor = aColor;
    vTiling = aTiling;
    vCustom = aCustom;
    vCustom2 = aCustom2;
}