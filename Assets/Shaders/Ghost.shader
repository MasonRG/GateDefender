Shader "Custom/Ghost"
{
	Properties
	{
		_Tint ("Color Tint", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Main Texture", 2D) = "white" {}
	}
	SubShader
	{

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Lighting Off
		Zwrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			Name "BASE"

			Stencil {
				Ref 2
				Comp NotEqual
				Pass Replace
			}

		

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Tint;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f IN) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, IN.uv);
				col = col * _Tint;
				return col;
			}
			ENDCG
		}
	}
}
