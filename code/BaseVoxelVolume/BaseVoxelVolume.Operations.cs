namespace Boxfish;

/// <summary>
/// Just a shrimple struct with a Vector3Int min and max value.
/// </summary>
public struct VoxelBounds
{
	public Vector3Int Mins;
	public Vector3Int Maxs;

	public VoxelBounds( Vector3Int mins, Vector3Int maxs )
	{
		Mins = mins;
		Maxs = maxs;
	}
}

file struct NVector3Int
{
	public int? x;
	public int? y;
	public int? z;

	public static implicit operator NVector3Int( Vector3Int v )
		=> new() { x = v.x, y = v.y, z = v.z };

	public static implicit operator Vector3Int( NVector3Int v )
		=> new Vector3Int(
			v.x.GetValueOrDefault(),
			v.y.GetValueOrDefault(),
			v.z.GetValueOrDefault()
		);
}

partial class BaseVoxelVolume<T, U>
{
	protected Chunk GetOrCreateChunk( Vector3Int position, (byte x, byte y, byte z)? local = null, Chunk relative = null )
	{
		if ( _chunks == null ) 
			return null;

		var globalPosition = new Vector3Int(
			((relative?.Position.x ?? 0) + (float)position.x / VoxelUtils.CHUNK_SIZE - (float)(local?.x ?? 0) / VoxelUtils.CHUNK_SIZE).CeilToInt(),
			((relative?.Position.y ?? 0) + (float)position.y / VoxelUtils.CHUNK_SIZE - (float)(local?.y ?? 0) / VoxelUtils.CHUNK_SIZE).CeilToInt(),
			((relative?.Position.z ?? 0) + (float)position.z / VoxelUtils.CHUNK_SIZE - (float)(local?.z ?? 0) / VoxelUtils.CHUNK_SIZE).CeilToInt()
		);

		// Check if we have a chunk already or are out of bounds.
		if ( Chunks.TryGetValue( globalPosition, out var chunk ) )
			return chunk;

		// Create new chunk.
		_chunks.Add(
			globalPosition,
			chunk = new Chunk( globalPosition.x, globalPosition.y, globalPosition.z, this )
		);

		return chunk;
	}

	/// <summary>
	/// Set voxel at 3D voxel position.
	/// <para>NOTE: This will not automatically update the chunk mesh, you will have to re-generate it manually.</para>
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="voxel"></param>
	/// <param name="relative"></param>
	/// <param name="createChunk"></param>
	public void SetVoxel( int x, int y, int z, T voxel, Chunk relative = null, bool createChunk = true )
	{
		var position = new Vector3Int( x, y, z );
		SetVoxel( position, voxel, relative );
	}

	/// <inheritdoc cref="SetVoxel(int, int, int, T, Chunk, bool)" />
	public void SetVoxel( Vector3Int position, T voxel, Chunk relative = null, bool createChunk = true )
	{
		var pos = GetLocalSpace( position.x, position.y, position.z, out var chunk, relative );
		if ( chunk == null )
		{
			if ( !createChunk || !IsValidVoxel( voxel ) )
				return;

			chunk = GetOrCreateChunk( position, pos, relative );
		}

		chunk.SetVoxel( pos.x, pos.y, pos.z, voxel );
	}

	/// <summary>
	/// Import a voxel file by path.
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public async Task Import( string path )
	{
		var chunks = await VoxelImporter.FromPath( path )
			.BuildAsync<T, U>();

		if ( chunks == null )
			return;

		SetChunks( chunks );

		await Task.CompletedTask;
	}

	/// <summary>
	/// Compute the voxelspace bounds of a collection of chunks.
	/// <para>NOTE: Uses the volume's chunks by default if no chunks param is assigned.</para>
	/// </summary>
	/// <param name="chunks"></param>
	/// <returns></returns>
	public VoxelBounds ComputeBounds( IDictionary<Vector3Int, Chunk> chunks = null )
	{
		chunks ??= _chunks;
		if ( chunks == null )
		{
			Logger.Warning( $"Tried to compute bounds for non-existing chunks?" );
			return default;
		}

		var min = (x: new NVector3Int(), y: new NVector3Int(), z: new NVector3Int());
		var max = (x: new NVector3Int(), y: new NVector3Int(), z: new NVector3Int());

		void Compare( Vector3Int pos, ref NVector3Int value, Func<int, int, bool> comparer )
		{
			if ( !chunks.TryGetValue( pos, out var chunk ) )
				return;

			for ( byte x = 0; x < VoxelUtils.CHUNK_SIZE; x++ )
				for ( byte y = 0; y < VoxelUtils.CHUNK_SIZE; y++ )
					for ( byte z = 0; z < VoxelUtils.CHUNK_SIZE; z++ )
					{
						var voxel = chunk.GetVoxel( x, y, z );
						if ( !IsValidVoxel( voxel ) )
							continue;

						var global = GetGlobalSpace( x, y, z, chunk );
						if ( value.x == null || comparer.Invoke( global.x, value.x.Value ) ) value.x = global.x;
						if ( value.y == null || comparer.Invoke( global.y, value.y.Value ) ) value.y = global.y;
						if ( value.z == null || comparer.Invoke( global.z, value.z.Value ) ) value.z = global.z;
					}
		}

		// Find chunk bounds first.
		foreach ( var (position, _) in chunks )
		{
			var pos = new Vector3Int( position.x, position.y, position.z );
			if ( max.x.x == null || pos.x > max.x.x ) max.x = pos;
			if ( max.y.y == null || pos.y > max.y.y ) max.y = pos;
			if ( max.z.z == null || pos.z > max.z.z ) max.z = pos;

			if ( min.x.x == null || pos.x < min.x.x ) min.x = pos;
			if ( min.y.y == null || pos.y < min.y.y ) min.y = pos;
			if ( min.z.z == null || pos.z < min.z.z ) min.z = pos;
		}

		// Find chunk's voxel bounds.
		var vMin = new NVector3Int();
		var minComparer = ( int a, int b ) => a < b;
		Compare( min.x, ref vMin, minComparer );
		Compare( min.y, ref vMin, minComparer );
		Compare( min.z, ref vMin, minComparer );

		var vMax = new NVector3Int();
		var maxComparer = ( int a, int b ) => a > b;
		Compare( max.x, ref vMax, maxComparer );
		Compare( max.y, ref vMax, maxComparer );
		Compare( max.z, ref vMax, maxComparer );

		// Return final voxel bounds.
		return new VoxelBounds( vMin, vMax );
	}
}
