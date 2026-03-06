#version 330 core

out vec2 FragColor;

in vec2 vUV;

uniform sampler2D u_LightOccluderMask;
uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.

int samplePriorityChannel(vec2 uv)
{
	vec4 texel = texture(u_LightOccluderMask, uv).rgba;
	if (texel.r > 0.0) return 1;
	if (texel.g > 0.0) return 2;
	if (texel.b > 0.0) return 3;
	if (texel.a > 0.0) return 4;
	return 0;
}

void main()
{
	int c = samplePriorityChannel(vUV);
	int l = samplePriorityChannel(vUV + vec2(-u_Resolution.z, 0.0));
	int r = samplePriorityChannel(vUV + vec2(u_Resolution.z, 0.0));
	int d = samplePriorityChannel(vUV + vec2(0.0, -u_Resolution.w));
	int u = samplePriorityChannel(vUV + vec2(0.0, u_Resolution.w));
	bool edge = (c != l) || (c != r) || (c != d) || (c != u);
	FragColor = edge ? vUV : vec2(-1.0, -1.0);
}
