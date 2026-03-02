#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.

float segment(vec2 p, vec2 a, vec2 b) {
    vec2 ap = p - a;
    vec2 ab = b - a;
    float h = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    return length(ap - ab * h);
}

void main() {
    // vec2 uv = vUV * u_Resolution.xy;
    // float d = segment(uv, vec2(0.0, u_Resolution.y), vec2(u_Resolution.x, 0.0));
    // float lineWidth = 8.0;
    // vec4 color = step(d, lineWidth) * vColor.rgba;
    // FragColor = color;
    FragColor = vColor;
}