namespace Boxfish.Components;

/// <summary>
/// A resource containing textures for a <see cref="VoxelVolume"/>.
/// <para>See https://sbox.game/boxfish/ or README.md for more explanation.</para>
/// </summary>
[GameResource( 
	"Voxel TextureAtlas", "voxatlas", "A resource containing textures for a VoxelVolume.",
	Category = "Boxfish", 
	Icon = "photo_library" 
)]
public sealed class AtlasResource : GameResource
{
	public class AtlasItem
	{
		[Header( "Item - Information" )]
		[KeyProperty, Placeholder( "Example Dirt" )]
		public string Name { get; set; }

		/// <summary>
		/// Do we cut out a region out of the RegionAtlas?
		/// </summary>
		[KeyProperty]
		public bool UseRegion { get; set; }

		/// <summary>
		/// Do we want to load this texture directly from an image?
		/// </summary>
		[KeyProperty, ImageAssetPath, ShowIf( nameof( UseRegion ), false )]
		public string Texture { get; set; }

		/// <summary>
		/// The region that we cut out from the RegionAtlas in pixels.
		/// </summary>
		[KeyProperty, ShowIf( nameof( UseRegion ), true )]
		public RectInt Region { get; set; }

		/// <summary>
		/// Is our voxel opaque? 
		/// <para>This determines if we should render voxels that are directly behind it.</para>
		/// </summary>
		[KeyProperty]
		public bool Opaque { get; set; }

		/// <summary>
		/// Called when a block is broken.
		/// </summary>
		[Header( "Item - Callbacks" )]
		[KeyProperty, Feature( "Callbacks" )]
		public Action<VoxelVolume.VoxelQueryData> OnBlockPlaced { get; set; }

		/// <summary>
		/// Called when a block is broken.
		/// </summary>
		[KeyProperty, Feature( "Callbacks" )]
		public Action<VoxelVolume.VoxelQueryData> OnBlockBroken { get; set; }

		/// <inheritdoc cref="_data" />
		[Header( "Item - Data" )]
		[KeyProperty, Feature( "Data" )]
		[JsonIgnore, Hide]
		public IReadOnlyDictionary<string, string> Data => _data;

		/// /// <summary>
		/// Container for an atlas item's custom data.
		/// <para>You can use this for embedding stuff like health, or step sounds inside the atlas item itself, then fetching those values through code.</para>
		/// </summary>
		[Header( "Item - Data" )]
		[KeyProperty, Feature( "Data" ), WideMode]
		private Dictionary<string, string> _data { get; set; }

		/// <summary>
		/// The index of our atlas item.
		/// </summary>
		[Hide, JsonIgnore]
		public ushort Index { get; set; }
	}

	/// <inheritdoc cref="_items" />
	[JsonIgnore, Hide] 
	public IReadOnlyList<AtlasItem> Items => _items;

	/// <summary>
	/// The compiled texture of this atlas.
	/// </summary>
	[Hide, JsonIgnore]
	public Texture Texture { get; private set; }

	/// <summary>
	/// Optional texture atlas used for regions.
	/// </summary>
	[JsonInclude, ImageAssetPath]
	public string RegionAtlas { get; set; }

	/// <summary>
	/// Size of each face's texture.
	/// </summary>
	[JsonInclude]
	public Vector2Int Size { get; set; } = 32;

	/// <summary>
	/// List of all the items in our atlas.
	/// </summary>
	[JsonInclude]
	[InlineEditor, WideMode, Space]
	[Header( "All atlas texture items should be added here!\nSee https://sbox.game/boxfish/ or README.md for more explanation." ), Title( "" )]
	private List<AtlasItem> _items { get; set; }

	private Color32[] ExtractColorRegion( Color32[] data, int dataWidth, int x, int y, int w, int h )
	{
		var region = new Color32[w * h];
		var position = 0;

		for ( int i = 0; i < w; i++ )
			for ( int j = 0; j < h; j++ )
			{
				var index = (y + i) * dataWidth + (x + j);
				region[position++] = data[index];
			}

		return region;
	}

	/// <summary>
	/// Look for an atlas item with the exact name.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="id"></param>
	/// <returns></returns>
	public bool TryFindByName( string name, out ushort id )
	{
		var item = _items.FirstOrDefault( x => x.Name == name );
		id = item?.Index ?? 0;
		return item != null;
	}

	/// <summary>
	/// Try get a specific atlas item by index.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool TryGet( int index, out AtlasItem item )
	{
		item = _items.ElementAtOrDefault( index );
		return item != null;
	}

