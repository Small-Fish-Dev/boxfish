namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	private bool trySpreadX( Chunk chunk, bool canSpreadX, ref bool[,,] tested, (byte x, byte y, byte z) start, ref (int x, int y, int z) size )
	{
		var yLimit = start.y + size.y;
		var zLimit = start.z + size.z;
		for ( byte y = start.y; y < yLimit && canSpreadX; ++y )
			for ( byte z = start.z; z < zLimit; ++z )
			{
				var newX = (byte)(start.x + size.x);
				if ( newX >= VoxelUtils.CHUNK_SIZE || tested[newX, y, z] || !IsValidVoxel( chunk.GetVoxel( newX, y, z ) ) )
					canSpreadX = false;
			}

		if ( canSpreadX )
		{
			for ( byte y = start.y; y < yLimit; ++y )
				for ( byte z = start.z; z < zLimit; ++z )
				{
					var newX = (byte)(start.x + size.x);
					tested[newX, y, z] = true;

					if ( !IsValidVoxel( chunk.GetVoxel( newX, y, z ) ) )
						return false;
				}

			++size.x;
		}

		return canSpreadX;
	}

	private bool trySpreadY( Chunk chunk, bool canSpreadY, ref bool[,,] tested, (byte x, byte y, byte z) start, ref (int x, int y, int z) size )
	{
		var xLimit = start.x + size.x;
		var zLimit = start.z + size.z;
		for ( byte x = start.x; x < xLimit && canSpreadY; ++x )
			for ( byte z = start.z; z < zLimit; ++z )
			{
				var newY = (byte)(start.y + size.y);
				if ( newY >= VoxelUtils.CHUNK_SIZE || tested[x, newY, z] || !IsValidVoxel( chunk.GetVoxel( x, newY, z ) ) )
					canSpreadY = false;
			}

		if ( canSpreadY )
		{
			for ( byte x = start.x; x < xLimit; ++x )
				for ( byte z = start.z; z < zLimit; ++z )
				{
					var newY = (byte)(start.y + size.y);
					tested[x, newY, z] = true;

					if ( !IsValidVoxel( chunk.GetVoxel( x, newY, z ) ) )
						return false;
				}

			++size.y;
		}

		return canSpreadY;
	}

	private bool trySpreadZ( Chunk chunk, bool canSpreadZ, ref bool[,,] tested, (byte x, byte y, byte z) start, ref (int x, int y, int z) size )
	{
		var xLimit = start.x + size.x;
		var yLimit = start.y + size.y;
		for ( byte x = start.x; x < xLimit && canSpreadZ; ++x )
			for ( byte y = start.y; y < yLimit; ++y )
			{
				var newZ = (byte)(start.z + size.z);
				if ( newZ >= VoxelUtils.CHUNK_SIZE || tested[x, y, newZ] || !IsValidVoxel( chunk.GetVoxel( x, y, newZ ) ) )
					canSpreadZ = false;
			}

		if ( canSpreadZ )
		{
			for ( byte x = start.x; x < xLimit; ++x )
				for ( byte y = start.y; y < yLimit; ++y )
				{
					var newZ = (byte)(start.z + size.z);
					tested[x, y, newZ] = true;

					if ( !IsValidVoxel( chunk.GetVoxel( x, y, newZ ) ) )
						return false;
				}

			++size.z;
		}

		return canSpreadZ;
	}

}
