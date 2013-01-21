using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent( typeof( MeshRenderer ) )]
[RequireComponent( typeof( MeshFilter ) )]
[ExecuteInEditMode]
public class SKSprite : MonoBehaviour
{
	public static bool isDoubleDensityScreen;
	
	public enum SpriteAnchor
	{
		TopLeft, TopCenter, TopRight,
		MiddleLeft, MiddleCenter, MiddleRight,
		BottomLeft, BottomCenter, BottomRight
	}
	
	// SpriteKit ivars
	public SKSpriteSheet spriteSheet;
	public string sourceImageName;
	public Vector2 pixelPerfectHDSize; // stores the actual size of the source image
	public Vector2 desiredSize; // store the desired size for the sprite. excess space will tile
	public Vector3 scale = new Vector3( 1, 1, 1 );
	public SpriteAnchor anchor = SpriteAnchor.TopLeft;
	public Color tintColor = Color.white;
	
	// MonoBehaviour overrides
	public new Renderer renderer;
	public MeshFilter meshFilter;
	
	private bool _isFlipped = false;
	
	// animation support
	public SKSpriteAnimation currentAnimation;
	public Dictionary<string, SKSpriteAnimationState> animations;
	
	
	#region Sprite Creation
	
	public static SKSprite createSprite( string imageName, SpriteAnchor anchor )
	{
		// find the sheet first
		var sheet = SKSpriteSheet.sheetWithSprite( imageName );
		
		return createSprite( sheet, imageName, anchor );
	}
	
	
	public static SKSprite createSprite( string sheetName, string imageName, SpriteAnchor anchor )
	{
		// find the sheet first
		var sheet = SKSpriteSheet.sheetWithName( sheetName );

		return createSprite( sheet, imageName, anchor );
	}
	
	
	public static SKSprite createSprite( SKSpriteSheet sheet, string imageName, SpriteAnchor anchor )
	{
		var targetGO = new GameObject( imageName );
		var sprite = targetGO.AddComponent<SKSprite>();
		sprite.spriteSheet = sheet;
		sprite.sourceImageName = imageName;
		sprite.anchor = anchor;
		
		var info = sheet.textureInfoForImage( imageName );
		sprite.pixelPerfectHDSize = sprite.desiredSize = info.size;
		sprite.renderer.sharedMaterial = sheet.getMaterial( isDoubleDensityScreen );
		sprite.generateMesh();
		
		return sprite;
	}

	#endregion
	

