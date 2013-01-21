using UnityEngine;
using System.Collections;


/// <summary>
/// Camera is set with an ortho size of 320. This is the proper size for retina devices so one Unity unit
/// will equal 1 pixel. On older, non-retina devices one Unity unit will be 0.5 pixels
/// 
/// 0, 0 is the bottom left corner and 960, 640 is the top right corner
/// </summary>
public class DemoCamera : MonoBehaviour
{
	private SKSprite _caveManDude;
	private float _zIndexForCreatedSprites = 0;
	
	
	void Awake()
	{
		_caveManDude = GameObject.Find( "dude" ).GetComponent<SKSprite>();
	}
	
	
	
	void Update()
	{
		if( Input.GetKeyDown( KeyCode.LeftArrow ) )
		{
			_caveManDude.faceBackwards();
			_caveManDude.startAnimation( "walk" );
		}
		
		if( Input.GetKeyDown( KeyCode.RightArrow ) )
		{
			_caveManDude.faceForwards();
			_caveManDude.startAnimation( "walk" );
		}
		
		if( Input.GetKeyUp( KeyCode.RightArrow ) || Input.GetKeyUp( KeyCode.LeftArrow ) )
		{
			if( _caveManDude.currentAnimation != null )
				_caveManDude.currentAnimation.stop();
		}
		
		if( Input.GetKeyDown( KeyCode.Space ) )
		{
			_caveManDude.startAnimation( "hit" );
		}
	}
	
	
	void OnGUI()
	{
		if( GUILayout.Button( "Make Random Sprite" ) )
		{
			// fetch all the sheets including those that are not yet in the scene
			var sheets = Resources.LoadAll( "SpriteSheets", typeof( SKSpriteSheet ) );
			
			var sheet = sheets[Random.Range( 0, sheets.Length - 1 )] as SKSpriteSheet;
			var index = Random.Range( 0, sheet.containedImages.Length - 1 );
			
			var sprite = SKSprite.createSprite( sheet, sheet.containedImages[index], SKSprite.SpriteAnchor.BottomLeft );
			sprite.transform.position = new Vector3( Random.Range( 0, Screen.width ), Random.Range( 0, Screen.width ), _zIndexForCreatedSprites-- );
		}

		
		if( GUILayout.Button( "Make Named Sprite (devil1)" ) )
		{
			var sprite = SKSprite.createSprite( "images2", "devil1", SKSprite.SpriteAnchor.BottomLeft );
			sprite.transform.position = new Vector3( Random.Range( 0, Screen.width ), Random.Range( 0, Screen.width ), _zIndexForCreatedSprites-- );
		}
		
		
		if( GUILayout.Button( "Play Gai's Animation" ) )
		{
			var go = GameObject.Find( "gai" );
			var sprite = go.GetComponent<SKSprite>();
			sprite.startAnimation( "gai" );
		}
		
		
		if( GUILayout.Button( "Flip Gai Sprite" ) )
		{
			var go = GameObject.Find( "gai" );
			var sprite = go.GetComponent<SKSprite>();
			sprite.flipHorizontally();
		}

	}
}
