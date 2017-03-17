using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OikosTools {
	public class Trigger : MonoBehaviour {

		public enum Type {
			OnColliderEnter,
			OnKeyDown,
			OnInteract,
			OnSceneLoad,

			// up
			OnColliderExit,
			OnKeyUp,

			// constant
			OnColliderStay,
			OnKey,
			OnGenericValueChange,

			External
		}

		public static Dictionary<Type, string> TypeNames = new Dictionary<Type, string>() {
			{ Type.OnColliderEnter, "Player touches or enters this area" },
			{ Type.OnKeyDown, "Player presses a key" },
			{ Type.OnInteract, "Player interacts with this" },
			{ Type.OnSceneLoad, "The scene loads" },

			{ Type.OnColliderExit, "Player is no longer inside this area" },
			{ Type.OnKeyUp, "Player releases a key" },

			{ Type.OnColliderStay, "Player is inside this area (constantly)" },
			{ Type.OnKey, "Player holds a key (constantly)" },
			{ Type.OnGenericValueChange, "A generic value changes (camera, MIDI, etc)" },

			{ Type.External, "Never (To be activated externally)" }
		};

		private static Type[] ConstantTypes = new Type[]{ Type.OnColliderStay, Type.OnKey, Type.OnGenericValueChange };
		public bool isTypeConstant { get { return ConstantTypes.Contains (type); } }

		public bool triggerAlways = false;
		public Trigger.Type type = Trigger.Type.OnColliderEnter;
		public List<Action> actions = new List<Action>();

		public KeyCode onKey_key;
		public GenericValueSystem.ValueType onGenericValue_type;
		public bool onInteract_onlyWhenLooking = true;
		public bool onInteract_onlyWhenClose = true;

		internal bool inside = false;


		// Use this for initialization
		void OnEnable () {
			int l = LayerMask.NameToLayer("Triggers");
			if (l >= 0 && gameObject.layer != l) {
				gameObject.layer = l;
			} else {
				Debug.LogWarning("Add a layer called 'Triggers'");
			}
			foreach(Action a in actions) {
				if (a.parent == null) {
					a.parent = this;
				}
				a.restoreValueOnExit = false;
			}

			if (type == Type.OnSceneLoad) {
				Scene.OnSceneInitialized += OnEnter;
			}

		}
		void OnDestroy() {
			if (type == Type.OnGenericValueChange)
				GenericValueSystem.instance.UnRegisterCallbacks(OnGenericValueChange);
			if (type == Type.OnSceneLoad)
				Scene.OnSceneInitialized -= OnEnter;
		}

		void Start() {
			if (type == Type.OnSceneLoad) {
				OnEnter();
			}
			foreach(Action a in actions) {
				a.Start();
			}
			if (type == Type.OnGenericValueChange) {
				GenericValueSystem.instance.RegisterCallback(onGenericValue_type, OnGenericValueChange);
				OnGenericValueChange(onGenericValue_type, GenericValueSystem.instance.GetValue(onGenericValue_type));
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (inside || triggerAlways) {
				if (Scene.current/* && Scene.current.inputEnabled*/) {
					if (type == Type.OnKeyDown) {
						if (Input.GetKeyDown(onKey_key))
							OnEnter();
						else if (Input.GetKeyUp(onKey_key))
							OnExit();
					} else if (type == Type.OnKeyUp) {
						if (Input.GetKeyUp(onKey_key))
							OnEnter();
					} else if (type == Type.OnKey) {
						if (Input.GetKey(onKey_key))
							OnEnter();
					}

					// interact
					//TODO: activate only when looking at the object?
					if (Scene.current.inputEnabled) {
						if (type == Type.OnInteract) {
							if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) {
								bool canInteract = true;
								if (onInteract_onlyWhenClose || onInteract_onlyWhenLooking)
									canInteract = Player.isActive && Player.instance.CanInteract(transform, onInteract_onlyWhenLooking, onInteract_onlyWhenClose);
								if (canInteract)
									OnEnter();
							}
						}
					}
				}

				if (type == Type.OnColliderStay) {
					OnEnter();
				}
			}

			foreach(Action a in actions) {
				a.Update();
			}

			//TODO: on interact
		}

		void OnTriggerEnter() {
			inside = true;
			if (type == Type.OnColliderEnter) {
				OnEnter();
			}
		}
		void OnTriggerExit() {
			inside = false;
			if (type == Type.OnColliderExit) {
				OnEnter();
			}
			if (type == Type.OnColliderEnter) {
				OnExit();
			}
		}
		void OnTriggerStay() {
			inside = true;
		}

		void OnEnter() {
			foreach(Action a in actions) {
				a.OnEnter();
			}
		}
		void OnExit() {
			foreach(Action a in actions) {
				a.OnExit();
			}
		}

		void OnGenericValueChange(GenericValueSystem.ValueType Type, float Value) {
			//Debug.Log("value " + Type + " changed to " + Value);
			foreach(Action a in actions) {
				a.ForceValue(Value);
			}
		}

		public void ForceTrigger() {
			OnEnter();
		}

		/*
		public void ChangeValue(Hashtable parameters) {
			PropertyChange.SetValue(parameters["component"], (string)parameters["property"], parameters["value"], (string)parameters["subproperty"]);
		}*/

		// editor
		void OnDrawGizmosSelected() {
			Trigger t = this;
			for(int j = 0; j < t.actions.Count; j++) {
				Action a = t.actions[j];
				
				switch (a.type) {
				case Action.Type.ChangeValue:
					if (t.isTypeConstant) {
						// handles
						Gizmos.color = Color.red;
						Gizmos.DrawWireCube(a.constantValueFromPosition, Vector3.one * .6f);
						Gizmos.color = Color.blue;
						Gizmos.DrawWireCube(a.constantValueToPosition, Vector3.one * .6f);
					}
					break;
				case Action.Type.MovePlayer:
					Gizmos.color = Color.green;
					Gizmos.DrawWireCube(a.changeValue_value.GetVector3(), Vector3.one * .6f);
					break;
				}
			}
			
		}
	}

	[System.Serializable]
	public class Action {
		public enum Type {
			ChangeValue,
			PlayAnimation,
			ChangeScene,
			PlaySound,
			FadeSound,
			StopSound,
			MovePlayer,
			ChangeCameraMode,
			ChangeAvatar,
			ChangePlayerSounds,
			ShowMusicalDialog,
			MoveObject,
			ActivateObject,
			ChangeMouseCursor,
			SwapSkybox
		}

		public static Dictionary<Type, string> TypeNames = new Dictionary<Type, string>() {
			{ Type.ChangeValue, "Change a value" },
			{ Type.PlayAnimation, "Play an animation" },
			{ Type.ChangeScene, "Change the scene" },
			{ Type.PlaySound, "Play a sound or all sounds" },
			{ Type.FadeSound, "Fade a sound or all sounds" },
			{ Type.StopSound, "Stop a sound or all sounds" },
			{ Type.MovePlayer, "Move player" },
			{ Type.MoveObject, "Move object" },
			{ Type.ActivateObject, "Activate or Deactivate something" },
			{ Type.ChangeCameraMode, "Switch Camera mode" },
			{ Type.ChangeAvatar, "Change player model" },
			{ Type.ChangePlayerSounds, "Change player sounds" },
			{ Type.ShowMusicalDialog, "Show a musical dialog" },
			{ Type.ChangeMouseCursor, "Change mouse cursor" },
			{ Type.SwapSkybox, "Change Skybox" }
		};

		public static Type[] ConstantTypes = new Type[]{ Type.ChangeValue };

		#region tweens
		public static Dictionary<Component, List<Dictionary<string, object>>> ACTIVE_ACTIONS= new Dictionary<Component, List<Dictionary<string,object>>>();
		public static void RegisterTween(Component comp, Action action, string property, string subproperty) {
			if (!ACTIVE_ACTIONS.ContainsKey(comp) || ACTIVE_ACTIONS[comp] == null) {
				ACTIVE_ACTIONS[comp] = new List<Dictionary<string,object>>();
			} else {
				// stop tweens that affect the same object
				for( int i = 0; i < ACTIVE_ACTIONS[comp].Count; i++ ) {
					Dictionary<string,object> dict = ACTIVE_ACTIONS[comp][i];
					if (property == (string)dict["property"] && subproperty == (string)dict["subproperty"]) {
						//Debug.Log("found an Action changing the same component's property/subproperty");
						(dict["action"] as Action).StopTween();
						ACTIVE_ACTIONS[comp].Remove(dict);
						i--;
					}
				}
			}
			Dictionary<string, object> newdict = new Dictionary<string, object>();
			newdict["action"] = action;
			newdict["property"] = property;
			newdict["subproperty"] = subproperty;
			ACTIVE_ACTIONS[comp].Add(newdict);
		}
		public static void UnRegisterTween(Component comp, Action action) {
			if (ACTIVE_ACTIONS[comp] == null) {
				Debug.LogWarning("Action " + action + " tried to unregister a tween for component " + comp + " but no tween was found in the first place", comp.gameObject);
				return;
			}
			for( int i = 0; i < ACTIVE_ACTIONS[comp].Count; i++ ) {
				Dictionary<string, object> dict = ACTIVE_ACTIONS[comp][i];
				if ((Action)dict["action"] == action) {
					//Debug.Log("unregistering tween for action " + action);
					ACTIVE_ACTIONS[comp].RemoveAt(i);
					break;
				}
			}
		}
		#endregion

		public Type type = Type.ChangeValue;

		[SerializeField()]
		public GameObject target;

		#region parameters

		public bool restoreValueOnExit = false;

		// ChangeValue
		public float duration = 1;
		public float waitTime = 0;
		[SerializeField()]
		public ActionValue changeValue_value = new ActionValue();
		[SerializeField()]
		public Component changeValue_component;
		[SerializeField()]
		public string changeValue_property = "";
		[SerializeField()]
		public string changeValue_subproperty = "";

		// // constant values
		public enum ValueTargetType { 
			Object, Position, Player
		}
		public ValueTargetType changeValue_constant_from_type = ValueTargetType.Object;
		public Transform changeValue_constant_from_transform;
		public Vector3 changeValue_constant_from_position = new Vector3(1,0,0);
		public float changeValue_constant_farDistance = 20;
		public ValueTargetType changeValue_constant_to_type = ValueTargetType.Player;
		public Transform changeValue_constant_to_transform;
		public Vector3 changeValue_constant_to_position = Vector3.zero;
		public float changeValue_constant_nearDistance = 1;
		public bool changeValue_constant_changeOnLoad = true;
		public AnimationCurve changeValue_constant_curve = AnimationCurve.EaseInOut(0,0,1,1);

		[SerializeField()]
		public ActionValue changeValue_constant_finalRange_from;
		[SerializeField()]
		public ActionValue changeValue_constant_finalRange_to;

		public Vector3 constantValueFromPosition { 
			get { 
				if (changeValue_constant_from_type == ValueTargetType.Object && changeValue_constant_from_transform != null)
					return changeValue_constant_from_transform.position;
				if (changeValue_constant_from_type == ValueTargetType.Player && Player.instance != null)
					return Player.instance.transform.position;
				return parent.transform.TransformPoint(changeValue_constant_from_position);
			}
		}
		public Vector3 constantValueToPosition { 
			get { 
				if (changeValue_constant_to_type == ValueTargetType.Object && changeValue_constant_to_transform != null)
					return changeValue_constant_to_transform.position;
				if (changeValue_constant_to_type == ValueTargetType.Player && Player.instance != null)
					return Player.instance.transform.position;
				return parent.transform.TransformPoint(changeValue_constant_to_position);
			}
		}
		public Vector3 farPoint { get { return Vector3.MoveTowards(constantValueFromPosition, constantValueToPosition, changeValue_constant_farDistance); } }
		public Vector3 nearPoint { get { return Vector3.MoveTowards(constantValueFromPosition, constantValueToPosition, changeValue_constant_nearDistance); } }
				
		// ChangeScene
		public string changeScene_scenePath = "";
		//public float changeScene_duration = 12;
		public AudioClip changeScene_transitionSound;
		public Texture2D changeScene_transitionTexture;
		public Texture2D changeScene_transitionWaveTexture;

		// CameraMode
		public CameraController.CameraMode changeCameraMode_mode = CameraController.CameraMode.ThirdPerson;
		public bool changeCameraMode_topdownFollow = false;
		public float changeCameraMode_time = 3;
		public Transform changeCameraMode_target;

		// Avatar
		public GameObject changeAvatar_prefab;

		// Animation
		[SerializeField()]
		public Animation playAnimation_animation;
		public string playAnimation_animation_clip = "";
		public bool playAnimation_loop = true;
		public bool playAnimation_queue = false;

		// Move Player
		public enum PlayerMovementType {
			ForcePosition, Poke//, AddForce
		}
		public PlayerMovementType movePlayer_movementType = PlayerMovementType.ForcePosition;

		// bool
		public bool activate_flipValue = false;

		// Change player sound
		public AudioClip playerSound_footsteps;
		public AudioClip playerSound_jump;
		public AudioClip playerSound_land;
		public AudioClip playerSound_air;

		// dialog
		public MusicalDialog musicalDialog_dialog;

		#endregion

		// Sound
		public enum SoundChangeType { Single, Multiple, All }
		public SoundChangeType sound_changeType = SoundChangeType.Single;
		public AudioSource sound_audioSource;
		public AudioSource[] sound_audioSources = new AudioSource[]{};
		public bool sound_allSounds = false;

		// Material
		[SerializeField()]
		public Material changeMaterial_material;

		public Texture2D texture;
		public Vector2 changeCursor_hotspot = new Vector2(0.5f,0.5f);


		#region private
		[SerializeField()]
		public Trigger parent;
		object _previousValue;
		object _nextValue;
		float _timer_valueTo = -1;
		float _timer_wait = -1;
		bool _everActivated = false;

		#endregion

		public Action(Trigger T) {
			parent = T;
			changeValue_constant_from_transform = T.transform;
		}

		public void OnEnter() {
			if (!parent.isTypeConstant && waitTime > 0) {
				// wait
				_timer_wait = waitTime;
				return;
			}
			Activate();

		}
		public void OnExit() {
			//Debug.Log ("action exit!");
			// restore values to original
			if (restoreValueOnExit && _everActivated) {
				if (type == Type.ChangeValue || type == Type.FadeSound) {
					StopTween();
					ChangeValueTo(_previousValue, _nextValue);
				}
			}
		}

		void Activate() {
			_everActivated = true;
			//Debug.Log ("action enter! " + type, parent);
			if (type == Type.ChangeValue) {
				//TriggerManager.ChangeValueOverTime(target, time, value);
				//TODO: get type of a subproperty to properly put the right field
				object value = PropertyChange.GetValue(changeValue_component, changeValue_property, changeValue_subproperty);
				if (value != null) {
					if (!parent.isTypeConstant) {
						_previousValue = value;
						value = changeValue_value.Get(value.GetType());
						
						ChangeValueTo(value, _previousValue);
					} else {
						float d = Vector3.Distance(constantValueFromPosition, constantValueToPosition);
						d -= changeValue_constant_nearDistance;
						d /= changeValue_constant_farDistance - changeValue_constant_nearDistance;
						d = 1-Mathf.Clamp01(d);
						d = changeValue_constant_curve.Evaluate(d);
						
						value = ActionValue.Lerp(value.GetType(), changeValue_constant_finalRange_from, changeValue_constant_finalRange_to, d);
						
						UpdateValue(value);
					}
				}
			} else if (type == Type.MovePlayer) {
				if (movePlayer_movementType == PlayerMovementType.ForcePosition) {
					changeValue_component = Player.instance.transform;
					changeValue_property = "position";
					changeValue_subproperty = "";
					ChangeValueTo(changeValue_value.GetVector3(), Player.instance.transform.position);
				} else if (movePlayer_movementType == PlayerMovementType.Poke) {
					Player.instance.controller.Move(changeValue_value.GetVector3());
				}
				
			} else if (type == Type.MoveObject) {
				if (movePlayer_movementType == PlayerMovementType.ForcePosition) {
					changeValue_component = target.transform;
					changeValue_property = "position";
					changeValue_subproperty = "";
					ChangeValueTo(changeValue_value.GetVector3(), target.transform.position);
				} else if (movePlayer_movementType == PlayerMovementType.Poke) {
					changeValue_component = target.transform;
					changeValue_property = "position";
					changeValue_subproperty = "";
					ChangeValueTo(target.transform.position + changeValue_value.GetVector3(), target.transform.position);
				}
				
			} else if (type == Type.ActivateObject) {
				bool state = activate_flipValue ? !target.activeSelf : changeValue_value.GetBool();
				target.SetActive(state);
			} else if (type == Type.PlayAnimation) {
				if (playAnimation_loop)
					playAnimation_animation.clip.wrapMode = WrapMode.Loop;
				if (playAnimation_queue) {
					playAnimation_animation.PlayQueued(playAnimation_animation_clip, QueueMode.CompleteOthers);
				} else {
					playAnimation_animation.Play(playAnimation_animation_clip);
				}
			} else if (type == Type.ChangeScene) {
				// TODO: transition through a manager
				TransitionManager.instance.TransitionTo(System.IO.Path.GetFileNameWithoutExtension(changeScene_scenePath), 12, changeScene_transitionTexture, changeScene_transitionWaveTexture, changeScene_transitionSound);
			} else if (type == Type.PlaySound) {
				foreach(var sound in GetSounds()) {
					if (sound != null && sound.gameObject.activeSelf)
						sound.Play();
				}
			} else if (type == Type.FadeSound) {
				if (sound_changeType == SoundChangeType.Single) {
					changeValue_component = sound_audioSource;
					changeValue_property = "volume";
					changeValue_subproperty = "";
					ChangeValueTo(changeValue_value.GetFloat(), sound_audioSource.volume);
				} else if (sound_changeType == SoundChangeType.All) {
					Scene.current.FadeAllSounds(changeValue_value.GetFloat(), duration);
				}
				
			} else if (type == Type.StopSound) {
				/*
				changeValue_component = sound_audioSource;
				changeValue_property = "volume";
				changeValue_subproperty = "";
				changeValue_value.Set(0f);
				ChangeValueTo(changeValue_value.GetFloat(), sound_audioSource.volume);
				*/
				foreach(var sound in GetSounds()) {
					if (sound != null)
						sound.Stop();
				}
				
			} else if (type == Type.ChangeCameraMode) {
				CameraController.instance.MoveCamera(changeCameraMode_mode, changeCameraMode_time, changeCameraMode_target);
				if (changeCameraMode_mode == CameraController.CameraMode.TopDownWithMouseControls)
					CameraController.instance._topdown_follow = changeCameraMode_topdownFollow;
			} else if (type == Type.ChangeAvatar) {
				Player.instance.SwitchAvatar(changeAvatar_prefab);
			} else if (type == Type.ChangePlayerSounds) {
				Player.instance.SwitchSounds(playerSound_footsteps, playerSound_jump, playerSound_land, playerSound_air);
			} else if (type == Type.ShowMusicalDialog) {
				if (musicalDialog_dialog) musicalDialog_dialog.Play();
			} else if (type == Type.ChangeMouseCursor) {
				if (texture != null) {
					Cursor.SetCursor(texture, new Vector2(texture.width * changeCursor_hotspot.x, texture.height * changeCursor_hotspot.y) , CursorMode.Auto);
				}
			} else if (type == Type.SwapSkybox) {
				if (changeMaterial_material != null) {
					RenderSettings.skybox = changeMaterial_material;
				}
			}
		}

		public void Start() {
			if (parent.isTypeConstant && changeValue_constant_changeOnLoad) {
				Activate ();
			}
			if (target == null)
				target = parent.gameObject;
			/*
			if (parent.isTypeConstant && target.collider.GetType() == typeof(SphereCollider) && (target.collider as SphereCollider).radius * parent.transform.localToWorldMatrix < changeValue_constant_farDistance) {
				(target.collider as SphereCollider).radius = changeValue_constant_farDistance * parent.transform.worldToLocalMatrix;
			}
			*/
		}

		public void Update() {
			if (waitTime > 0 && _timer_wait > 0) {
				_timer_wait -= Time.deltaTime;
				if (_timer_wait < 0)
					Activate();
				
			}
			if (_timer_valueTo > 0 && !parent.isTypeConstant && duration > 0) {
				UpdateValue(ActionValue.Lerp(_previousValue, _nextValue, 1-Mathf.Clamp01(_timer_valueTo / duration)));
				_timer_valueTo -= Time.deltaTime;
				if (_timer_valueTo < 0) {
					// tween ended
					UnRegisterTween(changeValue_component, this);
					UpdateValue(_nextValue);
					OnValueChangeEnd(_nextValue);
				}
			}
		}

		AudioSource[] GetSounds() {
			if (sound_changeType == SoundChangeType.Single) {
				return new AudioSource[] {sound_audioSource};
			}
			if (sound_changeType == SoundChangeType.Multiple) {
				return sound_audioSources;
			}
			if (sound_changeType == SoundChangeType.All) {
				return Scene.current.sceneSounds;
			}
			return new AudioSource[]{};
		}

		void ChangeValueTo(object value, object PreviousValue = null) {
			if (PreviousValue == null)
				PreviousValue = PropertyChange.GetValue(changeValue_component, changeValue_property, changeValue_subproperty);
			_previousValue = PreviousValue;
			_nextValue = value;
			if (duration == 0 || value.GetType() == typeof(string)) {
				UpdateValue(value);
				OnValueChangeEnd(value);
			} else {
				StartTween(PreviousValue, value);
			}
		}

		void StartTween(object from, object to) {
			if (duration <= 0) {
				UpdateValue(to);
				return;
			}
			_timer_valueTo = duration;
			_previousValue = from;
			_nextValue = to;
			RegisterTween(changeValue_component, this, changeValue_property, changeValue_subproperty);
		}
		public void StopTween() {
			_timer_valueTo = -1;
			// don't unregister here!
			OnValueChangeEnd();
		}

		public void ForceValue(float Ratio) {
			object value = PropertyChange.GetValue(changeValue_component, changeValue_property, changeValue_subproperty);
			if (value != null) {
				value = ActionValue.Lerp(value.GetType(), changeValue_constant_finalRange_from, changeValue_constant_finalRange_to, Ratio);
				UpdateValue(value);
			}
		}

		void UpdateValue(object value) {
			PropertyChange.SetValue(changeValue_component, changeValue_property, value, changeValue_subproperty);
		}

		void OnValueChangeEnd(object value = null) {
			
		}

	}

	[System.Serializable]
	public class ActionValue {
		[SerializeField()]
		private float v_float = 0;
		[SerializeField()]
		private int v_int = 0;
		[SerializeField()]
		private string v_string = "";
		[SerializeField()]
		private Color v_color = Color.white;//{ new colorKeys[2]{Color.black,Color.white} };
		[SerializeField()]
		private Gradient v_gradient = new Gradient();//{ new colorKeys[2]{Color.black,Color.white} };
		[SerializeField()]
		private Vector2 v_v2 = Vector2.zero;
		[SerializeField()]
		private Vector3 v_v3 = Vector3.zero;
		[SerializeField()]
		private bool v_bool = false;

		public object Get(System.Type type) {
			object o = null;
			switch(type.ToString()) {
			case "System.Single":
				o = v_float;
			break;
			case "System.Int32":
				o = v_int;
				break;
			case "System.String":
				o = v_string;
				break;
			case "UnityEngine.Color":
				o = v_color;
				break;
			case "UnityEngine.Gradient":
				o = v_gradient;
				break;
			case "UnityEngine.Vector2":
				o = v_v2;
				break;
			case "UnityEngine.Vector3":
				o = v_v3;
				break;
			case "System.Boolean":
				o = v_bool;
				break;
			default:
				Debug.LogWarning("Type " + type.ToString() + " is unsupported");
				break;
			}

			return o;
		}

		public void Set(object Value) {
			if (Value == null)
				return;
			try { 
				switch(Value.GetType().ToString()) {
				case "System.Single":
					v_float = (float)Value;
					break;
				case "System.Int32":
					v_int = (int)Value;
					break;
				case "System.String":
					v_string = (string)Value;
					break;
				case "UnityEngine.Color":
					v_color = (Color)Value;
					break;
				case "UnityEngine.Gradient":
					v_gradient = (Gradient)Value;
					break;
				case "UnityEngine.Vector2":
					v_v2 = (Vector2)Value;
					break;
				case "UnityEngine.Vector3":
					v_v3 = (Vector3)Value;
					break;
				case "System.Boolean":
					v_bool = (bool)Value;
					break;
				}
			} catch (System.Exception e) {
				Debug.Log("Error setting a value of type " + Value.GetType() + ", " + e.ToString());
			}
		}

		public float GetFloat() { return v_float; }
		public int GetInt() { return v_int; }
		public string GetString() { return v_string; }
		public Color GetColor() { return v_color; }
		public Gradient GetGradient() { return v_gradient; }
		public Vector2 GetVector2() { return v_v2; }
		public Vector3 GetVector3() { return v_v3; }
		public bool GetBool() { return v_bool; }

		public static object Lerp(System.Type type, ActionValue av1, ActionValue av2, float lerp) {
			if (av1 == null || av2 == null)
				return null;
			//lerp = Mathf.Clamp01(lerp);
			if (type == typeof(float)) {
				return Tools.GetMinMaxValue(av1.GetFloat(), av2.GetFloat(), lerp);
			} else if (type == typeof(int)) {
				return Mathf.RoundToInt(Tools.GetMinMaxValue(av1.GetInt(), av2.GetInt(), lerp));
			} else if (type == typeof(Color)) {
				return Color.Lerp(av1.GetColor(), av2.GetColor(),lerp);
			} else if (type == typeof(Vector2)) {
				return Vector2.Lerp(av1.GetVector2(), av2.GetVector2(), lerp);
			} else if (type == typeof(Vector3)) {
				return Vector3.Lerp(av1.GetVector3(), av2.GetVector3(), lerp);
			} else if (type == typeof(bool)) {
				return lerp < 0.5f ? av1.GetBool() : av2.GetBool();
			}
			return null;
		}

		public static object Lerp(object from, object to, float lerp) {
			if (from == null || to == null)
				return null;
			lerp = Mathf.Clamp01(lerp);
			System.Type type = from.GetType();
			if (type == typeof(float)) {
				return Tools.GetMinMaxValue((float)from, (float)to, lerp);
			} else if (type == typeof(int)) {
				return Mathf.RoundToInt(Tools.GetMinMaxValue((int)from, (int)to, lerp));
			} else if (type == typeof(Color)) {
				return Color.Lerp((Color)from, (Color)to,lerp);
			} else if (type == typeof(Vector2)) {
				return Vector2.Lerp((Vector2)from, (Vector2)to, lerp);
			} else if (type == typeof(Vector3)) {
				return Vector3.Lerp((Vector3)from, (Vector3)to, lerp);
			} else if (type == typeof(bool)) {
				return lerp < 0.5f ? (bool)from : (bool)to;
			}
			return null;
		}

	}
}