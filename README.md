# SeriousGame
Serious Game multi-player creation 
Follow these steps to configure the environment:
1) Install Unity, version 5.4.0f3
2) Install 3D Studio Max 2015 (for import mesh items)
3) Install Photon Server SDK_v4-0-29-11263
4) Install Oculus Runtime 0.8.0.0  (for Oculus Rift DK2)

Follow these steps to start the game:
1) Start Photon Server (photon control –> Load Balancing(my Cloud) -> Start as application , get the IP address under Game Server IP config -> Current IP);
2) Start Unity and select this project(downloaded on Git).
3) Open a scene, one of two available under /project/scenes/ and edit Network script on the NetworkObject GameObject present in the scene. Set MasterIP value with IP on which Photon Server was started previuosly.  
4) Start the game.
