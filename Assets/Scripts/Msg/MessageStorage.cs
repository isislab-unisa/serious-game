using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class LocalizedString
{
	public LanguageType Language;
	public string String;
}

[Serializable]
public class TextItem
{
	public string Key;
	private int KeyHash;
	public LocalizedString[] Items;

	public void Init()
	{
		KeyHash = Key.GetHashCode();
	}

	public bool IsKey(int key)
	{
		return KeyHash == key;
	}
}

public class MessageStorage : MonoBehaviour
{
	public TextItem[] Items;

	void Awake()
	{
		for(int i = 0; i < Items.Length; ++i)
		{
			Items[i].Init();
		}
	}

	public string GetString(string key)
	{
		int keyHash = key.GetHashCode();
		for(int i = 0; i < Items.Length; ++i)
		{
			if(Items[i].IsKey(keyHash))
			{
				LocalizedString thisLanguageString = Items[i].Items.FirstOrDefault(s => s.Language == PersistentScript.Instance.Language);
				if(thisLanguageString != null)
					return thisLanguageString.String;
				LocalizedString defaultLanguageString = Items[i].Items.FirstOrDefault(s => s.Language == LanguageType.Default);
				if(defaultLanguageString != null)
					return defaultLanguageString.String;
				return string.Empty;
			}
		}
		return string.Empty;
	}
}
