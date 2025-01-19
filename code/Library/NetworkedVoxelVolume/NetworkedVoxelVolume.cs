namespace Boxfish.Library;

public partial class NetworkedVoxelVolume 
	: VoxelVolume, Component.INetworkSnapshot
{
	#region Snapshots
	void INetworkSnapshot.ReadSnapshot( ref ByteStream reader )
	{
		// Read the ByteStream.
		var length = reader.Length;
		var buffer = new byte[length];
		reader.Read( buffer, 0, length );

		// Uncompress and deserialize data.
		var uncompressed = buffer.Decompress();
		Deserialize( uncompressed );
	}

	void INetworkSnapshot.WriteSnapshot( ref ByteStream writer )
	{
		var serialized = Serialize().Compress();
		writer.Write( serialized );
	}
	#endregion

	/// <summary>
	/// This is called by the host to serialize the current world and send it to the connecting client.
	/// </summary>
	/// <returns></returns>
	public virtual byte[] Serialize()
	{
		return [];
	}

	/// <summary>
	/// This is called upon snapshot loading, we apply the snapshot data to the world here.
	/// </summary>
	/// <param name="data"></param>
	public virtual void Deserialize( byte[] data )
	{

	}
}
