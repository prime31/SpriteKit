using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor( typeof( Transform ) )]
public class SKSpriteTransformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var transform = (Transform)target;

        // Replicate the standard transform inspector gui
        if( transform.gameObject.GetComponent( typeof( SKSprite ) ) )
        {
            var clicked = false;
            var sprite = transform.gameObject.GetComponent<SKSprite>();

            if( sprite.spriteSheet != null ) //  && !Application.isPlaying
            {
                EditorGUIUtility.LookLikeControls();
                EditorGUI.indentLevel = 0;
				
                var position = EditorGUILayout.Vector3Field( "Position", transform.localPosition );
                var scale = EditorGUILayout.Vector3Field( "Scale", sprite.scale );
				
                GUILayout.Label( "Rotation" );
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                var eulers = new Vector3( 0f, 0f, (int)EditorGUILayout.FloatField( "Degrees", -transform.localEulerAngles.z ) ) * -1f;
                EditorGUI.indentLevel = 0;
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space( 5f );

                EditorGUIUtility.LookLikeControls();

                if( GUI.changed || clicked )
                {
                    Undo.RecordObject( transform, "Transform Change" );

					
					sprite.scale = scale;
					transform.localEulerAngles = eulers;
                    transform.localPosition = fixIfNaN( position );
                    sprite.generateMesh();
                }
            }
        }
        else
        {
            EditorGUIUtility.LookLikeControls();
            EditorGUI.indentLevel = 0;
            Vector3 position = EditorGUILayout.Vector3Field( "Position", transform.localPosition );
            Vector3 eulerAngles = EditorGUILayout.Vector3Field( "Rotation", transform.localEulerAngles );
            Vector3 scale = EditorGUILayout.Vector3Field( "Scale", transform.localScale );
            EditorGUIUtility.LookLikeControls();

            if( GUI.changed )
            {
                Undo.RecordObject( transform, "Transform Change" );

                transform.localPosition = fixIfNaN( position );
                transform.localEulerAngles = fixIfNaN( eulerAngles );
                transform.localScale = fixIfNaN( scale );
            }
        }
    }
	
	
    private Vector3 fixIfNaN( Vector3 v )
    {
        if( float.IsNaN( v.x ) )
            v.x = 0;
		
        if( float.IsNaN( v.y ) )
            v.y = 0;

        if( float.IsNaN( v.z ) )
            v.z = 0;

        return v;
    }

}