namespace Boxfish;

partial class VoxFormat : VoxelFormat
{
	public override string[] Extensions { get; } = ["vox"];

#pragma warning disable SB3000 // Hotloading not supported
	private static uint[] defaultPalette =
	[
		0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
		0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
		0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
		0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
		0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
		0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
		0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
		0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
		0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
		0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
		0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
		0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
		0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
		0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
		0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
		0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
	];

	private static Color32[] Palette = defaultPalette.Select( i => new Color32( i ) ).ToArray();
	private static IReadOnlyDictionary<string, Type> Chunks = new Dictionary<string, Type>()
	{
		["MAIN"] = typeof( Chunk ),
		["SIZE"] = typeof( SizeChunk ),
		["XYZI"] = typeof( XYZIChunk ),
		["RGBA"] = typeof( RGBAChunk ),

		["nSHP"] = typeof( ShapeChunk ),
		["nGRP"] = typeof( GroupChunk ),
		["nTRN"] = typeof( TransformChunk )
	};
#pragma warning restore SB3000 // Hotloading not supported

	private static Chunk ReadChunk( BinaryReader reader )
	{
		var id = new string( reader.ReadChars( 4 ) );
		if ( id.Length == 0 )
			return null;

		// Read haed.
		var size = reader.ReadInt32();
		var childSize = reader.ReadInt32();
		var data = reader.ReadBytes( size );
		if ( !Chunks.TryGetValue( id, out var type ) )
		{
			//Logger.Warning( $"VoxParser unsupported chunk ID {id}. " );
			return new Chunk();
		}

		// Construct and read chunk.
		Chunk chunk;
		using ( var contentStream = new MemoryStream( data ) )
		using ( var content = new BinaryReader( contentStream ) )
		{
			chunk = (Chunk)TypeLibrary.Create( type.FullName, type, new object[] { content } );
		}

		chunk.Bytes = size;
		chunk.Identifier = id;

		var children = new List<Chunk>();
		var read = 0;

		// Read children.
		while ( read < childSize )
		{
			var child = ReadChunk( reader );
			if ( child == null )
				break;

			children.Add( child );
			read += child.Bytes;
		}

		chunk.Children = children.ToArray();

		return chunk;
	}

	private static string ReadString( BinaryReader reader )
	{
		var length = reader.ReadInt32();
		var characters = reader.ReadChars( length );
		return new string( characters );
	}

	private static Dictionary<string, string> ReadDictionary( BinaryReader reader )
	{
		var capacity = reader.ReadInt32();
		var dictionary = new Dictionary<string, string>( capacity );
		for ( int i = 0; i < capacity; i++ )
			dictionary[ReadString( reader )] = ReadString( reader );
		return dictionary;
	}

