namespace Boxfish.Components;

/// <summary>
/// Default voxel struct used for the <see cref="VoxelVolume"/> component.
/// </summary>
public struct Voxel
{
	public byte R {  get; set; }
	public byte G { get; set; }
	public byte B { get; set; }
	public bool Valid { get; set; }

	/// <summary>
	/// The index of our voxel's texture in our atlas.
	/// </summary>
	public ushort Texture { get; set; }

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

	public Voxel( Color32 color, ushort texture ) :	this()
	{
		Color = color;
		Texture = texture;
	}
}
