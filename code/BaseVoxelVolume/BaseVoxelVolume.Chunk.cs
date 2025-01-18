namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// Our container of chunks in this volume.
	/// </summary>
	public IReadOnlyDictionary<Vector3Int, Chunk> Chunks => _chunks;
	internal Dictionary<Vector3Int, Chunk> _chunks = new();

	/// <summary>
	/// Call this method to override the chunks of this voxel volume.
	/// </summary>
	public void SetChunks( Dictionary<Vector3Int, Chunk> dictionary )
	{
		_chunks = dictionary;

		if ( dictionary is null )
			return;

		foreach ( var (_, chunk) in dictionary )
			chunk.SetParent( this );
	}

	/// <summary>
	/// Our chunk structure, contains a 3-dimensional array of voxels.
	/// </summary>
	public sealed class Chunk : IEquatable<Chunk>
	{
		private const int CHUNK_SIZE_P2 = VoxelUtils.CHUNK_SIZE * VoxelUtils.CHUNK_SIZE;
		private const int CHUNK_SIZE_P3 = CHUNK_SIZE_P2 * VoxelUtils.CHUNK_SIZE;

#pragma warning disable SB3000 // Hotloading not supported
		private static Vector3Int[] _neighbors = 
		[
			new ( 1, 0, 0 ),
			new ( -1, 0, 0 ),
			new ( 0, 1, 0 ),
			new ( 0, -1, 0 ),
			new ( 0, 0, 1 ),
			new ( 0, 0, -1 ),
		];
#pragma warning restore SB3000 // Hotloading not supported

		public int X { get; }
		public int Y { get; }
		public int Z { get; }
		public Vector3Int Position => new( X, Y, Z );

		internal bool Empty { get; set; }

		private BaseVoxelVolume<T, U> _volume;
		private T[] _voxels;

		private Dictionary<Vector3Int, Chunk> _chunks;

		public Chunk( int x, int y, int z, Dictionary<Vector3Int, Chunk> chunks = null )
		{
			X = x;
			Y = y;
			Z = z;

			_chunks = chunks;
			_voxels = new T[CHUNK_SIZE_P3];
		}

		public Chunk( int x, int y, int z, BaseVoxelVolume<T, U> volume = null )
		{
			X = x;
			Y = y;
			Z = z;

			_volume = volume;
			_chunks = volume?._chunks;
			_voxels = new T[CHUNK_SIZE_P3];
		}

		/// <summary>
		/// Flattens the 3D-coordinates into an index of our voxel array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public int Flatten( byte x, byte y, byte z )
			=> x + y * VoxelUtils.CHUNK_SIZE + z * CHUNK_SIZE_P2;

		public void SetParent( BaseVoxelVolume<T, U> volume )
		{
			_volume = volume;
			_chunks = volume._chunks;
		}

		public T GetVoxel( byte x, byte y, byte z )
			=> _voxels[Flatten( x, y, z )];

		public T[] GetVoxels() => _voxels;

		/// <inheritdoc cref="BaseVoxelVolume{T, U}.Query(int, int, int, BaseVoxelVolume{T, U}.Chunk)"/>
		public VoxelQueryData RelativeQuery( int x, int y, int z )
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
			=> _voxels[Flatten( x, y, z )] = voxel;

		/// <summary>
		/// Get neighbors depending on the local position given in the parameters.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="includeSelf"></param>
		/// <returns></returns>
		public IEnumerable<Chunk> GetNeighbors( byte x, byte y, byte z, bool includeSelf = true )
		{
			// Let's include this chunk too if we want.
			if ( includeSelf ) yield return this;

			// Yield return affected neighbors.
			foreach ( var direction in _neighbors )
			{
				// Check if we should include the neighbor.
				if ( _chunks.TryGetValue( Position + direction, out var result )
				 && ((direction.x == 1 && x >= VoxelUtils.CHUNK_SIZE - 1) || (direction.x == -1 && x <= 0)
				  || (direction.y == 1 && y >= VoxelUtils.CHUNK_SIZE - 1) || (direction.y == -1 && y <= 0)
				  || (direction.z == 1 && z >= VoxelUtils.CHUNK_SIZE - 1) || (direction.z == -1 && z <= 0)) )
				{
					yield return result;
					continue;
				}
			}

			int GetBorderDirection( int value )
			{
				if ( value <= 0 ) return -1;
				if ( value >= VoxelUtils.CHUNK_SIZE - 1 ) return 1;
				return 0;
			}

			// Get corner for refreshing AO aswell :D
			var directions = new Vector3Int( GetBorderDirection( x ), GetBorderDirection( y ), GetBorderDirection( z ) );
			var corner = Position + directions;
			if ( !corner.Equals( Position ) && _chunks.TryGetValue( corner, out var chunk ) )
				yield return chunk;
		}

		public bool Equals( Chunk other )
		{
			return other.Position.Equals( Position );
		}

		public override bool Equals( object obj )
		{
			return obj is Chunk other
				&& Equals( other );
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}
	}
}
