#version 330 core

out vec2 FragColor;

in vec2 vUV;

const float OCCUPANCY_THRESHOLD = 0.5;
const float DEPTH_EDGE_THRESHOLD = 0.5;

uniform sampler2D u_LightOccluderMask;
uniform vec4 u_SDFResolution;   // xy is resolution, zw is 1/resolution.

float sampleOcc(vec2 uv)
{
	return texture(u_LightOccluderMask, uv).g;
}

void main()
{
	vec2 c = texture(u_LightOccluderMask, vUV).rg;
	vec2 l = texture(u_LightOccluderMask, vUV + vec2(-u_SDFResolution.z, 0.0)).rg;
	vec2 r = texture(u_LightOccluderMask, vUV + vec2( u_SDFResolution.z, 0.0)).rg;
	vec2 d = texture(u_LightOccluderMask, vUV + vec2(0.0, -u_SDFResolution.w)).rg;
	vec2 u = texture(u_LightOccluderMask, vUV + vec2(0.0, u_SDFResolution.w)).rg;
	bool cIn = c.g >= OCCUPANCY_THRESHOLD;
	bool cLeftIn = l.g >= OCCUPANCY_THRESHOLD;
	bool cRightIn = r.g >= OCCUPANCY_THRESHOLD;
	bool cDownIn = d.g >= OCCUPANCY_THRESHOLD;
	bool cUpIn = u.g >= OCCUPANCY_THRESHOLD;
	bool occEdge = (cIn != cLeftIn)
		|| (cIn != cRightIn)
		|| (cIn != cDownIn)
		|| (cIn != cUpIn);
	bool depthEdge = (cIn && cLeftIn && abs(c.r - l.r) > DEPTH_EDGE_THRESHOLD)
		|| (cIn && cRightIn && abs(c.r - r.r) > DEPTH_EDGE_THRESHOLD)
		|| (cIn && cDownIn && abs(c.r - d.r) > DEPTH_EDGE_THRESHOLD)
		|| (cIn && cUpIn && abs(c.r - u.r) > DEPTH_EDGE_THRESHOLD);
	bool edge = occEdge || depthEdge;
	FragColor = edge ? vUV : vec2(-1.0, -1.0);
}
