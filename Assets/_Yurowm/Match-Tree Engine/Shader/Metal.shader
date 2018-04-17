Shader "BerryShader/Metal" {
Properties {
	_Color ("Color", Color) = (1,1,1,1)
	_Emission ("Emission Color", Color) = (1,1,1,1)
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader {
	LOD 200
	Tags { "RenderType"="Geometry"}
	
CGPROGRAM
#pragma surface surf Lambert

samplerCUBE _Cube;

fixed3 _Emission;
fixed3 _Color;
fixed4 _ReflectColor;

struct Input {
	float2 uv_MainTex;
	float3 worldRefl;
};

void surf (Input IN, inout SurfaceOutput o) {
	
	fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
	o.Albedo = _Color + reflcol.rgb * _ReflectColor.rgb;
	o.Emission = _Emission.rgb;
}
ENDCG
}
	
FallBack "Reflective/VertexLit"
} 