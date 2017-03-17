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
