namespace Boxfish.Components;

[StructLayout( LayoutKind.Sequential )]
public struct VoxelVertex
{
	// x = 4 bits
	// y = 4 bits
	// z = 4 bits
	// Face = 3 bits
	// AO = 2 bits
	// Vertex Index = 3 bits
	// Texture Index = 12 bits
	private readonly uint data;

	// R = 8 bits
	// G = 8 bits
	// B = 8 bits
	// A = 8 bits (not used, for now)
	private readonly uint data2;

	public static readonly VertexAttribute[] Layout = new VertexAttribute[1]
	{
		new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.UInt32, 3, 10 ),
	};

	public VoxelVertex( byte x, byte y, byte z, byte vertex, byte face, byte ao, Voxel voxel )
	{
		var data = ((voxel.Texture & 0xFFF) << 20)
			| ((vertex & 0x7) << 17)
			| ((ao & 0x3) << 15)
			| ((face & 0x7) << 12)
			| ((z & 0xF) << 8) | ((y & 0xF) << 4) | (x & 0xF);

		this.data = (uint)data;
		this.data2 = voxel.Color.RawInt;
	}
}
