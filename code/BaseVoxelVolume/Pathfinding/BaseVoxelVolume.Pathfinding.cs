namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// Check if there's only air above
	/// </summary>
	/// <param name="position"></param>
	/// <param name="amount"></param>
	/// <returns></returns>
	public bool IsAboveFree( Vector3Int position, int amount = 3 )
	{
		for ( int z = 1; z <= amount; z++ )
			if ( Query( position + new Vector3Int( 0, 0, z ) ).HasVoxel )
				return false;

		return true;
	}

	/// <summary>
	/// Find the nearest walkable voxel, EXPENSIVE! Only checks below, no reason to check up
	/// </summary>
	/// <param name="point"></param>
	/// <param name="maxDistance"></param>
	/// <param name="possibleTokenSource"></param>
	/// <returns></returns>
	public VoxelQueryData? GetClosestWalkable( Vector3Int point, int maxDistance = 400, CancellationTokenSource possibleTokenSource = null )
	{
		if ( !this.IsValid() )
			return null;

		var initial = Query( point );

		if ( initial.HasVoxel && IsAboveFree( initial.GlobalPosition ) ) // Worth a try!
			return initial;

		for ( int z = -1; z > -3; z-- )
		{
			try
			{
				if ( possibleTokenSource is { Token.IsCancellationRequested: true } ) return null;
			}
			catch ( ObjectDisposedException ) { return null; }

			var belowVoxel = Query( point + new Vector3Int( 0, 0, z ) );

			if ( belowVoxel.HasVoxel && IsAboveFree( belowVoxel.GlobalPosition ) ) // Worth a shot!
				return belowVoxel;
		}

		var directions = new Vector3Int[]
		{
			new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
			new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
			new Vector3Int(0, 0, -1)
		};

		var queue = new Queue<Vector3Int>();
		var visited = new HashSet<Vector3Int>();

		queue.Enqueue( initial.GlobalPosition );
		visited.Add( initial.GlobalPosition );

		while ( queue.Count > 0 )
		{
			try
			{
				if ( possibleTokenSource is { Token.IsCancellationRequested: true } ) return null;
			}
			catch ( ObjectDisposedException ) { return null; }

			var currentPos = queue.Dequeue();
			if ( !this.IsValid() || currentPos.Distance( initial.GlobalPosition ) > maxDistance )
				continue;

			var current = Query( currentPos );

			if ( current.HasVoxel )
			{
				if ( IsAboveFree( current.GlobalPosition, 3 ) )
					return current;

				// Since this voxel isn't air we know the ones below aren't available
				visited.Add( current.GlobalPosition - new Vector3Int( 0, 0, 1 ) );
				visited.Add( current.GlobalPosition - new Vector3Int( 0, 0, 2 ) );
				visited.Add( current.GlobalPosition - new Vector3Int( 0, 0, 3 ) );
			}

			foreach ( var direction in directions )
			{
				var neighbor = current.GlobalPosition + direction;

				if ( !visited.Contains( neighbor ) )
				{
					queue.Enqueue( neighbor );
					visited.Add( neighbor );
				}
			}
		}

		return null;
	}


	/// <summary>
	/// Computes a path from the starting point to a target point. Reversing the path if needed.
	/// </summary>
	/// <param name="startNode">The starting point of the path.</param>
	/// <param name="endNode">The desired destination point of the path.</param>
	/// <param name="token">A cancellation token used to cancel computing the path.</param>
	/// <param name="maxDistance">Max distance it will search for, then will just approach the closest point</param>
	/// <param name="acceptPartial">If it doesn't find the target, return the closest node found</param>
	/// <param name="reversed">Whether or not to reverse the resulting path.</param>
	/// <returns>A path</returns>
	public async Task<AStarPath> ComputePath( AStarNode startNode, AStarNode endNode, CancellationToken token, float maxDistance = 2048f, bool acceptPartial = true, bool reversed = false )
	{
		var path = new List<AStarNode>();
		var maxCells = 8192;
		var openSet = new Heap<AStarNode>( maxCells );
		var openSetReference = new Dictionary<int, AStarNode>();
		var closedSet = new HashSet<AStarNode>();

		openSet.Add( startNode );
		openSetReference.Add( startNode.GetHashCode(), startNode );

		try
		{
			var cellsChecked = 0;

			while ( openSet.Count > 0 && cellsChecked < maxCells )
			{
				if ( token.IsCancellationRequested )
				{
					return new AStarPath( path );
				}

				var currentNode = openSet.RemoveFirst();
				closedSet.Add( currentNode );

				if ( currentNode.Data.GlobalPosition == endNode.Data.GlobalPosition )
				{
					RetracePath( ref path, startNode, currentNode );
					break;
				}

				foreach ( var neighbour in currentNode.GetNeighbours() )
				{
					if ( closedSet.Contains( neighbour ) )
						continue;

					var isInOpenSet = openSetReference.ContainsKey( neighbour.GetHashCode() );
					var currentNeighbour = isInOpenSet ? openSetReference[neighbour.GetHashCode()] : neighbour;
					var newMovementCostToNeighbour = currentNode.gCost + currentNode.Distance( currentNeighbour );
					var distanceToTarget = currentNeighbour.Distance( endNode );

					if ( distanceToTarget > maxDistance ) continue;

					if ( newMovementCostToNeighbour < currentNeighbour.gCost || !isInOpenSet )
					{
						currentNeighbour.gCost = newMovementCostToNeighbour;
						currentNeighbour.hCost = distanceToTarget;
						currentNeighbour.Parent = currentNode;

						if ( !isInOpenSet )
						{
							openSet.Add( currentNeighbour );
							openSetReference.Add( currentNeighbour.GetHashCode(), currentNeighbour );
						}
					}
				}

				cellsChecked++;
			}
		}
		catch ( OperationCanceledException )
		{
			await Task.CompletedTask;
			return new AStarPath( path );
		}

		if ( path.Count == 0 && acceptPartial )
		{
			var closestNode = closedSet
				.OrderBy( x => x.hCost )
				.FirstOrDefault( x => x.gCost != 0f );

			if ( closestNode != null )
			{
				RetracePath( ref path, startNode, closestNode );
			}
		}

		if ( reversed )
		{
			path.Reverse();
		}

		await Task.CompletedTask;
		return new AStarPath( path );
	}

	private static void RetracePath( ref List<AStarNode> pathList, AStarNode startNode, AStarNode targetNode )
	{
		if ( pathList == null )
			return;
		if ( targetNode == null || startNode == null ) return;

		var currentNode = targetNode;

		while ( currentNode != startNode )
		{
			pathList.Add( currentNode );
			currentNode = currentNode.Parent;
		}
		pathList.Reverse();

		var fixedList = new List<AStarNode>();

		foreach ( var node in pathList )
		{
			if ( node.Parent == null )
				continue;

			var newNode = new AStarNode( node.Parent.Data, node ) { Volume = startNode.Volume };
			fixedList.Add( newNode );
		}

		pathList = fixedList;
		//pathList = pathList.Select( node => new AStarNode( node.Parent.Current, node, node.MovementTag ) ).ToList(); // Cell connections are flipped when we reversed earlier
	}

	public bool LineOfSight( AStarNode start, AStarNode end, bool debugShow = false )
	{
		var startingPosition = start.Data.GlobalPosition;
		var endingPosition = end.Data.GlobalPosition;
		var distanceInSteps = (int)Math.Ceiling( VoxelToWorld( startingPosition ).Distance( VoxelToWorld( endingPosition ) ) / Scale );

		var lastNode = start;
		for ( int i = 0; i <= distanceInSteps; i++ )
		{
			var direction = (endingPosition - lastNode.Data.GlobalPosition).Normal;
			var nodeToCheck = lastNode.GetNeighbourInDirection( direction );
			if ( nodeToCheck == null ) return false;

			//if ( debugShow )
			//	DebugOverlay.BBox( BBox.FromPositionAndSize( VoxelToWorld( nodeToCheck.Data.GlobalPosition ) + VoxelUtility.Scale / 2f, VoxelUtility.Scale * 1.1f ), DebugStyle.Solid, Color.Gray, 1f );

			if ( nodeToCheck == end ) return true;
			if ( nodeToCheck == lastNode ) continue;
			if ( !nodeToCheck.IsNeighbour( lastNode ) ) return false;

			lastNode = nodeToCheck;

		}

		return true;
	}
}