	public void Awake()
	{
		renderer = gameObject.renderer;
		meshFilter = GetComponent<MeshFilter>();
		
		if( spriteSheet == null )
			return;
		
		isDoubleDensityScreen = Screen.width > 480;

		generateMesh();
		renderer.sharedMaterial = spriteSheet.getMaterial( isDoubleDensityScreen );
	}
	
	
	void OnApplicationQuit()
	{
		renderer.material = null;
	}
	
	
	public void generateMesh()
	{
		if( spriteSheet == null || sourceImageName == null )
			return;

		Mesh mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
		
		mesh.name = "SKMesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		generateVerts( ref mesh );
		
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	
	public void setUVs( Rect uvRect )
	{
		var uvs = new Vector2[4];
		
		if( !_isFlipped )
		{
	        uvs[0] = new Vector2( uvRect.xMin, uvRect.yMax );
	        uvs[1] = new Vector2( uvRect.xMin, uvRect.yMin );
	        uvs[2] = new Vector2( uvRect.xMax, uvRect.yMin );
	        uvs[3] = new Vector2( uvRect.xMax, uvRect.yMax );
		}
		else
		{
	        uvs[3] = new Vector2( uvRect.xMin, uvRect.yMax );
	        uvs[2] = new Vector2( uvRect.xMin, uvRect.yMin );
	        uvs[1] = new Vector2( uvRect.xMax, uvRect.yMin );
	        uvs[0] = new Vector2( uvRect.xMax, uvRect.yMax );
		}
		
		meshFilter.sharedMesh.uv = uvs;
	}
	
	
	/// <summary>
	/// returns the required offset for a given anchor assuming quads are built from the bottom left to top right
	/// </summary>
	protected Vector2 offsetForAnchor()
	{
		Vector2 anchorOffset;
		
		switch( anchor )
		{
			case SpriteAnchor.TopLeft:
				anchorOffset = new Vector2( 0, -desiredSize.y );
				break;
			case SpriteAnchor.TopCenter:
				anchorOffset = new Vector2( -desiredSize.x / 2, -desiredSize.y );
				break;
			case SpriteAnchor.TopRight:
				anchorOffset = new Vector2( -desiredSize.x, -desiredSize.y );
				break;
			case SpriteAnchor.MiddleLeft:
				anchorOffset = new Vector2( 0, -desiredSize.y / 2 );
				break;
			case SpriteAnchor.MiddleCenter:
				anchorOffset = new Vector2( -desiredSize.x / 2, -desiredSize.y / 2 );
				break;
			case SpriteAnchor.MiddleRight:
				anchorOffset = new Vector2( -desiredSize.x, -desiredSize.y / 2 );
				break;
			case SpriteAnchor.BottomLeft:
				anchorOffset = Vector2.zero;
				break;
			case SpriteAnchor.BottomCenter:
				anchorOffset = new Vector2( -desiredSize.x / 2, 0 );
				break;
			case SpriteAnchor.BottomRight:
				anchorOffset = new Vector2( -desiredSize.x, 0 );
				break;
			default:
				anchorOffset = Vector2.zero;
				break;
		}
		
		// apply scale
		anchorOffset.x *= scale.x;
		anchorOffset.y *= scale.y;
		
		return anchorOffset;
	}
	
	
	protected Vector2[] getUvsForClippedQuad( Rect uvRect, float clippedWidth, float clippedHeight, Vector2 orthoAdjustedPixelPerfectHDSize )
	{
		// undo the scale of the width/height
		clippedWidth /= scale.x;
		clippedHeight /= scale.y;

		uvRect.width *= ( clippedWidth / orthoAdjustedPixelPerfectHDSize.x );
		uvRect.height *= ( clippedHeight / orthoAdjustedPixelPerfectHDSize.y );

		var uvArrayForFullQuad = new Vector2[4];
        uvArrayForFullQuad[0] = new Vector2( uvRect.xMin, uvRect.yMax );
        uvArrayForFullQuad[1] = new Vector2( uvRect.xMin, uvRect.yMin );
        uvArrayForFullQuad[2] = new Vector2( uvRect.xMax, uvRect.yMin );
        uvArrayForFullQuad[3] = new Vector2( uvRect.xMax, uvRect.yMax );
		
		return uvArrayForFullQuad;
	}

	
   	protected virtual void generateVerts( ref Mesh mesh )
	{
		// fetch required information and cache the uv values
		var spriteInfo = spriteSheet.textureInfoForImage( sourceImageName );
		var uvRect = spriteInfo.uvRect;
		var uvArrayForFullQuad = new Vector2[4];
		
		// handle filling up the UV values. flipped get reversed
		if( !_isFlipped )
		{
	        uvArrayForFullQuad[0] = new Vector2( uvRect.xMin, uvRect.yMax );
	        uvArrayForFullQuad[1] = new Vector2( uvRect.xMin, uvRect.yMin );
	        uvArrayForFullQuad[2] = new Vector2( uvRect.xMax, uvRect.yMin );
	        uvArrayForFullQuad[3] = new Vector2( uvRect.xMax, uvRect.yMax );
		}
		else
		{
	        uvArrayForFullQuad[3] = new Vector2( uvRect.xMin, uvRect.yMax );
	        uvArrayForFullQuad[2] = new Vector2( uvRect.xMin, uvRect.yMin );
	        uvArrayForFullQuad[1] = new Vector2( uvRect.xMax, uvRect.yMin );
	        uvArrayForFullQuad[0] = new Vector2( uvRect.xMax, uvRect.yMax );
		}
		
		// create a multiplier to adjust for different ortho sizes
		var orthoAdjustment = ( 2f * spriteSheet.cameraOrthoSize ) / spriteSheet.targetScreenHeight;
		var orthoAdjustedPixelPerfectHDSize = pixelPerfectHDSize * orthoAdjustment;
		
		
		// mesh storage lists
		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var tris = new List<int>();
		var colors = new List<Color>();
		
		// figure out the number of quads we are going to need
		var horizontalQuadCount = Mathf.CeilToInt( desiredSize.x / pixelPerfectHDSize.x );
		var verticalQuadCount = Mathf.CeilToInt( desiredSize.y / pixelPerfectHDSize.y );
		var triIndex = 0;
		var anchorOffset = offsetForAnchor() * orthoAdjustment;

		// do we have enough room for the full width and height? this will only matter for the last row/column
		var clipHorizontally = desiredSize.x / pixelPerfectHDSize.x != horizontalQuadCount;
		var clipVertically = desiredSize.y / pixelPerfectHDSize.y != verticalQuadCount;
		
		// we build the quads from bottom left to top right
		for( var x = 0; x < horizontalQuadCount; x++ )
		{
			for( var y = 0; y < verticalQuadCount; y++ )
			{
				var offset = new Vector2( x * orthoAdjustedPixelPerfectHDSize.x, y * orthoAdjustedPixelPerfectHDSize.y );
				var rect = new Rect( offset.x * scale.x, offset.y * scale.y, orthoAdjustedPixelPerfectHDSize.x * scale.x, orthoAdjustedPixelPerfectHDSize.y * scale.y );
				rect.center += anchorOffset;
				
				var didClipHorizontally = false;
				var didClipVertically = false;
				if( clipHorizontally && x == horizontalQuadCount - 1 )
				{
					didClipHorizontally = true;
					
					// get the actual width of the clipped quad
					var quadWidth = desiredSize.x % orthoAdjustedPixelPerfectHDSize.x;
					rect.width = quadWidth * scale.x;
				}
				
				if( clipVertically && y == verticalQuadCount - 1 )
				{
					didClipVertically = true;
					
					// get the actual height of the clipped quad
					var quadHeight = desiredSize.y % orthoAdjustedPixelPerfectHDSize.y;
					rect.height = quadHeight * scale.y;
					
					// offset vertically to line up the quads
					//rect.y += ( originalHeight - rect.height );
				}
				
				// add the quad to our vert list
				verts.Add( new Vector3( rect.xMin, rect.yMax, 0 ) );
				verts.Add( new Vector3( rect.xMin, rect.yMin, 0 ) );
				verts.Add( new Vector3( rect.xMax, rect.yMin, 0 ) );
				verts.Add( new Vector3( rect.xMax, rect.yMax, 0 ) );
				
				if( didClipHorizontally || didClipVertically )
					uvs.AddRange( getUvsForClippedQuad( uvRect, rect.width, rect.height, orthoAdjustedPixelPerfectHDSize ) );
				else
					uvs.AddRange( uvArrayForFullQuad );
				
				tris.Add( triIndex + 3 );
				tris.Add( triIndex + 1 );
				tris.Add( triIndex );
				tris.Add( triIndex + 2 );
				tris.Add( triIndex + 1 );
				tris.Add( triIndex + 3 );
				triIndex += 4;
				
				colors.Add( tintColor );
				colors.Add( tintColor );
				colors.Add( tintColor );
				colors.Add( tintColor );
			} // end for
		} // end for
		

		mesh.vertices = verts.ToArray();
		mesh.colors = colors.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.uv = uvs.ToArray();
	}
	
	
	public void flipHorizontally()
	{
		_isFlipped = !_isFlipped;
		
		// reverse our uvs
		var uvs = meshFilter.sharedMesh.uv;
		System.Array.Reverse( uvs );
		meshFilter.sharedMesh.uv = uvs;
	}
	
	
	public void faceBackwards()
	{
		if( !_isFlipped )
			flipHorizontally();
	}
	
	
	public void faceForwards()
	{
		if( _isFlipped )
			flipHorizontally();
	}
	
	
	#region Animations
	
	void Update()
	{
		if( currentAnimation != null )
		{
			if( currentAnimation.tick( Time.deltaTime ) )
			{
				// animation done. handle the completion rule
				switch( currentAnimation.animationState.completeRule )
				{
					case SKSpriteAnimationState.CompleteRule.DestorySprite:
						Destroy( gameObject );
						break;
					case SKSpriteAnimationState.CompleteRule.HideSprite:
						renderer.enabled = false;
						break;
					case SKSpriteAnimationState.CompleteRule.RevertToOriginalSprite:
						setUVs( spriteSheet.textureInfoForImage( sourceImageName ).uvRect );
						break;
				}
				
				currentAnimation = null;
			}
		}
	}

	
	public void preloadAnimations( params string[] names )
	{
		if( animations == null )
			animations = new Dictionary<string, SKSpriteAnimationState>( names.Length );
		
		foreach( var name in names )
		{
			var animState = Resources.Load( "Animations/" + name, typeof( SKSpriteAnimationState ) ) as SKSpriteAnimationState;
			animations[name] = animState;
		}
	}
	
	
	public void startAnimation( string name )
	{
		// if we dont have this one loaded yet, load it up
		if( animations == null || !animations.ContainsKey( name ) )
			preloadAnimations( name );
		
		if( currentAnimation == null )
			currentAnimation = new SKSpriteAnimation( this );
		
		currentAnimation.animationState = animations[name];
	}
	
	#endregion
	
}
