using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class PlayerTypeInfo
{
    public PlayerType Player;
    public GameObject Point;
    public string PrefabName;
}

public class PlayerInfo : MonoBehaviour
{
    public PlayerTypeInfo[] Points;

    public GameObject GetRespawnPoint(PlayerType player)
    {
        return Points.First(p => p.Player == player).Point;
    }

    public string GetPrefabName(PlayerType player)
    {
        return Points.First(p => p.Player == player).PrefabName;
    }
}
