Remote Accelerometer Controller Sample for Android

Author: Lilac, Inc. http://www.lilac.co.kr

A complete Unity project that demonstrates the controlling of a remote android device scene with another android device with accelerometer.
Tested with Unity 4.3.4.

You can modify or reuse this package freely.
AllJoyn is used for communication and is subject to their license terms. https://www.alljoyn.org


Package structure:

Plugins/ - the AllJoyn plugin for Android 
Prefabs/ - prefabs used in scenes
Scenes/ - contains two test scenes
Scripts/
  AjNet - implements server and client. handles network events.
  AllJoynAgent - alljoyn helper class.
  MainScene - uses received accelerometer data on the server scene.
  NetClient - receives touch events and connects to server.
  NetServer - starts server and responds to network events.
  UiClient - displays client status messages.
  UiServer - displays server status messages.


How to build test apps:

Change the platform to Android in Unity Build Settings.

Plugins folder should be placed right below your root Assets folder.
/Assets/Plugins

The project has two scenes - the server scene and the client scene.
Two demo apks can be built with one of the scenes each. They are to be installed on two separate devices. The client apk should be installed on a device with accelerometer.

In Unity Build Settings,
Scenes/Server - include this to build server apk.
Scenes/Client - include this to build client apk.


How to test:

Two devices should be connected to the same WiFi network.
Run the built demo apps on each device.
Touch the client app once.

You can look around the server device scene by tilting the client device.
