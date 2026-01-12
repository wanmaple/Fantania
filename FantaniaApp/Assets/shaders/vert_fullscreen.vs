#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 4) in vec2 aUV;

out vec2 vUV;

void main() {
    vec2 posCS = aPos.xy * 2.0 - vec2(1.0);
    gl_Position = vec4(posCS.xy, 0.0, 1.0);
    vUV = aUV;
}