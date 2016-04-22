using UnityEngine;
using System;

public class LevelManager : Photon.MonoBehaviour
{
	public KeyCode GoNextLevelButton;
	public KeyCode GoPrevLevelButton;
	public KeyCode ResetLevelButton;

	void Start()
	{
		PhotonNetwork.automaticallySyncScene = true;
	}

	public void Update()
    {
        if (Input.GetKeyDown(GoNextLevelButton))
        {
            GoToNextLevel();
        }
        else if (Input.GetKeyDown(GoPrevLevelButton))
        {
            GoToPreviousLevel();
        }
        else if (Input.GetKeyDown(ResetLevelButton))
        {
            ResetCurrentLevel();
        }

    }

	void LoadLevel(int index)
	{
		if(PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.SetSendingEnabled(0, false);
			PhotonNetwork.isMessageQueueRunning = false;
			PhotonNetwork.LoadLevel(index);
			PhotonNetwork.SetSendingEnabled(0, true);
			PhotonNetwork.isMessageQueueRunning = true;
		}
	}
	
    public void GoToNextLevel()
    {
		LoadLevel(LevelUp);
    }

	public void GoToPreviousLevel()
    {
		LoadLevel(LevelDown);
	}

	public void GoToStartLevel()
    {
		LoadLevel(InitialLevel);
	}

	public void ResetCurrentLevel()
    {
		LoadLevel(Application.loadedLevel);
	}

    public static int LevelUp
    {
        get { return Application.loadedLevel+1; }
    }

    public static int LevelDown
    {
        get { return Application.loadedLevel-1; }
    }

    public static int InitialLevel
    {
        get { return 0; }
    }
    
}