using UnityEngine;
using AllJoynUnity;
using System.Collections;
using System.Collections.Generic;

public class AjNet {
	public interface NetManager {
		void SessionMemberAdded(string memberId);
		void SessionMemberRemoved(string memberId);
		void AccelReceived(string memberId, string accelation);
		void SessionLost();
		void connectFail();
		void DataReceived(string memberId, string type, string data);
	}
	
	private const string INTERFACE_NAME = "com.lilac.gyrotest";
	private const string SERVICE_NAME = "com.lilac.gyrotest";
	private const string SERVICE_PATH = "/remote";
	private const ushort SERVICE_PORT = 9232;
	private static readonly string[] connectArgs = {"null:"};
	private AllJoyn.BusAttachment msgBus;
	private MyBusListener busListener;
	private MySessionPortListener sessionPortListener;
	private TestBusObject testObj;
	private AllJoyn.InterfaceDescription testIntf;
	public static string serverText;
	public AllJoyn.SessionOpts opts;
	private bool sJoinComplete = false;

	private string sFoundName = null;
	private bool sJoinCalled = false;

	public uint theSessionId = 0;
	public enum Status { Idle, Server, Client };
	public Status status = Status.Idle;
	public static AjNet ajNet;
	MySessionListener sessionListener = new MySessionListener();
	
	public readonly HashSet<string> sessionMembers = new HashSet<string>();
	public NetManager manager;
	
	public static AjNet GetInstance() {
		if (ajNet == null) {
			ajNet = new AjNet();
		}
		
		return ajNet;
	}
	
	class TestBusObject : AllJoyn.BusObject
	{
		public TestBusObject (AllJoyn.BusAttachment bus, string path) : base(path, false)
		{
			
			AllJoyn.InterfaceDescription exampleIntf = bus.GetInterface (INTERFACE_NAME);
			AllJoyn.QStatus status = AddInterface (exampleIntf);
			if (!status) {
				serverText += "Server Failed to add interface " + status.ToString () + "\n";
				Debug.Log ("Server Failed to add interface " + status.ToString ());
			}
			
			AllJoyn.InterfaceDescription.Member catMember = exampleIntf.GetMember ("acc");
			status = AddMethodHandler (catMember, this.Acc);
			if (!status) {
				serverText += "Server Failed to add method handler " + status.ToString () + "\n";
				Debug.Log ("Server Failed to add method handler " + status.ToString ());
			}
			
			catMember = exampleIntf.GetMember ("data");
			status = AddMethodHandler (catMember, this.Data);
			if (!status) {
				serverText += "Server Failed to add method handler " + status.ToString () + "\n";
				Debug.Log ("Server Failed to add method handler " + status.ToString ());
			}
			
		}
		
		protected override void OnObjectRegistered ()
		{
			
			serverText += "Server ObjectRegistered has been called\n";
			Debug.Log ("Server ObjectRegistered has been called");
		}
		
		protected void Cat (AllJoyn.InterfaceDescription.Member member, AllJoyn.Message message)
		{
			
			string outStr = (string)message [0] + (string)message [1];
			AllJoyn.MsgArg outArgs = new AllJoyn.MsgArg();
			outArgs = outStr;
			
			AllJoyn.QStatus status = MethodReply (message, outArgs);
			if (!status) {
				serverText += "Server Ping: Error sending reply\n";
				Debug.Log ("Server Ping: Error sending reply");
			}
		}

		protected void Acc (AllJoyn.InterfaceDescription.Member member, AllJoyn.Message message)
		{
//			string in1 = message[0];
//			serverText += string.Format ("Acc {0} {1}\n", message.Sender, in1);
//			Debug.Log (string.Format ("Acc {0} {1}", message.Sender, in1));
//			
			if (ajNet.manager != null) {
				//ajNet.manager.ButtonPressed(message.Sender, message[0]);
				ajNet.manager.AccelReceived(message.Sender, message[0]);
			}

			AllJoyn.MsgArg outArgs = new AllJoyn.MsgArg();
			outArgs = (string) "";
			
			AllJoyn.QStatus status = MethodReply (message, outArgs);
			if (!status) {
				serverText += "Server Ping: Error sending reply\n";
				Debug.Log ("Server Ping: Error sending reply");
			}
		}
		
