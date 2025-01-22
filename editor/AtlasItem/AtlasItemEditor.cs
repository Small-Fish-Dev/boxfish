using Boxfish.Library;

namespace Boxfish.Editor;

[CustomEditor( typeof( AtlasResource.AtlasItem ) )]
public class AtlasItemEditor : ControlWidget
{
	const int DISPLAY_SIZE = 32;
	public override bool SupportsMultiEdit => false;

	private SerializedProperty Atlas { get; }
	private SerializedProperty UseRegion { get; }
	private SerializedProperty Region { get; }
	private SerializedProperty Texture { get; }

	public AtlasItemEditor( SerializedProperty property ) : base( property )
	{
		Layout = Layout.Column();
		Layout.Spacing = 2;

		var serializedObject = property.GetValue<AtlasResource.AtlasItem>()?.GetSerialized();
		if ( serializedObject is null ) // Create new value if null.
		{
			property.SetValue<AtlasResource.AtlasItem>( new() );
			serializedObject = property.GetValue<AtlasResource.AtlasItem>()?.GetSerialized();
		}

		if ( serializedObject is not null )
		{
			Texture = serializedObject.GetProperty( nameof( AtlasResource.AtlasItem.Texture ) );
			Region = serializedObject.GetProperty( nameof( AtlasResource.AtlasItem.Region ) );
			UseRegion = serializedObject.GetProperty( nameof( AtlasResource.AtlasItem.UseRegion ) );

			// Add control sheet.
			var sheet = new ControlSheet();
			sheet.IncludePropertyNames = true;
			sheet.AddObject( serializedObject );

			Layout.Add( sheet );
			Layout.AddStretchCell();
		}

		var parentObject = property.Parent?.ParentProperty?.Parent;
		if ( parentObject is not null )
		{			
			Atlas = parentObject.GetProperty( nameof( AtlasResource.RegionAtlas ) );

			// Set dirty on property changes.
			var resource = parentObject.Targets?.FirstOrDefault() as GameResource;
			if ( resource is not null && serializedObject is not null )
				serializedObject.OnPropertyChanged += prop =>
				{
					resource.StateHasChanged();
				};
		}

		Layout.AddRow().AddColumn().AddSpacingCell( DISPLAY_SIZE );
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		// Something fucked up??
		if ( UseRegion == null || Texture == null || Region == null || Atlas == null )
			return;

		var pixmap = (Pixmap)null;
		var region = default( RectInt );
		
		// Get texture from region.
		if ( UseRegion.GetValue<bool>() )
		{
			var path = Atlas?.GetValue<string>();
			var texture = Sandbox.Texture.Load( Sandbox.FileSystem.Mounted, path, false );
			if ( texture == null )
				return;

			region = Region.GetValue<RectInt>();
			pixmap = Pixmap.FromTexture( texture );
		}

		// Get texture.
		else
		{
			var path = Texture.GetValue<string>();
			var texture = Sandbox.Texture.Load( Sandbox.FileSystem.Mounted, path, false );
			if ( texture == null )
				return;

			pixmap = Pixmap.FromTexture( texture );
			region = new RectInt( 0, 0, texture.Width, texture.Height );
		}

		// Actually paint the texture.
		if ( pixmap == null )
			return;

		var isValid = region.Width != 0 
			&& region.Height != 0 
			&& (region.Width == region.Height || region.Width == region.Height * 6);

		if ( !isValid )
			return;

		var size = region.Height;
		var scale = (float)DISPLAY_SIZE / size;

		Paint.SetBrush( pixmap );
		Paint.Translate( Vector2.Up * (Height - region.Size * scale) + new Vector2( 8, -8 ) );
		Paint.Scale( scale, scale );
		Paint.Translate( -(Vector2)region.Position );

		{
			var isSingularFace = region.Width == region.Height;
			if ( isSingularFace )
			{
				for( int i = 0; i < 6; i++ )
					Paint.DrawRect( new Rect( region.Position + Vector2.Left * size * i, region.Size ) );
			}
			else Paint.DrawRect( new Rect( region.Position, region.Size ) );
		}

		Paint.ResetTransform();
		Paint.ClearBrush();
	}
}
