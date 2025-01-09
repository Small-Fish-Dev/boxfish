namespace Boxfish.Utility;

/// <summary>
/// A little helper static class for looking up baked AO values.
/// </summary>
public static class AmbientOcclusion
{
	// This is messy I know!
	private static readonly IReadOnlyDictionary<int, List<(sbyte x, sbyte y, sbyte z)[]>> _aoTable 
		= new Dictionary<int, List<(sbyte x, sbyte y, sbyte z)[]>>()
	{
		// +z
		[0] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (0, 1, -1), (-1, 1, 0), (-1, 1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (0, 1, 1), (-1, 1, 0), (-1, 1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (0, 1, 1), (1, 1, 0), (1, 1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (0, 1, -1), (1, 1, 0), (1, 1, -1) }
		},

		// -z
		[1] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (0, -1, -1), (1, -1, 0), (1, -1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (0, -1, 1), (1, -1, 0), (1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (0, -1, 1), (-1, -1, 0), (-1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (0, -1, -1), (-1, -1, 0), (-1, -1, -1) }
		},

		// -x
		[2] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, -1), (-1, 1, 0), (-1, 1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, -1), (-1, -1, 0), (-1, -1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, 1), (-1, -1, 0), (-1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, 1), (-1, 1, 0), (-1, 1, 1) }
		},

		// +y
		[3] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, 1), (0, 1, 1), (-1, 1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, 1), (0, -1, 1), (-1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, 1), (0, -1, 1), (1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, 1), (0, 1, 1), (1, 1, 1) }
		},

		// +x
		[4] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (1, 0, 1), (1, 1, 0), (1, 1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, 1), (1, -1, 0), (1, -1, 1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, -1), (1, -1, 0), (1, -1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, -1), (1, 1, 0), (1, 1, -1) }
		},

		// -y
		[5] = new()
		{
			new (sbyte, sbyte, sbyte)[3] { (1, 0, -1), (0, 1, -1), (1, 1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (1, 0, -1), (0, -1, -1), (1, -1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, -1), (0, -1, -1), (-1, -1, -1) },
			new (sbyte, sbyte, sbyte)[3] { (-1, 0, -1), (0, 1, -1), (-1, 1, -1) }
		}
	};

	private static int Occlusion<T, U>( BaseVoxelVolume<T, U>.Chunk chunk, Vector3Int localPos, int x, int y, int z )
		where T : struct
		where U : unmanaged
	{
		if ( chunk.RelativeQuery( localPos.x + x, localPos.y + z, localPos.z + y ).HasVoxel )
			return 1;

		return 0;
	}

	/// <summary>
	/// Builds AO values for a voxel based on the parameters.
	/// </summary>
	/// <param name="chunk"></param>
	/// <param name="localPos"></param>
	/// <param name="face"></param>
	/// <param name="vertex"></param>
	/// <returns>A value between 0 and 3 determining the ambient occlusion strength for a vertex.</returns>
	public static byte Fetch<T, U>( BaseVoxelVolume<T, U>.Chunk chunk, Vector3Int localPos, int face, int vertex ) 
		where T : struct
		where U : unmanaged
	{
		if ( !_aoTable.TryGetValue( face, out var values ) )
			return 0;

		var table = values[vertex];
		var ao = Occlusion( chunk, localPos, table[0].x, table[0].y, table[0].z )
			+ Occlusion( chunk, localPos, table[1].x, table[1].y, table[1].z )
			+ Occlusion( chunk, localPos, table[2].x, table[2].y, table[2].z );

		return (byte)ao;
	}
}
