using UnityEngine;
using System.Collections;

public class MainScene : MonoBehaviour {

	public Camera mainCamera; 
	private Transform cameraTransform;
	public NetServer myAllJoyn;
	private Vector3 vec = Vector3.zero;

	void Start () {
	
	}

	void Update () {
		if (vec.sqrMagnitude > 1)
			vec.Normalize();
		
		mainCamera.transform.LookAt(vec + mainCamera.transform.position);
	}
	
	public void setAccleration(Vector3 sndVec) {
		Debug.Log("main setAccleration (" + sndVec.x + ", "+sndVec.y + ", " + sndVec.z + ")");
	
		//IOS device
		//vec.x = sndVec.x;
		//vec.y = sndVec.y;
		//vec.z = sndVec.z;
		
		//android device
		vec.x = -sndVec.x;
		vec.y = -sndVec.y;
		vec.z = -sndVec.z;
	}
}
