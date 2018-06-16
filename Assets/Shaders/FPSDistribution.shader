Shader "Custom/FPSDistribution"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LowFPSColor ("Low fps color", color) = (1, 0, 0, 0)
		_MidFPSColor ("Mid fps color", color) = (1, 1, 0, 0)
		_HighFPSColor ("High fps color", color) = (0, 1, 0, 0)
		
		_LowMidThreshold ("Low - mid threshold", float) = 0.33
		_MidHighThreshold ("Mid - high threshold", float) = 0.66
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
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

			float _LowMidThreshold;
			float _MidHighThreshold;
			float4 _LowFPSColor;
			float4 _MidFPSColor;
			float4 _HighFPSColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//int index = i.uv.x * (HISTORY_LENGTH - 1);
				//fixed4 col = tex2D(_MainTex, i.uv);
				//fixed4 col = _Array[index];
				float midWeight = step(_LowMidThreshold, i.uv.x);		// for low: 0, for mid: 1, for hi: 1
				float lowWeight = 1 - midWeight;						// for low: 1, for mid: 0, for hi: 0
				float hiWeight = step(_MidHighThreshold, i.uv.x);		// for low: 0, for mid: 0, for hi: 1

				// low color: lowWeight
				// mid color: midweight - hiweight
				// hi color: hiweight

				float4 col = (lowWeight * _LowFPSColor) +
							 ((midWeight - hiWeight) * _MidFPSColor) +
							 (hiWeight * _HighFPSColor);

				return col;
			}
			ENDCG
		}
	}
}
