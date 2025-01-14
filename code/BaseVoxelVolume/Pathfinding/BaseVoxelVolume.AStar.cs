namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// We store information about A* nodes and some utility methods inside of this class.
	/// </summary>
	public class AStarNode : IHeapItem<AStarNode>, IEquatable<AStarNode>, IValid
	{
		public VoxelQueryData Data { get; internal set; }
		public BaseVoxelVolume<T, U> Volume { get; set; }
		public float gCost { get; set; } = 0f;
		public float hCost { get; set; } = 0f;
		public float fCost => gCost + hCost;
		public int HeapIndex { get; set; }
		public bool IsValid = false;
		bool IValid.IsValid => IsValid;
		public AStarNode Parent;

		public AStarNode() { }

		public AStarNode( VoxelQueryData data, AStarNode parent = null )
		{
			Data = data;
			Parent = parent;
			IsValid = true;
		}

		/// <summary>
		/// Get all neighbouring voxels, even ones we can jump on or land down
		/// </summary>
		/// <returns></returns>
		public IEnumerable<AStarNode> GetNeighbours()
		{
			for ( int x = -1; x <= 1; x++ )
				for ( int y = -1; y <= 1; y++ )
					for ( int z = -2; z <= 2; z++ )
					{
						if ( x == 0 && y == 0 ) continue; // Erm don't check the blocks we're on?
						var offset = new Vector3Int( x, y, z );
						var checkPosition = Data.GlobalPosition + offset;
						var queryData = Volume.Query( checkPosition );

						if ( !queryData.HasVoxel ) continue; // Not solid ground to stand
						if ( !Volume.IsAboveFree( checkPosition, Math.Min( 3, 3 - z ) ) ) continue; // Space is not free for us to occupy (DEFAULT IS 3 BLOCKS TALL, WILL MODIFY
						z = 3; // If you think about it, we checked the above voxels already

						yield return new AStarNode( queryData, this ) { Volume = Volume };
					}
		}

		/// <summary>
		/// Get the distance between the node's voxel's position
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public float Distance( AStarNode other ) => Data.GlobalPosition.Distance( other.Data.GlobalPosition );

		/// <summary>
		/// Get the horizontal distance between the node's voxel's position
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public float HorizontalDistance( AStarNode other ) => Data.GlobalPosition.WithZ( 0 ).Distance( other.Data.GlobalPosition.WithZ( 0 ) );

		public bool IsNeighbour( AStarNode node )
		{
			var xDistance = node.Data.GlobalPosition.x - Data.GlobalPosition.x;
			var yDistance = node.Data.GlobalPosition.y - Data.GlobalPosition.y;

			if ( xDistance < -1 || xDistance > 1 || yDistance < -1 || yDistance > 1 ) return false;
			if ( node == this ) return true;
			if ( xDistance == 0 && yDistance == 0 ) return false;

			foreach ( var neighbour in GetNeighbours() )
				if ( node == neighbour ) return true;

			return false;
		}

		/// <summary>
		/// Returns the neighbour in that direction
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public AStarNode GetNeighbourInDirection( Vector3 direction )
		{
			var horizontalDirection = direction.WithZ( 0 ).Normal;
			var localCoordinates = new Vector2Int( MathX.FloorToInt( horizontalDirection.x + 0.5f ), MathX.FloorToInt( horizontalDirection.y + 0.5f ) );
			var coordinatesToCheck = Data.GlobalPosition + localCoordinates;

			foreach ( var neighbour in GetNeighbours() )
				if ( neighbour.Data.GlobalPosition.x == coordinatesToCheck.x && neighbour.Data.GlobalPosition.y == coordinatesToCheck.y )
					return neighbour;

			return null;
		}

		public int CompareTo( AStarNode other )
		{
			var compare = fCost.CompareTo( other.fCost );
			if ( compare == 0 )
				compare = hCost.CompareTo( other.hCost );
			return -compare;
		}

		public static bool operator ==( AStarNode a, AStarNode b )
		{
			if ( ReferenceEquals( a, b ) ) return true; // Both are the same instance or both are null
			if ( a is null || b is null ) return false; // One is null and the other is not

			return a.Equals( b );
		}

		public static bool operator !=( AStarNode a, AStarNode b ) => !(a == b);

		public override bool Equals( object obj )
		{
			if ( obj == null ) return false;
			if ( obj is not AStarNode node ) return false;

			return Data.GlobalPosition.Equals( node.Data.GlobalPosition );
		}

		public bool Equals( AStarNode other )
		{
			if ( other == null ) return false;

			return Data.GlobalPosition.Equals( other.Data.GlobalPosition );
		}

		public override int GetHashCode()
		{
			return Data.GlobalPosition.GetHashCode();
		}
	}

	/// <summary>
	/// This struct defines the path full A* path.
	/// </summary>
	public struct AStarPath
	{
		public List<AStarNode> Nodes { get; private set; } = new();
		public int Count => Nodes?.Count() ?? 0;
		public bool IsEmpty => Nodes == null || Count == 0;

		public AStarPath() { }

		public AStarPath( List<AStarNode> nodes ) : this()
		{
			Nodes = nodes;
		}

		/// <summary>
		/// Return the total length of the path
		/// </summary>
		/// <returns></returns>
		public float GetLength()
		{
			var length = 0f;

			for ( int i = 0; i < Nodes.Count - 1; i++ )
				length += Nodes[i].Data.GlobalPosition.Distance( Nodes[i + 1].Data.GlobalPosition );

			return length;
		}

		/// <summary>
		/// Simplify the path by iterating over line of sights between the given segment size, joining them if valid
		/// </summary>
		/// <param name="segmentAmounts"></param>
		/// <param name="iterations"></param>
		/// <returns></returns>
		public void Simplify( int segmentAmounts = 2, int iterations = 1 ) // TODO DOESN'T WORK THAT WELL
		{
			for ( int iteration = 0; iteration < iterations; iteration++ )
			{
				var segmentStart = 0;
				var segmentEnd = Math.Min( segmentAmounts, Count - 1 );

				while ( Count > 2 && segmentEnd < Count - 1 )
				{
					var currentNode = Nodes[segmentStart];
					var nextNode = Nodes[segmentStart + 1];
					var furtherNode = Nodes[segmentEnd];

					if ( currentNode.Volume.LineOfSight( currentNode, furtherNode, false ) )
						for ( int toDelete = segmentStart + 1; toDelete < segmentEnd; toDelete++ )
							Nodes.RemoveAt( toDelete );

					if ( segmentEnd == Count - 1 )
						break;

					segmentStart++;
					segmentEnd = Math.Min( segmentStart + segmentAmounts, Count - 1 );
				}
			}
		}
	}
}
