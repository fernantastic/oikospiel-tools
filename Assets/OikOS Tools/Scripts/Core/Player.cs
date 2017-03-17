using UnityEngine;
using System.Collections;

namespace OikosTools {
	public class Player : MonoBehaviour {

		public enum ControlMode {
			FirstPerson,
			MouseFollow,
			ThirdPerson
		}

		private static Player _instance;
		public static Player instance { 
			get {
				/*if (_instance == null) {
					_instance = GameObject.FindObjectOfType<Player>();
				}
				if (_instance == null)
					Debug.LogError("Could not find a Player object!");*/
				return _instance;
			}
		}

		public static bool isActive { get { return instance != null && instance.gameObject.activeSelf && instance.gameObject.activeInHierarchy; } }

		public GameObject avatar;
		public Transform head;
		public Transform thirdPersonTarget;
		[HideInInspector()]
		public CharacterController controller;

		public float interactionRadius = 10;
		public float wiggleFastSpeed = 1;
		public float wiggleSlowSpeed = 1;

		public float topDownMoveSpeed = 1;
		public float topDownRotateSpeed = 1;

		public MeshFilter meshFilter { get { return avatar.GetComponentInChildren<MeshFilter>(); } }
		public Camera avatarCamera { get { return head.GetComponentInChildren<Camera>(); } }

		FirstPersonDrifter _fps;
		public FirstPersonDrifter fps { get { if (_fps == null) _fps = GetComponent<FirstPersonDrifter>(); return _fps; } }
		MouseLook _mouseLookHorizontal;
		MouseLook _mouseLookVertical;
		ControlMode controlMode = ControlMode.FirstPerson;

		RaycastHit _rh = new RaycastHit();
		Vector3 _mouseFollow_target;

		void Awake() {
			_instance = this;

			_fps = GetComponent<FirstPersonDrifter>();
			_mouseLookHorizontal = GetComponent<MouseLook>();
			_mouseLookVertical = head.GetComponent<MouseLook>();
			controller = GetComponent<CharacterController>();

			SwitchControlMode(ControlMode.FirstPerson);
		}

		void Start() {
			SetGenericValues();
		}

		void Update() {
			SetGenericValues();

			bool canMove = true;
			if (Scene.current != null && !Scene.current.inputEnabled) canMove = false;
			if (CameraController.instance != null && CameraController.instance.blockMovement) canMove = false;
			if (Game.instance != null && Game.instance.pauseMenu != null && Game.instance.pauseMenu.visible) canMove = false;

			if (controlMode == ControlMode.FirstPerson && _fps.enabled != canMove) {
				_fps.enabled = _mouseLookHorizontal.enabled = canMove;
				head.gameObject.SetActive(canMove);
			}

			if (canMove) {
				
				if (controlMode == ControlMode.MouseFollow) {
					Ray r = CameraController.instance.gameCamera.ScreenPointToRay(Input.mousePosition);
					Debug.DrawRay(r.origin, r.direction * 50);
					if (Physics.Raycast(r, out _rh, Mathf.Infinity, 1 >> LayerMask.GetMask("Player"))) {
						_mouseFollow_target = _rh.point;
						_mouseFollow_target += Vector3.up * (controller.height + 0.1f);
					}
					if(Vector3.Distance(transform.position, _mouseFollow_target) > 0.6f) {
						transform.position = Vector3.MoveTowards(transform.position, _mouseFollow_target, 20 * topDownMoveSpeed * Time.deltaTime);
						var q = Quaternion.LookRotation(_mouseFollow_target - transform.position);
						transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 1000 * topDownRotateSpeed * Time.deltaTime);
					}
					// slider for distance to cursor
					var vp = CameraController.instance.gameCamera.WorldToViewportPoint(transform.position);
					vp.z = 0;
					var vc = CameraController.instance.gameCamera.ScreenToViewportPoint(Input.mousePosition);
					vc.z = 0;
					GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CursorDistanceToPlayer, Mathf.Clamp01(Vector3.Distance(vp, vc)));
					//Debug.Log("player "  + vp + " cursor " + vc + " distance " + cursorDistance);
				} else if (controlMode == ControlMode.ThirdPerson) {


					transform.position = Vector3.MoveTowards(transform.position, _mouseFollow_target, 20 * topDownMoveSpeed * Time.deltaTime);

					var v = GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.CursorDistanceToPlayer);
					if (v>0)
						GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CursorDistanceToPlayer, Mathf.SmoothStep(v, 0, 0.2f));
				} else if (controlMode == ControlMode.FirstPerson) {
					var v = GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.CursorDistanceToPlayer);
					if (v>0)
						GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CursorDistanceToPlayer, Mathf.SmoothStep(v, 0, 0.2f));
				}
			}
		}

		void SetGenericValues() {
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CameraHorizontalAngle, Tools.GetNormalizedValue(-180, 180, transform.localEulerAngles.y - 180));
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CameraVerticalAngle, Tools.GetNormalizedValue(_mouseLookVertical.minimumY, _mouseLookVertical.maximumY, _mouseLookVertical.rotationY));
			//Debug.Log("camera angld y " + GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.CameraVerticalAngle));
		}

		public void SwitchAvatar(GameObject To) {
			foreach(Transform t in avatar.transform) {
				Destroy(t.gameObject);
			}
			GameObject prefab = Instantiate(To) as GameObject;
			prefab.transform.parent = avatar.transform;
			prefab.transform.localPosition = Vector3.zero;
			prefab.transform.localEulerAngles = Vector3.zero;
		}

		public void SwitchSounds(AudioClip Footsteps, AudioClip Jump, AudioClip Land, AudioClip Air) {
			FirstPersonDrifter fp = GetComponent<FirstPersonDrifter>();
			if (Footsteps != null) {
				fp.footstepSource.clip = Footsteps;
				fp.footstepSource.Play();
			}
			if (Jump != null)
				fp.jumpSource.clip = Jump;
			if (Land != null)
				fp.landSource.clip = Land;
			if (Air != null) {
				fp.airSource.clip = Air;
				fp.airSource.Play();
			}
		}

		public void SwitchControlMode(ControlMode Mode) {

			if (Mode == ControlMode.FirstPerson) {
				Cursor.visible = false;
				Screen.lockCursor = true;
			} else if (Mode == ControlMode.ThirdPerson || Mode == ControlMode.MouseFollow) {
				Cursor.visible = true;
				Screen.lockCursor = false;
				_mouseFollow_target = transform.position;
			}

			_fps.enabled = _mouseLookHorizontal.enabled = Mode == ControlMode.FirstPerson;
			head.gameObject.SetActive(true);

			controlMode = Mode;
		}
		public void SwitchControlMode() { SwitchControlMode(controlMode); }

		public bool CanInteract(Transform Target, bool checkViewport, bool checkDistance) {
			Camera c = avatarCamera;
			Vector3 sp = c.WorldToViewportPoint(Target.position);
			bool canInteract = false;
			if (checkViewport)
				canInteract = sp.x > 0 && sp.x < 1 && sp.y > 0 && sp.y < 1;
			if (checkDistance && canInteract)
				canInteract = Vector3.Distance(Target.position, head.position) <= interactionRadius;
			return canInteract;
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(head.position, interactionRadius);
		}
	}
}
