using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;



public class SKAssetPostprocessor : AssetPostprocessor
{
	private static System.DateTime? _lastImport = null;
	
	
	private void OnPostprocessTexture( Texture2D texture )
	{
		// avoid processing multiple times
		if( _lastImport.HasValue && System.DateTime.Now.Subtract( _lastImport.Value ).TotalSeconds < 20 )
			return;
		
		// bail out if this change was because of the SKTextureUtil doing its job with a PSD
		if( SKTextureUtil.isCurrentlyRefreshingSourceImages )
			return;
		
		var filename = Path.GetFileName( assetPath );
		var sheet = SKSpriteSheet.sheetWithImageSourceFolder( Path.GetDirectoryName( assetPath ) );

		if( sheet != null )
		{
			_lastImport = System.DateTime.Now;
			Debug.Log( "SpriteKit detected that a texture updated (" + filename + ") from sprite sheet: " + sheet.name + ". Refreshing now." );
			sheet.refreshSourceImages();
		}
	}
	
	
	/* The intention of this is to disconnect all textures from the material but it gets called in the Editor when pressing play
	   AFTER the awake method is called so this wont work.
	[UnityEditor.Callbacks.PostProcessScene]
	static void onPostProcessScene()
	{
		// disconnect all the textures from the materials
		foreach( var sheet in SKTextureUtil.getAllSpriteSheets() )
		{
			sheet.material.mainTexture = null;
		}
		
		//AssetDatabase.SaveAssets();
	}
	*/

}
