
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
using System;

namespace OikosTools {
/**
 * Manages the values used by graphics and sounds
 * */
public class GenericValueSystem : MonoBehaviour {
	
	public static GenericValueSystem instance {
		get {
			if (_instance == null) {
				var go = new GameObject("_Value System");
				_instance = go.AddComponent<GenericValueSystem>();
			}
			return _instance;
		}
	}
	static GenericValueSystem _instance;
	
	public enum ValueType
	{
		Unassigned,
		CameraHorizontalAngle,
		CameraVerticalAngle,
		CursorX,
		CursorY,
		MouseWiggleFast,
		MouseWiggleSlow,
		CursorDistanceToPlayer
		/*
		MidiControl1,
		MidiControl2,
		MidiControl3,
		MidiControl4,
		MidiControl5,
		MidiControl6,
		MidiControl7,
		MidiControl8,
		MidiControl9,
		MidiControl10,
		MidiControl11,
		MidiControl12,
		MidiControl13,
		MidiControl14,
		MidiControl15,
		MidiControl16,
		MidiControl17,
		MidiControl18
		*/
	}
	public static int ValueTypeAmount { get { return Enum.GetValues(typeof(ValueType)).Length; } }
	
	public enum ButtonState { NotAButton, Unpressed, Pressed }

	public static System.Action OnAnyValueChanged;
	
	private List<float> currentControlValues;
	
	/// CALLBACKS
	private List<ValueType> _dirtyValues; // which callbacks must we call
	public delegate void Broadcaster(ValueType T, float V);
	private List<Broadcaster>[] _callbacksOnValueChange;
	
	
	void Awake () {
		
		currentControlValues = new List<float>(ValueTypeAmount);
		for (int i = 0; i < currentControlValues.Capacity; i++) {
			currentControlValues.Add(0.0f);
		}
		
		Reset();

	}
	
	void OnSceneLoaded() {
		// set it so all callbacks will be called on Update()
		SetAllValuesAsDirty();
	}
	
	
	// Update is called once per frame
	void Update () {
		// process stuff
		try {
			for (int i = 0; i < _dirtyValues.Count; i++) {
				CallValueChange(_dirtyValues[i]);
			}
			//ChangeValue(ValueType.AreSlidersDown, 1-GetValuesMaxDelta(false, true));
		} catch (System.Exception e) {
			Debug.LogWarning(e);
		}
		_dirtyValues.Clear();
	}
	
	public void Reset() {
		//Debug.Log("Reset");
		_dirtyValues = new List<ValueType>();
		_callbacksOnValueChange = new List<Broadcaster>[currentControlValues.Count];
		
		// set all values to 0?
		/*
		for (int i = 0; i < currentControlValues.Capacity; i++) {
			currentControlValues[i] = 0.0f;
		}
		*/
	}
	
	public void SetAllValuesAsDirty() {
		int callbacks = 0;
		
		foreach ( ValueType v in Enum.GetValues(typeof(ValueType)) ) {
			if(_callbacksOnValueChange[(int)v] != null)callbacks += _callbacksOnValueChange[(int)v].Count;
			
			if (!_dirtyValues.Contains(v)) _dirtyValues.Add (v);
		}
		//Debug.Log("Setting "+ callbacks + " callbacks to be called");
	}
	
	public void SetAllValuesToZero() {
		for (int i = 0; i < currentControlValues.Capacity; i++) {
			currentControlValues[i] = 0;
		}
	}
	
	// CALLBACKS 
	public void RegisterCallback(ValueType Type, Broadcaster BOnValueChange)
	{
		if (Type == ValueType.Unassigned) return;
		//Debug.Log("Registering callback " + BOnValueChange);
		int t = (int)Type;
		if (BOnValueChange != null) {
			if (_callbacksOnValueChange[t] == null) 
				_callbacksOnValueChange[t] = new List<Broadcaster>();
			_callbacksOnValueChange[t].Add(BOnValueChange);
		}
	}

	public void UnRegisterCallbacks(Broadcaster BOnValueChange) {
		for (int i = 0; i < currentControlValues.Count; i++) {
			if (_callbacksOnValueChange[i] != null && _callbacksOnValueChange[i].Contains(BOnValueChange)) {
				_callbacksOnValueChange[i].RemoveAll( v => v.Target == BOnValueChange.Target );
			}
		}
	}
	
	
	private void CallValueChange(ValueType Type)
	{
		int i;
		List<Broadcaster> cbs;
		cbs = _callbacksOnValueChange[(int)Type];
		if (cbs != null) { for (i = 0; i < cbs.Count; i++) { cbs[i](Type,currentControlValues[(int)Type]); } }
	}
	
	// VALUES
	public void ChangeValue(ValueType Type, float Value)
	{
		if(Type == ValueType.Unassigned) return;
		Value = Mathf.Clamp01(Value);
		if (Mathf.Approximately(currentControlValues[(int)Type], Value)) return; // do not change more than once
		// THE MOST IMPORTANT LINE THAT MAKES EVERYTHING WORK
		currentControlValues[(int)Type] = Value;
		if (OnAnyValueChanged != null)
			OnAnyValueChanged();
		_dirtyValues.Add(Type);
	}
	
	public float GetValue(ValueType Type)
	{
		return currentControlValues[(int)Type];
	}
	
	public void ForceValuesUpdate() {
		for (int i = 0; i < ValueTypeAmount; i++) {
			CallValueChange((ValueType)i);
		}
	}
	public void ForceValuesUpdate(ValueType Type) {
		CallValueChange(Type);
	}

}
}