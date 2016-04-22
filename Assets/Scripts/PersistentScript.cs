using UnityEngine;
using System.Collections;

public class PersistentScript {

	public PlayerType playerType;
	public LanguageType Language;
	public bool GameStarted;
	//public GameState gameState;

    private static PersistentScript instance;

    public static PersistentScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PersistentScript();
            }
            return instance;
        }
    }

}
