#undef ARMANDODEBUG

using UnityEngine;
using System.Collections.Generic;

public class Network : MonoBehaviour
{
	PersistentScript persistentScript = PersistentScript.Instance;
	public bool offlineMode = false;
    public PlayerInfo SpawnPoints;
	public int SendRate;
	bool connecting = false;
	public string MasterIP;
	//private RoomInfo[] roomsList;
	float randomRespawnCoordinate=1F;
	
	void Start(){
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Player 0");
		PhotonNetwork.sendRate = SendRate;
		PhotonNetwork.sendRateOnSerialize = SendRate;
		if(persistentScript.GameStarted)
		{
			SpawnMyPlayer();
		}
	}

	void Connect(){
		if (offlineMode) {
			PhotonNetwork.offlineMode=true;
			OnJoinedLobby();
            return;
		}
		PhotonNetwork.ConnectToMaster(MasterIP, 5055, "master", "1.0");
	}

#if ARMANDODEBUG
    public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log ("Player Connected "+ player.name);
		foreach(PhotonPlayer pl in PhotonNetwork.playerList){
			int playerId = 1 + (pl.ID * 1000);
			Debug.Log ("test " + PhotonPlayer.Find(player.ID));
			Debug.Log ("test " + playerId);
			//Debug.Log ("Get pobkect "+PhotonView.Find(pl.ID).gameObject.tag);
		}
	}
	
	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{    
		Debug.Log ("Player Disconnected "+ player.name);
		int playerId = 1 + (player.ID * 1000);
		Debug.Log ("Exit  "+ PhotonView.Find(playerId).gameObject.tag);
		//PhotonView.Find (player.ID).gameObject.SetActive(false);
	}
#endif

    void Update()
    {
        if (!PhotonNetwork.connected && connecting == false)
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {   
                persistentScript.playerType = PlayerType.KinectOculus;
                StartGame();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                persistentScript.playerType = PlayerType.Kinect;
                StartGame();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                persistentScript.playerType = PlayerType.Oculus;
                StartGame();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                persistentScript.playerType = PlayerType.Joypad;
                StartGame();
            }

           
        }
    }
	
	
	void OnGUI()
	{
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

        /*
        this menu cannot be shown with virtual reality enable 
        
        if (!PhotonNetwork.connected && connecting == false)
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            var w = GUILayout.Width(400);
            var h = GUILayout.Height(100);
            var textW = GUILayout.Width(100);
            var textH = GUILayout.Height(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Insert Server IP: ");
            MasterIP = GUILayout.TextField(MasterIP, 15, textW, textH);
            //PhotonNetwork.player.name = GUILayout.TextField(" ",15);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("KINECT-OCULUS", w, h))
            {
                persistentScript.playerType = PlayerType.KinectOculus;
                StartGame();
            }
            if (GUILayout.Button("KINECT", w, h))
            {
                persistentScript.playerType = PlayerType.Kinect;
                StartGame();
            }
            if (GUILayout.Button("OCULUS", w, h))
            {
                persistentScript.playerType = PlayerType.Oculus;
                StartGame();
            }

            if (GUILayout.Button("JOYPAD", w, h))
            {
                persistentScript.playerType = PlayerType.Joypad;
                StartGame();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (PhotonNetwork.connected == true && connecting == false)
            {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }

        }
        */

    }
	

	/*

	void OnReceivedRoomListUpdate()
    {
		roomsList = PhotonNetwork.GetRoomList();
	}*/
	
	private void StartGame(){
		connecting = true;
		Connect();
	}
	
	void OnJoinedLobby()
	{
		PhotonNetwork.JoinRandomRoom();
	}
	
	void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}	
	
	void OnJoinedRoom()
	{
		connecting = false;
		SpawnMyPlayer();
	}
	
	void SpawnMyPlayer()
	{
		float randomXValue = Random.Range (-randomRespawnCoordinate, randomRespawnCoordinate);
		float randomZValue = Random.Range (-randomRespawnCoordinate, randomRespawnCoordinate);
		Vector3 randomCoordinate;
        GameObject playerRespawnPoint = SpawnPoints.GetRespawnPoint(persistentScript.playerType);
        randomXValue += playerRespawnPoint.transform.position.x;
        randomZValue += playerRespawnPoint.transform.position.z;
        randomCoordinate = new Vector3(randomXValue, playerRespawnPoint.transform.position.y, randomZValue);
        PhotonNetwork.Instantiate(SpawnPoints.GetPrefabName(persistentScript.playerType), randomCoordinate, playerRespawnPoint.transform.rotation, 0);
		persistentScript.GameStarted = true;
        GameObject.FindGameObjectWithTag("MainCamera").SetActive(false);
	}


}