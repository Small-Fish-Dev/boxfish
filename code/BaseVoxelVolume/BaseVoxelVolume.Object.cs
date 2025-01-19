namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	/// <summary>
	/// All of the chunk objects in this volume.
	/// </summary>
	protected IReadOnlyDictionary<Chunk, ChunkObject> Objects => _objects;
	private Dictionary<Chunk, ChunkObject> _objects = new();

	/// <summary>
	/// Get or create a <see cref="ChunkObject"/>.
	/// </summary>
	/// <param name="chunk"></param>
	/// <returns></returns>
	protected ChunkObject GetChunkObject( Chunk chunk )
	{
		ICollection collection = _objects;

		lock ( collection.SyncRoot )
		{
			if ( chunk == null )
			{
				Logger.Error( $"Tried to get ChunkObject of null Chunk?" );
				return null;
			}

			if ( !_objects.TryGetValue( chunk, out var chunkObject ) )
				_objects.Add( chunk, chunkObject = new()
				{
					Position = chunk.Position,
					Parent = this
				} );

			return chunkObject;
		}
	}

	/// <summary>
	/// Go through all ChunkObjects and clear them.
	/// </summary>
	protected void DestroyObjects()
	{
		ICollection collection = _objects;

		lock ( collection.SyncRoot )
		{
			foreach ( var (_, obj) in _objects )
				obj?.Dispose();

			_objects.Clear();
		}
	}

	/// <summary>
	/// This is the class for physical chunks in the world.
	/// <para>We store a <see cref="Sandbox.SceneObject"/> and a <see cref="Sandbox.PhysicsBody"/> here.</para>
	/// </summary>
	protected sealed class ChunkObject
		: IEquatable<ChunkObject>, 
		  IDisposable
	{
		public Vector3 WorldPosition => (Vector3)Position * Parent.Scale * VoxelUtils.CHUNK_SIZE;

		public Vector3Int Position { get; internal set; }
		public BaseVoxelVolume<T, U> Parent { get; internal set; }
		public SceneObject SceneObject { get; private set; }
		public PhysicsBody Body { get; private set; }
		public PhysicsShape Shape { get; private set; }

		public void Rebuild( Model model, bool physics = true )
		{
			if ( !Parent.IsValid() )
			{
				Logger.Error( $"No parent for ChunkObject found?" );
				return;
			}

			var scene = Parent.Scene;
			if ( !scene.IsValid() )
			{
				Logger.Error( $"No scene for ChunkObject found?" );
				Dispose();
				return;
			}

			if ( model == null )
			{
				Dispose();
				return;
			}

			SceneObject ??= new( scene.SceneWorld, Model.Error );
			if ( !SceneObject.IsValid() )
				return;

			var position = WorldPosition + Parent.Scale / 2f;

			// Recreate physics if needed.
			if ( physics )
			{
				var part = model.Physics?.Parts?.FirstOrDefault()?.Meshes?.FirstOrDefault();
				if ( part != null )
				{
					if ( !Body.IsValid() )
					{
						Body = new( scene.PhysicsWorld );
						Body.SetComponentSource( Parent );
					}

					Shape?.Remove();
					Shape = Body.AddShape( part, new Transform( position ), false, false );
				}
			}

			// SceneObject for rendering.
			Parent.SetAttributes( SceneObject.Attributes );
			SceneObject.Batchable = true;
			SceneObject.Model = model;
			SceneObject.Position = Position;
			SceneObject.SetComponentSource( Parent );
		}

		public bool Equals( ChunkObject other )
		{
			return other.Position.Equals( Position );
		}

		public override bool Equals( object obj )
		{
			return obj is ChunkObject other
				&& Equals( other );
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		public void Dispose()
		{
			if ( SceneObject.IsValid() )
				SceneObject.Delete();

			SceneObject = null;

			if ( Shape.IsValid() )
				Shape.Remove();

			Shape = null;

			if ( Body.IsValid() )
				Body.Remove();

			Body = null;
		}
	}
}