		protected void Data (AllJoyn.InterfaceDescription.Member member, AllJoyn.Message message)
		{
//			string in1 = message[0];
//			serverText += string.Format ("Acc {0} {1}\n", message.Sender, in1);
//			Debug.Log (string.Format ("Acc {0} {1}", message.Sender, in1));
//			
			if (ajNet.manager != null) {
				ajNet.manager.DataReceived(message.Sender, message[0], message[1]);
			}

			AllJoyn.MsgArg outArgs = new AllJoyn.MsgArg();
			outArgs = (string) "";
			
			AllJoyn.QStatus status = MethodReply (message, outArgs);
			if (!status) {
				serverText += "Server Ping: Error sending reply\n";
				Debug.Log ("Server Ping: Error sending reply");
			}
		}
	}
	
	class MyBusListener : AllJoyn.BusListener
	{
		
		protected override void ListenerRegistered (AllJoyn.BusAttachment busAttachment)
		{
			serverText += "Server ListenerRegistered: busAttachment=" + busAttachment + "\n";
			Debug.Log ("Server ListenerRegistered: busAttachment=" + busAttachment);
		}
		
		protected override void NameOwnerChanged (string busName, string previousOwner, string newOwner)
		{
			
			if (string.Compare (SERVICE_NAME, busName) == 0) {
				serverText += "Server NameOwnerChanged: name=" + busName + ", oldOwner=" +
					previousOwner + ", newOwner=" + newOwner + "\n";
				Debug.Log ("Server NameOwnerChanged: name=" + busName + ", oldOwner=" +
				           previousOwner + ", newOwner=" + newOwner);
			}
		}

		protected override void FoundAdvertisedName(string name, AllJoyn.TransportMask transport, string namePrefix)
		{
			serverText += "Client FoundAdvertisedName(name=" + name + ", prefix=" + namePrefix + ")\n";
			Debug.Log("Client FoundAdvertisedName(name=" + name + ", prefix=" + namePrefix + ")");
			if(string.Compare(SERVICE_NAME, name) == 0)
			{
				ajNet.sFoundName = name;
			}
		}
	}
	
	class MySessionPortListener : AllJoyn.SessionPortListener
	{
		protected override bool AcceptSessionJoiner (ushort sessionPort, string joiner, AllJoyn.SessionOpts opts)
		{
			
			if (sessionPort != SERVICE_PORT) {
				serverText += "Server Rejecting join attempt on unexpected session port " + sessionPort + "\n";
				Debug.Log ("Server Rejecting join attempt on unexpected session port " + sessionPort);
				return false;
			}
			serverText += "Server Accepting join session request from " + joiner +
				" (opts.proximity=" + opts.Proximity + ", opts.traffic=" + opts.Traffic +
					", opts.transports=" + opts.Transports + ")\n";
			Debug.Log ("Server Accepting join session request from " + joiner +
			           " (opts.proximity=" + opts.Proximity + ", opts.traffic=" + opts.Traffic +
			           ", opts.transports=" + opts.Transports + ")");
			
			return true;
		}
		
		protected override void SessionJoined (ushort sessionPort, uint sessionId, string joiner)
		{
			Debug.Log ("Session Joined!!!!!! " + joiner);
			serverText += "Session Joined!!!!!! " + joiner + "\n";
			ajNet.theSessionId = sessionId;
			ajNet.sJoinComplete = true;
			ajNet.sessionMembers.Add(joiner);
			if (ajNet.manager != null) {
				ajNet.manager.SessionMemberAdded(joiner);
			}
			
			ajNet.msgBus.SetSessionListener(ajNet.sessionListener, sessionId);
		}
	}
	
	class MySessionListener : AllJoyn.SessionListener {
		protected override void SessionLost (uint sessionId, AllJoyn.SessionListener.SessionLostReason reason) {
			ajNet.sJoinComplete = false;
			if (ajNet.manager != null) {
				ajNet.manager.SessionLost();
			}
			serverText += "SessionLost " + reason + "\n";
		}

		protected override void SessionMemberAdded (uint sessionId, string uniqueName) {
			ajNet.sessionMembers.Add(uniqueName);
			if (ajNet.manager != null) {
				ajNet.manager.SessionMemberAdded(uniqueName);
			}
			serverText += "SessionMemberAdded " + uniqueName + "\n";
		}
		