	public async override Task<Dictionary<Vector3Int, BaseVoxelVolume<T, U>.Chunk>> Parse<T, U>( byte[] rawData, VoxelImporter importer )
	{
		var colorImporter = default( T ) as IColorImporter<T>;
		if ( colorImporter is null )
		{
			Logger.Warning( $"VoxFormat requires IColorImporter<T> to be implemented in your voxel struct." );
			return null;
		}

		using var stream = new MemoryStream( rawData );
		using var reader = new BinaryReader( stream );

		// Let's read the root chunk.
		var id = new string( reader.ReadChars( 4 ) );
		var version = reader.ReadInt32();
		var main = ReadChunk( reader );
		if ( main is null || main.Children?.Length == 0 )
		{
			Logger.Warning( $"VoxParser main chunk was empty?" );
			return null;
		}

		// Get the chunks that we need to construct our voxel chunks.
		var palette = main.GetChild<RGBAChunk>()?.Palette ?? Palette;
		var chunks = new Dictionary<Vector3Int, BaseVoxelVolume<T, U>.Chunk>();
		void SetVoxel( int x, int y, int z, T voxel )
		{
			var position = new Vector3Int(
				((float)x / VoxelUtils.CHUNK_SIZE).FloorToInt(),
				((float)y / VoxelUtils.CHUNK_SIZE).FloorToInt(),
				((float)z / VoxelUtils.CHUNK_SIZE).FloorToInt()
			);

			if ( !chunks.TryGetValue( position, out var chunk ) )
				chunks[position] = chunk = new BaseVoxelVolume<T, U>.Chunk( position.x, position.y, position.z, chunks );

			var localSpace = (
				x: (byte)((x % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE),
				y: (byte)((y % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE),
				z: (byte)((z % VoxelUtils.CHUNK_SIZE + VoxelUtils.CHUNK_SIZE) % VoxelUtils.CHUNK_SIZE)
			);

			chunk.SetVoxel( localSpace.x, localSpace.y, localSpace.z, voxel );
		}

		var shapes = main.GetChildren<ShapeChunk>();
		var transformNodes = main.GetChildren<TransformChunk>();
		var models = main.GetChildren<XYZIChunk>();
		var sizes = main.GetChildren<SizeChunk>();
		var targetModels = models
			.Select( model => (Model: model, TransformIndex: 0) )
			.ToList();

		var bounds = (mins: Vector3Int.Zero, maxs: Vector3Int.Zero);
		var transforms = new Dictionary<(Chunk Chunk, int Index), ChunkTransform>();
		var indices = new Dictionary<Chunk, int>();

		foreach ( var transformNode in transformNodes )
		{
			var transform = transformNode.Transform;
			var shape = shapes.FirstOrDefault( shape => shape.ID == transformNode.ChildNode );
			if ( shape is null )
				continue;

			foreach ( var modelIndex in shape.ModelIDs )
			{
				var model = models.ElementAtOrDefault( modelIndex );
				if ( model is null )
					continue;

				_ = indices.TryGetValue( model, out var index );
				transforms[(model, index)] = transform;
				if ( index >= 1 )
					targetModels.Add( (model, index) );

				indices[model] = index + 1;
			}
		}

		foreach ( var sizeChunk in sizes )
		{
			var size = new Vector3Int( sizeChunk.x, sizeChunk.y, sizeChunk.z );
			var index = Array.IndexOf( main.Children, sizeChunk );
			var model = main.Children.ElementAtOrDefault( index + 1 ) as XYZIChunk;
			if ( model is null ) continue;

			if ( !transforms.TryGetValue( (model, 0), out var transform ) )
				transform = default;

			var offset = transform.Translation;
			var position = offset - size / 2;

			if ( position.x < bounds.mins.x ) bounds.mins.x = position.x;
			if ( position.y < bounds.mins.y ) bounds.mins.y = position.y;
			if ( position.z < bounds.mins.z ) bounds.mins.z = position.z;

			if ( position.x > bounds.maxs.x ) bounds.maxs.x = position.x;
			if ( position.y > bounds.maxs.y ) bounds.maxs.y = position.y;
			if ( position.z > bounds.maxs.z ) bounds.maxs.z = position.z;
		}

		foreach ( var (model, transformIndex) in targetModels )
		{
			var index = Array.IndexOf( main.Children, model );
			var size = main.Children.ElementAtOrDefault( index - 1 ) as SizeChunk;
			if ( size is null ) continue;

			var length = model.Values.Length;
			if ( !transforms.TryGetValue( (model, transformIndex), out var transform ) )
				transform = default;

			var offset = transform.Translation;
			offset -= new Vector3Int( size.x, size.y, size.z ) / 2;
			offset -= bounds.mins;

			for ( int i = 0; i < length; i++ )
			{
				var data = model.Values[i];
				var voxel = new Vector3Int( data.x, data.y, data.z ) + offset;
				var color = palette[data.i];

				SetVoxel( voxel.x, voxel.y, voxel.z, colorImporter.Create( color ) );
			}
		}

		stream.Dispose();
		await GameTask.CompletedTask;
		return chunks;
	}
}
