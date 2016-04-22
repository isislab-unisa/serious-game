using UnityEngine;
using System.Collections;

public class PrefabInstancer : MonoBehaviour
{
	public Transform[] Prefabs;
	private GameObject[] Instanced;

	void Awake()
	{
		if(Instanced == null)
			Init();
	}

	void Init()
	{
		Instanced = new GameObject[Prefabs.Length];
		for(int i = 0; i < Prefabs.Length; ++i)
		{
			Instanced[i] = Instantiate(Prefabs[i]).gameObject;
		}
	}

	public T GetPrefabComponent<T>() where T : Component
	{
		if(Instanced == null)
			Init();
		for(int i = 0; i < Instanced.Length; ++i)
		{
			T curInstance = Instanced[i].GetComponent<T>();
			if(curInstance != null)
				return curInstance;
		}
		return null;
	}
	
}
