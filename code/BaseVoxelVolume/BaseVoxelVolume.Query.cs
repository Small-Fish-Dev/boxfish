namespace Boxfish;

public abstract partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// The result of queries giving information about a voxel, it's position, etc.. 
	/// </summary>
	public struct VoxelQueryData
	{
		public T Voxel;
		public bool HasVoxel;
		public (byte x, byte y, byte z) LocalPosition;
		public Vector3Int GlobalPosition;
		public Chunk Chunk;
	}

	/// <summary>
	/// Apply our GameObject's transform to a 3D-vector.
	/// </summary>
	/// <param name="vector"></param>
	/// <returns></returns>
	public Vector3 ApplyWorldTransform( Vector3 vector )
		=> new Vector3( (vector - WorldPosition) * WorldRotation.Inverse / WorldScale + WorldPosition );

	/// <summary>
	/// Converts a world position to a VoxelVolume position.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="transformed"></param>
	/// <returns></returns>
	public Vector3Int WorldToVoxel( Vector3 position, bool transformed = true )
	{
		var relative = transformed 
			? ApplyWorldTransform( position ) - WorldPosition
			: position;

		return new Vector3Int(
			(relative.x / Scale).FloorToInt(),
			(relative.y / Scale).FloorToInt(),
			(relative.z / Scale).FloorToInt()
		);
	}

	/// <summary>
	/// Converts a VoxelVolume position to a world position.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="transformed"></param>
	/// <returns></returns>
	public Vector3 VoxelToWorld( Vector3Int position, bool transformed = true )
	{
		if ( !transformed )
			return position * Scale;

		return position * Scale * WorldRotation * WorldScale + WorldPosition;
	}

	/// <summary>
	/// Converts global VoxelVolume coordinates to local, also outs the chunk.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="chunk"></param>
	/// <param name="relative"></param>
	/// <returns>The position local to the chunk.</returns>
	public (byte x, byte y, byte z) GetLocalSpace( int x, int y, int z, out Chunk chunk, Chunk relative = null )
	{
		var position = new Vector3Int(
			((float)(x + (relative?.X ?? 0) * VoxelUtils.CHUNK_SIZE) / VoxelUtils.CHUNK_SIZE).FloorToInt(),
			((float)(y + (relative?.Y ?? 0) * VoxelUtils.CHUNK_SIZE) / VoxelUtils.CHUNK_SIZE).FloorToInt(),
			((float)(z + (relative?.Z ?? 0) * VoxelUtils.CHUNK_SIZE) / VoxelUtils.CHUNK_SIZE).FloorToInt()
		);

		chunk = null;
		_ = Chunks?.TryGetValue( position, out chunk );

		return (
			(byte)((x % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE),
			(byte)((y % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE),
			(byte)((z % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE)
		);
	}

	/// <summary>
	/// Converts local coordinates of a chunk to global coordinates in a VoxelWorld.
	/// <para>NOTE: The position range expected is 0-15.</para>
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="relative"></param>
	/// <returns></returns>
	public static Vector3Int GetGlobalSpace( byte x, byte y, byte z, Chunk relative )
	{
		return new Vector3Int(
			x + (relative?.X ?? 0) * VoxelUtils.CHUNK_SIZE,
			y + (relative?.Y ?? 0) * VoxelUtils.CHUNK_SIZE,
			z + (relative?.Z ?? 0) * VoxelUtils.CHUNK_SIZE
		);
	}

	/// <summary>
	/// Gets VoxelQueryData by offset relative to a chunk, or Chunks[0, 0, 0].
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="relative"></param>
	/// <returns></returns>
	public VoxelQueryData Query( int x, int y, int z, Chunk relative = null )
	{
		// Get the new chunk's position based on the offset.
		var position = new Vector3Int(
			(relative?.X ?? 0) + ((x + 1) / (float)VoxelUtils.CHUNK_SIZE - 1).CeilToInt(),
			(relative?.Y ?? 0) + ((y + 1) / (float)VoxelUtils.CHUNK_SIZE - 1).CeilToInt(),
			(relative?.Z ?? 0) + ((z + 1) / (float)VoxelUtils.CHUNK_SIZE - 1).CeilToInt()
		);

		// Calculate new voxel position.
		var chunk = (Chunk)null;
		_ = Chunks?.TryGetValue( position, out chunk );

		var vx = (byte)((x % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE);
		var vy = (byte)((y % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE);
		var vz = (byte)((z % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE);

		var voxel = chunk?.GetVoxel( vx, vy, vz );
		return new VoxelQueryData
		{
			Chunk = chunk,
			Voxel = voxel ?? default,
			HasVoxel = IsValidVoxel( voxel ?? default ),
			LocalPosition = (vx, vy, vz),
			GlobalPosition = new Vector3Int(
				x + (relative?.X ?? 0) * (VoxelUtils.CHUNK_SIZE - 1), 
				y + (relative?.Y ?? 0) * (VoxelUtils.CHUNK_SIZE - 1), 
				z + (relative?.Z ?? 0) * (VoxelUtils.CHUNK_SIZE - 1) 
			)
		};
	}

	/// <inheritdoc cref="Query( int, int, int, Chunk )"/>
	public VoxelQueryData Query( Vector3Int offset, Chunk relative = null )
		=> Query( offset.x, offset.y, offset.z, relative );
}
