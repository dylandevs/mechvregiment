 
Shader "Toon/Surf"
{
    Properties
    {
       _Color ("Main Color", Color) = (1,1,1,1)
       _MainTex ("Base", 2D) = "" {}
       _BlendTex ("Under Tone", 2D) = "" {}
       _Ramp ("Lighting Ramp", 2D) = "" {}
       _Amount ("Distort Amount", Range(0,0.1)) = 0.05
       _WaveSize ("Wave Size", Range(0.1,10)) = 1.0
       _TimeScale ("Time Scale", Range(0,10)) = 1.0
       _TexTimeScale ("Texture Speed", Range(0,5)) = 1.0
       _BlendAlpha ("Blend Alpha", Range(1,-1)) = -0.5
       _NormalInfluence ("Normal Influence", Range (0,1)) = 0.5
    }
    SubShader
    {
       Tags { "Queue"="Geometry-9" "IgnoreProjector"="True" "RenderType"="Transparent" }
       Lighting Off
       LOD 200
       Blend SrcAlpha OneMinusSrcAlpha
 
       CGPROGRAM
       #pragma surface surf Ramp vertex:vert
 
       sampler2D _Ramp;

       fixed4 _Color;
       sampler2D _MainTex;
       sampler2D _BlendTex;
       float _BlendAlpha;

       half4 _Direction;
       float _Amount;
       float _TimeScale;
       float _TexTimeScale;
       float _WaveSize;
       float _NormalInfluence;
 
       struct Input {
         float2 uv_MainTex;
         float4 _Time;
       };
 
       void surf (Input IN, inout SurfaceOutput o) {
         fixed4 c = ( ( 1 - _BlendAlpha ) *  tex2D( _MainTex, IN.uv_MainTex - _Time.xx * _TexTimeScale) + _BlendAlpha * tex2D( _BlendTex, IN.uv_MainTex + _Time.xx * _TexTimeScale) ) * _Color;
         o.Albedo = c.rgb;
         o.Alpha = c.a;
       }

       void vert(inout appdata_full v)
       {
         float4 vertoffset = (lerp(float4 (v.normal, 1), v.color, _NormalInfluence) * sin (_Time.y * _TimeScale + (v.vertex.y * _WaveSize) + (v.vertex.x * -_WaveSize) - (v.vertex.z * _WaveSize)) * _Amount) * v.color.a;
         v.vertex += vertoffset;
       }

        half4 LightingRamp (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = dot (s.Normal, lightDir);
          half diff = NdotL * 0.5 + 0.5;
          half3 ramp = tex2D (_Ramp, float2(diff)).rgb;
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (ramp*1.3) * (atten * 2);
          c.a = s.Alpha;
          return c;
       }

       ENDCG
    }
 
    Fallback "Transparent/VertexLit"
}