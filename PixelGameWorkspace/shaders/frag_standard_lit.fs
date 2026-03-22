#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec2 vUV2;
in vec4 vColor;
in vec3 vWorldPos;
in vec4 vRotationScale; // x = rotation (radians), yz = scale, w is not used.

const int MAX_TILES = 32;
const int MAX_LIGHTS = 64;
const int MAX_TILE_LIGHT_INDICES = 256;
const int MAX_LIGHT_TEXTURES = 8;

const float SPECULAR_INNER = 0.9848077; // cos(10 degrees)
const float SPECULAR_OUTER = 0.9659258; // cos(15 degrees)

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Albedo;
uniform sampler2D u_Normal;
uniform sampler2D u_Specular;
uniform float u_Cutoff;
uniform mat3 u_View;
uniform int u_LightingLayer;
uniform sampler2D u_LightOccluderMask;
uniform vec4 u_LightLayerDepth;
uniform vec4 u_ShadowArguments; // x: fadeStart, y: fadeEnd, zw unused
uniform vec4 u_EnvAmbient;
uniform vec4 u_EnvLight; // xyz: direction, w: intensity
uniform vec4 u_EnvLightColor;

uniform vec4 u_TileGridInfo; // x: tileSize, y: tilesX, z: tilesY, w: lightCount
uniform int u_TileOffsets[MAX_TILES];
uniform int u_TileCounts[MAX_TILES];
uniform int u_TileLightIndices[MAX_TILE_LIGHT_INDICES];
uniform vec4 u_LightPosRadius[MAX_LIGHTS]; // xyz: world position, w: radius
uniform vec4 u_LightColors[MAX_LIGHTS];    // rgb: color, a: intensity scale
uniform vec4 u_LightArgs[MAX_LIGHTS];      // xyz: unused, w: intensity
uniform int u_LightLayers[MAX_LIGHTS];
uniform int u_LightTextureIndices[MAX_LIGHTS];

float computeShadow(vec2 fragPos, vec2 lightPos, int fragLayer, int lightLayer)
{
	int layerDiff = max(fragLayer - lightLayer, 0);
	vec2 fragSS = (u_View * vec3(fragPos, 1.0)).xy;
	vec2 lightSS = (u_View * vec3(lightPos, 1.0)).xy;
	float fragZ = u_LightLayerDepth[fragLayer];
	float lightZ = u_LightLayerDepth[lightLayer];
	vec3 fragWS = vec3(fragPos, fragZ);
	vec3 lightWS = vec3(lightPos, lightZ);
	float dist = length(fragWS - lightWS);
	float fade = clamp((dist - u_ShadowArguments.x) / (u_ShadowArguments.y - u_ShadowArguments.x), 0.0, 1.0);
	float zDiff = abs(fragZ - lightZ);
	float t = 0.0;
	for (int layer = fragLayer - 1; layer > lightLayer; layer--)
	{
		t += (u_LightLayerDepth[layer + 1] - u_LightLayerDepth[layer]) / zDiff;
		vec2 sampleUV = mix(fragSS, lightSS, t) * u_Resolution.zw;
		sampleUV.y = 1.0 - sampleUV.y;
		vec4 mask = texture(u_LightOccluderMask, sampleUV);
		if (mask[layer] > 0.0) return fade;
	}
	return 1.0;
}

float computeSunShadow(vec2 fragPos, vec3 sunDir, int fragLayer)
{
	if (abs(sunDir.z) < 0.1) return 1.0;
	float fragZ = u_LightLayerDepth[fragLayer];
	vec3 fragWS = vec3(fragPos, fragZ);
	vec3 sunWS = fragWS + sunDir * 1000.0; // Arbitrary large distance
	float t = 0.0;
	for (int layer = fragLayer - 1; layer >= 0; layer--)
	{
		t += (u_LightLayerDepth[layer + 1] - u_LightLayerDepth[layer]) / abs(sunDir.z);
		vec2 sampleUV = (u_View * vec3(fragPos + sunDir.xy * t, 1.0)).xy * u_Resolution.zw;
		sampleUV.y = 1.0 - sampleUV.y;
		if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0) break;
		vec4 mask = texture(u_LightOccluderMask, sampleUV);
		if (mask[layer] > 0.0) return 0.5;
	}
	return 1.0;
}

