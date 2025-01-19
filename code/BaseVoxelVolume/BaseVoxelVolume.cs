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
	/// Our transform matrix, uses the GameObject's World transforms.
	/// </summary>
	public Matrix TransformMatrix =>
		  Matrix.CreateScale( WorldScale )
		* Matrix.CreateRotation( WorldRotation )
		* Matrix.CreateTranslation( WorldPosition );

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

	/// <summary>
	/// We need this to determine the if this voxel opaque or not, for rendering non-opaque voxels that are directly behind opaque voxels. 
	/// </summary>
	/// <param name="voxel"></param>
	/// <returns></returns>
	public virtual bool IsOpaqueVoxel( T voxel ) => false;

	/// <summary>
	/// This is called for every chunk <see cref="SceneObject"/>'s <see cref="RenderAttributes"/>.
	/// </summary>
	/// <param name="attributes"></param>
	public virtual void SetAttributes( RenderAttributes attributes ) { }

	protected override void OnEnabled()
	{
		// if ( !_chunks.Any() ) return;
		// Task.RunInThreadAsync( () => GenerateMeshes( _chunks.Values, true ) );
	}

	protected override void OnDisabled()
	{
		// DestroyObjects();
	}

	protected override void DrawGizmos()
	{
		// Draw gizmo borders for our chunks.
		if ( _chunks == null || !_chunks.Any() ) 
			return;

		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.LineThickness = 1f;

		foreach ( var (position, _) in _chunks )
		{
			// Draw chunk border.
			var pos = (Vector3)position * Scale * VoxelUtils.CHUNK_SIZE;
			var bounds = new BBox( pos, pos + (Vector3)VoxelUtils.CHUNK_SIZE * Scale );

			Gizmo.Draw.LineBBox( bounds );
		}
	}

	protected override void OnDestroy()
	{
		// Dispose these!
		DestroyObjects();
		_editorChunks?.Clear();
		_chunks?.Clear();
	}
}
