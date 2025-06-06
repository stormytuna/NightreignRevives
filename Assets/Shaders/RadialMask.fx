sampler uImage0 : register(s0);

float2 textureSize;
float progress;

float2 snapToGrid(float2 coords) {
    float2 pixelCoords = coords * textureSize;
    float2 snappedCoords = floor(pixelCoords / 2) * 2;
    float2 snappedUV = snappedCoords / textureSize;
    return snappedUV;
}

float4 RadialMask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 maskUV = snapToGrid(coords);

    float2 center = float2(0.5, 0.5);
    float2 toMaskUV = maskUV - center;
    float angle = atan2(-toMaskUV.y, toMaskUV.x);

    float normalizedAngle = frac((angle / (2 * 3.14159265)) + 0.75);
    float mask = step(progress, normalizedAngle);

    float4 color = tex2D(uImage0, coords);
    return color * mask * sampleColor.a;
}

technique Technique1
{
    pass RadialMaskPass
    {
        PixelShader = compile ps_3_0 RadialMask();
    }
}