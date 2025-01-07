namespace Boxfish;

/// <summary>
/// The abstract component, you can decide what kind of voxel structure you want.
/// <para>Don't use this if you don't understand it, see <see cref="Components.VoxelVolume"/> instead.</para>
/// </summary>
/// <typeparam name="T">The voxel structure that you want to use. Example: <see cref="Components.Voxel"/>.</typeparam>
[Hide]
public abstract partial class BaseVoxelVolume<T> 
	: Component where T : struct
{
	/// <summary>
	/// Our container of chunks in this volume.
	/// </summary>
	public IReadOnlyDictionary<Vector3Int, Chunk<T>> Chunks => _chunks;
	internal readonly Dictionary<Vector3Int, Chunk<T>> _chunks = new();

	/// <summary>
	/// Scale of our voxels.
	/// </summary>
	public abstract float Scale { get; }

	/// <summary>
	/// The material we want to use for our chunks.
	/// </summary>
	public abstract Material Material { get; }

	/// <summary>
	/// We need this to determine the if this voxel solid or not, because our Voxels are structs. 
	/// </summary>
	/// <param name="voxel"></param>
	/// <returns></returns>
	public abstract bool IsValidVoxel( T voxel );
}
