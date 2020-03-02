#ifndef HEXMESH_SHADER
#define HEXMESH_SHADER

void WaterWave_float(float2 inUV, float4 inColor, float4 inNoise, float inTime, out float3 outColor, out float outAlpha)
{
    float shore = inUV.y;
    shore = sqrt(shore);

    float distortion1 = inNoise.x * (1 - shore);
    float foam1 = sin((shore+distortion1) * 10 - inTime);
    foam1 *= foam1;

    float distortion2 = inNoise.y * (1 - shore);
    float foam2 = sin((shore+distortion2) * 10 - inTime+2);
    foam2 *= foam2 * 0.7;

    float foam = max(foam1, foam2) * shore;

    float4 c = saturate(inColor + foam);
    outColor.xyz = c.xyz;
    outAlpha = c.w;
}

void WaterWave_half(half2 inUV, half4 inColor, half4 inNoise, half inTime, out half3 outColor, out half outAlpha)
{
    half shore = inUV.y;
    shore = sqrt(shore);

    half distortion1 = inNoise.x * (1 - shore);
    half foam1 = sin((shore+distortion1) * 10 - inTime);
    foam1 *= foam1;

    half distortion2 = inNoise.y * (1 - shore);
    half foam2 = sin((shore+distortion2) * 10 - inTime+2);
    foam2 *= foam2 * 0.7;

    half foam = max(foam1, foam2) * shore;

    half4 c = saturate(inColor + foam);
    outColor.xyz = c.xyz;
    outAlpha = c.w;
}

float Foam_float (float shore, float2 worldXZ, sampler2D noiseTex) {
//	float shore = IN.uv_MainTex.y;
	shore = sqrt(shore);

	float2 noiseUV = worldXZ + _Time.y * 0.25;
	float4 noise = tex2D(noiseTex, noiseUV * 0.015);

	float distortion1 = noise.x * (1 - shore);
	float foam1 = sin((shore + distortion1) * 10 - _Time.y);
	foam1 *= foam1;

	float distortion2 = noise.y * (1 - shore);
	float foam2 = sin((shore + distortion2) * 10 + _Time.y + 2);
	foam2 *= foam2 * 0.7;

	return max(foam1, foam2) * shore;
}

float Foam_half (float shore, float2 worldXZ, sampler2D noiseTex) {
//	float shore = IN.uv_MainTex.y;
	shore = sqrt(shore);

	float2 noiseUV = worldXZ + _Time.y * 0.25;
	float4 noise = tex2D(noiseTex, noiseUV * 0.015);

	float distortion1 = noise.x * (1 - shore);
	float foam1 = sin((shore + distortion1) * 10 - _Time.y);
	foam1 *= foam1;

	float distortion2 = noise.y * (1 - shore);
	float foam2 = sin((shore + distortion2) * 10 + _Time.y + 2);
	foam2 *= foam2 * 0.7;

	return max(foam1, foam2) * shore;
}

float Waves_float (float2 worldXZ, sampler2D noiseTex) {
	float2 uv1 = worldXZ;
	uv1.y += _Time.y;
	float4 noise1 = tex2D(noiseTex, uv1 * 0.025);

	float2 uv2 = worldXZ;
	uv2.x += _Time.y;
	float4 noise2 = tex2D(noiseTex, uv2 * 0.025);

	float blendWave = sin(
		(worldXZ.x + worldXZ.y) * 0.1 +
		(noise1.y + noise2.z) + _Time.y
	);
	blendWave *= blendWave;

	float waves =
		lerp(noise1.z, noise1.w, blendWave) +
		lerp(noise2.x, noise2.y, blendWave);
	return smoothstep(0.75, 2, waves);
}

float Waves_half (float2 worldXZ, sampler2D noiseTex) {
	float2 uv1 = worldXZ;
	uv1.y += _Time.y;
	float4 noise1 = tex2D(noiseTex, uv1 * 0.025);

	float2 uv2 = worldXZ;
	uv2.x += _Time.y;
	float4 noise2 = tex2D(noiseTex, uv2 * 0.025);

	float blendWave = sin(
		(worldXZ.x + worldXZ.y) * 0.1 +
		(noise1.y + noise2.z) + _Time.y
	);
	blendWave *= blendWave;

	float waves =
		lerp(noise1.z, noise1.w, blendWave) +
		lerp(noise2.x, noise2.y, blendWave);
	return smoothstep(0.75, 2, waves);
}

#endif
