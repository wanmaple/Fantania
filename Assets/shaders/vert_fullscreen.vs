#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aUV;

out vec2 vUV;

void main() {
    vec2 posCS = aPos * 2.0 - 1.0;
    gl_Position = vec4(posCS.xy, 0.0, 1.0);
    vUV = aUV;
}