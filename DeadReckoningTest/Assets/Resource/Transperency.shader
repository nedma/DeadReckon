// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/DiffuseTransparency" {
Properties {
	_Color ("Color", Color) = (0.26,0.19,0.16)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Transparency ("Transparency", Range (0.0, 1.0)) = 1.0
}
SubShader {
	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
	LOD 150
	//ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha
	
	
CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
float _Transparency;
fixed3 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color;
	o.Alpha = _Transparency;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
