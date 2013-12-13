using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;


[CustomEditor( typeof( SKSprite ) )]
public class SKSpriteEditor : Editor
{
	protected SKSprite _sprite;
	
	
	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI();
		_sprite = (SKSprite)target;
		
		// sprite properties
		if( _sprite.spriteSheet != null )
		{
			_sprite.desiredSize = EditorGUILayout.Vector2Field( "Desired Size", _sprite.desiredSize );
			
			if( GUILayout.Button( "Set Size to Image Size" ) )
			{
				Undo.RecordObject( _sprite, "Undo Make Pixel Perfect" );
				_sprite.desiredSize = _sprite.pixelPerfectHDSize;
				GUI.changed = true;
			}
		}
		_sprite.tintColor = EditorGUILayout.ColorField( "Tint Color", _sprite.tintColor );
		_sprite.anchor = anchorSelector( _sprite.anchor, "Sprite Anchor" );

		
		// sprite sheet
		var spriteSheets = SKTextureUtil.getAllSpriteSheets().Select( x => x.name ).ToList();
		var spriteSheetSeletedIndex = 0;
		if( _sprite.spriteSheet == null )
		{
			spriteSheets.Insert( 0, "Choose Sprite Sheet" );
		}
		else
		{
			// we have a sprite sheet. we want it to be selected
			spriteSheetSeletedIndex = Array.IndexOf( spriteSheets.ToArray(), _sprite.spriteSheet.name );
		}
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label( "Sprite Sheet" );
		var newSpriteSheetIndex = EditorGUILayout.Popup( spriteSheetSeletedIndex, spriteSheets.ToArray() );
		EditorGUILayout.EndHorizontal();
		
		if( newSpriteSheetIndex != spriteSheetSeletedIndex )
		{
			_sprite.spriteSheet = SKTextureUtil.getAllSpriteSheets().Where( x => x.name == spriteSheets[newSpriteSheetIndex] ).Select( x => x ).First();
			_sprite.renderer.material = _sprite.spriteSheet.material;
			_sprite.sourceImageName = _sprite.spriteSheet.containedImages[0];
		}
		

		
		if( _sprite.spriteSheet != null )
		{
			var currentIndex = -1;
			
			var currentImage = _sprite.spriteSheet.containedImages.Where( item => _sprite.sourceImageName != null && item.EndsWith( _sprite.sourceImageName ) ).FirstOrDefault();
			if( currentImage != null )
				currentIndex = Array.IndexOf( _sprite.spriteSheet.containedImages, currentImage );
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "Sprite Name" );
			var newIndex = EditorGUILayout.Popup( currentIndex, _sprite.spriteSheet.containedImages );
			EditorGUILayout.EndHorizontal();
			
