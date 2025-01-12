namespace Boxfish;

public partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// Derive from this to add your own importers.
	/// </summary>
	public abstract class VoxelFormat
	{
		public abstract string[] Extensions { get; }
		public abstract Task<Dictionary<Vector3Int, Chunk>> Parse( byte[] rawData, Importer importer );
	}

	/// <summary>
	/// Our builder struct for importing voxel models.
	/// </summary>
	public struct Importer
	{
#pragma warning disable SB3000 // Hotloading not supported
		private static List<VoxelFormat> _formats { get; set; }
#pragma warning restore SB3000 // Hotloading not supported

		public string Path { get; set; }
		public VoxelFormat Format { get; set; }
		public string Extension => Path.Length > 1 ? System.IO.Path.GetExtension( Path )[1..] : string.Empty;
		public Func<Color32, T> ColorImporter { get; set; }

		private static void LoadFormats()
		{
			_formats = TypeLibrary.GetTypes<BaseVoxelVolume<T, U>.VoxelFormat>()?
				.Where( type => !type.IsAbstract )
				.Select( type => type.Create<VoxelFormat>() )
				.ToList();
		}

		/// <summary>
		/// Begin building map data from a file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Importer Create( string path )
		{
			var builder = new Importer()
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
			foreach ( var adasa in _formats ) Log.Error( string.Join( ", ", adasa.Extensions ) );
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
		/// <typeparam name="V"></typeparam>
		/// <returns></returns>
		public Importer WithFormat<V>() where V : VoxelFormat
		{
			return this with
			{
				Format = TypeLibrary.Create<V>()
			};
		}

		/// <summary>
		/// Adds a color importer to this builder.
		/// </summary>
		/// <param name="func"></param>
		/// <returns></returns>
		public Importer WithColorImporter( Func<Color32, T> func )
		{
			return this with
			{
				ColorImporter = func
			};
		}

		/// <summary>
		/// Build voxel data from the map data asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task<Dictionary<Vector3Int, Chunk>> BuildAsync()
		{
			// Format was null?
			if ( Format is null )
			{
				Logger.Warning( $"Tried to build map with no valid voxel format? [ext: {Extension}]" );
				return new();
			}

			// Read file and parse.
			var result = (Dictionary<Vector3Int, Chunk>)null;
			var content = await FileSystem.Mounted.ReadAllBytesAsync( Path );
			result = await Format.Parse( content, this );

			// Result was null?
			if ( result is null )
			{
				Logger.Warning( $"Voxel importer failed to import, result was null?" );
			}

			return result;
		}
	}
}
