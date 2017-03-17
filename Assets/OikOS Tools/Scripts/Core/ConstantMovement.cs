using UnityEngine;
using System.Collections;

public class ConstantMovement : MonoBehaviour {
	public enum Type {
		RotateConstantly,
		MoveConstantly,
		ScaleConstantly,
		MoveBySinewave,
		RotateBySinewave,
		ScaleBySinewave
	}
	public Type type = Type.RotateConstantly;

	public Vector3 velocity = Vector3.zero;
	public Space space = Space.Self;
	public float sineFrequency = 0.25f;

	void Update () {
		float sine = Mathf.Sin(Time.time * sineFrequency);
		if (type == Type.MoveConstantly) {
			transform.Translate(velocity * Time.deltaTime, space);
		} else if (type == Type.RotateConstantly) {
			transform.Rotate(velocity * Time.deltaTime, space);
		} else if (type == Type.MoveBySinewave) {
			transform.Translate(velocity * sine, space);
		} else if (type == Type.RotateBySinewave) {
			transform.Rotate(velocity * sine, space);
		} else if (type == Type.ScaleConstantly) {
			transform.localScale = transform.localScale + velocity * Time.deltaTime;
		} else if (type == Type.ScaleBySinewave) {
			transform.localScale = transform.localScale + velocity * sine;
		}
	}

}
