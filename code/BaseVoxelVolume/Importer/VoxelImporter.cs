namespace Boxfish;

/// <summary>
/// Derive from this to add your own importers.
/// </summary>
public abstract class VoxelFormat
{
	/// <summary>
	/// Array of all supported extensions for this format.
	/// </summary>
	public abstract string[] Extensions { get; }

	/// <summary>
	/// This method is executed during the BuildAsync process of <see cref="VoxelImporter"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	/// <param name="rawData"></param>
	/// <param name="importer"></param>
	/// <returns></returns>
	public abstract Task<Dictionary<Vector3Int, BaseVoxelVolume<T, U>.Chunk>> Parse<T, U>( byte[] rawData, VoxelImporter importer ) 
		where T : struct
		where U : unmanaged;
}

/// <summary>
/// Implement this struct on your own voxel struct if you want to import with the Boxfish importers.
/// </summary>
/// <typeparam name="T">Same types as your Voxel.</typeparam>
public interface IColorImporter<T>
	where T : struct
{
	virtual T Create( Color32 color ) { return default ( T ); }
}

/// <summary>
/// Our builder struct for importing voxel models.
/// </summary>
public struct VoxelImporter
{
#pragma warning disable SB3000 // Hotloading not supported
	private static List<VoxelFormat> _formats { get; set; }
#pragma warning restore SB3000 // Hotloading not supported

	public string Path { get; set; }
	public VoxelFormat Format { get; set; }
	public string Extension => Path.Length > 1 ? System.IO.Path.GetExtension( Path )[1..] : string.Empty;

	private static void LoadFormats()
	{
		_formats = TypeLibrary.GetTypes<VoxelFormat>()?
			.Where( type => !type.IsAbstract )
			.Select( type => type.Create<VoxelFormat>() )
			.ToList();
	}

	/// <summary>
	/// Begin building map data from a file path.
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static VoxelImporter FromPath( string path )
	{
		var builder = new VoxelImporter()
		{
			Path = path,
		};

		// Tried to input empty path?
		if ( string.IsNullOrEmpty( path ) )
		{
			Logger.Warning( $"Cannot create MapBuilder with empty path." );
			return builder;
		}

		// Make sure path exists.
		if ( !FileSystem.Mounted.FileExists( path ) )
		{
			Logger.Warning( $"File at \"{path}\" does not exist." );
			return builder;
		}

		// Try fetch format.
		LoadFormats();

		var format = _formats?.FirstOrDefault( format => format.Extensions?.Contains( builder.Extension ) ?? false );
		if ( format is null )
		{
			Logger.Warning( $"No valid voxel format for file extension \"{builder.Extension}\" found." );
			return builder;
		}

		builder.Format = format;

		return builder;
	}

	/// <summary>
	/// Override the format of this builder.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <returns></returns>
	public VoxelImporter WithFormat<U>() 
		where U : VoxelFormat
	{
		return this with
		{
			Format = TypeLibrary.Create<U>()
		};
	}

	/// <summary>
	/// Build voxel data from the map data asynchronously.
	/// </summary>
	/// <returns></returns>
	public async Task<Dictionary<Vector3Int, BaseVoxelVolume<T, U>.Chunk>> BuildAsync<T, U>()
		where T : struct
		where U : unmanaged
	{
		// Format was null?
		if ( Format is null )
		{
			Logger.Warning( $"Tried to build map with no valid voxel format? [ext: {Extension}]" );
			return null;
		}

		// Read file and parse.
		var result = (Dictionary<Vector3Int, BaseVoxelVolume<T, U>.Chunk>)null;
		var content = await FileSystem.Mounted.ReadAllBytesAsync( Path );

		// No binary content found?
		if ( content is null )
		{
			return null;
		}

		result = await Format.Parse<T, U>( content, this );

		// Result was null?
		if ( result is null )
		{
			Logger.Warning( $"Voxel importer failed to import, result was null?" );
		}

		return result;
	}
}
