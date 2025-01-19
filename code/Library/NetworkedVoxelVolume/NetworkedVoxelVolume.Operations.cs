namespace Boxfish.Library;

partial class NetworkedVoxelVolume
{
	/// <summary>
	/// Set a radius of voxels and broadcast the change to other clients.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	/// <param name="voxel"></param>
	[Rpc.Broadcast]
	public void BroadcastSetRadius( Vector3Int position, float radius, Voxel voxel )
	{
		var half = (int)(radius / 2f + 0.5f);
		for ( int x = -half; x <= half; x++ )
			for ( int y = -half; y <= half; y++ )
				for ( int z = -half; z <= half; z++ )
				{
					var current = position + new Vector3Int( x, y, z );
					var distance = current.Distance( position );
					if ( distance >= radius / 2f )
						continue;

					SetTrackedVoxel( current, voxel );
				}
	}

	/// <summary>
	/// Set a bounds of voxels and broadcast the change to other clients.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="bounds"></param>
	/// <param name="voxel"></param>
	[Rpc.Broadcast]
	public void BroadcastSetBounds( Vector3Int position, VoxelBounds bounds, Voxel voxel )
	{
		for ( int x = bounds.Mins.x; x < bounds.Maxs.y; x++ )
			for ( int y = bounds.Mins.y; y < bounds.Maxs.y; y++ )
				for ( int z = bounds.Mins.z; z < bounds.Maxs.z; z++ )
				{
					var pos = new Vector3Int( x, y, z );
					SetTrackedVoxel( position + pos, voxel );
				}
	}

	/// <summary>
	/// Set a singular voxel and broadcast the change to other clients.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="voxel"></param>
	[Rpc.Broadcast]
	public void BroadcastSet( Vector3Int position, Voxel voxel )
	{
		SetTrackedVoxel( position, voxel );
	}
}
