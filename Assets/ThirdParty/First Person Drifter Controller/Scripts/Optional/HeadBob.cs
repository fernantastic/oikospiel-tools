// original by Mr. Animator
// adapted to C# by @torahhorse
// http://wiki.unity3d.com/index.php/Headbobber

using UnityEngine;
using System.Collections;

public class HeadBob : MonoBehaviour
{	
	private float bobbingSpeed = 0.25f; 
	public float bobbingAmount = 0.05f; 
	public float  midpoint = 0.6f; 
	
	private float timer = 0.0f; 
 
	void Update ()
	{ 
	    float waveslice = 0.0f; 
	    float horizontal = Input.GetAxis("Horizontal"); 
	    float vertical = Input.GetAxis("Vertical"); 
	    
	    if (Mathf.Abs(horizontal) == 0f && Mathf.Abs(vertical) == 0f)
	    { 
	       timer = 0.0f; 
	    } 
	    else
	    { 
	       waveslice = Mathf.Sin(timer); 
	       timer = timer + bobbingSpeed; 
	       if (timer > Mathf.PI * 2f)
	       { 
	          timer = timer - (Mathf.PI * 2f); 
	       } 
	    } 
	    if (waveslice != 0f)
	    { 
	       float translateChange = waveslice * bobbingAmount; 
	       float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical); 
	       totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f); 
	       translateChange = totalAxes * translateChange;
	       
	       Vector3 localPos = transform.localPosition;
	       localPos.y = midpoint + translateChange * Time.timeScale; 
	       transform.localPosition = localPos;
	    } 
	    else
	    { 
	    	Vector3 localPos = transform.localPosition;
	    	localPos.y = midpoint; 
	    	transform.localPosition = localPos;
	    } 
	}
}
