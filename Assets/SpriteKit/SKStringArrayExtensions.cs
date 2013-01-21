using UnityEngine;
using System.Collections;


public static class SKStringArrayExtensions
{
	public static bool contains( this string[] arr, string item )
	{
		for( var i = 0; i < arr.Length; i++ )
		{
			if( arr[i] == item )
				return true;
		}
		return false;
	}
}
