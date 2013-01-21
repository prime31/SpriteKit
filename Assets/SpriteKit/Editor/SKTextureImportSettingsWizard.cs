using UnityEngine;
using UnityEditor;
using System.Collections;



public class SKTextureImportSettingsWizard : ScriptableWizard
{
	public TextureImporterFormat textureFormat;
	public FilterMode filterMode;
	public int maxTextureSize = 4096;
	public TextureImporterType textureType;
	
	
	// editor prefs keys
	private static string _textureFormatKey = "_textureFormatKey";
	private static string _filterModeKey = "_filterModeKey";
	private static string _maxTextureSizeKey = "_maxTextureSizeKey";
	private static string _textureTypeKey = "_textureTypeKey";
	
	
	[MenuItem( "SpriteKit/Texture Import Preferences..." )]
	static void textureImportPreferences()
	{
		var helper = EditorWindow.GetWindow<SKTextureImportSettingsWizard>( true, "SpriteKit Texture Import Preferences" );
		helper.minSize = new Vector2( 300, 170 );
		helper.maxSize = new Vector2( 300, 170 );
		
		helper.textureFormat = getTextureImportFormat();
		helper.filterMode = getFilterMode();
		helper.maxTextureSize = getMaxTextureSize();
		helper.textureType = getTextureImportType();
	}
	
	
	#region Getters for the saved settings
	
	public static TextureImporterFormat getTextureImportFormat()
	{
		return (TextureImporterFormat)EditorPrefs.GetInt( _textureFormatKey, (int)TextureImporterFormat.ARGB32 );
	}
	
	
	public static FilterMode getFilterMode()
	{
		return (FilterMode)EditorPrefs.GetInt( _filterModeKey, (int)FilterMode.Bilinear );
	}
	
	
	public static int getMaxTextureSize()
	{
		return EditorPrefs.GetInt( _maxTextureSizeKey, 4096 );
	}
	
	
	public static TextureImporterType getTextureImportType()
	{
		return (TextureImporterType)EditorPrefs.GetInt( _textureTypeKey, (int)TextureImporterType.GUI );
	}
	
	#endregion

	
	
	void OnGUI()
	{
		textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup( "Texture Format", textureFormat );
		filterMode = (FilterMode)EditorGUILayout.EnumPopup( "Filter Mode", filterMode );
		maxTextureSize = EditorGUILayout.IntField( "Max Texture Size", maxTextureSize );
		textureType = (TextureImporterType)EditorGUILayout.EnumPopup( "Texture Type", textureType );
	
		GUILayout.Space( 25 );
		
		if( GUILayout.Button( "Reset to Defaults" ) )
		{
			EditorPrefs.DeleteKey( _filterModeKey );
			EditorPrefs.DeleteKey( _textureFormatKey );
			EditorPrefs.DeleteKey( _textureTypeKey );
			EditorPrefs.DeleteKey( _maxTextureSizeKey );
			
			textureFormat = getTextureImportFormat();
			filterMode = getFilterMode();
			maxTextureSize = getMaxTextureSize();
			textureType = getTextureImportType();
		}
		
		GUILayout.Space( 15 );
		
		if( GUILayout.Button( "Save" ) )
		{
			if( !Mathf.IsPowerOfTwo( maxTextureSize ) )
			{
				EditorUtility.DisplayDialog( "SpriteKit Max Texture Size Error", "Max texture size should be a power of 2", "OK" );
				return;
			}
			
			EditorPrefs.SetInt( _textureFormatKey, (int)textureFormat );
			EditorPrefs.SetInt( _filterModeKey, (int)filterMode );
			EditorPrefs.SetInt( _maxTextureSizeKey, maxTextureSize );
			EditorPrefs.SetInt( _textureTypeKey, (int)textureType );
			Close();
		}
	}

}
