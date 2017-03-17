// by @torahhorse

// Instructions:
// this is a pause menu for your drifting game
// it has options for field of view, invert y axis, and mouse sensitivity
// it also saves those values.
// To use it just put the script on something and change the styles in the inspector.
// the Menu prefab comes with an already defined style

// note: you're meant to use this as a starting point / debug. i dont wanna see this vanilla Arial stuff in yr games

using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
	public static bool paused = false;
	
	public float minFOV = 45.0f;
	public float maxFOV = 100.0f;
	
	public float minSensitivity = 1.0f;
	public float maxSensitivity = 20.0f;
	
	public GUIStyle pauseMenuStyle;
	public GUIStyle bgStyle;
	public GUIStyle scrollBarStyle;
	public GUIStyle scrollButtonStyle;
	
	private int littleButtonWidth;
	private int littleButtonHeight;
	private int buttonWidth;
	private int buttonHeight;
	private int sliderWidth;
	private int sliderHeight;
	
	private int fontSize;
	
	private float fovSlider = .5f;
	private float sensitivitySlider = .5f;
	
	private bool mouseSettings = false;
	private bool quitDialog = false;
	
	private string invertYText = "Invert Y Axis";
	
	private MouseLook camMouseLook;
	private MouseLook capsuleMouseLook;
	
	private float defaultFOV;
	private float defaultSensitivity;
	
	// Use this for initialization
	void Start ()
	{
		//set button dimensions
		SetButtonDimensions();
		// just making this shit up
		fontSize = (int)(Screen.width / 24.4f);
	
		pauseMenuStyle.fontSize = fontSize;
	
		fovSlider = Camera.main.fieldOfView;
		
		camMouseLook = Camera.main.GetComponent<MouseLook>();
		capsuleMouseLook = GameObject.FindWithTag("Player").GetComponent<MouseLook>();
		sensitivitySlider = capsuleMouseLook.sensitivityX;
		
		// save the default values
		defaultFOV = fovSlider;
		defaultSensitivity = sensitivitySlider;
		
		RememberInvertY();
		RememberFOV();
		RememberSensitivity();
	}
	
	void ResetToDefault()
	{
		// reset FOV
		fovSlider = defaultFOV;
		SetFOV();
		
		// reset Sensitivity
		sensitivitySlider = defaultSensitivity;
		SetSensitivity();
		
		// reset invert Y
		PlayerPrefs.SetInt("InvertY", 1);
		InvertY();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( Input.GetKeyDown(KeyCode.Escape) )
		{
			PauseGame();
		}
	}
	
	void OnGUI()
	{
		if( paused )
		{
			
			if( !mouseSettings && !quitDialog)
			{
				DrawPauseMenuBG();
				
				// Resume game button
				if(GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, buttonHeight, buttonWidth, buttonHeight), "Resume", pauseMenuStyle))
				{
					PauseGame();
				}
				
				// Reset level button
				if(GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, buttonHeight*2, buttonWidth, buttonHeight), "Reset Level", pauseMenuStyle))
				{
					print("Reset Level");
					PauseGame();
					Application.LoadLevel(Application.loadedLevel);
				}
				
				// options button
				if(GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, buttonHeight*3, buttonWidth, buttonHeight), "Options", pauseMenuStyle))
				{
					mouseSettings = true;
				}
				
				// Quit button
				if(GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, buttonHeight*4, buttonWidth, buttonHeight), "Quit Game", pauseMenuStyle))
				{
					quitDialog = true;
				}
			}
			else if( mouseSettings)
			{
				DrawPauseMenuBG();
				
				// BACK button
				if( GUI.Button(new Rect(littleButtonWidth/2, 0, littleButtonWidth, littleButtonHeight), "Back", pauseMenuStyle) )
				{
					mouseSettings = false;
				}
				
				// invertY button
				if( GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/2 - buttonHeight*2, buttonWidth, buttonHeight), invertYText, pauseMenuStyle) )
				{
					InvertY();
				}
				
				// SFX Label
				GUI.Label(new Rect( Screen.width/2 - buttonWidth/2 - sliderWidth, Screen.height/2 - buttonHeight, buttonWidth, buttonHeight), "Field of view [" + (int)fovSlider + "]", pauseMenuStyle);
				
				// MUSIC label
				GUI.Label(new Rect( Screen.width/2 - buttonWidth/2 - sliderWidth, Screen.height/2, buttonWidth, buttonHeight), "Sensitivity [" + (int)sensitivitySlider + "]", pauseMenuStyle);
				
				/*
				// FOV SLIDER
				GUI.skin.horizontalSliderThumb = scrollButtonStyle;
				fovSlider = GUI.HorizontalSlider (new Rect (Screen.width/2, Screen.height/2 - buttonHeight + sliderHeight/2, sliderWidth, sliderHeight), fovSlider, minFOV, maxFOV, scrollBarStyle, scrollButtonStyle);
				SetFOV();
				*/
				
				// SENSITIVITY SLIDER
				GUI.skin.horizontalSliderThumb = scrollButtonStyle;
				sensitivitySlider = GUI.HorizontalSlider (new Rect (Screen.width/2, Screen.height/2 + sliderHeight/2, sliderWidth, sliderHeight), sensitivitySlider, minSensitivity, maxSensitivity, scrollBarStyle, scrollButtonStyle);
				SetSensitivity();
				
				if( GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/2 + buttonHeight, buttonWidth, buttonHeight), "Reset to default", pauseMenuStyle) )
				{
					ResetToDefault();
				}
			}
			else if( quitDialog )
			{
				DrawPauseMenuBG();
				// Are you sure label
				GUI.Label(new Rect( Screen.width/2 - buttonWidth/2, Screen.height/2 - buttonHeight, buttonWidth, buttonHeight), "Are you sure?", pauseMenuStyle);
			
				// yes
				if( GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/2, buttonWidth, buttonHeight), "Yes", pauseMenuStyle) )
				{
					Application.Quit();
				}
				// no
				if( GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/2 + buttonHeight, buttonWidth, buttonHeight), "No", pauseMenuStyle) )
				{
					quitDialog = false;
				}
			}
		}
	}
	
	
	void SetFOV()
	{
		/*
		CameraZoom zoom = Camera.main.GetComponent<CameraZoom>();
		if( zoom != null )
		{
			zoom.SetBaseFOV(fovSlider);
		}
		
		Camera.main.fieldOfView = fovSlider;
		*/
		
		PlayerPrefs.SetFloat("FOV", fovSlider);
	}
	
	void SetSensitivity()
	{
		camMouseLook.SetSensitivity(sensitivitySlider);
		capsuleMouseLook.SetSensitivity(sensitivitySlider);
		
		PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider);
	}
	
	void InvertY()
	{
		// check for invertY in playerprefs
		if( PlayerPrefs.HasKey("InvertY") )
		{
			// if not invertYd
			if( PlayerPrefs.GetInt("InvertY") < 1)
			{
				camMouseLook.invertY = true;
				
				PlayerPrefs.SetInt("InvertY", 1);
				
				invertYText = "Normal Y Axis";
			}
			else
			{
				camMouseLook.invertY = false;
				
				PlayerPrefs.SetInt("InvertY", 0);
				
				invertYText = "Invert Y Axis";
			}
		}
		else
		{
			print("Couldn't find 'invertY' in PlayerPrefs");	
		}
	}
	
	// toggle pause state
	public void PauseGame()
	{
		paused = !paused;
		Time.timeScale = 1.0f - Time.timeScale;
		
		// pause or unpause the music
		if( paused )
		{
			print("Game Paused");
		}
		else
		{
			print("Game Resumed");
		}
	}
	
	void DrawPauseMenuBG()
	{
		// Make a background box
		GUI.Box(new Rect(-10, -10, Screen.width + 20, Screen.height + 20), "", bgStyle);
	}
	
	void RememberInvertY()
	{
		// check if there's a playerpref for invert Y
		if( !PlayerPrefs.HasKey("InvertY") )
		{
			print("No InvertY setting in PlayerPrefs, creating");
			PlayerPrefs.SetInt("InvertY", 0);
		}
		else
		{
			// set the text based on remembered setting
			if( PlayerPrefs.GetInt("InvertY") > 0)
			{
				// reset invert Y
				PlayerPrefs.SetInt("InvertY", 0);
				Invoke("InvertY", .01f);
			}
		}	
	}
	
	// Functions for storing settings in player prefs
	void RememberFOV()
	{
		// check if there's a playerpref for FOV
		if( !PlayerPrefs.HasKey("FOV") )
		{
			print("No FOV in PlayerPrefs, creating");
			PlayerPrefs.SetFloat("FOV", Camera.main.fieldOfView);
		}
		else
		{
				// set FOV
			fovSlider = PlayerPrefs.GetFloat("FOV");
			Invoke("SetFOV", .02f); 
		}	
	}
	
	void RememberSensitivity()
	{
		// check if there's a playerpref for FOV
		if( !PlayerPrefs.HasKey("Sensitivity") )
		{
			print("No Sensitivity in PlayerPrefs, creating");
			PlayerPrefs.SetFloat("Sensitivity", capsuleMouseLook.sensitivityX);
		}
		else
		{
				// set FOV
			sensitivitySlider = PlayerPrefs.GetFloat("Sensitivity");
			Invoke("SetSensitivity", .02f); 
		}	
	}
	
	void SetButtonDimensions()
	{
		littleButtonWidth = Screen.width / 12;
		littleButtonHeight = Screen.height / 8;
		buttonWidth = Screen.width / 2;
		buttonHeight = Screen.height / 6;
		sliderWidth = Screen.width / 6;
		sliderHeight = Screen.height / 12;
	}
}
