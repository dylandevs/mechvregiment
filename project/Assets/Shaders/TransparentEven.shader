Shader "Transparent/EvenLit" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Ambient ("Ambient", Range (0.01, 1)) = 0.5
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		CGPROGRAM
		// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
		#pragma exclude_renderers gles
		#pragma surface surf Lambert alpha
		#pragma vertex vert

		sampler2D _MainTex;
		fixed4 _Color;
		float _Ambient;

		struct Input {
			float2 uv_MainTex;
		};
            
		struct v2f {
			float3 normal : NORMAL;
		};
      
		v2f vert (inout appdata_base v)
		{
			v2f o;
			v.normal = _Ambient;
			return o;
		}
            
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	} 
	FallBack "Transparent/Diffuse"
}
