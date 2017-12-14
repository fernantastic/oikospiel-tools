
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
	[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
	public class Follow : MonoBehaviour {

		public enum TargetType {
			Player,
			Object
		}

		public Transform target;
		public TargetType targetType = TargetType.Player;

		public float sightRadius = 7;
		public float stopAtDistance = 1.15F;
		public float speed = 2;
		public string animationOnSeen = "walk";
		public string animationOnUnseen = "idle";

		bool _sawTarget = false;

		void Start () {
			if (targetType == TargetType.Player)
				target = Player.instance.transform;
		}

		void Update () {
			bool seen = Vector3.Distance(transform.position, target.position) < sightRadius;
			if (seen != _sawTarget) {
				Animation anim = GetComponentInChildren<Animation>();
				if (seen) {
					if (anim && anim[animationOnSeen] != null)
						anim.Play(animationOnSeen);
				} else {
					if (anim && anim[animationOnUnseen] != null)
						anim.Play(animationOnUnseen);
				}
				_sawTarget = seen;
			}
			if (_sawTarget) {
				if (Vector3.Distance(transform.position, target.position) > stopAtDistance) {
					transform.LookAt(target, Vector3.up);
					transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
				}

			}
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = _sawTarget ? Color.green : Color.gray;
			if (target != null)
				Gizmos.DrawLine(transform.position, target.position);
			//Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, sightRadius);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, stopAtDistance);
		}
	}
}