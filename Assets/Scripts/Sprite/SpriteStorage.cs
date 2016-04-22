using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class SpriteItem
{
    public string Key;
    private int KeyHash;
    public Sprite sprite;
    
    public void Init()
    {
        KeyHash = Key.GetHashCode();
    }

    public bool IsKey(int key)
    {
        return KeyHash == key;
    }
}

public class SpriteStorage : MonoBehaviour
{
    public SpriteItem[] Items;

    void Awake()
    {
        for (int i = 0; i < Items.Length; ++i)
        {
            Items[i].Init();
        }
    }

    public Sprite GetSprite(string key)
    {
        int keyHash = key.GetHashCode();
        for (int i = 0; i < Items.Length; ++i)
        {
            if (Items[i].IsKey(keyHash))
            {
                Sprite sprite = Items[i].sprite;
                if (sprite != null)
                    return sprite;
                return null;
            }
        }
        return null;
    }
}
