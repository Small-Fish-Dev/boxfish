namespace Boxfish;

/// <summary>
/// Our chunk structure, contains a 3-dimensional array of voxels.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Chunk<T> 
	: IEquatable<Chunk<T>>
	where T : struct
{
	public int X { get; }
	public int Y { get; }
	public int Z { get; }
	public Vector3Int Position => new( X, Y, X );

	internal bool Empty { get; set; }

	private BaseVoxelVolume<T> _volume;
	private T[,,] _voxels;

	private Dictionary<Vector3Int, Chunk<T>> Chunks => _volume._chunks;

	public Chunk( int x, int y, int z, BaseVoxelVolume<T> volume = null )
	{
		X = x;
		Y = y;
		Z = z;

		_volume = volume;
		_voxels = new T[VoxelUtils.CHUNK_SIZE, VoxelUtils.CHUNK_SIZE, VoxelUtils.CHUNK_SIZE];
	}

	public T GetVoxel( byte x, byte y, byte z )
		=> _voxels[x, y, z];

	public T[,,] GetVoxels()
		=> _voxels;

	/// <inheritdoc cref="BaseVoxelVolume{T}.Query(int, int, int, Chunk{T})"/>
	public BaseVoxelVolume<T>.VoxelQueryData RelativeQuery( int x, int y, int z )
		=> _volume.Query( x, y, z, this );

	/// <summary>
	/// Set voxel in local position 
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="voxel"></param>
	/// <exception cref="IndexOutOfRangeException">When accessing outside of the 0-15 range.</exception>
	public void SetVoxel( byte x, byte y, byte z, T voxel = default )
		=> _voxels[x, y, z] = voxel;

	/// <summary>
	/// Get neighbors depending on the local position given in the parameters.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="includeSelf"></param>
	/// <returns></returns>
	public IEnumerable<Chunk<T>> GetNeighbors( byte x, byte y, byte z, bool includeSelf = true )
	{
		// Let's include this chunk too if we want.
		if ( includeSelf ) yield return this;

		var neighbors = new Vector3Int[]
		{
			new( 1, 0, 0 ),
			new( -1, 0, 0 ),
			new( 0, 1, 0 ),
			new( 0, -1, 0 ),
			new( 0, 0, 1 ),
			new( 0, 0, -1 ),
		};

		// Yield return affected neighbors.
		foreach ( var direction in neighbors )
		{
			// Check if we should include the neighbor.
			if ( Chunks.TryGetValue( Position + direction, out var result )
			 && ((direction.x == 1 && x >= VoxelUtils.CHUNK_SIZE - 1) || (direction.x == -1 && x <= 0)
			  || (direction.y == 1 && y >= VoxelUtils.CHUNK_SIZE - 1) || (direction.y == -1 && y <= 0)
			  || (direction.z == 1 && z >= VoxelUtils.CHUNK_SIZE - 1) || (direction.z == -1 && z <= 0)) )
			{
				yield return result;
				continue;
			}
		}

		// Check last corner.
		// (This is a hacky fix for AO...)
		var directions = new Vector3Int(
			x: x <= 0
				? -1
				: x >= VoxelUtils.CHUNK_SIZE - 1
					? 1
					: 0,
			y: y <= 0
				? -1
				: y >= VoxelUtils.CHUNK_SIZE - 1
					? 1
					: 0,
			z: z <= 0
				? -1
				: z >= VoxelUtils.CHUNK_SIZE - 1
					? 1
					: 0
		);

		var corner = Position + directions;
		if ( !corner.Equals( Position ) && Chunks.TryGetValue( corner, out var chunk ) )
			yield return chunk;
	}

	public bool Equals( Chunk<T> other )
	{
		return other.Position.Equals( Position );
	}

	public override bool Equals( object obj )
	{
		return obj is Chunk<T> other
			&& Equals( other );
	}

	public override int GetHashCode()
	{
		return Position.GetHashCode();
	}
}
