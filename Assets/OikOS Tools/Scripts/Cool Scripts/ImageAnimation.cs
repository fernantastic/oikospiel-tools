using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageAnimation : MonoBehaviour {

	public Sprite[] frames;

	public float fps = 5;
	Image image;

	// Use this for initialization
	void OnEnable () {
		image = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		if (frames.Length == 0) return;
		var s = frames[Mathf.FloorToInt(Mathf.Repeat(Time.time * fps,frames.Length))];
		if (s && image.sprite != s) image.sprite = s;

	}
}
