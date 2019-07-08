using UnityEngine;
using System.Collections;

public class UiClient : MonoBehaviour {
	public NetClient main;
	
	public GameObject text;
	//private Vector3 beforeVec = Vector3.zero;
	private bool isConnect = false;
	private Vector3 vec = Vector3.zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		TextMesh t = (TextMesh)text.GetComponent (typeof(TextMesh));
		
		if (isConnect) {
			vec = Input.acceleration;
			vec.Normalize();
			
			string str = vec.x
				+ "\n " + vec.y
					+ "\n " + vec.z;
			t.text = str;
		}
		else {
			if (Input.touchCount >= 1) {
				if(Input.GetTouch(0).phase == TouchPhase.Ended){
					main.clickConnect();
				}
			}
			//string str = (isTouch)? "Touch." : "not connected.";
			string str = (Input.touchCount >= 1)? "Touch." : "not connected.";
			t.text = str;
		}
	} 
	
	public void displayConBT(int num){
	}
	public void connectBT(bool res){
		isConnect = res;
	}
}
