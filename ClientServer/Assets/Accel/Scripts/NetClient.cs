using UnityEngine;
using System.Collections;

public class NetClient : MonoBehaviour, AjNet.NetManager {

	AjNet net;
	public Transform titleUI;
	UiClient myUI;
	bool conPress;
	Vector3 accBk;
	
	void Start () {	
		
		conPress = false;

		myUI = titleUI.GetComponent("UiClient") as UiClient;
		myUI.main = this;

		net = AjNet.GetInstance();
		net.manager = this;	

		accBk = Input.acceleration;
		
		net.StartClient();
	}
	public void clickConnect(){
		StartCoroutine("waitAndConnect",1.0f);
	}
	IEnumerator waitAndConnect(float wTime){
		yield return new WaitForSeconds(wTime);
		net.Connect();
		myUI.connectBT(net.Connected);
	}

	void Update () {
		
		if (net.status == AjNet.Status.Client && net.Connected) {
			accBk = Vector3.Lerp(accBk, Input.acceleration, Time.deltaTime * 10f);
			net.CallAcc(accBk);
		} else if(net.status == AjNet.Status.Client && net.Connected == false){
			if(Input.touchCount >= 1){
				
				Touch touch= Input.touches[0];		
				if (touch.phase == TouchPhase.Began) {
					conPress = true;
				}else if(touch.phase == TouchPhase.Ended) {
					if(conPress == true){
						clickConnect();
					}
					conPress = false;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
	}

	void OnGUI() {
	}
	void AjNet.NetManager.SessionMemberAdded(string memberId) {
		Debug.Log ("SessionMemberAdded " + memberId);	
		AjNet.serverText += "me:sessionMemberAdded\n";
	}
	
	void AjNet.NetManager.SessionMemberRemoved(string memberId) {
		Debug.Log ("SessionMemberRemoved " + memberId);
	}
	
	void AjNet.NetManager.AccelReceived(string memberId, string button) {
		Debug.Log ("+++++AccelReceived " + memberId + " " + button);
	}
	
	void AjNet.NetManager.DataReceived(string memberId, string type, string data) {
		
	}

	void AjNet.NetManager.SessionLost() {
		AjNet.serverText += "me:sessionLost\n";
		myUI.connectBT(false);
	}

	void AjNet.NetManager.connectFail() {
		myUI.connectBT(false);
	}

	
}
