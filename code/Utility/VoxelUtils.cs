namespace Boxfish.Utility;

public enum FacesEnum : byte
{
	Up = 0,
	Down = 1, 
	Backward = 2,
	Left = 3,
	Forward = 4,
	Right = 5
}

/// <summary>
/// A static class containing constants and other utility.
/// </summary>
public static class VoxelUtils
{
	public const int CHUNK_SIZE = 16;
	public const float METER = 1f / 0.0254f;

	/// <summary>
	/// Indices for cube faces.
	/// </summary>
	public static readonly byte[] FaceIndices = 
	[
		0, 1, 2, 3,
		7, 6, 5, 4,
		0, 4, 5, 1,
		1, 5, 6, 2,
		2, 6, 7, 3,
		3, 7, 4, 0
	];

	/// <summary>
	/// Directions for each face.
	/// </summary>
	public static readonly (sbyte x, sbyte y, sbyte z)[] Directions = 
	[
		(0, 0, 1),
		(0, 0, -1),
		(-1, 0, 0),
		(0, 1, 0),
		(1, 0, 0),
		(0, -1, 0)
	];

	/// <summary>
	/// The offsets for each face of a cube.
	/// </summary>
	public static readonly Vector3[] Positions = 
	[
		new Vector3( -0.5f, -0.5f, 0.5f ),
		new Vector3( -0.5f, 0.5f, 0.5f ),
		new Vector3( 0.5f, 0.5f, 0.5f ),
		new Vector3( 0.5f, -0.5f, 0.5f ),
		new Vector3( -0.5f, -0.5f, -0.5f ),
		new Vector3( -0.5f, 0.5f, -0.5f ),
		new Vector3( 0.5f, 0.5f, -0.5f ),
		new Vector3( 0.5f, -0.5f, -0.5f )
	];
}
