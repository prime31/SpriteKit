using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public static class SKTextureUtil
{
	public static string defaultPath = "Assets/SKAssets";
	public static string defaultResourcesPath = "Assets/SKAssets/Resources";
	public static string defaultAtlasResourcesPath = "Assets/SKAssets/Resources/Atlases";
	public static string defaultAnimationsPath = "Assets/SKAssets/Resources/Animations";
	public static string defaultSpriteSheetPath = "Assets/SKAssets/Resources/SpriteSheets";
	public static string defaultShader = "SpriteKit/Basic";
	public static int atlasPadding = 1;
	public static bool isCurrentlyRefreshingSourceImages = false;
	
	
    public static void saveAtlasPng( string path, string name, Texture2D tex )
    {
        if( !Directory.Exists( path ) )
            Directory.CreateDirectory( path );
		
		var fullPath = path + System.IO.Path.DirectorySeparatorChar + name + ".png";
        var newAtlas = !File.Exists( fullPath );

        using( var fs = new FileStream( fullPath, FileMode.Create ) )
		{
			using( var bw = new BinaryWriter( fs ) )
			{
        		bw.Write( tex.EncodeToPNG() );
        		bw.Close();
        		fs.Close();
			}
		}
		
        if( newAtlas )
        {
            var textureImporter = AssetImporter.GetAtPath( fullPath ) as TextureImporter;
            if( textureImporter != null )
            {
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = false;
                textureImporter.textureFormat = SKTextureImportSettingsWizard.getTextureImportFormat();
				textureImporter.textureType = SKTextureImportSettingsWizard.getTextureImportType();
                textureImporter.filterMode = SKTextureImportSettingsWizard.getFilterMode();
                textureImporter.maxTextureSize = SKTextureImportSettingsWizard.getMaxTextureSize();
				textureImporter.wrapMode = TextureWrapMode.Clamp;

                AssetDatabase.ImportAsset( fullPath, ImportAssetOptions.Default );
            }
        }
    }
	
	
	public static Rect[] rebuildAtlas( Texture2D[] textures, string filename, bool generateSdAtlas )
	{
		var texture = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
		var rects = texture.PackTextures( textures, atlasPadding, 4096 );
		
		// if generateSdAtlas is true, we make 2 atlases one being half the size of the source images
		var sourceAtlasFilename = filename + ( generateSdAtlas ? "@2x" : string.Empty );
		saveAtlasPng( defaultAtlasResourcesPath, sourceAtlasFilename, texture );
		
		// save a half sized version as well if required
		if( generateSdAtlas )
		{
			var sourcePath = Path.Combine( defaultAtlasResourcesPath, filename + "@2x.png" );
			var destPath = Path.Combine( defaultAtlasResourcesPath, filename + ".png" );
			var sdTexture = resizeTexture( sourcePath, texture.width / 2, texture.height / 2, destPath );
			saveAtlasPng( defaultAtlasResourcesPath, filename, sdTexture );
			GameObject.DestroyImmediate( sdTexture );
		}
		
		GameObject.DestroyImmediate( texture );
		Resources.UnloadUnusedAssets();
		
		return rects;
	}
	
	
	public static SKSpriteSheet createSpriteSheet( string name, string sourceFolder, bool generateSdTexture, int cameraOrthoSize, int targetScreenHeight )
	{
		// validate that there are images in the folder and that the name has not been used
		var allSpriteSheets = getAllSpriteSheets();
		foreach( var s in allSpriteSheets )
		{
			if( s.imageSourceFolder.EndsWith( name ) )
			{
				Debug.LogError( "folder " + name + " is already contained by another sprite sheet" );
				return null;
			}
		}
		
		if( Directory.GetFiles( sourceFolder ).Length == 0 )
		{
			Debug.LogError( "folder " + name + " has no images in it" );
			return null;
		}
		
		var sheet = ScriptableObject.CreateInstance<SKSpriteSheet>();
		sheet.name = name;
		sheet.imageSourceFolder = sourceFolder;
		sheet.hasHdAtlas = generateSdTexture;
		sheet.cameraOrthoSize = cameraOrthoSize;
		sheet.targetScreenHeight = targetScreenHeight;
		sheet.refreshSourceImages();
		
		// material creation
		//var atlasPath = Path.Combine( defaultResourcesPath, sheet.name + "_atlas@2x.png" );
        sheet.material = new Material( Shader.Find( defaultShader ) );
        //sheet.material.SetTexture( "_MainTex", Resources.LoadAssetAtPath( atlasPath, typeof( Texture ) ) as Texture );
        AssetDatabase.CreateAsset( sheet.material, Path.Combine( defaultPath, name ) + "_material.mat" );
		
		var path = Path.Combine( defaultSpriteSheetPath, name + "_sheet.asset" );
		AssetDatabase.CreateAsset( sheet, path );
		AssetDatabase.SaveAssets();
		
		return sheet;
	}
	
	
	public static List<SKSpriteSheet> getAllSpriteSheets()
	{
		if( !Directory.Exists( defaultSpriteSheetPath ) )
			Directory.CreateDirectory( defaultSpriteSheetPath );
		
		var sheets = new List<SKSpriteSheet>();
		
		foreach( var file in Directory.GetFiles( defaultSpriteSheetPath, "*_sheet.asset" ) )
		{
			var sheet = AssetDatabase.LoadMainAssetAtPath( file ) as SKSpriteSheet;
			sheets.Add( sheet );
		}
		
		return sheets;
	}
	
	
	public static List<SKSpriteAnimationState> getAllSpriteAnimations()
	{
		if( !Directory.Exists( defaultAnimationsPath ) )
			Directory.CreateDirectory( defaultAnimationsPath );
		
		var animations = new List<SKSpriteAnimationState>();
		
		foreach( var file in Directory.GetFiles( defaultAnimationsPath, "*.asset" ) )
		{
			var sheet = AssetDatabase.LoadMainAssetAtPath( file ) as SKSpriteAnimationState;
			if( sheet != null )
				animations.Add( sheet );
		}
		
		return animations;
	}
	
	
	public static void refreshSourceImages( this SKSpriteSheet sheet )
	{
		// set a flag while we import the PSD to avoid our AssetPostprocessor from getting an endless loop of death
		isCurrentlyRefreshingSourceImages = true;
		
		try
		{
			var textureInfoList = new List<SKTextureInfo>();
			var files = Directory.GetFiles( sheet.imageSourceFolder ).Where( f => !f.EndsWith( ".meta" ) ).ToArray();
			var textures = new Dictionary<string,Texture2D>( files.Length );
			var texturesNotToDestroy = new List<string>();
			var containedImages = new List<string>();
			
			float progress = 1f;
			
			foreach( var f in files )
			{
				EditorUtility.DisplayProgressBar( "Creating Sprite Atlases..", "processing image at path: " + f, progress++ / files.Length );
				
				var path = Path.Combine( sheet.imageSourceFolder, Path.GetFileName( f ) );
				Texture2D tex = null;
	
				if( Path.GetExtension( path ) == ".png" )
				{
					// we load directly from disk so that the textures are guaranteed readable
					tex = new Texture2D( 0, 0 );
					tex.LoadImage( File.ReadAllBytes( f ) );
				}
				else if( Path.GetExtension( path ).ToLower() == ".psd" || Path.GetExtension( path ) == ".gif" )
				{
					var texImporter = TextureImporter.GetAtPath( path ) as TextureImporter;
					texImporter.isReadable = true;
					AssetDatabase.ImportAsset( path );

					tex = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D;
					if( tex != null )
						texturesNotToDestroy.Add( tex.name );
				}
				
	
				if( tex != null )
				{
					textures.Add( Path.GetFileName( f ), tex );
					containedImages.Add( Path.GetFileName( f ) );
				}
			}
			
			sheet.containedImages = containedImages.ToArray();
			
			// pack all the textures and make a lookup dictionary
			var textureArray = textures.Select( x => x.Value ).ToArray();
			var rects = SKTextureUtil.rebuildAtlas( textureArray, sheet.name.Replace( "_sheet", string.Empty ) + "_atlas", sheet.hasHdAtlas );
			var texToRect = new Dictionary<string,Rect>( textures.Count );
			
			for( var i = 0; i < textureArray.Length; i++ )
			{
				var key = textures.Where( y => y.Value == textureArray[i] ).Select( x => x.Key ).First();
				texToRect[key] = rects[i];
			}
			
			// create our textureInfos
			foreach( var item in texToRect )
			{
				var tex = textures[item.Key];
				
				var info = new SKTextureInfo();
				info.file = item.Key;
				info.uvRect = item.Value;
				info.size = new Vector2( tex.width, tex.height );
				textureInfoList.Add( info );
			}
			
			// clean up textures
			foreach( var tex in textures )
			{
				// only destroy textures that we loaded from pngs
				if( !texturesNotToDestroy.Contains( tex.Value.name ) )
					GameObject.DestroyImmediate( tex.Value, true );
			}
			textures.Clear();
			
			// not sure why getting the asset path triggers a save but it does for some reason
			AssetDatabase.GetAssetPath( sheet );
			sheet.imageTextureInfo = textureInfoList.ToArray();
			//AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty( sheet );
		}
		catch( System.Exception e )
		{
			Debug.LogError( "Something went wrong creating the atlas: " + e );
		}
		finally
		{
			EditorUtility.ClearProgressBar();
			isCurrentlyRefreshingSourceImages = false;
		}
	}
	
	
	public static SKTextureInfo textureInfoForSourceImage( this SKSpriteSheet sheet, string sourceImageName )
	{
		foreach( var info in sheet.imageTextureInfo )
		{
			if( info.file == sourceImageName )
				return info;
		}
		
		return null;
	}

	
	public static string makePathRelativeToProject( string path )
	{
		var removePortion = Application.dataPath.Replace( "Assets", string.Empty );
		return path.Replace( removePortion, string.Empty );
	}
	
	
	private static Texture2D resizeTexture( string pathToImage, int width, int height, string outputFilename )
	{
		var args = string.Format( "-z {0} {1} {2} --out {3}", height, width, pathToImage, outputFilename );
		
		var proc = new System.Diagnostics.Process
		{
    		StartInfo = new System.Diagnostics.ProcessStartInfo
			{
        		FileName = "sips",
		        Arguments = args,
		        UseShellExecute = false,
		        RedirectStandardOutput = true,
		        CreateNoWindow = true
			}
		};
		
		proc.WaitForExit();
		proc.Start();
		
		while( !proc.StandardOutput.EndOfStream )
		{
			proc.StandardOutput.ReadLine();
		}
		
		var tex = new Texture2D( 0, 0 );
		tex.LoadImage( File.ReadAllBytes( outputFilename ) );
		File.Delete( outputFilename );
		
		return tex;
	}

}
