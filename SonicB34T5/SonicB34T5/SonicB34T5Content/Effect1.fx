uniform extern float4x4 WorldViewProj : WORLDVIEWPROJECTION;

uniform extern texture UserTexture;

 

struct VS_OUTPUT

{

    float4 position  : POSITION;

    float4 textureCoordinate : TEXCOORD0;

};

 

sampler textureSampler = sampler_state

{

Texture = <UserTexture>;

    MinFilter = POINT;  
    MagFilter = POINT;
	MipFilter = POINT;


};

 

VS_OUTPUT Transform(

    float4 Position  : POSITION, 

    float4 TextureCoordinate : TEXCOORD0 )

{

    VS_OUTPUT Out = (VS_OUTPUT)0;

 

    Out.position = mul(Position, WorldViewProj);

    Out.textureCoordinate = TextureCoordinate;

 

    return Out;

}

 

float4 ApplyTexture(VS_OUTPUT vsout) : COLOR

{

return tex2D(textureSampler, vsout.textureCoordinate).rgba;

}

 

technique TransformAndTexture

{

    pass P0

    {

        vertexShader = compile vs_2_0 Transform();

        pixelShader  = compile ps_2_0 ApplyTexture();

    }

}