	/// <summary>
	/// Get a specific atlas item at index.
	/// <para>NOTE: Can return null if index not within bounds.</para>
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public AtlasItem Get( int index ) => _items.ElementAtOrDefault( index );

	public void Build()
	{
		// Calculate required width and height.
		var depth = 6;

		if ( Items == null ) return;

		// Create our texture.
		Texture?.Dispose();
		Texture = Texture.CreateArray( Size.x, Size.y, Items.Count * depth )
			.WithName( $"atlas_albedo" )
			.Finish();

		// Go through all atlas items.
		var z = 0;
		var index = (ushort)0;

		// Try to load region atlas if needed.
		var regionPixels = default( Color32[] );
		var regionWidth = 0;
		if ( !string.IsNullOrEmpty( RegionAtlas ) )
		{ 
			var source = Texture.Load( FileSystem.Mounted, RegionAtlas, false );
			if ( source == null )
			{
				Logger.Warning( $"Failed to load texture atlas for AtlasResource, refer to https://sbox.game/fish/boxfish." );
				return;
			}

			regionWidth = source.Width;
			regionPixels = source.GetPixels();
		}

		foreach ( var item in Items )
		{
			item.Index = index;
			index++;

			z += depth;

			// Load region from atlas.
			if ( item.UseRegion )
			{
				if ( regionPixels == null )
				{
					Logger.Warning( $"{item.Name} at [{index - 1}] can't create item with UseRegion if no valid RegionAtlas was defined." );
					continue;
				}

				var allFaces = item.Region.Width == (int)Size.x * 6;
				if ( (item.Region.Width != (int)Size.x && !allFaces)
				   || item.Region.Height != (int)Size.y )
				{
					Logger.Warning( $"{item.Name} at [{index - 1}] has invalid Region size compared to expected TextureSize." );
					continue;
				}

				if ( allFaces )
				{
					for ( int i = 0; i < depth; i++ )
					{
						var buffer = ExtractColorRegion( regionPixels, regionWidth, item.Region.Left + i * Size.x, item.Region.Top, Size.x, Size.y );
						var pixels = buffer
							.SelectMany( v => new[] { v.r, v.g, v.b, v.a } )
							.ToArray();

						Texture.Update3D( pixels, 0, 0, z - depth + i, Size.x, Size.y, 1 );
					}
				}
				else
				{
					var buffer = ExtractColorRegion( regionPixels, regionWidth, item.Region.Left, item.Region.Top, Size.x, Size.y );
					var pixels = buffer
						.SelectMany( v => new[] { v.r, v.g, v.b, v.a } )
						.ToArray();

					// Uhh...
					for ( int i = 0; i < depth; i++ )
						Texture.Update3D( pixels, 0, 0, z - depth + i, Size.x, Size.y, 1 );
				}

				continue;
			}

			// Load from whole texture.
			if ( string.IsNullOrEmpty( item.Texture ) )
			{
				Logger.Warning( $"{item.Name} at [{index - 1}] had no texture set." );
				continue;
			}

			{
				var source = Texture.Load( FileSystem.Mounted, item.Texture, false );
				var allFaces = source.Width == (int)Size.x * 6;
				if ( source == null
					|| (source.Width != (int)Size.x && !allFaces)
					|| source.Height != (int)Size.y )
				{
					Logger.Warning( $"Failed to load texture for item {item.Name} at [{index - 1}], refer to https://sbox.game/fish/boxfish." );
					continue;
				}

				var sourcePixels = source.GetPixels();

				if ( allFaces )
				{
					for ( int i = 0; i < depth; i++ )
					{
						var buffer = ExtractColorRegion( sourcePixels, source.Width, Size.x * i, 0, Size.x, Size.y );
						var pixels = buffer
							.SelectMany( v => new[] { v.r, v.g, v.b, v.a } )
							.ToArray();

						Texture.Update3D( pixels, 0, 0, z - depth + i, Size.x, Size.y, 1 );
					}
				}
				else
				{
					var buffer = ExtractColorRegion( sourcePixels, source.Width, 0, 0, Size.x, Size.y );
					var pixels = buffer
						.SelectMany( v => new[] { v.r, v.g, v.b, v.a } )
						.ToArray();

					// Uhh...
					for ( int i = 0; i < depth; i++ )
						Texture.Update3D( pixels, 0, 0, z - depth + i, Size.x, Size.y, 1 );
				}
			}
		}

		Log.Info( $"Successfully built texture for our AtlasResource." );
	}

	protected override void PostLoad()
	{
		Build();
	}

	protected override void PostReload()
	{
		Build();
	}
}
