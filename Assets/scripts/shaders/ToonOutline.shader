Shader "Toon/Outline" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Outline("Outline Thickness", Range(0.0, 0.3)) = 0.002
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_UnlitThreshhold("Unlit threshhold", Range(-1, 1)) = 0.0
	}
 
	CGINCLUDE
	#include "UnityCG.cginc"
 
	sampler2D _MainTex;
	half4 _MainTex_ST;
 
	half _Outline;
	half4 _OutlineColor;
	float _UnlitThreshhold;
 
	struct appdata {
		half4 vertex : POSITION;
		half4 uv : TEXCOORD0;
		half3 normal : NORMAL;
		fixed4 color : COLOR;
	};
 
	struct v2f {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		fixed4 color : COLOR;
		half3 normal : NORMAL;
	};
	ENDCG
 
	SubShader 
	{
		Tags {
			"RenderType"="Opaque"
			"Queue" = "Transparent"
		}
		
		Pass{
			Name "OUTLINE"
 
			Cull Front
 
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
 
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				half3 norm = mul((half3x3)UNITY_MATRIX_IT_MV, v.normal);
				half2 offset = TransformViewToProjection(norm.xy);
				o.pos.xy += offset * o.pos.z * _Outline;
				o.color = _OutlineColor;
				return o;
			}
 
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 o;
				o = i.color;
				return o;
			}
			ENDCG
		}
 
		Pass 
		{
			Name "TEXTURE"
 
			Cull Back
			ZWrite On
			ZTest LEqual
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
 
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.normal = UnityObjectToWorldNormal(v.normal); 
				return o;
			}
 
			fixed4 frag(v2f i) : COLOR 
			{
				fixed4 o;
				o = tex2D(_MainTex, i.uv.xy);
				float3 normalDirection = normalize(i.normal);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuse = 0.4 + 0.6 * step(_UnlitThreshhold, dot(normalDirection, lightDirection));
				return o * float4(diffuse, 1.0);
			}
			ENDCG
		}
	} 
}
