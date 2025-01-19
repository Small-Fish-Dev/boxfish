namespace Boxfish.Library;

partial class VoxelVolume
{
	/// <summary>
	/// The frequency of chunk mesh updates, should be somewhere around 1 / 30f (used for <see cref="TickUpdate"/>).
	/// <para>NOTE: Set to 0 to completely ignore updates.</para>
	/// </summary>
	public virtual float UpdateFrequency { get; } = 1 / 30f;

	/// <summary>
	/// The time passed since <see cref="TickUpdate"/> has been called.
	/// </summary>
	protected TimeSince SinceUpdate { get; private set; }

	/// <summary>
	/// Which chunks have been tracked to change.
	/// </summary>
	protected readonly Dictionary<Chunk, bool> TrackedChanges = new();

	/// <summary>
	/// Track a voxel change, we will go through these in <see cref="TickUpdate"/>.
	/// </summary>
	/// <param name="chunk"></param>
	/// <param name="changed"></param>
	/// <param name="offset"></param>
	protected void TrackChange( Chunk chunk, bool changed, (byte x, byte y, byte z)? offset = null )
	{
		// Updates are disabled.
		if ( UpdateFrequency == 0 ) return;

		if ( chunk == null )
			return;

		// Add tracked chunk.
		if ( TrackedChanges.ContainsKey( chunk ) )
		{
			TrackedChanges[chunk] |= changed;
		}
		else
		{
			TrackedChanges.Add( chunk, changed );
		}

		// Add neighbors to rebuild visual mesh.
		if ( offset.HasValue )
		{
			var pos = offset.Value;
			var neighbors = chunk.GetNeighbors( pos.x, pos.y, pos.z, false );
			foreach ( var neighbor in neighbors )
			{
				if ( neighbor == null )
					continue;

				TrackedChanges.TryAdd( neighbor, false );
			}

			return;
		}
	}

	/// <summary>
	/// Set a voxel and track the change, this will automatically regenerate chunks if you have ticked updates enabled.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="voxel"></param>
	/// <param name="relative"></param>
	public virtual void SetTrackedVoxel( Vector3Int position, Voxel voxel, Chunk relative = null )
	{
		// Get local position to a chunk, set the voxel.
		var pos = GetLocalSpace( position.x, position.y, position.z, out var chunk, relative );
		if ( chunk is null )
		{
			if ( !voxel.Valid )
				return;

			chunk = GetOrCreateChunk( position, pos, relative );
		}

		if ( chunk is null )
		{
			Log.Warning( $"Failed to GetOrCreate chunk, this shouldn't happen." );
			return;
		}

		var previous = chunk.GetVoxel( pos.x, pos.y, pos.z );
		chunk.SetVoxel( pos.x, pos.y, pos.z, voxel );

		// Track change.
		var prevValid = previous.Valid;
		var newValid = voxel.Valid;
		var changed = (!prevValid && newValid) || (prevValid && !newValid);
		TrackChange( chunk, changed, pos );
	}

	/// <inheritdoc cref="SetTrackedVoxel(Vector3Int, Voxel, BaseVoxelVolume{Voxel, VoxelVertex}.Chunk)"/>
	public void SetTrackedVoxel( int x, int y, int z, Voxel voxel, Chunk relative = null )
	{
		var position = new Vector3Int( x, y, z );
		SetTrackedVoxel( position, voxel, relative );
	}

	/// <summary>
	/// Tick update the changed chunks.
	/// <para>NOTE: This will update the chunks directly, you shouldn't call this multiple times a frame.</para>
	/// </summary>
	public void TickUpdate()
	{
		if ( TrackedChanges.Any() )
		{
			var chunks = new KeyValuePair<Chunk, bool>[TrackedChanges.Count];
			(TrackedChanges as ICollection<KeyValuePair<Chunk, bool>>).CopyTo( chunks, 0 );
			TrackedChanges.Clear();

			_ = Task.RunInThreadAsync( () => _ = GenerateMeshes( chunks ) );
			SinceUpdate = 0f;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Tick updates.
		if ( SinceUpdate >= UpdateFrequency )
			TickUpdate();
	}
}
