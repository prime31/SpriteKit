using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[ExecuteInEditMode]
public class SKNineSliceSprite : SKSprite
{
	public RectOffset offsets;
	
	
   	protected override void generateVerts( ref Mesh mesh )
	{
		// fetch required information and cache the uv values
		var spriteInfo = spriteSheet.textureInfoForImage( sourceImageName );
		var uvRect = spriteInfo.uvRect;
		var uvs = new Vector2[16];
		
		// we want to get the uv size of all the offsets that we already have in the RectOffset
		// we use a Vector4 because RectOffset is ints only and we must have floats here. we will consider: x: left, y: top, z: right, w: bottom
		var uvOffsets = Vector4.zero;
		uvOffsets.x = (float)offsets.left / pixelPerfectHDSize.x * uvRect.width;
		uvOffsets.y = (float)offsets.top / pixelPerfectHDSize.y * uvRect.height;
		uvOffsets.z = (float)offsets.right / pixelPerfectHDSize.x * uvRect.width;
		uvOffsets.w = (float)offsets.bottom / pixelPerfectHDSize.y * uvRect.height;
		
		// handle filling up the UV values
        uvs[0] = new Vector2( uvRect.xMin, uvRect.yMax );
		uvs[1] = new Vector2( uvRect.xMin + uvOffsets.x, uvRect.yMax );
		uvs[2] = new Vector2( uvRect.xMax - uvOffsets.z, uvRect.yMax );
		uvs[3] = new Vector2( uvRect.xMax, uvRect.yMax );
		
        uvs[4] = new Vector2( uvRect.xMin, uvRect.yMax - uvOffsets.y );
		uvs[5] = new Vector2( uvRect.xMin + uvOffsets.x, uvRect.yMax - uvOffsets.y );
		uvs[6] = new Vector2( uvRect.xMax - uvOffsets.z, uvRect.yMax - uvOffsets.y );
		uvs[7] = new Vector2( uvRect.xMax, uvRect.yMax - uvOffsets.y );
		
        uvs[8] = new Vector2( uvRect.xMin, uvRect.yMin + uvOffsets.w );
		uvs[9] = new Vector2( uvRect.xMin + uvOffsets.x, uvRect.yMin + uvOffsets.w );
		uvs[10] = new Vector2( uvRect.xMax - uvOffsets.z, uvRect.yMin + uvOffsets.w );
		uvs[11] = new Vector2( uvRect.xMax, uvRect.yMin + uvOffsets.w );
		
        uvs[12] = new Vector2( uvRect.xMin, uvRect.yMin );
		uvs[13] = new Vector2( uvRect.xMin + uvOffsets.x, uvRect.yMin );
		uvs[14] = new Vector2( uvRect.xMax - uvOffsets.z, uvRect.yMin );
		uvs[15] = new Vector2( uvRect.xMax, uvRect.yMin );


		// vert storage list and prep the anchor offset
		var verts = new List<Vector3>( 16 );
		var anchorOffset = offsetForAnchor();
		
		// create a multiplier to adjust for different ortho sizes
		var orthoAdjustment = ( 2f * spriteSheet.cameraOrthoSize ) / spriteSheet.targetScreenHeight;
		var orthoAdjustedDesiredSize = desiredSize * orthoAdjustment;

		// we build the quads from bottom left to top right
		var rect = new Rect( 0, 0, orthoAdjustedDesiredSize.x * scale.x, orthoAdjustedDesiredSize.y * scale.y );
		rect.center += anchorOffset;

		// add the quad to our vert list
		verts.Add( new Vector3( rect.xMin, rect.yMax, 0 ) ); // tl
		verts.Add( new Vector3( rect.xMin + offsets.left, rect.yMax, 0 ) );
		verts.Add( new Vector3( rect.xMax - offsets.right, rect.yMax, 0 ) );
		verts.Add( new Vector3( rect.xMax, rect.yMax, 0 ) ); // tr
		
		verts.Add( new Vector3( rect.xMin, rect.yMax - offsets.top, 0 ) );
		verts.Add( new Vector3( rect.xMin + offsets.left, rect.yMax - offsets.top, 0 ) );
		verts.Add( new Vector3( rect.xMax - offsets.right, rect.yMax - offsets.top, 0 ) );
		verts.Add( new Vector3( rect.xMax, rect.yMax - offsets.top, 0 ) );
		
		verts.Add( new Vector3( rect.xMin, rect.yMin + offsets.bottom, 0 ) );
		verts.Add( new Vector3( rect.xMin + offsets.left, rect.yMin + offsets.bottom, 0 ) );
		verts.Add( new Vector3( rect.xMax - offsets.right, rect.yMin + offsets.bottom, 0 ) );
		verts.Add( new Vector3( rect.xMax, rect.yMin + offsets.bottom, 0 ) );
		
		verts.Add( new Vector3( rect.xMin, rect.yMin, 0 ) ); // bl
		verts.Add( new Vector3( rect.xMin + offsets.left, rect.yMin, 0 ) );
		verts.Add( new Vector3( rect.xMax - offsets.right, rect.yMin, 0 ) );
		verts.Add( new Vector3( rect.xMax, rect.yMin, 0 ) ); // br
		
		
		var tris = new int[] { 1, 4, 0, 5, 4, 1, /**/ 2, 5, 1, 6, 5, 2, /**/ 3, 6, 2, 7, 6, 3, /**/ 5, 8, 4, 9, 8, 5, /**/ 6, 9, 5, 10, 9, 6, /**/ 7, 10, 6, 11, 10, 7, /**/ 9, 12, 8, 13, 12, 9, /**/ 10, 13, 9, 14, 13, 10, /**/ 11, 14, 10, 15, 14, 11 };
		var colors = new Color[] { tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor, tintColor };


		mesh.vertices = verts.ToArray();
		mesh.colors = colors;
		mesh.triangles = tris;
		mesh.uv = uvs;
	}

}
