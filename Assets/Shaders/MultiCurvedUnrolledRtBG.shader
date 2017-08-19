Shader "Custom/MultiCurvedSpaceWaveBg"
{
	Properties
	{
		_ScrollSpeedX("Scroll speed x", float) = 0.0
		_ScrollSpeedY("Scroll speed y", float) = 0.0
		_MainTex("Texture", 2D) = "white" {}
		_BgTex("Background", 2D) = "white" {}

		[PerRendererData]
		_TintColor("Tint color", color) = (1, 1, 1, 1)
	}

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				#define MAX_WEIGHTED_OBJECTS 16
				#define MAX_RIPPLES 4
				#define RIPPLE_WIDTH 1

				float4 _Array[MAX_WEIGHTED_OBJECTS];
				float4 _Ripples[MAX_RIPPLES];

				float _ScrollSpeedX;
				float _ScrollSpeedY;

				sampler2D _MainTex;
				sampler2D _BgTex;
				half _MaxWeight;
				float4 _TintColor;

				struct vertexInput
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct vertexOutput
				{
					float4 pos : SV_POSITION;
					float2 tex : TEXCOORD0;
				};

				vertexOutput vert(vertexInput input)
				{
					float4 worldSpacePos = mul(unity_ObjectToWorld, input.vertex);
					float weight = 0;

					[unroll] for (int i = 0; i < MAX_WEIGHTED_OBJECTS; ++i)
					{
						float4 item = _Array[i];
						float dist = length(worldSpacePos.xy - item.xy);
						weight += item.z * (item.w - (clamp(dist - item.w, 0, item.w)));
					}
					
					[unroll] for (int j = 0; j < MAX_RIPPLES; ++j)
					{
						float4 item = _Ripples[j];
						float dist = clamp(length(worldSpacePos.xy - item.xy) - item.z, -RIPPLE_WIDTH, RIPPLE_WIDTH);
						weight += sign(item.z) * (lerp(item.w, 0, abs(dist) / RIPPLE_WIDTH));
					}

					worldSpacePos.z += weight;
					float4 bg = tex2Dlod(_BgTex, input.texcoord);
					if (bg.r > 0.5)
					{
						worldSpacePos.z += 8;
					}

					vertexOutput o;
					o.pos = mul(UNITY_MATRIX_VP, worldSpacePos);
					o.tex.xy = input.texcoord.xy + frac(_Time.y * float2(_ScrollSpeedX, _ScrollSpeedY));
					return o;
				}

				float4 frag(vertexOutput input) : COLOR
				{
					return tex2D(_MainTex, input.tex.xy) * _TintColor + tex2D(_BgTex, input.tex.xy);
				}

				ENDCG
			}
		}
}