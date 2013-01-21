using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


[CustomEditor( typeof( SKNineSliceSprite ) )]
public class SKNineSliceSpriteEditor : SKSpriteEditor
{
	public override void OnInspectorGUI()
	{
		var sprite = (SKNineSliceSprite)target;
		
		GUILayout.BeginHorizontal();
		sprite.offsets.left = (int)EditorGUILayout.FloatField( "left", sprite.offsets.left );
		sprite.offsets.right = (int)EditorGUILayout.FloatField( "right", sprite.offsets.right );
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		sprite.offsets.top = (int)EditorGUILayout.FloatField( "top", sprite.offsets.top );
		sprite.offsets.bottom = (int)EditorGUILayout.FloatField( "bottom", sprite.offsets.bottom );
		GUILayout.EndHorizontal();
		
		
		// validate. just make sure the offsets are larger than the image
		if( (float)sprite.offsets.horizontal > sprite.pixelPerfectHDSize.x || (float)sprite.offsets.vertical > sprite.pixelPerfectHDSize.y )
			EditorGUILayout.HelpBox( "What the fuck are you thinking?", MessageType.Error );
		
		base.OnInspectorGUI();
	}

}
