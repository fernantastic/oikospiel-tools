
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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text.RegularExpressions;

namespace OikosTools {
	public class TextBlock : MonoBehaviour {
		
		public GameObject container;
		public GameObject dialogObject;
		public GameObject syllableObject;
		public GameObject waitObject;

		public Image containerBackground;

		GameObject[] dialogLines;
		int _currentLine = 0;

		public bool waitingForInput { set { waitObject.SetActive(value); } }

		void Start() {
			if (syllableObject != null) {
				syllableObject.transform.SetParent(transform);
				syllableObject.SetActive(false);
			}
			dialogLines = new GameObject[50];
			dialogLines[0] = dialogObject;
			for (int i = 1; i < dialogLines.Length; i++) {
				dialogLines[i] = Instantiate(dialogObject) as GameObject;
				dialogLines[i].transform.SetParent(dialogObject.transform.parent);
				dialogLines[i].transform.localPosition = dialogObject.transform.localPosition;
				dialogLines[i].transform.localEulerAngles = dialogObject.transform.localEulerAngles;
				dialogLines[i].transform.localScale = dialogObject.transform.localScale;
				dialogLines[i].name = dialogObject.name + " (Line "+i+")";
				dialogLines[i].SetActive(false);
			}
			Hide();

		}
		
		public void Show(int Lines = 1) {
			//Debug.Log("Show dialog textBlock Lines:"+Lines);
			container.SetActive(true);
			for(int i = 0; i < Mathf.Min(Lines,dialogLines.Length); i++) {
				dialogLines[i].SetActive(true);
				AddLine(i, " "); // workaround for first button not responding to hover events
			}
			_currentLine = 0;
			waitingForInput = false;

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		public GameObject AddLine(int LineIndex, string str) {
			/*
			if (str.IndexOf('\n') != -1) {
				str = str.Remove(str.IndexOf('\n'), 1);
			}
			*/

			GameObject go = (GameObject)Instantiate(syllableObject);
			go.transform.SetParent(dialogLines[LineIndex].transform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = Vector3.one;
			go.transform.SetAsLastSibling();
			go.name = "Syllable " + str;
			go.SetActive(true);


			iTween.ScaleFrom(go, iTween.Hash("scale", Vector3.one * 0.15f, "time", 0.25f, "eastype", iTween.EaseType.easeOutBounce));

			go.GetComponentInChildren<Text>().text = str;

			return go;
		}
		public GameObject AddLine(string str) { return AddLine(_currentLine, str); }

		public void NextLine() { _currentLine = Mathf.Min(_currentLine + 1, dialogLines.Length-1); }
		
		public void Clear() {
			foreach(GameObject dobj in dialogLines) {
				int childs = dobj.transform.childCount;
				for (int i = childs - 1; i >= 0; i--)
				{
					GameObject.Destroy(dobj.transform.GetChild(i).gameObject);
				}
			}
		}

		public void Hide() {
			container.SetActive(false);
			waitingForInput = false;
			Clear();
			/*for(int i = 0; i < dialogObject.transform.GetChildCount(); i++) {
				Destroy(dialogObject.transform.GetChild(i));
				i = 0;
			}*/
			if (Player.isActive) Player.instance.SwitchControlMode();
		}

		public void OnMouseHover(GameObject SyllableObject) {
			//Debug.Log("hover on " + SyllableObject.GetComponentInChildren<Text>().text);
			/*
			int lineIndex = System.Array.IndexOf(dialogLines, SyllableObject.transform.parent);
			int syllableIndex = SyllableObject.transform.GetSiblingIndex() - 1; // -1 is a fix for first button not being hoverable
			MusicalDialog.current.OnSyllableHover(lineIndex, syllableIndex);
			*/
			MusicalDialog.current.OnSyllableHover(SyllableObject);
			SyllableObject.transform.localScale = Vector3.one * 1.3f;
			iTween.ScaleTo(SyllableObject, Vector3.one, 0.7f);
		}

		public void SwitchToStyle(MusicalDialogStyle Style) {
			
			var rt_container = container.GetComponent<RectTransform>();

			if (Style.dialogAlignment == MusicalDialogStyle.DialogAlignment.Top) {
				rt_container.anchorMin = new Vector2(0, 1);
				rt_container.anchorMax = new Vector2(1, 1);
				rt_container.pivot = new Vector2(0.5f, 1);
				rt_container.anchoredPosition3D = new Vector3(0, -Style.dialogMargin);
			} else if (Style.dialogAlignment == MusicalDialogStyle.DialogAlignment.Middle) {
				rt_container.anchorMin = new Vector2(0, 0.5f);
				rt_container.anchorMax = new Vector2(1, 0.5f);
				rt_container.pivot = new Vector2(0.5f, 0.5f);
				rt_container.anchoredPosition3D = new Vector3(0, 0);//Style.dialogMargin);
			} else if (Style.dialogAlignment == MusicalDialogStyle.DialogAlignment.Bottom) {
				rt_container.anchorMin = new Vector2(0, 0);
				rt_container.anchorMax = new Vector2(1, 0);
				rt_container.pivot = new Vector2(0.5f, 0);
				rt_container.anchoredPosition3D = new Vector3(0, Style.dialogMargin);
			}

			for (int i = 0; i < dialogLines.Length; i++) {
				var d = dialogLines[i].GetComponent<TextBlockDialogLine>();

				var rt = d.GetComponent<RectTransform>();
				var lg = d.GetComponent<HorizontalLayoutGroup>();

				var pivot = Vector3.one * .5f;
				if (Style.textAlignment == TextAlignment.Left) pivot.x = 0;
				if (Style.textAlignment == TextAlignment.Center) pivot.x = 0.5f;
				if (Style.textAlignment == TextAlignment.Right) pivot.x = 1;
				rt.pivot = pivot;

				var slg = syllableObject.GetComponent<HorizontalLayoutGroup>();
				lg.padding = new RectOffset(Style.textPadding.left, Style.textPadding.right, 0, 0);
				slg.padding = new RectOffset(0,0, Style.textPadding.top, Style.textPadding.bottom);
				
				if (Style.font != null) d.text.font = Style.font;
				d.text.color = Style.fontColor;
				d.text.fontStyle = Style.fontStyle;
				d.text.fontSize = Style.fontSize;
				d.text.lineSpacing = Style.lineSpacing;

				containerBackground.color = Style.backgroundColor;
				containerBackground.sprite = Style.backgroundTexture;


			}
		}

	}

}