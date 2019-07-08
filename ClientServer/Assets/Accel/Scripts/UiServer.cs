using UnityEngine;
using System.Collections;

public class UiServer : MonoBehaviour {
	public GameObject text;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void connect(bool res){
		TextMesh t = (TextMesh)text.GetComponent (typeof(TextMesh));
		string str = "";
		str = (res)? "connected." : "not connected.";
		t.text = str;
	}
	
	public void ready(bool res) {
		TextMesh t = (TextMesh)text.GetComponent (typeof(TextMesh));
		string str = "";
		str = (res)? "ready!" : "not ready.";
		t.text = str;
	}
}
