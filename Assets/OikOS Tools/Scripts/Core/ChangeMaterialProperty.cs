using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class ChangeMaterialProperty : MonoBehaviour {

	[SerializeField()]
	public int propertyIndex = 0;
	[SerializeField()]
	public string stored_propertyType;
	[SerializeField()]
	public string stored_propertyName;

	public float value_float = 0;
	public Color value_color = Color.white;

	// Update is called once per frame
	void Update () {
		if (propertyIndex >= 0) {
			Material m = GetComponent<Renderer>().material;
			if (stored_propertyType == "System.Single") {
				if (m.GetFloat(stored_propertyName) != value_float)
					m.SetFloat(stored_propertyName, value_float);
			} else if (stored_propertyType == "UnityEngine.Color") {
			//	if (m.GetColor(stored_propertyName) != value_color)
					m.SetColor(stored_propertyName, value_color);
			}
		}
	}
}
