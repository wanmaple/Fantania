#version 330 core

out vec2 FragColor;

in vec2 vUV;

uniform vec4 u_SDFResolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_JfaPrev;
uniform float u_JumpPixels;

float uvDistancePx(vec2 a, vec2 b)
{
	vec2 d = (a - b) * u_SDFResolution.xy;
	return length(d);
}

void main()
{
	vec2 bestSeed = texture(u_JfaPrev, vUV).xy;
	float bestDist = bestSeed.x >= 0.0 ? uvDistancePx(vUV, bestSeed) : 1e20;
	for (int oy = -1; oy <= 1; oy++)
	{
		for (int ox = -1; ox <= 1; ox++)
		{
			vec2 offset = vec2(float(ox), float(oy)) * (u_JumpPixels * u_SDFResolution.zw);
			vec2 sampleUV = clamp(vUV + offset, vec2(0.0), vec2(1.0));
			vec2 candidate = texture(u_JfaPrev, sampleUV).xy;
			float d = candidate.x < 0.0 ? 1e20 : uvDistancePx(vUV, candidate);
			bestSeed = d < bestDist ? candidate : bestSeed;
			bestDist = d < bestDist ? d : bestDist;
		}
	}
	FragColor = bestSeed;
}
