#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec4 aRotationScale;
layout (location = 3) in vec2 aUV;
layout (location = 4) in vec2 aUV2;

out vec4 vColor;
out vec2 vUV;
out vec2 vUV2;
out vec3 vWorldPos;
out vec4 vRotationScale;

uniform mat3 u_View;
uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.

void main() {
    vec3 posVS = u_View * vec3(aPos.xy, 1.0);
    vec2 posNDC = posVS.xy * u_Resolution.zw;
    posNDC.y = 1.0 - posNDC.y;
    vec2 posCS = posNDC * 2.0 - vec2(1.0);
    gl_Position = vec4(posCS.xy, aPos.z / 4096.0, 1.0);
    vColor = aColor;
    vUV = vec2(aUV.x, aUV.y);
    vUV2 = aUV2;
    vWorldPos = aPos.xyz;
    vRotationScale = aRotationScale;
}