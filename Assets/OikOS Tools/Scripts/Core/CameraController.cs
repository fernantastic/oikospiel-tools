
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
	public class CameraController : MonoBehaviour {

		public enum CameraMode {
			FirstPerson,
			ThirdPerson,
			CustomCamera,
			TopDownWithMouseControls
		}

		public float thirdPersonMoveTime = 2.5f;
		public float thirdPersonRotateTime = 2.5f;

		public float thirdPersonDistance = 4.07f;
		public float thirdPersonHeight = 1.67f;
		public float thirdPersonAngle = 17f;

		public float topDownDistance = 60f;
		public float topDownCameraFollowSpeed = 1f;

		CameraMode mode = CameraMode.FirstPerson;

		Vector3 _targetPosition = Vector3.zero;
		Quaternion _targetRotation = Quaternion.identity;
		Vector3 _topdownPosition = Vector3.zero;
		Quaternion _topdownRotation = Quaternion.LookRotation(Vector3.down);
		float weight_firstperson = 1;
		float weight_thirdperson = 0;
		float weight_custom = 0;
		float weight_topdown = 0;
		Transform _thirdPersonEasedObject;

		internal Camera gameCamera;

		internal bool _topdown_follow = false;

		bool _blockMovement = false;
		internal bool blockMovement { get { return _blockMovement; } }

		public static CameraController instance;
		void Awake () {
			instance = this;
			gameCamera = GetComponent<Camera>();

			_blockMovement = false;

			mode = CameraMode.FirstPerson;

			_thirdPersonEasedObject = new GameObject("_ThirdPersonCameraTarget").transform;
			_thirdPersonEasedObject.parent = transform.parent;
		}

		void Start() {
			if(Player.isActive) {
				_thirdPersonEasedObject.position = Player.instance.thirdPersonTarget.position;
				_thirdPersonEasedObject.rotation = Player.instance.thirdPersonTarget.rotation;
			}
		}

		void Update() {
			if(!Player.isActive)
				return;
			// smoothly move third person camera target
			Player.instance.thirdPersonTarget.localPosition = new Vector3(0,0.4f,-1) * thirdPersonDistance + Vector3.up * thirdPersonHeight;
			Player.instance.thirdPersonTarget.localEulerAngles = new Vector3(thirdPersonAngle,0,0);
			iTween.MoveUpdate(_thirdPersonEasedObject.gameObject, Player.instance.thirdPersonTarget.position, thirdPersonMoveTime);
			iTween.RotateUpdate(_thirdPersonEasedObject.gameObject, Player.instance.thirdPersonTarget.eulerAngles, thirdPersonRotateTime);

			if (mode == CameraMode.TopDownWithMouseControls && _topdown_follow)
				_topdownPosition = iTween.Vector3Update(_topdownPosition, Player.instance.transform.position + Vector3.up * topDownDistance, topDownCameraFollowSpeed);

			// update based on weight
			Vector3 position = transform.position;
			position = Vector3.Lerp (position, Player.instance.head.position, weight_firstperson);
			position = Vector3.Lerp (position, _thirdPersonEasedObject.position, weight_thirdperson);
			position = Vector3.Lerp (position, _targetPosition, weight_custom);
			position = Vector3.Lerp (position, _topdownPosition, weight_topdown);
			Quaternion rotation = transform.rotation;
			rotation = Quaternion.Lerp (rotation, Player.instance.head.rotation, weight_firstperson);
			rotation = Quaternion.Lerp (rotation, _thirdPersonEasedObject.rotation, weight_thirdperson);
			rotation = Quaternion.Lerp (rotation, _targetRotation, weight_custom);
			rotation = Quaternion.Lerp (rotation, _topdownRotation, weight_topdown);
			transform.position = position;
			transform.rotation = rotation;
		}

		public void MoveCamera(CameraMode Mode, float Time = 3, Transform CustomTarget = null) {


			if (CustomTarget != null && Vector3.Distance(CustomTarget.position, _targetPosition) < 0.1f && Mathf.Abs(Quaternion.Angle(CustomTarget.rotation, _targetRotation)) < 2)
				Time = 0;

			if (Mode == mode && (Mode == CameraMode.FirstPerson || Mode == CameraMode.ThirdPerson || Mode == CameraMode.TopDownWithMouseControls))
				Time = 0;

			if (Mode == CameraMode.CustomCamera && CustomTarget == null)
				return;
			
			if (Mode == CameraMode.TopDownWithMouseControls) {
				Player.instance.SwitchControlMode(Player.ControlMode.MouseFollow);
				_topdownPosition = Player.instance.transform.position + Vector3.up * topDownDistance;
			}
			else {
				Player.instance.SwitchControlMode(Player.ControlMode.FirstPerson);
			}

			// block movement 
			_blockMovement = Time > 0 && Mode == CameraMode.TopDownWithMouseControls;

			mode = Mode;
			iTween.Stop(gameObject);


			iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", weight_firstperson, "to", mode == CameraMode.FirstPerson ? 1f : 0f, "onupdate", "UpdateFirstPersonWeight", "oncomplete", "OnValueTweenComplete"));
			iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", weight_thirdperson, "to", mode == CameraMode.ThirdPerson ? 1f : 0f, "onupdate", "UpdateThirdPersonWeight"));
			iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", weight_custom, "to", mode == CameraMode.CustomCamera ? 1f : 0f, "onupdate", "UpdateCustomWeight"));
			iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", weight_topdown, "to", mode == CameraMode.TopDownWithMouseControls ? 1f : 0f, "onupdate", "UpdateTopdownWeight"));

			if (Mode == CameraMode.CustomCamera) {
				if (_targetPosition != CustomTarget.position && _targetRotation != CustomTarget.rotation) {
					if (Mathf.Approximately(weight_custom, 0) || (_targetPosition == Vector3.zero && _targetRotation == Quaternion.identity)) {
						_targetPosition = CustomTarget.position;
						_targetRotation = CustomTarget.rotation;
					} else {
						// custom camera to custom camera
						iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", _targetPosition, "to", CustomTarget.position, "onupdate", "UpdateCustomPosition"));
						iTween.ValueTo(gameObject, iTween.Hash("time", Time, "from", _targetRotation, "to", CustomTarget.rotation, "onupdate", "UpdateCustomRotation"));

					}
				}
			}

		}

		public void UpdateFirstPersonWeight(float Value) { weight_firstperson = Value;  }
		public void UpdateThirdPersonWeight(float Value) { weight_thirdperson = Value;  }
		public void UpdateCustomWeight(float Value) { weight_custom = Value;  }
		public void UpdateTopdownWeight(float Value) { weight_topdown = Value;  }

		public void UpdateCustomPosition(Vector3 Position) { _targetPosition = Position; }
		public void UpdateCustomRotation(Quaternion Rotation) { _targetRotation = Rotation; }

		public void OnValueTweenComplete() {
			_blockMovement = false;
		}

		
	}
}