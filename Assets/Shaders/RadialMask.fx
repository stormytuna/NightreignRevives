sampler uImage0 : register(s0);

float progress;

float4 RadialMask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 toCoords = coords - center;
    float angle = atan2(-toCoords.y, toCoords.x);

    float normalizedAngle = frac((angle / (2 * 3.14159265)) + 0.75);
    float mask = step(progress, normalizedAngle);

    float4 color = tex2D(uImage0, coords);
    return color * mask;
}

technique Technique1
{
    pass RadialMaskPass
    {
        PixelShader = compile ps_3_0 RadialMask();
    }
}