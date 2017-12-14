
// OikOS Toolkit - Visual game making tools for Unity

// Developed by Fernando Ramallo
// Copyright (C) 2017 David Kanaga

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Shader "Custom/Offset Vertices By Sine Outwards" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DispTex ("Displace by texture", 2D) = "white" {}
		_DispTiling ( "Texture Scale", float ) = 1
		
		_DispIntensity ( "By Texture", Range(0,1) ) = 1
		_Height ( "Max Height", float ) = 1
		_SineIntensity ( "By Sinewave", Range(0,1) ) = 1
		_Frequency ( "Sinewave Frequency", float ) = 0.25
		_Speed ( "Sinewave Speed", float ) = 5
		
	}
	SubShader {
		Tags {"IgnoreProjector"="True" "RenderType"="Opaque"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow
		#pragma target 3.0
		#pragma glsl
		#include "UnityCG.cginc"
		

		half4 _Color;
		sampler2D _MainTex;
		sampler2D _DispTex;
		float _SineIntensity;
		float _DispIntensity;
		float _DispTiling;
		float _Height;
		float _Frequency;
		float _Speed;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Emission = c.rgb * _Color;
			o.Alpha = c.a * _Color.a;
		}
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float sineDisp = sin((v.vertex.x + v.vertex.y + v.vertex.z) * _Frequency + _Time * _Speed + _Time) * _SineIntensity;
			float3 worldVertex = mul ( unity_WorldToObject, v.vertex );
			float2 uv = float2(frac(worldVertex.x * _DispTiling) + frac(worldVertex.y * _DispTiling),frac(worldVertex.z * _DispTiling));
			float texDisp = tex2Dlod(_DispTex, float4(uv,1,1)) * _DispIntensity;
			
			v.vertex.xyz += v.normal * ((-0.5 + sineDisp) * (-0.5 + texDisp) * _Height) / 1.0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
