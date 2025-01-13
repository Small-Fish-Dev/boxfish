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
	public float VoxelScale { get; set; } = VoxelUtils.METER;

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
		if ( Atlas is null )
		{
			Logger.Warning( $"Atlas is null, please assign an AtlasResource to {GameObject}!" );
			return;
		}

		attributes.Set( "VoxelScale", VoxelScale );
		attributes.Set( "VoxelAtlas", Atlas?.Texture );
	}

	protected override void OnStart()
	{
		base.OnStart();
		Atlas?.Build();
	}
}
