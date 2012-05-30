

texture RenderTargetTexture; 
texture CurFrameVelocityTexture;
texture LastFrameVelocityTexture;

float4x4 mWorld;
float4x4 mWorldViewProjection;
float4x4 mWorldViewProjectionLast;

float4x4 MatrixTransform : register(vs, c0);
float PixelBlurConst = 1.0f;
static   float NumberOfPostProcessSamples = 80.0f; 


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler RenderTargetSampler = 
sampler_state
{
    Texture = <RenderTargetTexture>;
    MinFilter = POINT;  
    MagFilter = POINT;
	MipFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

sampler CurFramePixelVelSampler = 
sampler_state
{
    Texture = <CurFrameVelocityTexture>;
    MipFilter = POINT;
    MipFilter = POINT;
    MagFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

 

 

 

sampler LastFramePixelVelSampler = 
sampler_state
{
    Texture = <LastFrameVelocityTexture>;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

 


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position : POSITION;   // position of the vertex 
    float2 TextureUV : TEXCOORD0;  // typical texture coords stored here
    float2 VelocityUV : TEXCOORD1;  // per-vertex velocity stored here
};


//-----------------------------------------------------------------------------
// Name: WorldVertexShader     
// Type: Vertex shader                                      
// Desc: In addition to standard transform and lighting, it calculates the velocity 
//       of the vertex and outputs this as a texture coord.
//-----------------------------------------------------------------------------
VS_OUTPUT WorldVertexShader( float4 vPos : POSITION, 
                             float3 vNormal : NORMAL,
                             float2 vTexCoord0 : TEXCOORD0 )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
    float4 vPosProjSpaceCurrent; 
    float4 vPosProjSpaceLast; 
  
    vNormalWorldSpace = normalize(mul(vNormal, (float3x3)mWorld)); // normal (world space)
     
    vPosProjSpaceCurrent = mul(vPos, mWorldViewProjection);
    vPosProjSpaceLast = mul(vPos, mWorldViewProjectionLast);
     
    Output.Position = vPosProjSpaceCurrent;
	 
    vPosProjSpaceCurrent /= vPosProjSpaceCurrent.w;
    vPosProjSpaceLast /= vPosProjSpaceLast.w;
     
    float2 velocity = vPosProjSpaceCurrent - vPosProjSpaceLast;    
     
    velocity /= 2.0f;   
	 
    Output.VelocityUV = velocity;
        
  
    Output.TextureUV = vTexCoord0; 
    
    return Output;    
}

//-----------------------------------------------------------------------------
// Pixel shader output structure for no MRT
//-----------------------------------------------------------------------------
struct PS_OUTPUT_NO_MRT
{
    // The pixel shader can output 2+ values simulatanously if 
    // d3dcaps.NumSimultaneousRTs > 1
    
    float4 RGBColor      : COLOR0;  // Pixel color    
};

//-----------------------------------------------------------------------------
// Name:  WorldPixelShaderPhase2
// Desc: Uses multiple render passes to output 2 values one in each
//       pixel shader.  The two values are color and velocity.
//-----------------------------------------------------------------------------
 
PS_OUTPUT_NO_MRT WorldPixelShaderPass2(VS_OUTPUT In)
{
    PS_OUTPUT_NO_MRT Output;
    Output.RGBColor = float4(In.VelocityUV,1.0f,1.0f);
    return Output;
}

//-----------------------------------------------------------------------------
// Name: PostProcessMotionBlurPS 
// Type: Pixel shader                                      
// Desc: Uses the pixel's velocity to sum up and average pixel in that direction
//       to create a blur effect based on the velocity in a fullscreen
//       post process pass.
//-----------------------------------------------------------------------------


float4 PostProcessMotionBlurPS_2_0( float2 OriginalUV : TEXCOORD0 ) : COLOR
{
    float2 pixelVelocity;
    
 
    float4 curFramePixelVelocity = tex2D(CurFramePixelVelSampler, OriginalUV);
    float4 lastFramePixelVelocity = tex2D(LastFramePixelVelSampler, OriginalUV);
 
    float curVelocitySqMag = curFramePixelVelocity.r * curFramePixelVelocity.r +
                             curFramePixelVelocity.g * curFramePixelVelocity.g;
    float lastVelocitySqMag = lastFramePixelVelocity.r * lastFramePixelVelocity.r +
                              lastFramePixelVelocity.g * lastFramePixelVelocity.g;
                                   
    if( lastVelocitySqMag > curVelocitySqMag )
    {
        pixelVelocity.x =  lastFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -lastFramePixelVelocity.g * PixelBlurConst;
    }
    else
    {
        pixelVelocity.x =  curFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -curFramePixelVelocity.g * PixelBlurConst;    
    }
    
    // For each sample, sum up each sample's color in "Blurred" and then divide
    // to average the color after all the samples are added.
    float3 Blurred = 0;    
    for(float i = 0; i < 12; i++)
    {   
        // Sample texture in a new spot based on pixelVelocity vector 
        // and average it with the other samples        
        float2 lookup = pixelVelocity * i / 12 + OriginalUV;
        
        // Lookup the color at this new spot
        float4 Current = tex2D(RenderTargetSampler, lookup);
        
        // Add it with the other samples
        Blurred += Current.rgb;
    }
    
    // Return the average color of all the samples
    return float4(Blurred / 12, 1.0f);
}


float4 PostProcessMotionBlurPS_3_0( float2 OriginalUV : TEXCOORD0 ) : COLOR
{
    float2 pixelVelocity;
    
 
    float4 curFramePixelVelocity = tex2D(CurFramePixelVelSampler, OriginalUV);
    float4 lastFramePixelVelocity = tex2D(LastFramePixelVelSampler, OriginalUV);
 
    float curVelocitySqMag = curFramePixelVelocity.r * curFramePixelVelocity.r +
                             curFramePixelVelocity.g * curFramePixelVelocity.g;
    float lastVelocitySqMag = lastFramePixelVelocity.r * lastFramePixelVelocity.r +
                              lastFramePixelVelocity.g * lastFramePixelVelocity.g;
                                   
    if( lastVelocitySqMag > curVelocitySqMag )
    {
        pixelVelocity.x =  lastFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -lastFramePixelVelocity.g * PixelBlurConst;
    }
    else
    {
        pixelVelocity.x =  curFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -curFramePixelVelocity.g * PixelBlurConst;    
    }
    
    // For each sample, sum up each sample's color in "Blurred" and then divide
    // to average the color after all the samples are added.
    float3 Blurred = 0;    
    for(float i = 0; i < NumberOfPostProcessSamples; i++)
    {   
        // Sample texture in a new spot based on pixelVelocity vector 
        // and average it with the other samples        
        float2 lookup = pixelVelocity * i / NumberOfPostProcessSamples + OriginalUV;
        
        // Lookup the color at this new spot
        float4 Current = tex2D(RenderTargetSampler, lookup);
        
        // Add it with the other samples
        Blurred += Current.rgb;
    }
    
    // Return the average color of all the samples
    return float4(Blurred / NumberOfPostProcessSamples, 1.0f);
}

void SpriteVertexShader(inout float4 color    : COLOR0, 
                        inout float2 texCoord : TEXCOORD0, 
                        inout float4 position : SV_Position) 
{ 
    position = mul(position, MatrixTransform); 
}

//-----------------------------------------------------------------------------
// Name: WorldWithVelocity
// Type: Technique                                     
// Desc: Renders the scene's color in pass 0 and writes 
//       pixel velocity in pass 1.
//-----------------------------------------------------------------------------
technique WorldWithVelocity
{

    pass P1
    {          
        VertexShader = compile vs_2_0 WorldVertexShader();
        PixelShader  = compile ps_2_0 WorldPixelShaderPass2();
    }
}
//-----------------------------------------------------------------------------
// Name: PostProcessMotionBlur
// Type: Technique                                     
// Desc: Renders a full screen quad and uses velocity information stored in 
//       the textures to blur image.
//-----------------------------------------------------------------------------
technique PostProcessMotionBlur_2_0
{
    pass P0
    {        
        //PixelShader = compile ps_2_0 PostProcessMotionBlurPS_2_0();
        PixelShader = compile ps_2_0 PostProcessMotionBlurPS_2_0();
		// VertexShader = compile vs_2_0 SpriteVertexShader(); 
    }
}

technique PostProcessMotionBlur_3_0
{
    pass P0
    {        
	    VertexShader = compile vs_3_0 SpriteVertexShader(); 
        PixelShader = compile ps_3_0 PostProcessMotionBlurPS_3_0();
		  
    }
}