void main() {
	vec4 albedo = texture(u_Albedo, vUV) * vColor;
	if (albedo.a < u_Cutoff) {
		discard;
	}
	vec4 normalTexel = texture(u_Normal, vUV);
	float specular = texture(u_Specular, vUV).r;	// Specular贴图必须是张灰度图，所以只需要取R通道
	float lightingMask = clamp(normalTexel.a, 0.0, 1.0);
	vec3 normalSample = normalTexel.xyz * 2.0 - 1.0;
	normalSample.y = -normalSample.y;
	float ca = cos(vRotationScale.x);
	float sa = sin(vRotationScale.x);
	mat2 rot = mat2(ca, -sa, sa, ca);
	mat2 normalMat = rot * mat2(1.0 / vRotationScale.y, 0.0, 0.0, 1.0 / vRotationScale.z);
	vec2 transformedXY = normalMat * normalSample.xy;
	vec3 N = normalize(vec3(transformedXY, max(normalSample.z, 0.0001)));
	float tileSize = max(u_TileGridInfo.x, 1.0);
	int tilesX = int(u_TileGridInfo.y);
	int tilesY = int(u_TileGridInfo.z);
	int lightCount = int(u_TileGridInfo.w);
	vec2 screenPos = vec2(gl_FragCoord.x, u_Resolution.y - gl_FragCoord.y);
	int tileX = int(floor(screenPos.x / tileSize));
	int tileY = int(floor(screenPos.y / tileSize));
	tileX = clamp(tileX, 0, max(tilesX - 1, 0));
	tileY = clamp(tileY, 0, max(tilesY - 1, 0));
	int tileId = tileY * tilesX + tileX;
	vec3 lightAccum = vec3(0.0);
	int start = u_TileOffsets[tileId];
	int count = u_TileCounts[tileId];
	int endExclusive = min(start + count, MAX_TILE_LIGHT_INDICES);
	for (int p = start; p < endExclusive; p++) {
		int lightIndex = u_TileLightIndices[p];
		int lightLayer = u_LightLayers[lightIndex];
		vec4 posRadius = u_LightPosRadius[lightIndex];
		float radius = max(posRadius.w, 0.0001);
		vec2 toLightXY = posRadius.xy - vWorldPos.xy;
		float toLightZ = u_LightLayerDepth[u_LightingLayer] - u_LightLayerDepth[lightLayer];
        vec3 toLight = vec3(toLightXY, toLightZ);
		vec3 L = normalize(toLight);
		float NdotL = max(dot(N, L), 0.0);
        float len = clamp(length(toLight) / radius, 0.0, 1.0);
        vec2 lightUV = len * 0.5 + vec2(0.5);
		int texIndex = clamp(u_LightTextureIndices[lightIndex], 0, MAX_LIGHT_TEXTURES - 1);
		float lightAttenuation = max(1.0 - len, 0.0);
		vec4 lightColor = u_LightColors[lightIndex];
		vec3 R = reflect(-L, N);
		float RdotZ = max(dot(R, vec3(0.0, 0.0, 1.0)), 0.0);
		float specularStrength = smoothstep(SPECULAR_OUTER, SPECULAR_INNER, RdotZ);
		float intensity = u_LightArgs[lightIndex].w;
		lightColor.rgb *= intensity;
		float shadow = computeShadow(vWorldPos.xy, posRadius.xy, u_LightingLayer, lightLayer);
		vec3 contribution = (lightColor.rgb * lightAttenuation * NdotL + lightColor.rgb * specular * specularStrength) * shadow;
		lightAccum += contribution;
	}
	{
		// Add environment lighting
		vec3 envLightDir = -normalize(u_EnvLight.xyz);
		vec3 envLightColor = u_EnvLightColor.rgb * u_EnvLight.w;
		float NdotL = max(dot(N, envLightDir), 0.0);
		vec3 R = reflect(-envLightDir, N);
		float RdotZ = max(dot(R, vec3(0.0, 0.0, 1.0)), 0.0);
		float specularStrength = smoothstep(SPECULAR_OUTER, SPECULAR_INNER, RdotZ);
		float shadow = computeSunShadow(vWorldPos.xy, envLightDir, u_LightingLayer);
		vec3 envContribution = (envLightColor * NdotL + envLightColor * specular * specularStrength) * shadow;
		lightAccum += envContribution;
	}
	vec3 unlitColor = albedo.rgb * u_EnvAmbient.rgb;
	vec3 litColor = unlitColor + albedo.rgb * lightAccum;
	vec3 finalColor = mix(unlitColor, litColor, lightingMask);
	FragColor = vec4(finalColor, albedo.a);
}