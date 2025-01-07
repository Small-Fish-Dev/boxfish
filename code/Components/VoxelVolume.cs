namespace Boxfish.Components;

/// <summary>
/// Example voxel volume component.
/// <para>If you want more freedom, see <see cref="BaseVoxelVolume{T}"/> instead.</para>
/// </summary>
[Icon( "deployed_code" )]
public sealed class VoxelVolume
	: BaseVoxelVolume<Voxel>
{
	/// <summary>
	/// The scale of our voxels in inches.
	/// <para>Defaulted to 1 meter.</para>
	/// </summary>
	[Property, Icon( "arrow_range" ), Range( 0.1f, 50f )]
	public float VoxelScale { get; set; } = 1f / 0.0254f;

	#region Abstract class implementation
	public override Material Material => throw new NotImplementedException();

	public override float Scale => VoxelScale;

	public override bool IsValidVoxel( Voxel voxel ) => voxel.Valid;
	#endregion
}
