#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec4 vTiling;
in vec4 vCustom;
in vec4 vCustom2;   // xyzw indicates Left, Top, Right, Bottom scale of edge.

uniform sampler2D uMainTexture;
uniform sampler2D uGradientNoise;
uniform sampler1D uEdges;   // rgba indicates Left, Top, Right, Bottom.
uniform vec4 uAmplitudes;   // rgba indicates Left, Top, Right, Bottom.
uniform vec4 uOutline; // rgb is outline color, a is outline size.

void main() {
    vec2 hollowLRMax = uAmplitudes.xz / vCustom.x;
    vec2 hollowTBMax = uAmplitudes.yw / vCustom.y;
    // 必须要用GL_CLAMP_TO_EDGE采样，但是这里因为偏移可能超过1，所以得处理一下
    float leftS = vUV.y * vCustom2.x;
    leftS = fract(leftS);
    float rightS = vUV.y * vCustom2.z;
    rightS = fract(rightS);
    float topS = vUV.x * vCustom2.y;
    topS = fract(topS);
    float bottomS = vUV.x * vCustom2.w;
    bottomS = fract(bottomS);
    float noiseL = texture(uEdges, leftS).r;
    float hollowLeft = hollowLRMax.x * (1.0 - noiseL);
    float noiseR = texture(uEdges, rightS).b;
    float hollowRight = hollowLRMax.y * (1.0 - noiseR);
    float noiseT = texture(uEdges, topS).g;
    float hollowTop = hollowTBMax.x * (1.0 - noiseT);
    float noiseB = texture(uEdges, bottomS).a;
    float hollowBottom = hollowTBMax.y * (1.0 - noiseB);
    if (vUV.x <= hollowLeft || vUV.x >= 1.0 - hollowRight || vUV.y <= hollowTop || vUV.y >= 1.0 - hollowBottom) discard;
    float dxLeft = vUV.x - hollowLeft;
    float dxRight = 1.0 - hollowRight - vUV.x;
    float dyTop = vUV.y - hollowTop;
    float dyBottom = 1.0 - hollowBottom - vUV.y;
    float t = (dxLeft <= uOutline.w / vCustom.x || dxRight <= uOutline.w / vCustom.x || dyTop <= uOutline.w / vCustom.y || dyBottom <= uOutline.w / vCustom.y) ? 1.0 : 0.0;
    vec2 uv = vUV * vTiling.xy + vTiling.zw;
    vec3 color = texture(uGradientNoise, vUV).rgb * texture(uGradientNoise, uv).a * vColor.rgb;
    FragColor = vec4(mix(color, uOutline.rgb, t), vColor.a);
}