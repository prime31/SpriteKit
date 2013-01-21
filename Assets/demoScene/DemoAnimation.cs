using UnityEngine;
using System.Collections;


/// <summary>
/// simple example showing how to play an animation on load
/// </summary>
public class DemoAnimation : MonoBehaviour
{
	public string animationName;
	
	
	void Start()
	{
		var sprite = GetComponent<SKSprite>();
		if( sprite == null )
			return;
		
		sprite.startAnimation( animationName );
	}
}
