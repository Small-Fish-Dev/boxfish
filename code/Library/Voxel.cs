namespace Boxfish.Library;

/// <summary>
/// Default voxel struct used for the <see cref="VoxelVolume"/> component.
/// </summary>
public struct Voxel : IColorImporter<Voxel>
{
	/// <summary>
	/// A static reference to an empty readonly voxel.
	/// </summary>
	public readonly static Voxel Empty = new Voxel() { Valid = false };

	public byte R;
	public byte G;
	public byte B;
	public bool Valid;

	/// <summary>
	/// The index of our voxel's texture in our atlas.
	/// <para>NOTE: We only have 12-bits of numbers available, so the max amount of textures is 4096.</para>
	/// </summary>
	public ushort Texture;

	/// <summary>
	/// The Color32 value of this voxel, NOTE: the alpha channel is ignored.
	/// </summary>
	public Color32 Color
	{
		get => new( R, G, B );
		set
		{
			R = value.r; 
			G = value.g;
			B = value.b;
		}
	}

	public Voxel()
	{
		Valid = true;
	}

	public Voxel( Color32 color, ushort texture = 0 ) :	this()
	{
		Color = color;
		Texture = texture;
	}

	// Importer.
	Voxel IColorImporter<Voxel>.Create( Color32 color )
	{
		return new Voxel( color );
	}
}
