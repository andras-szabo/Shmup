Shader "Custom/MultiCurvedSpaceWave"
{
	Properties
	{
		_ScrollSpeedX("Scroll speed x", float) = 0.0
		_ScrollSpeedY("Scroll speed y", float) = 0.0
		_MainTex("Texture", 2D) = "white" {}

		_UVDistance("UV Distance of BG", float) = 10.0

		_RotationAngle("Rot angle", float) = 0.0
		_MaxDelta("Max delta", float) = 5.0
		_CentreY("Centre Y", float) = 5.0

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
				#include "HLSLSupport.cginc"

				#define MAX_WEIGHTED_OBJECTS 16
				#define MAX_RIPPLES 4
				#define RIPPLE_WIDTH 1

				float4 _Array[MAX_WEIGHTED_OBJECTS];
				float4 _Ripples[MAX_RIPPLES];

				float4 _CurveOrigin;

				float _ScrollSpeedX;
				float _ScrollSpeedY;

				float4x4 _UVRotationMatrix;
				float _UVDistance;

				sampler2D _MainTex;
				float4 _TintColor;
				float4 _ObjectWorldPos;

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

				float _CentreY;
				float _MaxDelta;
				float _RotationAngle;

				float2x2 GetUVRotMatrix(float2 worldPos)
				{
					float dy = clamp(worldPos.y - _CentreY, 0, _MaxDelta);
					float angle = lerp(0, _RotationAngle, dy / _MaxDelta);
					float angleInRad = radians(angle);
					float s, c;
					sincos(angleInRad, s, c);
					return float2x2(c, -s, s, c);
				}
		
				float2 CalculateWorldSpaceUV(float4 worldSpacePos)
				{
					float2 tex = worldSpacePos.xy / _UVDistance;
					return mul(GetUVRotMatrix(worldSpacePos.xy), tex) + frac(_Time.y * float2(_ScrollSpeedX, _ScrollSpeedY * _ProjectionParams.x));
				}

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

					vertexOutput o;
					o.pos = mul(UNITY_MATRIX_VP, worldSpacePos);
					o.tex = CalculateWorldSpaceUV(worldSpacePos);
					o.tex.y *= _ProjectionParams.x;

					return o;
				}

				float4 frag(vertexOutput input) : COLOR
				{
					return tex2D(_MainTex, input.tex.xy) * _TintColor;
				}

				ENDCG
			}
		}
}