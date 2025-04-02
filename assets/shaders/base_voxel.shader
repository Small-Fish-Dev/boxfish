FEATURES
{
    #include "common/features.hlsl"	
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
}

COMMON
{
	#include "common/shared.hlsl"
	StaticCombo( S_MODE_DEPTH, 0..1, Sys( ALL ) );

	float4x4 g_flTransformMatrix < Attribute( "TransformMatrix" ); >;
	float g_flVoxelScale < Attribute( "VoxelScale" ); Default( 1.0 ); >;
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"

	uint2 vData : TEXCOORD10 < Semantic( None ); >;
};


struct PixelInput
{
	#include "common/pixelinput.hlsl"

	float3 vNormal : TEXCOORD15;
	float fOcclusion : TEXCOORD14;
	float3 vTexCoord : TEXCOORD9;	
	float4 vColor : TEXCOORD13;
};

VS
{
	#include "common/vertex.hlsl"
	#include "lib/voxel_utils.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		// Turn our 32-bit unsigned integers back to the actual data.
		int3 position = int3( i.vData.x & 0xF, (i.vData.x >> 4) & 0xF, (i.vData.x >> 8) & 0xF );

		uint textureIndex = (i.vData.x >> 20) & 0xFFF;
		uint vertexIndex = (i.vData.x >> 17) & 0x7;

		float ao = pow( 0.75, (i.vData.x >> 15) & 0x3 );

		uint face = (i.vData.x >> 12) & 0x7;
		float3 normal = Voxel::GetFaceNormal(face);

		float4 color = float4( 
		    (i.vData.y & 0xFFu),
		    ((i.vData.y >> 8) & 0xFFu),
		    ((i.vData.y >> 16) & 0xFFu),
		    ((i.vData.y >> 24) & 0xFFu)
		) / 255.0f;

		// Set local position.
		float3 localPosition = (position + Voxel::GetOffset(vertexIndex)) * g_flVoxelScale;
		i.vPositionOs = localPosition;

		// Process vertex, figure out chunk position and global position.
		PixelInput o = ProcessVertex( i );
		float3 chunkPosition = o.vPositionWs - localPosition;
		float3 globalPosition = mul(g_flTransformMatrix, float4(chunkPosition * 16 * g_flVoxelScale + localPosition, 1.0f)).xyz;

		o.vPositionPs = Position3WsToPs(globalPosition);
		o.vPositionWs.xyz = globalPosition;

		// Set some other values.
		o.vNormal = normal;
		o.fOcclusion = ao;
		o.vTexCoord = float3( Voxel::GetUV(vertexIndex, face).xy, textureIndex * 6 + face );
		o.vColor = float4( SrgbGammaToLinear( color.rgb ), 1 ) * Voxel::GetFaceMultiplier(face);

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
	#define CUSTOM_TEXTURE_FILTERING

	#include "common/pixel.hlsl"

	CreateTexture2DArray( g_tAlbedo ) < Attribute( "VoxelAtlas" ); SrgbRead( true ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); > ;    

	SamplerState g_sSampler < Filter( POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;

	RenderState( CullMode, DEFAULT );	

	BoolAttribute( translucent, true );
	RenderState( AlphaToCoverageEnable, true );

	float4 MainPs( PixelInput i ) : SV_Target0
	{   
		float4 albedo = Tex2DArrayS( g_tAlbedo, g_sSampler, i.vTexCoord.xyz ).rgba;

		Material m = Material::Init();
		m.Albedo = albedo.rgb * i.vColor.rgb * i.fOcclusion;
		m.Normal = i.vNormal;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = i.fOcclusion;
		m.TintMask = 1;
		m.Opacity = albedo.a;
		m.Emission = 0;
		m.Transmission = 0;
		
		return ShadingModelStandard::Shade( i, m );
	}
}