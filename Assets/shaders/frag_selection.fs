#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec4 vTiling;
in vec4 vCustom;    // xy is size. zw is anchor.
in vec4 vCustom2;

uniform sampler2D uMainTexture;
uniform vec4 uParams;   // x is radius, y is thickness  (all in texels)

float rounded_rectangle(vec2 p, vec2 half_size, vec4 radius) {
    radius.xy = p.x > 0.0 ? radius.xy : radius.zw;
    vec2 d = abs(p) - half_size + vec2(radius.x);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius.x;
}

float circle(vec2 p, float radius) {
    return length(p) - radius;
}

void main() {
    vec2 uv = vUV * 2.0 - 1.0;
    vec2 size = vCustom.xy;
    vec2 wh = uv * size;
    vec2 halfsize = size;
    float outer = rounded_rectangle(wh, halfsize, vec4(uParams.x));
    float inner = rounded_rectangle(wh, halfsize - vec2(uParams.y), vec4(uParams.x));
    outer = step(outer, 0.0);
    inner = step(inner, 0.0);
    vec2 anchor = vCustom.zw * 2.0 - 1.0;
    vec2 center = anchor * (size - vec2(uParams.y));
    float c = circle(wh - center, uParams.y * 2.0);
    c = step(c, 0.0);
    vec4 color = vec4(max(c, outer - inner)) * vColor;
    if (color.a <= 0.02) discard;
    FragColor = color;
}