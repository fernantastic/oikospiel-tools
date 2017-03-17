using UnityEngine;
using UnityEditor;
using System.Collections;
using OikosTools;

public class EditorUtils {

	[MenuItem("David/Make a camera that looks like the Scene view")]
	public static void CreateCameraTarget() {
		GameObject go = new GameObject("Camera Target");
		Undo.RegisterCreatedObjectUndo(go, "Create camera");
		go.transform.position = SceneView.currentDrawingSceneView.camera.transform.position;
		go.transform.rotation = SceneView.currentDrawingSceneView.rotation;
		go.AddComponent<Camera>();
		go.GetComponent<Camera>().enabled = false;
		go.GetComponent<Camera>().depth = -100;
		Scene scene = GameObject.FindObjectOfType<Scene>();
		if (scene != null && scene.transform.FindChild("Camera")) {
			Undo.RecordObject(scene.transform.FindChild("Camera").GetComponent<Camera>(), "Change field of view");
			go.GetComponent<Camera>().fieldOfView = scene.transform.FindChild("Camera").GetComponent<Camera>().fieldOfView;

		}
		Selection.activeGameObject = go;
	}

	[MenuItem("David/Move player to the Scene view (exactly)")]
	public static void MovePlayer() {

		Player player = GameObject.FindObjectOfType<Player>();
		Scene scene = GameObject.FindObjectOfType<Scene>();
		if (scene != null && player != null) {
			Undo.RecordObject(player.transform, "Move player");
			player.transform.position = SceneView.currentDrawingSceneView.camera.transform.position;
			player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.z,SceneView.currentDrawingSceneView.camera.transform.eulerAngles.y,player.transform.eulerAngles.z);
			Transform sceneCamera = scene.transform.FindChild("Camera");
			Camera headCamTarget = player.transform.FindChild("Head").GetComponentInChildren<Camera>();
			if (headCamTarget) {
				Undo.RecordObject(sceneCamera, "Move camera");
				sceneCamera.position = headCamTarget.transform.position;
				sceneCamera.rotation = headCamTarget.transform.rotation;
			}
		}
	}

	[MenuItem("David/Move player to the Scene view's floor")]
	public static void MovePlayerCollide() {
		Player player = GameObject.FindObjectOfType<Player>();
		Scene scene = GameObject.FindObjectOfType<Scene>();
		if (scene != null && player != null) {
			Camera sceneViewCam = SceneView.currentDrawingSceneView.camera;
			Vector3 v = Vector3.zero;

			RaycastHit rh = new RaycastHit();
			Ray r = sceneViewCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
			if (Physics.Raycast(r, out rh, Mathf.Infinity, 1 >> LayerMask.GetMask("Player"))) {
				v = rh.point + Vector3.up * (/*player.GetComponent<CharacterController>().height * 0.5f + */0.1f);;
				Undo.RecordObject(player.transform, "Move player");

				player.transform.position = v;
				//player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.z,SceneView.currentDrawingSceneView.camera.transform.eulerAngles.y,player.transform.eulerAngles.z);
				Transform sceneCamera = scene.transform.FindChild("Camera");
				Camera headCamTarget = player.transform.FindChild("Head").GetComponentInChildren<Camera>();
				if (headCamTarget) {
					Undo.RecordObject(sceneCamera, "Move camera");

					sceneCamera.position = headCamTarget.transform.position;
					sceneCamera.rotation = headCamTarget.transform.rotation;
				}
			}
			
		}
	}


	// Edit this line to define your shortcut in the editor
	// In this example, #1 is the shortcut
	//	% = ctrl (win) or cmd (osx)
	//	# = shift
	//	& = alt
	//	_ = no key modifier
	[MenuItem ("Scene Shortcuts/Edit CORE #1", false, 0)]
	public static void EditScene1()
	{
		if( EditorApplication.SaveCurrentSceneIfUserWantsTo() )
			EditorApplication.OpenScene("Assets/Scenes/Menues/_CORE.unity");
	}

	[MenuItem ("Scene Shortcuts/Edit examplesliders #2", false, 0)]
	public static void EditScene2()
	{
		if( EditorApplication.SaveCurrentSceneIfUserWantsTo() )
			EditorApplication.OpenScene("Assets/Scenes/Examples/example_sliders.unity");
	}

	public static void DrawArray(SerializedObject sobject, string propertyName) {
		sobject.Update();
		//EditorGUIUtility.LookLikeInspector();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(sobject.FindProperty(propertyName), true);
		if (EditorGUI.EndChangeCheck())
			sobject.ApplyModifiedProperties();
		//EditorGUIUtility.LookLikeControls();
	}

}
