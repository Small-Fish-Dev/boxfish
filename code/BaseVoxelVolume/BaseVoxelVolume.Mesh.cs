namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// Should we ignore out of bounds faces? 
	/// <para>NOTE: This does not affect the +Z faces.</para>
	/// </summary>
	public virtual bool IgnoreOOBFaces { get; } = true;

	/// <summary>
	/// Shrimple structure containing voxel mesh information such as vertices.
	/// </summary>
	protected struct VoxelMesh
	{
		public List<U> Vertices { get; } = new();
		public List<int> Indices { get; } = new();
		public Mesh Mesh { get; set; }

		public int Offset { get; set; }
		public bool Valid { get; set; }

		public VoxelMesh() { }

		public void CreateBuffers( VertexAttribute[] layout )
		{
			if ( Indices.Count == 0 )
				return;

			Valid = true;
			Mesh.CreateVertexBuffer<U>( Vertices.Count, layout, Vertices.ToArray() );
			Mesh.CreateIndexBuffer( Indices.Count, Indices.ToArray() );
		}
	}

	/// <summary>
	/// Our method that creates a single vertex based on the parameters.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="vertexIndex"></param>
	/// <param name="face"></param>
	/// <param name="ao"></param>
	/// <param name="voxel"></param>
	/// <returns></returns>
	public abstract U CreateVertex( Vector3Int position, int vertexIndex, int face, int ao, T voxel );

	/// <summary>
	/// Builds multiple chunk meshes.
	/// </summary>
	/// <param name="chunks"></param>
	/// <param name="physics"></param>
	/// <returns></returns>
	public async Task GenerateMeshes( IEnumerable<Chunk> chunks, bool physics = true )
	{
		Sandbox.Utility.Parallel.ForEach( chunks, chunk => _ = GenerateMesh( chunk, physics ) );
		await Task.CompletedTask;
	}

	/// <summary>
	/// Builds multiple chunk meshes with each one optionally getting physics.
	/// </summary>
	/// <param name="chunks"></param>
	/// <returns></returns>
	public async Task GenerateMeshes( IEnumerable<KeyValuePair<Chunk, bool>> chunks )
	{
		Sandbox.Utility.Parallel.ForEach( chunks, kvp => _ = GenerateMesh( kvp.Key, kvp.Value ) );
		await Task.CompletedTask;
	}

	/// <summary>
	/// Build a chunk mesh.
	/// </summary>
	/// <param name="chunk"></param>
	/// <param name="physics"></param>
	/// <returns></returns>
	public virtual async Task GenerateMesh( Chunk chunk, bool physics = true )
	{
		if ( chunk == null 
		  || Chunks == null 
		  || !Scene.IsValid() ) return;

		await Task.MainThread();
		var chunkObject = !Scene.IsEditor ? GetChunkObject( chunk ) : null;
		await Task.WorkerThread();

		// Let's create a VoxelMesh.
		var voxelMesh = new VoxelMesh();
		voxelMesh.Mesh = new Mesh( Material );

		var tested = new bool[VoxelUtils.CHUNK_SIZE, VoxelUtils.CHUNK_SIZE, VoxelUtils.CHUNK_SIZE];
		var collisionBuffer = new Utility.VertexBuffer();
		collisionBuffer.Init( true );

		chunk.Empty = true;
		physics &= Collisions;

		for ( byte x = 0; x < VoxelUtils.CHUNK_SIZE; x++ )
			for ( byte y = 0; y < VoxelUtils.CHUNK_SIZE; y++ )
				for ( byte z = 0; z < VoxelUtils.CHUNK_SIZE; z++ )
				{
					var voxel = chunk.GetVoxel( x, y, z );
					if ( !IsValidVoxel( voxel ) )
						continue;

					chunk.Empty = false;

					var position = new Vector3Int( x, y, z );

					// Let's start checking for collisions.
					if ( !tested[x, y, z] && !Scene.IsEditor && physics )
					{
						tested[x, y, z] = true;

						var start = (x: x, y: y, z: z);
						var size = (x: 1, y: 1, z: 1);
						var canSpread = (x: true, y: true, z: true);

						// Calculate how much we can fill.
						while ( canSpread.x || canSpread.y || canSpread.z )
						{
							canSpread.x = false;// trySpreadX( chunk, canSpread.x, ref tested, start, ref size );
							canSpread.y = false;// trySpreadY( chunk, canSpread.y, ref tested, start, ref size );
							canSpread.z = false;// trySpreadZ( chunk, canSpread.z, ref tested, start, ref size );
						}

						var scale = new Vector3( size.x, size.y, size.z ) * Scale;
						var pos = new Vector3( start.x, start.y, start.z ) * Scale
							+ scale / 2f
							- Scale / 2f;

						collisionBuffer.AddCube( pos, scale, Rotation.Identity );
					}

					// Build vertices.
					var drawCount = 0;
					var isOpaque = IsOpaqueVoxel( voxel );
					for ( var i = 0; i < 6; i++ )
					{
						var direction = VoxelUtils.Directions[i];
						var neighbour = Query( position.x + direction.x, position.y + direction.y, position.z + direction.z, chunk );
						
						var ignoreFace = IgnoreOOBFaces && neighbour.Chunk == null && direction.z != 1;
						var shouldDraw = !isOpaque && IsOpaqueVoxel( neighbour.Voxel );

						if ( (neighbour.HasVoxel && !shouldDraw) || ignoreFace )
							continue;

						for ( var j = 0; j < 4; ++j )
						{
							var vertexIndex = VoxelUtils.FaceIndices[(i * 4) + j];
							var ao = Utility.AmbientOcclusion.Fetch( chunk, position, i, j );

							var vertex = CreateVertex( position, vertexIndex, (byte)i, ao, voxel );
							voxelMesh.Vertices.Add( vertex );
						}

						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 0 );
						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 2 );
						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 1 );
						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 2 );
						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 0 );
						voxelMesh.Indices.Add( voxelMesh.Offset + drawCount * 4 + 3 );

						drawCount++;
					}

					voxelMesh.Offset += 4 * drawCount;
				}

		await Task.MainThread();

		// Check if we actually end up with vertices.
		if ( !chunk.Empty )
		{
			var builder = Model.Builder;
			voxelMesh.CreateBuffers( Layout );

			if ( voxelMesh.Valid )
				builder.AddMesh( voxelMesh.Mesh );
			else
			{
				_ = builder.Create();

				if ( chunkObject == null )
				{
					//if ( _gizmoCache.ContainsKey( chunk.Position ) )
						//_gizmoCache.Remove( chunk.Position );
				}
				else
				{
					chunkObject.Rebuild( null, physics );
				}

				return;
			}

			if ( chunkObject == null ) // We are in editor.
			{
				var model = builder.Create();
				/*if ( _gizmoCache.ContainsKey( chunk.Position ) )
					_gizmoCache.Remove( chunk.Position );

				_gizmoCache.Add( chunk.Position, model );*/
				return;
			}

			if ( physics )
				builder = builder.AddCollisionMesh( collisionBuffer.Vertices.ToArray(), collisionBuffer.Indices.ToArray() );
			
			await GameTask.WorkerThread();
			chunkObject.Rebuild( builder.Create(), physics );

			return;
		}

		// Remove the Gizmo chunk.
		//else if ( voxelChunk == null && _gizmoCache.ContainsKey( chunk.Position ) )
			//_gizmoCache.Remove( chunk.Position );

		// Let's remove our empty chunk!
		_objects.Remove( chunk );
		_chunks.Remove( chunk.Position );

		chunkObject?.Destroy();
	}
}
