
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

using UnityEngine;    // For Debug.Log, etc.

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System;
using System.Runtime.Serialization;
using System.Reflection;

using System.Collections.Generic;

namespace OikosTools {
// === This is the info container class ===
[Serializable ()]
//[assembly:AssemblyVersion ("0.1.*.*")]
public class Saver : ISerializable {
	
	// === Values ===
	// Edit these during gameplay
	public string 	version = "";
	public string 	lastScene = "";
	// === /Values ===
	
	// The default constructor. Included for when we call it during Save() and Load()
	public Saver () {}
	
	// This constructor is called automatically by the parent class, ISerializable
	// We get to custom-implement the serialization process here
	public Saver (SerializationInfo info, StreamingContext ctxt)
	{
		// Get the values from info and assign them to the appropriate properties. Make sure to cast each variable.
		// Do this for each var defined in the Values section above
		version = (string)info.GetValue("version", typeof(string));
		lastScene = (string)info.GetValue("lastScene", typeof(string));
	}
	
	// Required by the ISerializable class to be properly serialized. This is called automatically
	public void GetObjectData (SerializationInfo info, StreamingContext ctxt)
	{
		// Repeat this for each var defined in the Values section
		info.AddValue("version", (version));
		info.AddValue("lastScene", (lastScene));
	}
}

// === This is the class that will be accessed from scripts ===
public class SaveLoad {
	
	public static string currentFilePath = "save.cfg";    // Edit this for different save files
	Saver _data;
	public Saver data {
		get {
			if (_data == null) {
				Load();
			}
			return _data;
		}
	}
	
	public string version { get {
			return System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
		}
	}
	
	private static SaveLoad _instance;
	public static SaveLoad instance { get { if (_instance == null) {_instance = new SaveLoad(); } return _instance; } }
	
	public SaveLoad() {
		_instance = this;
	}
	
	// Call this to write data
	public void Save ()  // Overloaded
	{
		Save(currentFilePath);
	}
	public void Save (string filePath)
	{
		//Saver data = new Saver ();
		if (_data == null)
			Load();
		
		Stream stream = File.Open(filePath, FileMode.Create);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		bformatter.Serialize(stream, _data);
		stream.Close();
		Debug.Log("Finished saving settings");
	}
	
	// Call this to load from a file into "data"
	public void Load ()  { Load(currentFilePath);  }   // Overloaded
	public void Load (string filePath) 
	{
		Debug.Log ("Loading settings");
		bool createNewSavefile = false;
		
		_data = new Saver ();
		Stream stream = File.Open(filePath, FileMode.OpenOrCreate);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		if (stream.Length != 0)
		{
			// load
			try {
				_data = (Saver)bformatter.Deserialize(stream);
			} catch (Exception e) {
				Debug.Log("Error loading savefile: " + e);
				createNewSavefile = true;
			}
			// check version
			if (_data.version.Length > 0 && _data.version != version && Globals.ERASE_SAVEFILE_IF_VERSION_MISMATCH) { 
				Debug.LogWarning("Savefile version mismatch. Savefile is v" + _data.version + " and build is v" + version);
				createNewSavefile = true;
			}
			
		} else {
			// no savefile found, or empty file
			Debug.Log("No savefile found");
			createNewSavefile = true;
		}
		stream.Close();
		
		
		
		if (createNewSavefile) {
			Debug.Log("Creating new savefile for version " + version);
			_data = new Saver();
			_data.version = version;
			Save ();
		}
		
		#if UNITY_EDITOR
		if (Globals.EDITOR_IGNORE_LAST_SCENE)
			_data.lastScene = "";
		#endif
		
		Debug.Log(string.Format("Finished loading settings, v{0}",_data.version));
	}
	
}

// === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
// Do not change this
public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
	public override Type BindToType( string assemblyName, string typeName )
	{ 
		if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
		{ 
			Type typeToDeserialize = null; 
			
			assemblyName = Assembly.GetExecutingAssembly().FullName; 
			
			// The following line of code returns the type. 
			typeToDeserialize = Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 
			
			return typeToDeserialize; 
		} 
		
		return null; 
	} 
}
}