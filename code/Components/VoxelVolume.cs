namespace Boxfish.Components;

/// <summary>
/// Example voxel volume component.
/// <para>If you want more freedom, see <see cref="BaseVoxelVolume{T, U}"/> instead.</para>
/// </summary>
[Icon( "view_in_ar" )]
public class VoxelVolume
	: BaseVoxelVolume<Voxel, VoxelVertex>
{
	/// <summary>
	/// The scale of our voxels in inches.
	/// <para>Defaulted to 1 meter.</para>
	/// </summary>
	[Property, Range( 0.1f, 50f ), Category( "Appearance" )]
	public float VoxelScale { get; set; } = 1f / 0.0254f;

	#region Abstract class implementation
	public override Material Material => Material.FromShader( "shaders/base_voxel.shader" );

	public override float Scale => VoxelScale;

	public override bool Collisions => true;

	public override VertexAttribute[] Layout => VoxelVertex.Layout;

	public override VoxelVertex CreateVertex( Vector3Int position, int vertexIndex, int face, int ao, Voxel voxel )
		=> new VoxelVertex( (byte)position.x, (byte)position.y, (byte)position.z, (byte)vertexIndex, (byte)face, (byte)ao, voxel );

	public override bool IsValidVoxel( Voxel voxel ) => voxel.Valid;
	#endregion

	protected override void OnStart()
	{
		base.OnStart();

		// some funky testing :D
		GameTask.RunInThreadAsync( async () => {
			await GameTask.WorkerThread();

			Chunk CreatePerlinChunk( int x, int y )
			{
				var chunk = new Chunk( x, y, 0, this );

				for ( byte i = 0; i < VoxelUtils.CHUNK_SIZE; i++ )
					for ( byte j = 0; j < VoxelUtils.CHUNK_SIZE; j++ )
					{
						var noise = Sandbox.Utility.Noise.Perlin( x * VoxelUtils.CHUNK_SIZE + i, y * VoxelUtils.CHUNK_SIZE + j );
						var height = (int)(noise * VoxelUtils.CHUNK_SIZE - 1);

						for ( byte k = 0; k < height; k++ )
						{
							chunk.SetVoxel( i, j, k, new Voxel( Game.Random.Color().ToColor32() ) );
						}
					}

				_chunks.Add( new Vector3Int( x, y, 0 ), chunk );
				return chunk;
			}

			for ( int x = -8; x < 8; x++ )
				for ( int y = -8; y < 8; y++ )
				{
					var chunk = CreatePerlinChunk( x, y );
				}

			await GenerateMeshes( _chunks.Values );
		} );
	}
}
