using UnityEngine;
using System.Collections;



public class SKSpriteAnimationState : ScriptableObject
{
	public enum CompleteRule
	{
		None,
		DestorySprite,
		HideSprite,
		RevertToOriginalSprite
	}
	
	public string[] imageNames;
	public SKSpriteSheet spriteSheet;
	public WrapMode wrapMode = WrapMode.Once;
	public int iterations = 1;
	public float speed = 1;
	public float delay = 0;
	public float framesPerSecond = 5;
	public CompleteRule completeRule = CompleteRule.RevertToOriginalSprite;
	public bool playAutomatically = true;
	
	public SKTextureInfo[] textureInfo { get; set; }
	
	
	public void OnEnable()
	{
		if( imageNames == null )
			return;

		// prep our actual SKTextureInfo array from the imageNames
		textureInfo = new SKTextureInfo[imageNames.Length];
		
		for( var i = 0; i < imageNames.Length; i++ )
			textureInfo[i] = spriteSheet.textureInfoForImage( imageNames[i] );
	}

}