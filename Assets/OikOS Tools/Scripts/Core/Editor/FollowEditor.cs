
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
using System.Collections.Generic;
using UnityEditor;

namespace OikosTools {
	[CustomEditor(typeof(Follow)), CanEditMultipleObjects()]
	public class FollowEditor : Editor {

		public override void OnInspectorGUI ()
		{
			//base.OnInspectorGUI ();
			Follow f = target as Follow;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(f, "Change Follow value");


			f.sightRadius = EditorGUILayout.FloatField("Sight distance", f.sightRadius);
			if (f.sightRadius < 0.1f)
				f.sightRadius = 0.1f;
			f.stopAtDistance = EditorGUILayout.FloatField("Stop at", f.stopAtDistance);
			if (f.stopAtDistance < 0)
				f.stopAtDistance = 0;

			f.speed = EditorGUILayout.FloatField("Speed", f.speed);

			f.animationOnSeen = EditorTools.AnimationPopup("Animation On Seen", f.GetComponentInChildren<Animation>(), f.animationOnSeen);
			f.animationOnUnseen = EditorTools.AnimationPopup("Animation On Unseen", f.GetComponentInChildren<Animation>(), f.animationOnUnseen);
			/*
			f.targetType = (Follow.TargetType)EditorGUILayout.EnumPopup("Follow what?", f.targetType);
			if (f.targetType == Follow.TargetType.Object) {
				f.target = (Transform)EditorGUILayout.ObjectField("Target Object", f.target, typeof(Transform), true);
			}
			*/

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(f);
			}

		}

		void OnSceneGUI() {
			Follow f = target as Follow;

			Handles.matrix = f.transform.localToWorldMatrix;
			Handles.color = Color.green;
			f.sightRadius = Handles.RadiusHandle(Quaternion.identity, Vector3.zero, f.sightRadius);
			Handles.color = Color.blue;
			f.stopAtDistance = Handles.RadiusHandle(Quaternion.identity, Vector3.zero, f.stopAtDistance);

		}


	}
}
