namespace Boxfish;

partial class BaseVoxelVolume<T, U>
{
	private Dictionary<Chunk, ChunkObject> _objects = new();

	/// <summary>
	/// Get or create a <see cref="ChunkObject"/>.
	/// </summary>
	/// <param name="chunk"></param>
	/// <returns></returns>
	protected ChunkObject GetChunkObject( Chunk chunk )
	{
		if ( chunk == null )
		{
			Log.Error( $"Tried to get ChunkObject of null Chunk?" );
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

	/// <summary>
	/// Go through all ChunkObjects and clear them.
	/// </summary>
	protected void DestroyObjects()
	{
		foreach ( var (_, obj) in _objects )
			obj?.Destroy();

		_objects.Clear();
	}

	/// <summary>
	/// This is the class for physical chunks in the world.
	/// <para>We store a <see cref="Sandbox.SceneObject"/> and a <see cref="Sandbox.PhysicsBody"/> here.</para>
	/// </summary>
	protected sealed class ChunkObject
		: IEquatable<ChunkObject>
	{
		public Vector3 WorldPosition => (Vector3)Position * Parent.Scale * VoxelUtils.CHUNK_SIZE;

		public Vector3Int Position { get; internal set; }
		public BaseVoxelVolume<T, U> Parent { get; internal set; }
		public SceneObject SceneObject { get; private set; }
		public PhysicsBody Body { get; private set; }
		public PhysicsShape Shape { get; private set; }

		~ChunkObject()
		{
			Destroy();
		}

		public void Rebuild( Model model, bool physics = true )
		{
			if ( !Parent.IsValid() )
			{
				Log.Error( $"No parent for ChunkObject found?" );
				return;
			}

			var scene = Parent.Scene;
			if ( !scene.IsValid() )
			{
				Log.Error( $"No scene for ChunkObject found?" );
				return;
			}

			if ( model == null )
			{
				SceneObject?.Delete();
				Shape?.Remove();
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
			SceneObject.Batchable = true;
			SceneObject.Attributes.Set( "VoxelScale", Parent.Scale );
			SceneObject.Model = model;
			SceneObject.Position = position;
			SceneObject.SetComponentSource( Parent );
		}

		public void Destroy()
		{
			if ( SceneObject.IsValid() )
				SceneObject.Delete();

			if ( Shape.IsValid() )
				Shape.Remove();

			if ( Body.IsValid() )
				Body.Remove();
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
	}
}
