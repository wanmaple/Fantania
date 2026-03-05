#version 330 core

out vec2 FragColor;

in vec2 vUV;

uniform sampler2D u_LightOccluderMask;
uniform sampler2D u_JfaNearest;
uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform float u_OccupancyThreshold;
uniform float u_DefaultZ;
uniform float u_MaxDistancePixels;

void main()
{
	vec2 nearestSeedUV = texture(u_JfaNearest, vUV).xy;
	float occ = texture(u_LightOccluderMask, vUV).g;
	float distancePx = u_MaxDistancePixels;
	float nearestZ = u_DefaultZ;
	vec2 deltaPx = (nearestSeedUV - vUV) * u_Resolution.xy;
	distancePx = min(length(deltaPx), u_MaxDistancePixels);
	nearestZ = texture(u_LightOccluderMask, nearestSeedUV).r;
	float signedDistance = occ >= u_OccupancyThreshold ? -distancePx : distancePx;
	FragColor = vec2(signedDistance, nearestZ);
}