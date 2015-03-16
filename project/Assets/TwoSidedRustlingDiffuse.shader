Shader "Custom/Two Sided/Rustled Diffuse"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		_Amount ("Amount", Range(0,.2)) = 0.1
		_Scale ("Scale", Range(0.1,10)) = 1.0
		_TimeScale ("Time Scale", Range(0,10)) = 1.0
		_Cutoff ("Cutoff", Range (0,1)) = 0.5
		_NormalInfluence ("Normal Influence", Range (0,1)) = 0.5
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Cull Off

		CGPROGRAM
		#pragma surface surf LambertDoubleSided vertex:vert addshadow alphatest:_Cutoff 

		struct Input
		{
			float2 uv_MainTex;
		};

		half4 _Direction;
		float _Amount;
		float _TimeScale;
		float _Scale;
		sampler2D _MainTex;
		half4 _Color;
		float _NormalInfluence;

		void vert(inout appdata_full v)
		{
			float4 offs = (lerp(float4 (v.normal, 1), v.color, _NormalInfluence) * sin (_Time.y * _TimeScale + (v.vertex.y * _Scale) + (v.vertex.x * _Scale) - (v.vertex.z * _Scale)) * _Amount) * v.color.a;
			v.vertex += offs;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = c.a * _Color.a;
		}

		fixed4 LightingLambertDoubleSided (SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed diff = abs ( dot (s.Normal, lightDir));
	
			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
			c.a = s.Alpha;
			return c;
		}
	
		ENDCG
	}
	Fallback "Transparent/Cutout/Diffuse"
}