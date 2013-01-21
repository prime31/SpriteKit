using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;



public class SKNewSpriteSheetWizard : ScriptableWizard
{
	private bool _generateSDTexture = true;
	private string _sourceFolder = string.Empty;
	private int _orthoSize = 320;
	private int _targetScreenHeight = 640;
	
	
	[MenuItem( "SpriteKit/Create New Sprite Sheet...", false, 0 )]
	static void newSpriteSheet()
	{
		var helper = EditorWindow.GetWindow<SKNewSpriteSheetWizard>( true, "SpriteKit New Sprite Sheet Wizard" );
		helper.minSize = new Vector2( 300, 200 );
		helper.maxSize = new Vector2( 300, 200 );
	}

	
	void OnGUI()
	{
		_generateSDTexture = EditorGUILayout.Toggle( "Auto-generate SD texture", _generateSDTexture );
		_orthoSize = EditorGUILayout.IntField( "Camera Ortho Size", _orthoSize );
		_targetScreenHeight = EditorGUILayout.IntField( "Target Screen Height", _targetScreenHeight );
		
		GUI.enabled = false;
		EditorGUILayout.TextField( "Image source folder", _sourceFolder );
		GUI.enabled = true;
		
		EditorGUILayout.HelpBox( "If your camera is setup with matching settings to your sprite sheet all sprites will automatically be pixel perfect.", MessageType.None );

	
		GUILayout.Space( 25 );
		
		if( GUILayout.Button( "Choose Image Source Folder" ) )
		{
			var folder = EditorUtility.OpenFolderPanel( "SpriteKit Image Source Folder", "Assets", null );
			if( folder != string.Empty )
			{
				if( !folder.Contains( "Assets" ) )
					EditorUtility.DisplayDialog( "SpriteKit Error", "The folder you chose is outside of your project folder. The folder must already be in your project.", "OK" );
				else
					_sourceFolder = SKTextureUtil.makePathRelativeToProject( folder );
			}
		}
		
		GUILayout.Space( 15 );
		
		if( GUILayout.Button( "Create Sprite Sheet" ) )
		{
			if( _sourceFolder == null || _sourceFolder == string.Empty )
				return;
			
			SKTextureUtil.createSpriteSheet( Path.GetFileName( _sourceFolder ), _sourceFolder, _generateSDTexture, _orthoSize, _targetScreenHeight );
			Close();
		}
	}

}
