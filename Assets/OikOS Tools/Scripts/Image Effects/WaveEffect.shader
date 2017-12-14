
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

Shader "Custom/WaveEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DispMap ("Displacement Map", 2D) = "white" {}
		_Intensity ( "Intensity", float ) = 1
		_WavinessX ("Horizontal Waviness", float) = 2
		_WavinessY ("Vertical Waviness", float) = 2
	}
	SubShader {
		ZTest Always 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } //Rendering settings

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata_img v){
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
				return o; 
			}

			sampler2D _MainTex;
			sampler2D _DispMap;
			float _Intensity;
			float _WavinessX;
			float _WavinessY;

			fixed4 frag (v2f i) : COLOR{
				float2 uv = i.uv;
				//uv.x += sin(uv.x * _Time * _WavinessX) * _Intensity + sin(uv.y * _Time * _WavinessY) * _Intensity;
				//uv.y += sin(uv.y + _Time * _WavinessY) * _Intensity + sin(uv.x * _Time * _WavinessX) * _Intensity;
				fixed4 disp = tex2D(_DispMap, i.uv);
				uv.x = uv.x + sin(uv.x * disp * _WavinessX + _Time * 25) * disp * _Intensity;
				uv.y = uv.y + cos(uv.y * disp * _WavinessY + _Time * 25) * disp * _Intensity;
				fixed4 color = tex2D(_MainTex, uv);

				return color;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}