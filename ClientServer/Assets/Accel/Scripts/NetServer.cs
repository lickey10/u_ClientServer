using UnityEngine;
using System.Collections;

public struct PlayerInfo {
	public int id;
	public string name;
}
public class NetServer : MonoBehaviour, AjNet.NetManager {

	AjNet net;
	MainScene main;
	public Transform titleUI;
	UiServer title;
	
	ArrayList userInfo;
	
	
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 30;
		
		userInfo = new ArrayList();
		
		main = GetComponent("MainScene") as MainScene;
		main.myAllJoyn = this;
		
		title = titleUI.GetComponent("UiServer") as UiServer;
		title.connect(false);
		
		net = AjNet.GetInstance();
		net.manager = this;
				
		if(net.StartServer()){
			title.ready(true);
		}else{
			title.ready(false);
		}
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
	}

	void OnGUI() {
	}
	
	void AjNet.NetManager.SessionMemberAdded(string memberId) {	
		Debug.Log ("SessionMemberAdded " + memberId);
		if (net.status == AjNet.Status.Server){
			title.connect(true);
		}
	}
	
	void AjNet.NetManager.SessionMemberRemoved(string memberId) {		
		Debug.Log ("SessionMemberRemoved " + memberId);
		if (net.status == AjNet.Status.Server){
			title.connect(false);
		}

	}
	
	void AjNet.NetManager.AccelReceived(string memberId, string acc) {
		if(acc != null){
			string[] strArr = acc.Split(';');
			Vector3 vec = new Vector3(float.Parse(strArr[0]),
									  float.Parse(strArr[1]),
									  float.Parse(strArr[2]));
			Debug.Log("TestAllJoyn receiver Vec : " + vec);
			main.setAccleration(vec);
		}
			
	}
	
	void AjNet.NetManager.DataReceived(string memberId, string type, string data) {

	}
	
	void AjNet.NetManager.SessionLost() {
		userInfo = new ArrayList();
	}
	void AjNet.NetManager.connectFail() {
		
	}
	int removeUser(string _name){
		int tCnt = -1;
		for(int i = 0;i < userInfo.Count;i++){
			PlayerInfo pInfo = (PlayerInfo)userInfo[i];
			if(pInfo.name.Equals(_name)){
				tCnt = i;
				break;
			}
		}
		
		userInfo.RemoveAt(tCnt);		
		if(userInfo.Count > 1){
			Debug.Log ("error : 2 over connected ");
		}
		return tCnt;
	}
		
}
