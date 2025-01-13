namespace Boxfish;

partial class VoxFormat
{
	private struct ChunkTransform
	{
		public byte Rotation { get; set; }
		public Vector3Int Translation { get; set; }
	}

	private interface INodeID
	{
		public int ID { get; set; }
	}

	private class Chunk
	{
		public string Identifier { get; set; }
		public int Bytes { get; set; }
		public Chunk[] Children { get; set; }

		public Chunk() { }
		public Chunk( BinaryReader reader ) { }

		public V GetChild<V>() where V : Chunk
			=> (V)Children?.FirstOrDefault( c => typeof( V ) == c.GetType() );

		public IEnumerable<V> GetChildren<V>() where V : Chunk
			=> Children?.Where( c => typeof( V ) == c.GetType() )?.Cast<V>();
	}

	private class GroupChunk : Chunk, INodeID
	{
		public int ID { get; set; }
		public int[] ChildNodes { get; set; }

		public GroupChunk( BinaryReader reader ) : base( reader )
		{
			ID = reader.ReadInt32();
			_ = ReadDictionary( reader ); // Ignore attributes..

			var capacity = reader.ReadInt32();
			ChildNodes = new int[capacity];
			for ( int i = 0; i < capacity; i++ )
				ChildNodes[i] = reader.ReadInt32();
		}
	}

	private class ShapeChunk : Chunk, INodeID
	{
		public int ID { get; set; }
		public int[] ModelIDs { get; set; }

		public ShapeChunk( BinaryReader reader ) : base( reader )
		{
			ID = reader.ReadInt32();
			_ = ReadDictionary( reader ); // Ignore attributes..

			var capacity = reader.ReadInt32();
			ModelIDs = new int[capacity];
			for ( int i = 0; i < capacity; i++ )
			{
				ModelIDs[i] = reader.ReadInt32();
				_ = ReadDictionary( reader );
			}
		}
	}

	private class TransformChunk : Chunk, INodeID
	{
		public int ID { get; set; }
		public int ChildNode { get; set; }
		public ChunkTransform Transform { get; set; }

		public TransformChunk( BinaryReader reader ) : base( reader )
		{
			ID = reader.ReadInt32();
			_ = ReadDictionary( reader ); // Ignore attributes, don't really care about them...
			ChildNode = reader.ReadInt32();

			_ = reader.ReadInt32(); // int32 	: reserved id (must be -1)
			_ = reader.ReadInt32(); // int32	: layer id
			_ = reader.ReadInt32(); // int32	: num of frames (must be greater than 0)

			var frames = ReadDictionary( reader );
			foreach ( var (key, value) in frames )
			{
				switch ( key )
				{
					case "_r": // No rotation, yet...?
						break;

					case "_f": // What even is this..????
						break;

					case "_t":
						var split = value.Split( ' ' );
						try
						{
							var x = Convert.ToInt32( split[0] );
							var y = Convert.ToInt32( split[1] );
							var z = Convert.ToInt32( split[2] );
							Transform = Transform with
							{
								Translation = new Vector3Int( x, y, z )
							};
						}
						catch
						{
							Logger.Warning( $"VoxImporter - Invalid transform on TransformChunk nTRN" );
						}

						break;
				}
			}
		}
	}

	private class XYZIChunk : Chunk
	{
		public int Voxels { get; set; }
		public (byte x, byte y, byte z, byte i)[] Values { get; set; }

		public XYZIChunk( BinaryReader reader ) : base( reader )
		{
			var values = new List<(byte x, byte y, byte z, byte i)>();
			Voxels = reader.ReadInt32();
			for ( int i = 0; i < Voxels; i++ )
				values.Add( (
					x: reader.ReadByte(),
					y: reader.ReadByte(),
					z: reader.ReadByte(),
					i: reader.ReadByte()
				) );

			Values = values.ToArray();
		}
	}

	private class SizeChunk : Chunk
	{
		public int x { get; set; }
		public int y { get; set; }
		public int z { get; set; }

		public SizeChunk( BinaryReader reader ) : base( reader )
		{
			x = reader.ReadInt32();
			y = reader.ReadInt32();
			z = reader.ReadInt32();
		}
	}

	private class RGBAChunk : Chunk
	{
		public Color32[] Palette { get; set; }

		public RGBAChunk( BinaryReader reader ) : base( reader )
		{
			Palette = new Color32[256];

			for ( int i = 0; i <= 254; i++ )
			{
				var r = reader.ReadByte();
				var g = reader.ReadByte();
				var b = reader.ReadByte();
				var a = reader.ReadByte();

				Palette[i + 1] = new Color32( r, g, b, a );
			}
		}
	}
}

