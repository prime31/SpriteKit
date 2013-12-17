using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SKTextureInfo
{
	public string file;
	public Rect uvRect;
	public Vector2 size;
	
	public override string ToString()
	{
		return string.Format( "file: {0}, size: {1}, uvRect: {2}", file, size, uvRect );
	}

}


public class SKSpriteSheet : ScriptableObject
{
	public Material material;
	public string[] containedImages;
	[SerializeField]
	public SKTextureInfo[] imageTextureInfo;
	public string imageSourceFolder;
	public bool hasHdAtlas;
	public int cameraOrthoSize;
	public int targetScreenHeight;
	
	
	public static SKSpriteSheet sheetWithName( string name )
	{
		// first we check to see if the sprite sheet is already in play
		var sheets = Resources.FindObjectsOfTypeAll( typeof( SKSpriteSheet ) );
		for( var i = 0; i < sheets.Length; i++ )
		{
			var sheet = sheets[i] as SKSpriteSheet;
			if( sheet.name == name )
				return sheet;
		}
		
		// didnt find it. load it from resources
		return Resources.Load( "SpriteSheets/" + name + "_sheet", typeof( SKSpriteSheet ) ) as SKSpriteSheet;
	}
	
	
	public static SKSpriteSheet sheetWithSprite( string spriteName )
	{
		var sheets = Resources.FindObjectsOfTypeAll( typeof( SKSpriteSheet ) );
		for( var i = 0; i < sheets.Length; i++ )
		{
			var sheet = sheets[i] as SKSpriteSheet;
			if( sheet.containedImages.contains( spriteName ) )
				return sheet;
		}
		
		return null;
	}
	
	
	public static SKSpriteSheet sheetWithImageSourceFolder( string imageSourceFolder )
	{
		var sheets = Resources.FindObjectsOfTypeAll( typeof( SKSpriteSheet ) );
		for( var i = 0; i < sheets.Length; i++ )
		{
			var sheet = sheets[i] as SKSpriteSheet;
			if( sheet.imageSourceFolder == imageSourceFolder )
				return sheet;
		}
		
		return null;
	}

	
	public Material getMaterial( bool isHD )
	{
		// load up a different texture if we are HD and we actually have an HD atlas
		var imageName = name.Replace( "_sheet", isHD && hasHdAtlas ? "_atlas@2x" : "_atlas" );
		
		if( material.mainTexture == null || material.mainTexture.name != imageName )
			material.mainTexture = Resources.Load( "Atlases/" + imageName, typeof( Texture2D ) ) as Texture2D;

		return material;
	}

	
	/// <summary>
	/// source name can be either just the file name or the file name and extension
	/// </summary>
	public SKTextureInfo textureInfoForImage( string sourceImageName )
	{
		if( sourceImageName == null )
			return null;
		
		for( var i = 0; i < imageTextureInfo.Length; i++ )
		{
			if( imageTextureInfo[i].file.StartsWith( sourceImageName ) )
				return imageTextureInfo[i];
		}
		
		Debug.LogError( "could not find image " + sourceImageName + " in SKSpriteSheet: " + this );
		
		return null;
	}

	
	public override string ToString()
	{
		return string.Format( "source folder: {0}, total images: {1}", imageSourceFolder, containedImages.Length );
	}
	
}
