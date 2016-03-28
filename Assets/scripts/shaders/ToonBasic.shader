Shader "Toon/Basic" {
	Properties {
		_Color("Outline Color", Color) = (0,0,0,1)
		_UnlitThreshhold("Unlit threshhold", Range(-1, 1)) = 0.0
	}
 
	SubShader {
		Pass {
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : POSITION;
				float3 normal : NORMAL;
			};

			fixed4 _Color;
			float _UnlitThreshhold;

			v2f vert(appdata i) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.normal = UnityObjectToWorldNormal(i.normal); 
				return o;
			}

			fixed4 frag(v2f i) : COLOR {				
				float3 normalDirection = normalize(i.normal);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuse = 0.4 + 0.6 * step(_UnlitThreshhold, dot(normalDirection, lightDirection));
				return _Color * float4(diffuse, 1.0);
			}

			ENDCG
		}
	}
}