			if( currentIndex != newIndex )
			{
				var selectedImage = _sprite.spriteSheet.containedImages[newIndex];
				var textureInfo = _sprite.spriteSheet.textureInfoForSourceImage( selectedImage );
				
				_sprite.pixelPerfectHDSize = textureInfo.size;
				_sprite.desiredSize = textureInfo.size;
				_sprite.sourceImageName = selectedImage;
				_sprite.Awake();
			}
		}
		
		
		GUILayout.Space( 10 );
		
		if( _sprite.spriteSheet != null )
		{
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Refresh Sprite Sheet" ) )
			{
				_sprite.spriteSheet.refreshSourceImages();
				refreshAllSprites();
			}
			
			
			// save colors so we can reset after making the destructive button
			var previousContentColor = GUI.contentColor;
			var previousBGColor = GUI.backgroundColor;
			GUI.contentColor = Color.red;
			GUI.backgroundColor = Color.red;
			
			if( GUILayout.Button( "Delete Sprite Sheet" ) )
			{
				if( EditorUtility.DisplayDialog( "SpriteKit Destruction Action Warning", "Are you sure you want to delete this sprite sheet?", "Yes, I'm Sure", "No!" ) )
				{
					// kill all assets associated with this sprite sheet
					var material = _sprite.spriteSheet.getMaterial( false );
					AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( material.mainTexture ) );
					
					if( _sprite.spriteSheet.hasHdAtlas )
					{
						material = _sprite.spriteSheet.getMaterial( true );
						AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( material.mainTexture ) );
					}
					
					// all done with the textures so now we kill the material
					AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( material ) );
					
					// find any animations that reference this sprite sheet
					var allSpriteAnimations = SKTextureUtil.getAllSpriteAnimations().Where( anim => anim.spriteSheet == _sprite.spriteSheet ).ToArray();
					foreach( var animation in allSpriteAnimations )
						AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( animation ) );
					
					AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( _sprite.spriteSheet ) );
					_sprite.spriteSheet = null;
					_sprite.sourceImageName = null;
				}
			}
			GUILayout.EndHorizontal();
			GUI.contentColor = previousContentColor;
			GUI.backgroundColor = previousBGColor;
		}
		
		
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			_sprite.Awake();
		}
	}
	
	
	private SKSprite.SpriteAnchor anchorSelector( SKSprite.SpriteAnchor current, string label )
	{
		GUILayout.BeginHorizontal( GUILayout.Width( 50 ) );
		
		EditorGUILayout.PrefixLabel( label );
		var ret = GUILayout.SelectionGrid( (int)current, new string[] { "TL", "TC", "TR", "ML", "MC", "MR", "BL", "BC", "BR" }, 3 );
		
		GUILayout.EndHorizontal();
		
		return (SKSprite.SpriteAnchor)ret;
	}

	
	[MenuItem( "SpriteKit/Create Animation from Selected Images...", false, 1 )]
	static void createAnimation()
	{
		var textures = new List<Texture2D>();
		
		// first validate that we have only Texture2Ds
		foreach( var o in Selection.objects )
		{
			if( o is Texture2D )
				textures.Add( o as Texture2D );
		}
		
		if( textures.Count == 0 )
		{
			EditorUtility.DisplayDialog( "Cannot Create Sprite Animation", "You did not select any images fool", "OK. I'm sorry." );
			return;
		}
		
		// figure out what sprite sheet the images are associated with and bail if it is more than one
		var allSpriteSheets = SKTextureUtil.getAllSpriteSheets();
		Func<string, string, SKSpriteSheet> spriteSheetWithImage = ( imageName, texturePath ) =>
		{
			foreach( var sheet in allSpriteSheets )
			{
				if( texturePath == sheet.imageSourceFolder && sheet.containedImages.Contains( imageName ) )
					return sheet;
			}
			return null;
		};
		
		SKSpriteSheet spriteSheetForAnimation = null;
		var spriteWidth = textures[0].width;
		var spriteHeight = textures[0].height;
		
		foreach( var tex in textures )
		{
			var texturePath = AssetDatabase.GetAssetPath( tex );
			var theSheet = spriteSheetWithImage( Path.GetFileName( texturePath ), Path.GetDirectoryName( texturePath ) );
			
			if( theSheet == null )
			{
				EditorUtility.DisplayDialog( "Cannot Create Sprite Animation", "An image was selected that is not in a SKSpriteSheet", "Close" );
				return;
			}
			
			if( spriteSheetForAnimation == null )
			{
				spriteSheetForAnimation = theSheet;
			}
			else
			{
				// if we found a different sprite sheet we stop
				if( spriteSheetForAnimation != theSheet )
				{
					EditorUtility.DisplayDialog( "Cannot Create Sprite Animation", "Images from multiple SKSpriteSheets were chosen", "Close" );
					return;
				}
			}
			
			if( tex.height != spriteHeight || tex.width != spriteWidth )
			{
				EditorUtility.DisplayDialog( "Cannot Create Sprite Animation", "Images of different sizes were found. All images must be the same size for an animation.", "Close" );
				return;
			}
		}
		
		var wizard = SKSpriteAnimationWizard.createWizard();
		wizard.spriteSheet = spriteSheetForAnimation;
		wizard.animationFrames = textures.Select( t => Path.GetFileName( AssetDatabase.GetAssetPath( t ) ) ).OrderBy( t => t, new NaturalSortComparer<string>() ).ToArray();
	}
	
	
	[MenuItem( "SpriteKit/Refresh all Sprites %r" )]
	static void refreshAllSprites()
	{
		var sprites = FindObjectsOfType( typeof( SKSprite ) );
		foreach( var s in sprites )
		{
			var sprite = s as SKSprite;
			sprite.Awake();
		}
	}
	
	
	[MenuItem( "SpriteKit/Clear all Material Textures" )]
	static void clearAllTextures()
	{
		foreach( var sheet in SKTextureUtil.getAllSpriteSheets() )
		{
			sheet.material.mainTexture = null;
		}
	}
	
	
	[MenuItem( "SpriteKit/Create SpriteKit.dlls..." )]
	static void createDLL()
	{
		var compileParams = new CompilerParameters();
		compileParams.OutputAssembly = Path.Combine( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Desktop ), "SpriteKit.dll" );
		compileParams.CompilerOptions = "/optimize";
		compileParams.ReferencedAssemblies.Add( Path.Combine( EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEngine.dll" ) );
		
		var source = getSourceForDLL( "Assets/SpriteKit/" );

		var codeProvider = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v3.0" } } );
    	var compilerResults = codeProvider.CompileAssemblyFromSource( compileParams, source );
		
    	if( compilerResults.Errors.Count > 0 )
    	{
    		foreach( var error in compilerResults.Errors )
    			Debug.LogError( error.ToString() );
		}
		else
		{
			createEditorDLL( compileParams.OutputAssembly );
		}
	}
	
	
	private static void createEditorDLL( string pathToSpriteKitDLL )
	{
		var compileParams = new CompilerParameters();
		compileParams.OutputAssembly = Path.Combine( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Desktop ), "SpriteKitEditor.dll" );
		compileParams.CompilerOptions = "/optimize";
		compileParams.ReferencedAssemblies.Add( Path.Combine( EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEngine.dll" ) );
		compileParams.ReferencedAssemblies.Add( Path.Combine( EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEditor.dll" ) );
		compileParams.ReferencedAssemblies.Add( pathToSpriteKitDLL );
		
		var source = getSourceForDLL( "Assets/SpriteKit/Editor/" );

		var codeProvider = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v3.0" } } );
    	var compilerResults = codeProvider.CompileAssemblyFromSource( compileParams, source );
		
    	if( compilerResults.Errors.Count > 0 )
    	{
    		foreach( var error in compilerResults.Errors )
    			Debug.LogError( error.ToString() );
		}
		else
		{
			EditorUtility.DisplayDialog( "SpriteKit DLLs Successfully Created", "SpriteKit.dll and SpriteKitEditor.dll should now be on your desktop. Place SpriteKitEditor.dll in the Editor folder of your project and the SpriteKit.dll in the Plugins folder. You will also need to copy SKBasic.shader into your project.", "OK" );
		}
	}
	
	
	static string[] getSourceForDLL( string path )
	{
		var source = new List<string>();

		foreach( var file in Directory.GetFiles( path, "*.cs" ) )
		{
			source.Add( File.ReadAllText( file ) );
		}
		
		foreach( var dir in Directory.GetDirectories( path ) )
		{
			if( Path.GetFileName( dir ) != "Editor" )
				source.AddRange( getSourceForDLL( dir ) );
		}
		
		return source.ToArray();
	}

}


public class NaturalSortComparer<T> : IComparer<string>, IDisposable
{
	private bool isAscending;
	
	public NaturalSortComparer( bool inAscendingOrder = true )
	{
		this.isAscending = inAscendingOrder;
	}
	
	
	#region IComparer<string> Members
	
	public int Compare( string x, string y )
	{
		throw new NotImplementedException();
	}
	
	#endregion
	
	
	#region IComparer<string> Members
	
	int IComparer<string>.Compare( string x, string y )
	{
		if( x == y )
			return 0;
	
		string[] x1, y1;
	
		if( !table.TryGetValue( x, out x1 ) )
		{
			x1 = Regex.Split( x.Replace( " ", "" ), "([0-9]+)" );
			table.Add( x, x1 );
		}
	
		if( !table.TryGetValue( y, out y1 ) )
		{
			y1 = Regex.Split( y.Replace( " ", "" ), "([0-9]+)" );
			table.Add( y, y1 );
		}
	
		int returnVal;
	
		for( int i = 0; i < x1.Length && i < y1.Length; i++ )
		{
			if( x1[i] != y1[i] )
			{
				returnVal = PartCompare( x1[i], y1[i] );
				return isAscending ? returnVal : -returnVal;
			}
		}
	
		if( y1.Length > x1.Length )
		{
			returnVal = 1;
		}
		else if( x1.Length > y1.Length )
		{ 
			returnVal = -1; 
		}
		else
		{
			returnVal = 0;
		}
	
		return isAscending ? returnVal : -returnVal;
	}
	
	private static int PartCompare( string left, string right )
	{
		int x, y;
		if( !int.TryParse( left, out x ) )
			return left.CompareTo( right );
	
		if( !int.TryParse( right, out y ) )
			return left.CompareTo( right );
	
		return x.CompareTo( y );
	}
	
	#endregion
	
	
	private Dictionary<string, string[]> table = new Dictionary<string, string[]>();
	
	
	public void Dispose()
	{
		table.Clear();
		table = null;
	}

}