namespace Boxfish.Library;

public partial class NetworkedVoxelVolume 
	: VoxelVolume, Component.INetworkSnapshot
{
	#region Snapshots
	void INetworkSnapshot.ReadSnapshot( ref ByteStream reader )
	{
		// Read from the ByteStream and deserialize.
		var length = reader.ReadRemaining;
		if ( length == 0 ) 
			return;

		var buffer = new byte[length];
		reader.Read( buffer, 0, length );

		// Uncompress and deserialize data.
		var uncompressed = buffer.Decompress();
		Deserialize( uncompressed );
	}

	void INetworkSnapshot.WriteSnapshot( ref ByteStream writer )
	{
		// Serialize and write to the ByteStream.
		var serialized = Serialize();
		if ( serialized.Length == 0 )
			return;

		var compressed = serialized.Compress();
		writer.Write( compressed );
	}
	#endregion

	/// <summary>
	/// This is called by the host to serialize the current world and send it to the connecting client.
	/// <para>NOTE: You should override this if you're doing something like: world generation... This can get expensive fast!</para>
	/// </summary>
	/// <returns></returns>
	public virtual byte[] Serialize()
	{
		if ( Chunks is null )
			return null;

		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );

		// Header
		writer.Write( Chunks.Count );

		// Chunks
		foreach ( var (position, chunk) in Chunks )
		{
			writer.Write( position.x );
			writer.Write( position.y );
			writer.Write( position.z );

			// Voxels
			var i = stream.Position;
			var count = (ushort)0;

			stream.Seek( i + sizeof( ushort ), SeekOrigin.Begin );
			for ( byte x = 0; x < VoxelUtils.CHUNK_SIZE; x++ )
				for ( byte y = 0; y < VoxelUtils.CHUNK_SIZE; y++ )
					for ( byte z = 0; z < VoxelUtils.CHUNK_SIZE; z++ )
					{
						var voxel = chunk.GetVoxel( x, y, z );
						if ( voxel.Valid )
						{
							writer.Write( x );
							writer.Write( y );
							writer.Write( z );

							writer.Write( voxel.R );
							writer.Write( voxel.G );
							writer.Write( voxel.B );
							writer.Write( voxel.Texture );

							count++;
						}
					}

			var j = stream.Position;
			stream.Seek( i, SeekOrigin.Begin );
			writer.Write( count );
			stream.Seek( j, SeekOrigin.Begin );
		}

		return stream
			.ToArray()
			.Compress();
	}

	/// <summary>
	/// This is called upon snapshot loading, we apply the snapshot data to the world here.
	/// <para>NOTE: You should override this if you're doing something like: world generation... This can get expensive fast!</para>
	/// </summary>
	/// <param name="data"></param>
	public virtual void Deserialize( byte[] data )
	{
		var chunks = new Dictionary<Vector3Int, Chunk>();
		using var stream = new MemoryStream( data.Decompress() );
		using var reader = new BinaryReader( stream );

		// Header
		var amount = reader.ReadInt32();

		// Chunks
		for ( int i = 0; i < amount; i++ )
		{
			var chunkX = reader.ReadInt32();
			var chunkY = reader.ReadInt32();
			var chunkZ = reader.ReadInt32();

			var chunk = new Chunk( chunkX, chunkY, chunkZ, chunks );
			chunks.TryAdd( new( chunkX, chunkY, chunkZ ), chunk );

			// Voxels
			var count = reader.ReadUInt16();
			for ( int j = 0; j < count; j++ )
			{
				var x = reader.ReadByte();
				var y = reader.ReadByte();
				var z = reader.ReadByte();

				var color = new Color32( reader.ReadByte(), reader.ReadByte(), reader.ReadByte() );
				var texture = reader.ReadUInt16();
				var voxel = new Voxel( color, texture );

				chunk.SetVoxel( x, y, z, voxel );
			}
		}

		// Set chunks, generate meshes.
		SetChunks( chunks );
		_ = GenerateMeshes( chunks.Values );
	}
}
