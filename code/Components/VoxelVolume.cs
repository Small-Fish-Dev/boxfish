namespace Boxfish.Components;

/// <summary>
/// Example voxel volume component.
/// <para>If you want more freedom, see <see cref="BaseVoxelVolume{T, U}"/> instead.</para>
/// </summary>
[Icon( "view_in_ar" ), Category( "Boxfish" )]
public class VoxelVolume
	: BaseVoxelVolume<Voxel, VoxelVertex>
{
	/// <summary>
	/// The reference to our atlas.
	/// </summary>
	[Property, Category( "Appearance" )]
	public AtlasResource Atlas { get; set; } 
		= ResourceLibrary.Get<AtlasResource>( "resources/base.voxatlas" );

	/// <summary>
	/// The scale of our voxels in inches.
	/// <para>Defaulted to 1 meter.</para>
	/// </summary>
	[Property, Range( 0.1f, 50f ), Category( "Appearance" )]
	public float VoxelScale { get; set; } = 1f / 0.0254f;

	#region Abstract class implementation
	private Material _material = Material.FromShader( "shaders/base_voxel.shader" );
	public override Material Material => _material;

	public override float Scale => VoxelScale;

	public override bool Collisions => true;

	public override VertexAttribute[] Layout => VoxelVertex.Layout;

	public override VoxelVertex CreateVertex( Vector3Int position, int vertexIndex, int face, int ao, Voxel voxel )
		=> new VoxelVertex( (byte)position.x, (byte)position.y, (byte)position.z, (byte)vertexIndex, (byte)face, (byte)ao, voxel );

	public override bool IsValidVoxel( Voxel voxel ) => voxel.Valid;

	public override bool IsOpaqueVoxel( Voxel voxel )
	{
		if ( Atlas == null ) return false;
		if ( Atlas.TryGet( voxel.Texture, out var item ) )
			return item.Opaque;

		return false;
	}
	#endregion

	public override void SetAttributes( RenderAttributes attributes )
	{
		base.SetAttributes( attributes );

		// Set some render attributes.
		attributes.Set( "VoxelScale", VoxelScale );
		attributes.Set( "VoxelAtlas", Atlas.Texture );
	}

	protected override async void OnStart()
	{
		base.OnStart();

		Atlas?.Build();

		// some funky testing :D
		var chunks = new Dictionary<Vector3Int, VoxelVolume.Chunk>();
		var seed = Game.Random.Int( 0, int.MaxValue - 1 );
		Chunk CreatePerlinChunk( int x, int y )
		{
			var chunk = new Chunk( x, y, 0, this );

			for ( byte i = 0; i < VoxelUtils.CHUNK_SIZE; i++ )
				for ( byte j = 0; j < VoxelUtils.CHUNK_SIZE; j++ )
				{
					var noise = Sandbox.Utility.Noise.Perlin( x * VoxelUtils.CHUNK_SIZE + i, y * VoxelUtils.CHUNK_SIZE + j, seed );
					var height = Math.Clamp( noise * VoxelUtils.CHUNK_SIZE, 1, VoxelUtils.CHUNK_SIZE );

					for ( byte k = 0; k < height; k++ )
					{
						const ushort GRASS = 1;
						const ushort DIRT = 2;
						var tex = k >= height - 1 ? GRASS : DIRT;

						chunk.SetVoxel( i, j, k, new Voxel( Color32.White, tex ) );
					}
				}

			return chunk;
		}

		for ( int x = -2; x < 5; x++ )
			for ( int y = -2; y < 5; y++ )
			{
				var chunk = CreatePerlinChunk( x, y );
				chunks.Add( new Vector3Int( x, y, 0 ), chunk );
			}

		/*var chunks = await Importer.Create( "voxel/summer_cottage.vox" )
			.WithColorImporter( col => new Voxel( col ) )
			.BuildAsync();*/

		SetChunks( chunks );
		await GenerateMeshes( chunks.Values );
	}
}
