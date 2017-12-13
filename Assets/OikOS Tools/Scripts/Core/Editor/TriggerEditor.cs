using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace OikosTools {
	[CustomEditor(typeof(Trigger))]
	public class TriggerEditor : Editor {


		public override void OnInspectorGUI ()
		{
			//base.OnInspectorGUI ();

			string[] names;

			Trigger t = (Trigger)target;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(t, "Change Trigger value");

			// initialization
			if (t.actions == null) {
				t.actions = new List<Action>() { new Action(t) };
			}

			

			//(Trigger.Type)t.type = EditorGUILayout.EnumPopup ("Type", t.type);
			LabelSeparator ("When...");

			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.Space();

			GUI.enabled = t.type != Trigger.Type.OnGenericValueChange;
			t.triggerAlways = !EditorGUILayout.ToggleLeft(" Inside the collider", !t.triggerAlways);
			GUI.enabled = true;
			EditorGUILayout.Space();

			//t.type = (Trigger.Type)DescriptiveEnumPopup (Trigger.Type, Trigger.TypeNames);
			names = new string[System.Enum.GetNames(typeof(Trigger.Type)).Length];
			for (int i = 0; i < names.Length; i++) {
				names[i] = Trigger.TypeNames[(Trigger.Type)i];
			}
			t.type = (Trigger.Type)EditorGUILayout.Popup((int)t.type, names);

			switch (t.type) {
				case Trigger.Type.OnColliderEnter:

				break;
				case Trigger.Type.OnKeyDown:
				case Trigger.Type.OnKeyUp:
					t.onKey_key = (KeyCode)EditorGUILayout.EnumPopup("Key", t.onKey_key);
				break;
				case Trigger.Type.OnInteract:
					EditorGUILayout.HelpBox("Activated when player Clicks or hits E", MessageType.Info);
					t.onInteract_onlyWhenLooking = EditorGUILayout.ToggleLeft("and Player's looking at this", t.onInteract_onlyWhenLooking);
					t.onInteract_onlyWhenClose = EditorGUILayout.ToggleLeft("and Player's close", t.onInteract_onlyWhenClose);
				break;
				
				case Trigger.Type.OnColliderExit:
				break;

				case Trigger.Type.OnColliderStay:
				
				break;
				case Trigger.Type.OnKey:
				t.onKey_key = (KeyCode)EditorGUILayout.EnumPopup("Key", t.onKey_key);
				break;
				case Trigger.Type.OnGenericValueChange:
				t.onGenericValue_type = (GenericValueSystem.ValueType)EditorGUILayout.EnumPopup("Value", t.onGenericValue_type);
				break;
			}

			// warnings
			/*
			switch (t.type) {
				case Trigger.Type.OnColliderEnter:
				case Trigger.Type.OnColliderExit:
				case Trigger.Type.OnColliderStay:
				case Trigger.Type.OnInteract:
				case Trigger.Type.OnKey:
					
					
				break;
			}
			*/

			// trigger
			if (!t.triggerAlways && t.type != Trigger.Type.OnGenericValueChange) {
				if (t.GetComponent<Collider> () == null)
					EditorGUILayout.HelpBox ("A collider is required! Add it with Add Component > Physics", MessageType.Error);
				else if (!t.GetComponent<Collider>().enabled)
					EditorGUILayout.HelpBox ("The collider component is disabled! Enable it with the checkmark next to its name", MessageType.Error);
				else if (!t.GetComponent<Collider>().isTrigger)
					EditorGUILayout.HelpBox ("Set Collider as 'Is Trigger'", MessageType.Error);
			}

			if (t.isTypeConstant)
				EditorGUILayout.HelpBox ("This trigger type will do actions every frame, might affect performance if used too much.", MessageType.Info);
		

			EditorGUILayout.Space();
			GUILayout.EndVertical();


			LabelSeparator ("Do...");

			if (t.actions.Count == 0)
				EditorGUILayout.HelpBox ("Add an action for this to do anything.", MessageType.Warning);

			
			for(int j = 0; j < t.actions.Count; j++) {

				Undo.RecordObject(t, "Change Action value");

				EditorGUILayout.BeginVertical("Button");
				EditorGUILayout.Space();

				Action a = t.actions[j];

				if (a.parent == null)
					a.parent = t;

				// X
				Rect r = EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Action:");
				GUI.color = Color.red;
				if (GUI.Button (new Rect(r.xMax - 18, r.yMin, 18, EditorGUIUtility.singleLineHeight), "x")) {
					t.actions.Remove(a);
					j--;
				}
				EditorGUILayout.EndVertical();
				GUI.color = Color.white;

				GUI.enabled = !t.isTypeConstant;
				EditorGUILayout.Space();
				a.waitTime = EditorGUILayout.FloatField(new GUIContent("Wait... (Seconds)", !GUI.enabled ? "Doesn't work on triggers marked as (constantly)" : ""), a.waitTime);
				EditorGUILayout.Space();
				GUI.enabled = true;

				List<int> supportedTypes = new List<int>();
				List<string> supportedTypes_labels = new List<string>();
				for (int i = 0; i < System.Enum.GetNames(typeof(Action.Type)).Length; i++) {
					if (t.isTypeConstant && !Action.ConstantTypes.Contains((Action.Type)i))
						continue;
					supportedTypes.Add(i);
					supportedTypes_labels.Add(Action.TypeNames[(Action.Type)i]);
				}
				if (!supportedTypes.Contains((int)a.type)) a.type = (Action.Type)supportedTypes[0];
				a.type = (Action.Type)supportedTypes[EditorGUILayout.Popup((int)a.type, supportedTypes_labels.ToArray())];

				switch (a.type) {

					case Action.Type.ChangeValue:

					// object
					a.target = EditorGUILayout.ObjectField("Target Object", a.target, typeof(GameObject), true) as GameObject;
					
					EditorGUI.indentLevel++;
					
					// get the object's components
					Component[] components = (a.target == null ? t.gameObject : a.target).GetComponents<Component>();
					// get the index of the currently selected component
					int componentIndex = 0;
					if (components.Length > 0 && a.changeValue_component != null && System.Array.IndexOf(components, a.changeValue_component) >= 0)
						componentIndex = System.Array.IndexOf(components, a.changeValue_component);
					// make a list of names to display
					string[] componentsLabels = new string[components.Length];
					for (int i = 0; i < components.Length; i++) {
						componentsLabels[i] = i + ": " + components[i].GetType().ToString() + (components[i] == t ? " (Self)" : "");
					}
					componentIndex = EditorGUILayout.Popup("Component", componentIndex, componentsLabels);
					if (componentIndex >= 0) {
						a.changeValue_component = components[componentIndex];
					} else {
						Debug.LogWarning("Can't find the component anymore, Target Object changed or the component was removed", this);
					}
					
					
					// properties
					
					Component c = a.changeValue_component;
					string[] properties = PropertyChange.GetProperties(c).ToArray();
					string[] propertiesLabels = new string[properties.Length];
					if (properties.Length > 0) {
						EditorGUI.indentLevel++;
						for (int i = 0; i < properties.Length; i++) {
							propertiesLabels[i] = string.Format("{0} ({1})", properties[i], PropertyChange.GetValue(c, properties[i]).GetType().Name);
						}
						int propertyIndex = EditorGUILayout.Popup("Property", System.Array.IndexOf(properties, a.changeValue_property), propertiesLabels);
						if (propertyIndex >=0) {
							a.changeValue_property = properties[propertyIndex];
						} else {
							//a.changeValue_property = "";
							if (a.changeValue_property.Length > 0)
								Debug.LogWarning("Can't find the property " + a.changeValue_property + " anymore", this);
						}
						
						if (a.changeValue_property.Length > 0) {
							// subproperties
							object property = PropertyChange.GetValue(a.changeValue_component, a.changeValue_property);
							List<string> subproperties = PropertyChange.GetProperties(property);
							List<string> subpropertiesLabels = new List<string>();
							if (subproperties.Count > 0) {
								EditorGUI.indentLevel++;
								
								subproperties.Insert(0,"");
								subpropertiesLabels.Add("None");
								for (int i = 1; i < subproperties.Count; i++) {
									subpropertiesLabels.Add(string.Format("{0} ({1})", subproperties[i].ToString(), ""));
								}
								int subpropertyIndex = EditorGUILayout.Popup("Subproperty", subproperties.IndexOf(a.changeValue_subproperty), subpropertiesLabels.ToArray());
								if (subpropertyIndex >=0)
									a.changeValue_subproperty = subproperties[subpropertyIndex];
								
								EditorGUI.indentLevel--;
							} else {
								a.changeValue_subproperty = "";
							}
							/*
							if (property.GetType() == typeof(Color) && a.changeValue_subproperty.Length == 0) {
								SerializedObject so = new SerializedObject(t);
								a.changeValue_color = EditorGUILayout.ColorField("Color", a.changeValue_color);
								if (GUI.changed) {
									so.ApplyModifiedProperties();
								}
							}
							*/
							/*
							// draw a editor gui gradient
							if (property.GetType() == typeof(Color) && a.changeValue_singleTargetSubProperty.Length == 0) {
								SerializedObject so = new SerializedObject(a);
								Gradient g = a.colorValueGradient;
								EditorGUILayout.PropertyField(so.FindProperty("colorValueGradient"), new GUIContent("Gradient"));
								if (GUI.changed) {
									so.ApplyModifiedProperties();
								}
							}
							*/
						} else {
							a.changeValue_subproperty = "";
						}
						EditorGUI.indentLevel--;
					}
					
					EditorGUI.indentLevel--;
					if (a.target == null) {
						EditorGUILayout.HelpBox("No Target Object defined, using itself.", MessageType.Info);
					}
					if (a.changeValue_component == null) {
						EditorGUILayout.HelpBox("Choose a component", MessageType.Error);
					} else if (a.changeValue_property.Length == 0) {
						EditorGUILayout.HelpBox("Choose a property", MessageType.Error);
					}


					if (!t.isTypeConstant) {

						FieldRestoreValue(a);

						//TODO: value of subproperty
						if (a.changeValue_component != null && a.changeValue_property.Length > 0) {
							object value = PropertyChange.GetValue(a.changeValue_component, a.changeValue_property, a.changeValue_subproperty);
							if (value != null) {
								if (value.GetType() == typeof(float)) a.changeValue_value.Set(EditorGUILayout.FloatField("Set value to", a.changeValue_value.GetFloat()));
								if (value.GetType() == typeof(int)) a.changeValue_value.Set(EditorGUILayout.IntField("Set value to", a.changeValue_value.GetInt()));
								if (value.GetType() == typeof(Color)) a.changeValue_value.Set(EditorGUILayout.ColorField("Set value to", a.changeValue_value.GetColor()));
								if (value.GetType() == typeof(Vector2)) a.changeValue_value.Set(EditorGUILayout.Vector2Field("Set value to", a.changeValue_value.GetVector2()));
								if (value.GetType() == typeof(Vector3)) a.changeValue_value.Set(EditorGUILayout.Vector3Field("Set value to", a.changeValue_value.GetVector3()));
								if (value.GetType() == typeof(string)) a.changeValue_value.Set(EditorGUILayout.TextField("Set value to", a.changeValue_value.GetString()));
							}
						}
						a.duration = EditorGUILayout.FloatField(new GUIContent("During (Seconds)", "0 = instant change"), a.duration);
					} else {
						// constant trigger

						LabelSeparator("...and the result will be...");
						object value = PropertyChange.GetValue(a.changeValue_component, a.changeValue_property, a.changeValue_subproperty);
						if (value != null) {
							if (value.GetType() == typeof(float)) {
								EditorGUILayout.LabelField("A number...");
								a.changeValue_constant_finalRange_from.Set(EditorGUILayout.FloatField("Between", a.changeValue_constant_finalRange_from.GetFloat()));//DualFloatFields("", a.changeValue_constant_finalRange, "Between", "and");
								a.changeValue_constant_finalRange_to.Set(EditorGUILayout.FloatField("and", a.changeValue_constant_finalRange_to.GetFloat()));//DualFloatFields("", a.changeValue_constant_finalRange, "Between", "and");
							} else if (value.GetType() == typeof(int)) {
								EditorGUILayout.LabelField("A round number...");
								a.changeValue_constant_finalRange_from.Set(EditorGUILayout.IntField("Between", a.changeValue_constant_finalRange_from.GetInt()));
								a.changeValue_constant_finalRange_to.Set(EditorGUILayout.IntField("and", a.changeValue_constant_finalRange_to.GetInt()));
							} else if (value.GetType() == typeof(Color)) {

								EditorGUILayout.LabelField("A color...");
								a.changeValue_constant_finalRange_from.Set(EditorGUILayout.ColorField("Between", a.changeValue_constant_finalRange_from.GetColor()));
								a.changeValue_constant_finalRange_to.Set(EditorGUILayout.ColorField("and", a.changeValue_constant_finalRange_to.GetColor()));
								/*
								// TODO: change a color subproperty?? whatever
								//if (a.changeValue_subproperty.Length == 0) {
									//Gradient g = (Gradient)a.changeValue_constant_finalRange_from.Get(typeof(Gradient));
									SerializedObject so = new SerializedObject(a.changeValue_constant_finalRange_from);
									EditorGUILayout.PropertyField(so.FindProperty("v_gradient"), new GUIContent("Gradient"));
									if (GUI.changed) {
										so.ApplyModifiedProperties();
									}
								//}
								*/

							} else if (value.GetType() == typeof(Vector2)) {
								EditorGUILayout.LabelField("A vector...");
								a.changeValue_constant_finalRange_from.Set(EditorGUILayout.Vector2Field("Between", a.changeValue_constant_finalRange_from.GetVector2()));
								a.changeValue_constant_finalRange_to.Set (EditorGUILayout.Vector2Field("and", a.changeValue_constant_finalRange_to.GetVector2()));
							} else if (value.GetType() == typeof(Vector3)) {
								EditorGUILayout.LabelField("A vector...");
								a.changeValue_constant_finalRange_from.Set (EditorGUILayout.Vector3Field("Between", a.changeValue_constant_finalRange_from.GetVector3()));
								a.changeValue_constant_finalRange_to.Set (EditorGUILayout.Vector3Field("and", a.changeValue_constant_finalRange_to.GetVector3()));
							}

						}
						if (t.type != Trigger.Type.OnGenericValueChange) {
							LabelSeparator("based on the distance from...");
							FieldTransformOrPosition(new Color(1,.3f,.3f), ref a.changeValue_constant_from_type, ref a.changeValue_constant_from_transform, ref a.changeValue_constant_from_position, ref a.changeValue_constant_farDistance);
							LabelSeparator("to...");
							FieldTransformOrPosition(new Color(.3f,.3f,1), ref a.changeValue_constant_to_type, ref a.changeValue_constant_to_transform, ref a.changeValue_constant_to_position, ref a.changeValue_constant_nearDistance);
						}

						a.changeValue_constant_curve = EditorGUILayout.CurveField("Curve", a.changeValue_constant_curve);
						if (a.changeValue_constant_curve.keys.Length == 0) {
							a.changeValue_constant_curve = AnimationCurve.EaseInOut(0,0,1,1);
						}

						a.changeValue_constant_changeOnLoad = EditorGUILayout.ToggleLeft(new GUIContent("Set value when scene loads", "Change the property when the level loads to avoid sudden changes when entering the area"), a.changeValue_constant_changeOnLoad);

						if (a.changeValue_constant_farDistance < a.changeValue_constant_nearDistance)
							a.changeValue_constant_farDistance = a.changeValue_constant_nearDistance;
					}

					break;
					case Action.Type.PlayAnimation:
						
						a.playAnimation_animation = EditorGUILayout.ObjectField ("Animation component", a.playAnimation_animation, typeof(Animation), true) as Animation;
						
						a.playAnimation_animation_clip = EditorTools.AnimationPopup("Animation", a.playAnimation_animation, a.playAnimation_animation_clip);

						a.playAnimation_loop = EditorGUILayout.ToggleLeft("Loop", a.playAnimation_loop);
						a.playAnimation_queue = EditorGUILayout.ToggleLeft("Queue (start after previous one's finished)", a.playAnimation_queue);
						if (a.playAnimation_queue)
							EditorGUILayout.HelpBox("This will play after the current animation. Keep in mind if the last one is a loop, this won't play!", MessageType.Info);
					    
					break;
					case Action.Type.ChangeScene:

						string[] scenes = AssetDatabase.FindAssets("t:scene");
						string[] scenenames = new string[scenes.Length];
						for (int i = 0; i < scenes.Length; i++) {
							//if (path.StartsWith("Assets/")) //remove the assets directory because Build Settings doesn't use it
							//    path = path.Remove(0, "Assets/".Length);
							scenenames[i] = AssetDatabase.GUIDToAssetPath(scenes[i]);
						}
						int sceneIndex = EditorGUILayout.Popup("Scene", System.Array.IndexOf(scenes,a.changeScene_sceneGUID), scenenames);
						if (sceneIndex > 0)
							a.changeScene_sceneGUID = scenes[sceneIndex];
						// check it's in build settings
						if (a.changeScene_sceneGUID.Length > 0) {
							string scenename = AssetDatabase.GUIDToAssetPath(a.changeScene_sceneGUID);
							a.changeScene_scenePath = scenename;
							bool onBuildList = false;
							foreach(EditorBuildSettingsScene s in EditorBuildSettings.scenes) {
								if (s.path.Contains(scenename) && s.enabled) {
									onBuildList = true;
									break;
								}
							}
							if (!onBuildList) {
								EditorGUILayout.HelpBox ("Add the scene to the Build Settings to load it:", MessageType.Error);
								GUI.color = Color.red;
								if (GUILayout.Button("Add Scene to Build List")) {
									List<EditorBuildSettingsScene> buildscenes = EditorBuildSettings.scenes.OfType<EditorBuildSettingsScene>().ToList();
									buildscenes.Add(new EditorBuildSettingsScene(scenename,true));
									EditorBuildSettings.scenes = buildscenes.ToArray();
								}
								GUI.color = Color.white;
							}
						} else {
							a.changeScene_scenePath = "";
						}
						a.changeScene_duration = EditorGUILayout.FloatField("Transition Duration", a.changeScene_duration);
						a.changeScene_transitionSound = (AudioClip)EditorGUILayout.ObjectField("Transition sound", a.changeScene_transitionSound, typeof(AudioClip), true);
						a.changeScene_transitionTexture = (Texture2D)EditorGUILayout.ObjectField("Transition Texture", a.changeScene_transitionTexture, typeof(Texture2D), true);
						a.changeScene_transitionWaveTexture = (Texture2D)EditorGUILayout.ObjectField("Wave Effect Texture", a.changeScene_transitionWaveTexture, typeof(Texture2D), true);

					break;
					case Action.Type.PlaySound:
						
						DrawSoundsSelection(a);

					break;
					case Action.Type.FadeSound:
						a.changeValue_value.Set(EditorGUILayout.Slider("Volume To", a.changeValue_value.GetFloat(), 0, 1));
						a.duration = EditorGUILayout.FloatField(new GUIContent("During (Seconds)","0 = Instant change"), a.duration);
						
						FieldRestoreValue(a);

						DrawSoundsSelection(a);

						

					break;
					case Action.Type.StopSound:
						/*
						a.sound_allSounds = EditorGUILayout.ToggleLeft("All sounds", a.sound_allSounds);
						GUI.enabled = !a.sound_allSounds;
						FieldAudioSource(a);
						GUI.enabled = true;
						*/
						//a.duration = EditorGUILayout.FloatField(new GUIContent("Delay (Seconds)","0 = Instant change"), a.duration);

						DrawSoundsSelection(a);
					break;
					case Action.Type.MovePlayer:
					case Action.Type.MoveObject:
						if (a.type == Action.Type.MoveObject) {
							a.target = EditorGUILayout.ObjectField("Target Object", a.target, typeof(GameObject), true) as GameObject;
							if (a.target == null) EditorGUILayout.HelpBox("Select a target object", MessageType.Error);
							a.duration = EditorGUILayout.FloatField(new GUIContent("During (Seconds)", "0 = instant change"), a.duration);
						} else {
							a.duration = 0;
						}

						/*
						a.changeValue_value.Set(EditorGUILayout.Vector3Field("Move to", a.changeValue_value.GetVector3()));
						a.duration = EditorGUILayout.FloatField(new GUIContent("During (Seconds)","0 = Instant change"), a.duration);
						
						a.movePlayer_movementType = Action.PlayerMovementType.ForcePosition;
						*/
						a.movePlayer_movementType = (Action.PlayerMovementType)EditorGUILayout.EnumPopup("How to move it?", a.movePlayer_movementType);
						if (a.movePlayer_movementType == Action.PlayerMovementType.ForcePosition) {
							a.changeValue_value.Set(EditorGUILayout.Vector3Field("Move to", a.changeValue_value.GetVector3()));
							a.duration = EditorGUILayout.FloatField(new GUIContent("During (Seconds)","0 = Instant change"), a.duration);
						} else if (a.movePlayer_movementType == Action.PlayerMovementType.Poke) {
							a.changeValue_value.Set(EditorGUILayout.Vector3Field("Move by (relative)", a.changeValue_value.GetVector3()));
							if (t.isTypeConstant)
								EditorGUILayout.HelpBox("The trigger is constant, this may shoot it player like crazy!", MessageType.Warning);
						}/* else if (a.movePlayer_movementType == Action.PlayerMovementType.AddForce) {
							a.changeValue_value.Set(EditorGUILayout.Vector3Field("Force (relative to player)", a.changeValue_value.GetVector3()));
							if (t.isTypeConstant)
								EditorGUILayout.HelpBox("The trigger is constant, this may shoot the player like crazy!", MessageType.Warning);
						}*/
						
						
					break;
					case Action.Type.ActivateObject:
						
						// object
						a.target = EditorGUILayout.ObjectField("Target Object", a.target, typeof(GameObject), true) as GameObject;
						if (a.target == null) EditorGUILayout.HelpBox("Select a target object", MessageType.Error);

						//a.activate_flipValue = EditorGUILayout.ToggleLeft("Flip the state", a.activate_flipValue);
						
						if (!a.activate_flipValue) {
							
							EditorGUILayout.BeginHorizontal();
							GUI.color = a.changeValue_value.GetBool() ? Color.green : Color.white;
							if (GUILayout.Button("ACTIVATE")) a.changeValue_value.Set(true);
							GUI.color = !a.changeValue_value.GetBool() ? Color.red : Color.white;
							if (GUILayout.Button("DEACTIVATE")) a.changeValue_value.Set(false);
							GUI.enabled = true;
							GUI.color = Color.white;
							EditorGUILayout.EndHorizontal();
							
							EditorGUILayout.HelpBox("The object's state will be set as: " + (a.changeValue_value.GetBool() ? "ACTIVE" : "INACTIVE"), MessageType.Info);
						} else {
							EditorGUILayout.HelpBox("The object's state will be FLIPPED", MessageType.Info);
						}


					break;
					case Action.Type.ChangeCameraMode:
						a.changeCameraMode_mode = (CameraController.CameraMode)EditorGUILayout.EnumPopup("Camera mode", a.changeCameraMode_mode);
						a.changeCameraMode_time = EditorGUILayout.FloatField("During (Seconds", a.changeCameraMode_time);
						if( a.changeCameraMode_mode == CameraController.CameraMode.CustomCamera ) {
							a.changeCameraMode_target = (Transform)EditorGUILayout.ObjectField("Camera Target", a.changeCameraMode_target, typeof(Transform), true);
							if (a.changeCameraMode_target == null)
							EditorGUILayout.HelpBox("Select an object to use as a camera target (Helps to use the menu option David > Make a camera...)", MessageType.Error);
						}
						if (a.changeCameraMode_mode == CameraController.CameraMode.TopDownWithMouseControls) {
							a.changeCameraMode_topdownFollow = EditorGUILayout.ToggleLeft("Follow the player", a.changeCameraMode_topdownFollow);
						}
					break;
					case Action.Type.ChangeAvatar:
						a.changeAvatar_prefab = EditorGUILayout.ObjectField("New avatar Prefab", a.changeAvatar_prefab, typeof(GameObject), true) as GameObject;
					break;
					case Action.Type.ChangePlayerSounds:
						EditorGUILayout.HelpBox("Leave a sound as None and it won't be changed.", MessageType.Info);
						a.playerSound_footsteps = EditorGUILayout.ObjectField("Footsteps", a.playerSound_footsteps, typeof(AudioClip), true) as AudioClip;
						a.playerSound_jump = EditorGUILayout.ObjectField("Jump", a.playerSound_jump, typeof(AudioClip), true) as AudioClip;
						a.playerSound_land = EditorGUILayout.ObjectField("Land", a.playerSound_land, typeof(AudioClip), true) as AudioClip;
						a.playerSound_air = EditorGUILayout.ObjectField("Air", a.playerSound_air, typeof(AudioClip), true) as AudioClip;
					break;
					case Action.Type.ShowMusicalDialog:
						a.musicalDialog_dialog = EditorGUILayout.ObjectField("Dialog", a.musicalDialog_dialog, typeof(MusicalDialog), true) as MusicalDialog;
					break;
					case Action.Type.ChangeMouseCursor:
						a.texture = EditorGUILayout.ObjectField("Texture", a.texture, typeof(Texture2D), true) as Texture2D;
						a.changeCursor_hotspot = EditorGUILayout.Vector2Field("Center (0 to 1)", a.changeCursor_hotspot);
						a.changeCursor_hotspot.x = Mathf.Clamp01(a.changeCursor_hotspot.x);
						a.changeCursor_hotspot.y = Mathf.Clamp01(a.changeCursor_hotspot.y);
					break;
					case Action.Type.SwapSkybox:
						a.changeMaterial_material = (Material)EditorGUILayout.ObjectField("Skybox material", a.changeMaterial_material, typeof(Material), true);
					break;
				}

				if (a.duration < 0) a.duration = 0;

				EditorGUILayout.Space();
				

				GUILayout.EndVertical();
				 
				if (GUI.changed)
					EditorUtility.SetDirty(t);
			}
			GUI.color = Color.green;
			if (GUILayout.Button ("Add an Action"))
				t.actions.Add(new Action(t));
			GUI.color = Color.white;

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(t);
			}

		}

		void LabelSeparator(string Text) {
			EditorGUILayout.Space();
			GUIStyle gs = GUI.skin.GetStyle("Label");
			gs.normal.textColor = Color.black;
			gs.fontStyle = FontStyle.Bold;
			gs.alignment = TextAnchor.MiddleCenter;
			EditorGUILayout.LabelField(Text, gs);
		}

		void LabelWarning(string Text) {
			GUIStyle gs = GUI.skin.GetStyle("Label");
			gs.normal.textColor = Color.red;
			gs.alignment = TextAnchor.MiddleCenter;
			gs.stretchHeight = false;
			gs.wordWrap = true;
			EditorGUILayout.LabelField(Text, gs);
		}

		void FieldAudioSource(Action a) {
			Trigger t = target as Trigger;
			a.sound_audioSource = EditorGUILayout.ObjectField ("Audio Source", a.sound_audioSource, typeof(AudioSource), true) as AudioSource;
			if (a.sound_audioSource == null && t.GetComponent<AudioSource>() == null) {
				a.sound_audioSource = null;
				EditorGUILayout.HelpBox ("Add an Audio Source.", MessageType.Warning);
			} else if (a.sound_audioSource == null && t.GetComponent<AudioSource>() != null || a.sound_audioSource == t.GetComponent<AudioSource>()) {
				a.sound_audioSource = t.GetComponent<AudioSource>();
				EditorGUILayout.HelpBox ("Found an Audio Source in this game object, I'll use that one.", MessageType.Info);
			}
			if (a.sound_audioSource != null) {
				if (a.sound_audioSource.clip == null) {
					EditorGUILayout.HelpBox ("The selected Audio Source doesn't have an audio clip. Nothing will play.", MessageType.Error);
				}
			}
		}
		void FieldRestoreValue(Action a) {
			Trigger t = target as Trigger;
			GUI.enabled = !t.isTypeConstant && (t.type == Trigger.Type.OnColliderEnter || t.type == Trigger.Type.OnKeyDown || t.type == Trigger.Type.OnKey);
			//if (t.isTypeConstant)
			//	return;
			//if (t.type == Trigger.Type.OnColliderEnter || t.type == Trigger.Type.OnKeyDown || t.type == Trigger.Type.OnKey) {

			/*
			a.restoreValueOnExit = EditorGUILayout.Toggle(new GUIContent("Restore value after", !GUI.enabled ? "Restore the original value after the trigger is left or the key is not held anymore": ""), a.restoreValueOnExit);
			if (GUI.enabled && a.restoreValueOnExit)
				EditorGUILayout.HelpBox ("Make sure to not overlap or place Triggers that change the same value nearby or the logic will break!", MessageType.Warning);
					
			*/
			//}
			GUI.enabled = true;
		}

		void FieldTransformOrPosition( Color color, ref Action.ValueTargetType valueType, ref Transform transform, ref Vector3 position, ref float margin) {
			// dual button
			// X

			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.Space();

			GUI.color = color;

			/*
			Rect r = EditorGUILayout.BeginVertical();
			float butwidth = r.width * .45f;
			float margin = (r.width * .5f - butwidth) * 2;
			GUI.enabled = useTransform;
			if (GUI.Button (new Rect(margin, r.yMin, butwidth, EditorGUIUtility.singleLineHeight), "Position"))
				useTransform = false;
			GUI.enabled = !useTransform;
			if (GUI.Button (new Rect(r.width * .5f + margin, r.yMin, butwidth, EditorGUIUtility.singleLineHeight), "Object"))
				useTransform = true;
			GUI.enabled = true;
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndVertical();

			*/

			valueType = (Action.ValueTargetType)EditorGUILayout.EnumPopup(valueType);
			bool useTransform = valueType == Action.ValueTargetType.Object;

			if (useTransform) {
				transform = EditorGUILayout.ObjectField (transform, typeof(Transform), true) as Transform;
			} else {
				if (valueType == Action.ValueTargetType.Position)
					position = EditorGUILayout.Vector3Field("",position);
			}

			//distance = DualFloatFields("Within the range", distance, "from", "to");
			margin = EditorGUILayout.FloatField("Margin", margin);

			Color c = color;
			c.a = 0.7f;
			GUI.color = c;
			if (GUILayout.Button("Focus editor on this")) {
				if (!useTransform || (useTransform && transform != null)) {
					GameObject previouslySelected = Selection.activeGameObject;
					GameObject g = new GameObject("_delete");
					g.transform.position = useTransform ? transform.position : position;
					Selection.activeGameObject = g;
					if (Selection.activeGameObject && SceneView.lastActiveSceneView)
						SceneView.lastActiveSceneView.FrameSelected();
					DestroyImmediate (g);
					Selection.activeGameObject = previouslySelected;
				}
			}

			GUI.color = Color.white;

			//valueType = useTransform ? Action.ValueTargetType.Object : Action.ValueTargetType.Position;

			/*
			Rect r = EditorGUILayout.BeginVertical();
			EditorGUI.MultiFloatField(r,new GUIContent[2]{new GUIContent("From"),new GUIContent("To")}, new float[2]{distanceRange.x,distanceRange.y});
			EditorGUILayout.EndVertical();
			*/

			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
		}

		Vector2 DualFloatFields(string Label, Vector2 Value, string LabelLeft = "", string LabelRight ="") {
			if (Label.Length > 0)
				EditorGUILayout.LabelField(Label);
			Rect r = EditorGUILayout.BeginHorizontal();
			float x = r.xMin;
			float butwidth = r.width * .45f;
			float margin = (r.width * .5f - butwidth);
			if (LabelLeft.Length > 0) {
				EditorGUI.LabelField(new Rect(x, r.yMin, r.width * .25f, EditorGUIUtility.singleLineHeight), LabelLeft);
				x += r.width * .25f;
			}
			Value.x = EditorGUI.FloatField(new Rect(x, r.yMin, r.width * .5f - x - margin * .5f, EditorGUIUtility.singleLineHeight), Value.x);
			x = r.width * .5f;
			if (LabelRight.Length > 0) {
				EditorGUI.LabelField(new Rect(x, r.yMin, r.width * .25f, EditorGUIUtility.singleLineHeight), LabelRight);
				x += r.width * .25f;
			}
			Value.y = EditorGUI.FloatField(new Rect(x, r.yMin, r.width - x, EditorGUIUtility.singleLineHeight), Value.y);
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndHorizontal();
			return Value;
		}

		void DrawSoundsSelection(Action a) {
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = a.sound_changeType != Action.SoundChangeType.Single;
			if (GUILayout.Button("SINGLE")) a.sound_changeType = Action.SoundChangeType.Single;
			/*GUI.enabled = a.sound_changeType != Action.SoundChangeType.Multiple;
			if (GUILayout.Button("MULTIPLE")) a.sound_changeType = Action.SoundChangeType.Multiple;*/
			GUI.enabled = a.sound_changeType != Action.SoundChangeType.All;
			if (GUILayout.Button("ALL SOUNDS")) a.sound_changeType = Action.SoundChangeType.All;
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
			if (a.sound_changeType == Action.SoundChangeType.Single) {
				FieldAudioSource(a);
			} else if (a.sound_changeType == Action.SoundChangeType.Multiple) {
				//EditorUtils.DrawArray(serializedObject, "sound_audioSources");
				EditorGUILayout.HelpBox("NOT SUPPORTED", MessageType.Error);
			} else if (a.sound_changeType == Action.SoundChangeType.All) {
				EditorGUILayout.HelpBox("ALL the sounds that are playing in the scene will be affected", MessageType.Warning);
			}
		}
		
		/*
		System.Enum DescriptiveEnumPopup(System.Enum Enum, Dictionary<System.Enum,string> Names, out System.Enum Value) {

			string[] names = new string[System.Enum.GetNames().Length];
			for (int i = 0; i < names.Length; i++) {
				names[i] = Names[(System.Enum)
			}
			int subpropertyIndex = EditorGUILayout.Popup("Subproperty", (int)Value, subpropertiesLabels.ToArray());


			return (PropertyChange.ChangeType)EditorGUILayout.EnumPopup("Control by", (System.Enum)gs.changeBy);
			

		}
		*/


		void OnSceneGUI() {
			Trigger t = target as Trigger;
			for(int j = 0; j < t.actions.Count; j++) {
				Action a = t.actions[j];
				
				switch (a.type) {

				case Action.Type.ChangeValue:
					if (t.isTypeConstant) {

						object value = PropertyChange.GetValue(a.changeValue_component, a.changeValue_property, a.changeValue_subproperty);
						
						// far
						Handles.color = Color.red;
						Handles.SphereCap(1,a.constantValueToPosition, Quaternion.identity,0.25f);
						Handles.DrawDottedLine(a.constantValueToPosition, a.farPoint, 10);
						Handles.DrawLine(a.farPoint, Vector3.MoveTowards(a.farPoint, a.nearPoint, Vector3.Distance(a.farPoint,a.nearPoint) *.5f));
						if (a.changeValue_constant_finalRange_from != null && value != null)
							Handles.Label(Vector3.MoveTowards(a.farPoint,a.nearPoint,0.5f), a.changeValue_constant_finalRange_from.Get(value.GetType()).ToString());
						//Handles.DrawLine(a.constantValueToPosition, a.constantValueToPosition);
						if (a.changeValue_constant_from_type == Action.ValueTargetType.Position)
							a.changeValue_constant_from_position = t.transform.InverseTransformPoint(Handles.DoPositionHandle(t.transform.TransformPoint(a.changeValue_constant_from_position), Quaternion.identity));
						a.changeValue_constant_farDistance = Handles.RadiusHandle(Quaternion.LookRotation(a.nearPoint - a.farPoint, Vector3.up), a.constantValueFromPosition, a.changeValue_constant_farDistance);
						// near
						Handles.color = Color.blue;
						Handles.SphereCap(1,a.constantValueFromPosition, Quaternion.identity,0.25f);
						Handles.DrawDottedLine(a.nearPoint, a.constantValueFromPosition, 10);
						Handles.DrawLine(Vector3.MoveTowards(a.farPoint, a.nearPoint, Vector3.Distance(a.farPoint,a.nearPoint) *.5f), a.nearPoint);
						if (a.changeValue_constant_to_type == Action.ValueTargetType.Position)
							a.changeValue_constant_to_position = t.transform.InverseTransformPoint(Handles.DoPositionHandle(t.transform.TransformPoint(a.changeValue_constant_to_position), Quaternion.identity));
						a.changeValue_constant_nearDistance = Handles.RadiusHandle(Quaternion.LookRotation(a.nearPoint - a.farPoint, Vector3.up), a.constantValueFromPosition, a.changeValue_constant_nearDistance);
						//a.changeValue_constant_to_distance = Handles.RadiusHandle(Quaternion.LookRotation(endPoint - startPoint, Vector3.up), a.constantValueToPosition, a.changeValue_constant_to_distance);
						if (a.changeValue_constant_finalRange_to != null && value != null)
							Handles.Label(Vector3.MoveTowards(a.nearPoint,a.farPoint,0.5f), a.changeValue_constant_finalRange_to.Get(value.GetType()).ToString());

						if (a.target) {
							Handles.color = Color.gray;
							Handles.DrawDottedLine(a.constantValueFromPosition, a.target.transform.position, 5);
						}
					}
					break;
				case Action.Type.MovePlayer:
				case Action.Type.MoveObject:
						Transform tgt = a.type == Action.Type.MovePlayer && Player.instance ? Player.instance.transform : (a.target ? a.target.transform : null);
						
						Vector3 finalPosition = a.changeValue_value.GetVector3();

						if (a.movePlayer_movementType == Action.PlayerMovementType.ForcePosition) {
							a.changeValue_value.Set(Handles.DoPositionHandle(finalPosition, Quaternion.identity));
						}
						if (tgt) {
							if (a.movePlayer_movementType == Action.PlayerMovementType.Poke) {
								finalPosition += tgt.position;
							}
							Handles.color = Color.green;
							Handles.DrawDottedLine(tgt.position, finalPosition, 10);
							Handles.SphereCap(0, finalPosition, Quaternion.identity, 0.25f);
							Handles.color = Color.gray;
							Handles.DrawDottedLine(t.transform.position, tgt.position, 10);
						}

					break;

				}
			}
		}
	}
}