		protected override void SessionMemberRemoved (uint sessionId, string uniqueName) {
			ajNet.sessionMembers.Remove(uniqueName);
			if (ajNet.manager != null) {
				ajNet.manager.SessionMemberRemoved(uniqueName);
			}
			serverText += "SessionMemberRemoved " + uniqueName + "\n";
		}
		
	}
	
	public bool StartServer ()
	{
		serverText = "";
		// Create message bus
		msgBus = new AllJoyn.BusAttachment ("myApp", true);
		
		// Add org.alljoyn.Bus.method_sample interface
		AllJoyn.QStatus status = msgBus.CreateInterface (INTERFACE_NAME, false, out testIntf);
		if (status) {
			serverText += "Server Interface Created.\n";
			Debug.Log ("Server Interface Created.");
			testIntf.AddMember(AllJoyn.Message.Type.MethodCall, "acc", "s", "s", "in1,out1");
			testIntf.AddMember(AllJoyn.Message.Type.MethodCall, "data", "ss", "s", "in1,in2,out1");
			testIntf.Activate ();
		} else {
			serverText += "Failed to create interface 'org.alljoyn.Bus.method_sample'\n";
			Debug.Log ("Failed to create interface 'org.alljoyn.Bus.method_sample'");
		}
		
		// Create a bus listener
		busListener = new MyBusListener ();
		if (status) {
			
			msgBus.RegisterBusListener (busListener);
			serverText += "Server BusListener Registered.\n";
			Debug.Log ("Server BusListener Registered.");
		}
		
		// Set up bus object
		testObj = new TestBusObject ( msgBus, SERVICE_PATH);
		// Start the msg bus
		if (status) {
			
			status = msgBus.Start ();
			if (status) {
				serverText += "Server BusAttachment started.\n";
				Debug.Log ("Server BusAttachment started.");
				msgBus.RegisterBusObject (testObj);
				
				for (int i = 0; i < connectArgs.Length; ++i) {
					status = msgBus.Connect (connectArgs [i]);
					if (status) {
						serverText += "BusAttchement.Connect(" + connectArgs [i] + ") SUCCEDED.\n";
						Debug.Log ("BusAttchement.Connect(" + connectArgs [i] + ") SUCCEDED.");
						break;
					} else {
						serverText += "BusAttachment.Connect(" + connectArgs [i] + ") failed.\n";
						Debug.Log ("BusAttachment.Connect(" + connectArgs [i] + ") failed.");
					}
				}
				if (!status) {
					serverText += "BusAttachment.Connect failed.\n";
					Debug.Log ("BusAttachment.Connect failed.");
				}
			} else {
				serverText += "Server BusAttachment.Start failed.\n";
				Debug.Log ("Server BusAttachment.Start failed.");
			}
		}
		
		// Request name
		if (status) {
			
			status = msgBus.RequestName (SERVICE_NAME,
			                             AllJoyn.DBus.NameFlags.ReplaceExisting | AllJoyn.DBus.NameFlags.DoNotQueue);
			if (!status) {
				serverText += "Server RequestName(" + SERVICE_NAME + ") failed (status=" + status + ")\n";
				Debug.Log ("Server RequestName(" + SERVICE_NAME + ") failed (status=" + status + ")");
			}
		}
		
		// Create session
		opts = new AllJoyn.SessionOpts (AllJoyn.SessionOpts.TrafficType.Messages, true,
		                                AllJoyn.SessionOpts.ProximityType.Any, AllJoyn.TransportMask.Any);
		if (status) {
			
			ushort sessionPort = SERVICE_PORT;
			sessionPortListener = new MySessionPortListener ();
			status = msgBus.BindSessionPort (ref sessionPort, opts, sessionPortListener);
			if (!status || sessionPort != SERVICE_PORT) {
				serverText += "Server BindSessionPort failed (" + status + ")\n";
				Debug.Log ("Server BindSessionPort failed (" + status + ")");
			}
		}
		
		// Advertise name
		if (status) {
			
			status = msgBus.AdvertiseName (SERVICE_NAME, opts.Transports);
			if (!status) {
				serverText += "Server Failed to advertise name " + SERVICE_NAME + " (" + status + ")\n";
				Debug.Log ("Server Failed to advertise name " + SERVICE_NAME + " (" + status + ")");
			}
		}
		serverText += "Completed BasicService Constructor\n";
		Debug.Log ("Completed BasicService Constructor");

		if (status) {
			this.status = Status.Server;
			return true;
		}

		return false;
	}
	
