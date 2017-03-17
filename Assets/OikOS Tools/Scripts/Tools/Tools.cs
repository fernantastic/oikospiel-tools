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