namespace Boxfish;

/// <summary>
/// The abstract component, you can decide what kind of voxel structure you want.
/// <para>Don't use this if you don't understand it, see <see cref="Components.VoxelVolume"/> instead.</para>
/// </summary>
/// <typeparam name="T">The voxel structure that you want to use. Example: <see cref="Components.Voxel"/>.</typeparam>
/// <typeparam name="U">The vertex structure that you want to use. Example: <see cref="Components.VoxelVertex"/></typeparam>
[Hide]
public abstract partial class BaseVoxelVolume<T, U> : Component 
	where T : struct 
	where U : unmanaged
{
	/// <summary>
	/// Scale of our voxels.
	/// </summary>
	public abstract float Scale { get; }

	/// <summary>
	/// The material we want to use for our chunks.
	/// </summary>
	public abstract Material Material { get; }

	/// <summary>
	/// Should we generate collisions?
	/// </summary>
	public abstract bool Collisions { get; }

	/// <summary>
	/// Our vertex layout.
	/// </summary>
	public abstract VertexAttribute[] Layout { get; }

	/// <summary>
	/// We need this to determine the if this voxel solid or not, because our Voxels are structs. 
	/// </summary>
	/// <param name="voxel"></param>
	/// <returns></returns>
	public abstract bool IsValidVoxel( T voxel );

	protected override void OnEnabled()
	{
		if ( !_chunks.Any() ) return;
		Task.RunInThreadAsync( () => GenerateMeshes( _chunks.Values, true ) );
	}

	protected override void OnDisabled()
	{
		DestroyObjects();
	}

	protected override void OnDestroy()
	{
		_chunks?.Clear();
	}
}
