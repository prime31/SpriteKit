using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class SKSpriteAnimationWizard : ScriptableWizard
{
    public string animationName = "animation";
	public WrapMode wrapMode = WrapMode.Loop;
	public int loopCount = 1;
	public float framesPerSecond = 5;
	public SKSpriteAnimationState.CompleteRule completeRule = SKSpriteAnimationState.CompleteRule.RevertToOriginalSprite;
	public SKSpriteSheet spriteSheet;
	public string[] animationFrames;
	
	
	private List<SKSpriteAnimationState> _allSpriteAnimations;
	    
	    
    public static SKSpriteAnimationWizard createWizard()
   	{
   		// get the contents of the plist or create a new one
		var helper = ScriptableWizard.DisplayWizard<SKSpriteAnimationWizard>( "SpriteKit Animation Maker", "Save Animation", "Cancel" );
		helper.minSize = new Vector2( 500, 400 );
		helper.maxSize = new Vector2( 500, 1000 );
		
		return helper;
    }
	
	
	private bool isAnimationNameValid()
	{
		if( _allSpriteAnimations == null )
			_allSpriteAnimations = SKTextureUtil.getAllSpriteAnimations();
		
		// validate that there are images in the folder and that the name has not been used
		foreach( var s in _allSpriteAnimations )
		{
			if( s.name == animationName )
				return false;
		}
		
		return true;
	}

	
	// Called when the 'save' button is pressed
    void OnWizardCreate()
    {
		// create the animations
		var animation = ScriptableObject.CreateInstance<SKSpriteAnimationState>();
		animation.spriteSheet = spriteSheet;
		animation.name = animationName;
		animation.imageNames = animationFrames;
		animation.wrapMode = wrapMode;
		animation.iterations = loopCount;
		animation.framesPerSecond = framesPerSecond;
		animation.completeRule = completeRule;
		
		
		// make sure we have our animations directory
		if( !Directory.Exists( SKTextureUtil.defaultAnimationsPath ) )
			Directory.CreateDirectory( SKTextureUtil.defaultAnimationsPath );
		
		// store the animation in the asset database
		var path = Path.Combine( SKTextureUtil.defaultAnimationsPath, animation.name + ".asset" );
		AssetDatabase.CreateAsset( animation, path );
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
    }
	    
	    
    void OnWizardOtherButton()
    {
    	this.Close();
    }
	
	
	// Context sensitive help
    void OnWizardUpdate()
    {
        helpString = "Choose a name for your animation";
		
		isValid = isAnimationNameValid();
    }
	
}