Shader "SpriteKit/Basic"
{
	Properties
	{
		_MainTex( "Texture (RGBA)", 2D ) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		

		Pass
		{
			CGPROGRAM
	      
			#pragma exclude_renderers ps3 xbox360
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			
			sampler2D _MainTex;
			

			struct a2v
			{
				float4 vertex : POSITION;
				float4 color: COLOR;
				float4 texcoord : TEXCOORD0;
			};

			
			struct v2f
			{
				float4 pos : SV_POSITION;
			    float4 color : COLOR0;
			    half2 uv : TEXCOORD0;
			};
			

			
			
			v2f vert( a2v input )
			{
			    v2f o;
			    o.uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, input.texcoord );
				o.pos = mul( UNITY_MATRIX_MVP, input.vertex );
			    o.color = input.color;
			    
			    return o;
			}
			

			half4 frag( v2f i ) : COLOR
			{
				i.color = tex2D( _MainTex, i.uv ) * i.color;
				return i.color;
			}
		
		    ENDCG
		}
	}


}