Shader "BerryShader/HSV"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Hue ("Hue", Range(-360, 360)) = 0
		_Saturation ("Saturation", Range(0, 1)) = 1
		_Value ("Value", Range(0, 2)) = 1
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
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha
		#pragma multi_compile DUMMY PIXELSNAP_ON

		sampler2D _MainTex;
		float _Hue;
		float _Saturation;
		float _Value;

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
		};
		
		float3 shift_col(float3 RGB, float3 shift) {
             float3 RESULT = float3(RGB);
             float VSU = shift.z*shift.y*cos(shift.x*3.14159265/180);
             float VSW = shift.z*shift.y*sin(shift.x*3.14159265/180);                 
 
             RESULT.x = (.299*shift.z+.701*VSU+.168*VSW)*RGB.x
				+ (.587*shift.z-.587*VSU+.330*VSW)*RGB.y
				+ (.114*shift.z-.114*VSU-.497*VSW)*RGB.z;

             RESULT.y = (.299*shift.z-.299*VSU-.328*VSW)*RGB.x
				+ (.587*shift.z+.413*VSU+.035*VSW)*RGB.y
				+ (.114*shift.z-.114*VSU+.292*VSW)*RGB.z;

			 RESULT.z = (.299*shift.z-.3*VSU+1.25*VSW)*RGB.x
				+ (.587*shift.z-.588*VSU-1.05*VSW)*RGB.y
				+ (.114*shift.z+.886*VSU-.203*VSW)*RGB.z;
			
			if (shift.z > 1)
				RESULT = shift.z - 1 + (2 - shift.z) * RESULT;

             return (RESULT);
             }


		void vert (inout appdata_full v, out Input o)
		{
			#if defined(PIXELSNAP_ON) && !defined(SHADER_API_FLASH)
			v.vertex = UnityPixelSnap (v.vertex);
			#endif
			v.normal = float3(0,0,-1);
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Emission = half3(shift_col(c.rgb, float3(_Hue, _Saturation, _Value))) * c.a;
			o.Alpha = c.a;
		}
		ENDCG
	}

Fallback "Transparent/VertexLit"
}
