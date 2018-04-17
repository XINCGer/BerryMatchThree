Shader "BerryShader/HSV Shift Addative"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
         _Speed ("_Speed", Float ) = 1
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One One

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma multi_compile DUMMY PIXELSNAP_ON

		sampler2D _MainTex;
		fixed4 _Color;
		float _Speed;

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
		};
		
		float3 shift_col(float3 RGB, float shift)
              {
              float3 RESULT = float3(RGB);
              float VSU = cos(shift.x);
                  float VSW = sin(shift.x);
                 
                  RESULT.x = (.299+.701*VSU+.168*VSW)*RGB.x
                          + (.587-.587*VSU+.330*VSW)*RGB.y
                          + (.114-.114*VSU-.497*VSW)*RGB.z;
                  
                  RESULT.y = (.299-.299*VSU-.328*VSW)*RGB.x
 
                        + (.587+.413*VSU+.035*VSW)*RGB.y
                          + (.114-.114*VSU+.292*VSW)*RGB.z;
                  
                  RESULT.z = (.299-.3*VSU+1.25*VSW)*RGB.x
                          + (.587-.588*VSU-1.05*VSW)*RGB.y
                          + (.114+.886*VSU-.203*VSW)*RGB.z;
                  
              return (RESULT);
              }

		void vert (inout appdata_full v, out Input o)
		{
			#if defined(PIXELSNAP_ON) && !defined(SHADER_API_FLASH)
			v.vertex = UnityPixelSnap (v.vertex);
			#endif
			v.normal = float3(0,0,-1);
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color * _Color;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Emission = half3(shift_col(c.rgb * c.a, _Time.x * _Speed));
		}
		ENDCG
	}

Fallback "Transparent/VertexLit"
}
