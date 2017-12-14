
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

using UnityEngine;
using System.Collections;

namespace OikosTools {
	public class Tools {

		static public float GetMinMaxValue(float Min, float Max, float Ratio)
		{
			float _min = Min < Max ? Min : Max;
			float _max = Min < Max ? Max : Min;
			if (Min > Max) Ratio = 1f - Ratio;
			return _min + Ratio * (_max - _min);
		}
		
		static public float GetMinMaxValueLog10(float Min, float Max, float Ratio)
		{
			//Ratio = Mathf.Abs(1-Mathf.Log10(1f + Ratio * 9f));
			//Ratio = Mathf.Exp(Ratio);
			
			if (Min < Max)
				Ratio = Mathf.Abs(1f - Mathf.Log10(1f + Mathf.Abs(1f-Ratio) * 9f));
			else
				Ratio = Mathf.Log10(1f + Mathf.Abs(1f-Ratio) * 9f);
			
			return GetMinMaxValue(Min, Max, Ratio);
		}

		static public float GetNormalizedValue(float Min, float Max, float Value)
		{
			return Mathf.Clamp01((Value-Min)/(Max-Min));
		}
		static public float GetNormalizedAngleValue(float Min, float Max, float Value)
		{
			Value -= 180;
			float v = Tools.GetNormalizedValue(Min, Max, Value);
			return Mathf.PingPong((v - 0.5f) * 2, 1);
		}

	}
}