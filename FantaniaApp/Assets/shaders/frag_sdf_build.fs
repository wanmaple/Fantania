#version 330 core

out vec2 FragColor;

in vec2 vUV;

uniform sampler2D u_LightOccluderMask;
uniform sampler2D u_JfaNearest;
uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform float u_MaxDistancePixels;

int samplePriorityChannel(vec2 uv)
{
	vec4 mask = texture(u_LightOccluderMask, uv).rgba;
	if (mask.r > 0.0) return 1;
	if (mask.g > 0.0) return 2;
	if (mask.b > 0.0) return 3;
	if (mask.a > 0.0) return 4;
	return 0;
}

void main()
{
	vec2 nearestSeedUV = texture(u_JfaNearest, vUV).xy;
	int priorityChannel = samplePriorityChannel(vUV);
	int nearestSeedChannel = nearestSeedUV.x >= 0.0 ? samplePriorityChannel(nearestSeedUV) : 0;
	vec2 deltaPx = (nearestSeedUV - vUV) * u_Resolution.xy;
	float distancePx = min(length(deltaPx), u_MaxDistancePixels);
	bool samePriorityRegion = priorityChannel > 0 && priorityChannel == nearestSeedChannel;
	float signedDistance = samePriorityRegion ? -distancePx : distancePx;
	FragColor = vec2(signedDistance, 0.0);
}