	public bool KeepRunning {
		get {
			return true;
		}
	}
	
	public bool Connected {
		get {
			return sJoinComplete;
		}
	}

	public bool StartClient()
	{
		serverText = "";
		serverText += "AllJoyn Library version: " + AllJoyn.GetVersion() +"\n";
		serverText += "AllJoyn Library buildInfo: " + AllJoyn.GetBuildInfo() + "\n";
		// Create message bus
		msgBus = new AllJoyn.BusAttachment("myApp", true);
		
		// Add org.alljoyn.Bus.method_sample interface
		AllJoyn.InterfaceDescription testIntf;
		AllJoyn.QStatus status = msgBus.CreateInterface(INTERFACE_NAME, false, out testIntf);
		if(status)
		{
			
			serverText += "Client Interface Created.\n";
			Debug.Log("Client Interface Created.");
			testIntf.AddMember(AllJoyn.Message.Type.MethodCall, "acc", "s", "s", "in1,out1");
			testIntf.AddMember(AllJoyn.Message.Type.MethodCall, "data", "ss", "s", "in1,in2,out1");
			testIntf.Activate();
		}
		else
		{
			serverText += "Client Failed to create interface 'org.alljoyn.Bus.method_sample'\n";
			Debug.Log("Client Failed to create interface 'org.alljoyn.Bus.method_sample'");
		}
		
		// Start the msg bus
		if(status)
		{
			
			status = msgBus.Start();
			if(status)
			{
				serverText += "Client BusAttachment started.\n";
				Debug.Log("Client BusAttachment started.");
			}
			else
			{
				serverText += "Client BusAttachment.Start failed.\n";
				Debug.Log("Client BusAttachment.Start failed.");
			}
		}
		
		// Connect to the bus
		if(status)
		{
			
			for (int i = 0; i < connectArgs.Length; ++i)
			{
				status = msgBus.Connect(connectArgs[i]);
				if (status)
				{
					serverText += "BusAttchement.Connect(" + connectArgs[i] + ") SUCCEDED.\n";
					Debug.Log("BusAttchement.Connect(" + connectArgs[i] + ") SUCCEDED.");
					break;
				}
				else
				{
					serverText += "BusAttachment.Connect(" + connectArgs[i] + ") failed.\n";
					Debug.Log("BusAttachment.Connect(" + connectArgs[i] + ") failed.");
				}
			}
			if(!status)
			{
				serverText += "BusAttachment.Connect failed.\n";
				Debug.Log("BusAttachment.Connect failed.");
			}
		}
		
		// Create a bus listener
		busListener = new MyBusListener();
		
		if(status)
		{
			
			msgBus.RegisterBusListener(busListener);
			serverText += "Client BusListener Registered.\n";
			Debug.Log("Client BusListener Registered.");
		}
		
		// Begin discovery on the well-known name of the service to be called
		status = msgBus.FindAdvertisedName(SERVICE_NAME);
		if(!status)
		{
			
			serverText += "Client org.alljoyn.Bus.FindAdvertisedName failed.\n";
			Debug.Log("Client org.alljoyn.Bus.FindAdvertisedName failed.");
		}

		if (status) {
			this.status = Status.Client;
			return true;
		}

		return false;
	}
	
	public bool Connect() {
		if (sJoinComplete)
			return sJoinComplete;
				
		sJoinCalled = true;
		// We found a remote bus that is advertising basic service's  well-known name so connect to it
		AllJoyn.SessionOpts opts = new AllJoyn.SessionOpts(AllJoyn.SessionOpts.TrafficType.Messages, false,
		                                                   AllJoyn.SessionOpts.ProximityType.Any, AllJoyn.TransportMask.Any);

		uint sessionId;
		
		Debug.Log("kcwon Client JoinSession sFoundName = " + sFoundName);
		Debug.Log("kcwon Client JoinSession SERVICE_PORT = " + SERVICE_PORT);
		
		AllJoyn.QStatus status = msgBus.JoinSession(sFoundName, SERVICE_PORT, sessionListener, out sessionId, opts);
		if(status)
		{
			serverText += "Client JoinSession SUCCESS (Session id=" + sessionId + ")\n";
			Debug.Log("Client JoinSession SUCCESS (Session id=" + sessionId + ")");
			sJoinComplete = true;
			theSessionId = sessionId;
		}
		else
		{
			serverText += "Client JoinSession failed (status=" + status.ToString() + ")\n";
			Debug.Log("Client JoinSession failed (status=" + status.ToString() + ")");
			ajNet.manager.connectFail();
		}
		
		return sJoinComplete;
	}
	
