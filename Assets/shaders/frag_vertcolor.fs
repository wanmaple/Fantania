#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec4 vTiling;
in vec4 vCustom;
in vec4 vCustom2;

uniform sampler2D uMainTexture;
uniform vec4 uOutline; // rgb is outline color, a is outline size.
uniform float uCutOff;

void main() {
    vec2 uv = vUV * vTiling.xy + vTiling.zw;
    vec4 color = texture(uMainTexture, uv) * vColor;
    vec2 pixelSize = vec2(1.0) / abs(vCustom.xy);
    if (color.a < uCutOff) {
        for (float i = 1.0; i <= uOutline.a; i += 1.0) {
            vec4 up = texture(uMainTexture, uv + vec2(0.0, -pixelSize.y) * i);
            if (up.a >= uCutOff) {
                FragColor = vec4(uOutline.rgb, 1.0);
                return;
            }
            vec4 down = texture(uMainTexture, uv + vec2(0.0, pixelSize.y) * i);
            if (down.a >= uCutOff) {
                FragColor = vec4(uOutline.rgb, 1.0);
                return;
            }
            vec4 left = texture(uMainTexture, uv + vec2(-pixelSize.x, 0.0) * i);
            if (left.a >= uCutOff) {
                FragColor = vec4(uOutline.rgb, 1.0);
                return;
            }
            vec4 right = texture(uMainTexture, uv + vec2(pixelSize.x, 0.0) * i);
            if (right.a >= uCutOff) {
                FragColor = vec4(uOutline.rgb, 1.0);
                return;
            }
        }
        discard;
    }
    FragColor = color;
}