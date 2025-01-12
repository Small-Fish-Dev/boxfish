namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// Set voxel at 3D voxel position.
	/// <para>NOTE: This will not automatically update the chunk mesh, you will have to re-generate it manually.</para>
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	/// <param name="voxel"></param>
	/// <param name="relative"></param>
	public void SetVoxel( int x, int y, int z, T voxel, Chunk relative = null )
	{
		var position = new Vector3Int( x, y, z );
		SetVoxel( position, voxel, relative );
	}

	/// <inheritdoc cref="SetVoxel(int, int, int, T, Chunk)" />
	public void SetVoxel( Vector3Int position, T voxel, Chunk relative = null )
	{
		var pos = GetLocalSpace( position.x, position.y, position.z, out var chunk, relative );
		if ( chunk == null ) 
			return;

		chunk.SetVoxel( pos.x, pos.y, pos.z, voxel );
	}
}