	public void Disconnect() {
		if (msgBus != null) {
			if (sJoinComplete)
				msgBus.LeaveSession(theSessionId);
			msgBus = null;
		}
		sJoinComplete = false;
	}
	
	public bool FoundAdvertisedName
	{
		get
		{
			return sFoundName != null && !sJoinCalled;
		}
	}
	
	public string CallAcc(Vector3 vec)
	{
		using(AllJoyn.ProxyBusObject remoteObj = new AllJoyn.ProxyBusObject(msgBus, SERVICE_NAME, SERVICE_PATH, theSessionId))
		{
			
			AllJoyn.InterfaceDescription alljoynTestIntf = msgBus.GetInterface(INTERFACE_NAME);
			if(alljoynTestIntf == null)
			{
				//throw new Exception("Client Failed to get test interface.");
				return "-1";
			}
			else
			{
				remoteObj.AddInterface(alljoynTestIntf);
				
				AllJoyn.Message reply = new AllJoyn.Message(msgBus);
				//AllJoyn.MsgArgs inputs = new AllJoyn.MsgArgs(1);
				AllJoyn.MsgArg inputs = new AllJoyn.MsgArg(1);
				string str = vec.x + ";" + vec.y + ";" + vec.z;
				Debug.Log("kcwon CallAcc send String : " + str);
				inputs[0] = str;

				//AllJoyn.QStatus status = remoteObj.MethodCallSynch(SERVICE_NAME, "btn", inputs, reply, 5000, 0);
				AllJoyn.QStatus status = remoteObj.MethodCall(SERVICE_NAME, "acc", inputs, reply,0, 0);
				
				if(status)
				{
					serverText += SERVICE_NAME + ".cat(path=" + SERVICE_PATH + ") returned \"" + (string)reply[0] + "\"\n";
					Debug.Log(SERVICE_NAME + ".cat(path=" + SERVICE_PATH + ") returned \"" + (string)reply[0] + "\"");
					return (string)reply[0];
				}
				else
				{
					serverText += "MethodCall on " + SERVICE_NAME + ".cat failed\n";
					Debug.Log("MethodCall on " + SERVICE_NAME + ".cat failed");
					return "-2";
				}
			}
		}
	}
	
	
	public string CallData(string type, string data) {
		using(AllJoyn.ProxyBusObject remoteObj = new AllJoyn.ProxyBusObject(msgBus, SERVICE_NAME, SERVICE_PATH, theSessionId))
		{
			
			AllJoyn.InterfaceDescription alljoynTestIntf = msgBus.GetInterface(INTERFACE_NAME);
			if(alljoynTestIntf == null)
			{
				//throw new Exception("Client Failed to get test interface.");
				return "-1";
			}
			else
			{
				remoteObj.AddInterface(alljoynTestIntf);
				
				AllJoyn.Message reply = new AllJoyn.Message(msgBus);
				//AllJoyn.MsgArgs inputs = new AllJoyn.MsgArgs(1);
				AllJoyn.MsgArg inputs = new AllJoyn.MsgArg(2);
				inputs[0] = type;
				inputs[1] = data;

				//AllJoyn.QStatus status = remoteObj.MethodCallSynch(SERVICE_NAME, "btn", inputs, reply, 5000, 0);
				AllJoyn.QStatus status = remoteObj.MethodCall(SERVICE_NAME, "data", inputs, reply,0, 0);
				
				if(status)
				{
					serverText += SERVICE_NAME + ".cat(path=" + SERVICE_PATH + ") returned \"" + (string)reply[0] + "\"\n";
					Debug.Log(SERVICE_NAME + ".cat(path=" + SERVICE_PATH + ") returned \"" + (string)reply[0] + "\"");
					return (string)reply[0];
				}
				else
				{
					serverText += "MethodCall on " + SERVICE_NAME + ".cat failed\n";
					Debug.Log("MethodCall on " + SERVICE_NAME + ".cat failed");
					return "-2";
				}
			}
		}
	}
}
