using UnityEngine;
using System.Collections;
using System;
using System.Linq;

[Serializable]
public class CollectableMessageItem
{
	public Collectable ObjectType;
	public string CollectedKey;
	[NonSerialized]
	public string CollectedMessage;
    public Sprite sprite;
}

public class PlayerMessage : Photon.MonoBehaviour
{
	public CollectableMessageItem[] Items;
	public int MsgTime;
	public MessagePriority Priority;
	private PrefabInstancer instancer;
	private MessageStorage msgStorage;
    private SpriteStorage spriteStorage;
	private OVRShowInfo showInfo;

	void Start()
	{
		instancer = FindObjectOfType<PrefabInstancer>();
		msgStorage = instancer.GetPrefabComponent<MessageStorage>();
        spriteStorage = instancer.GetPrefabComponent<SpriteStorage>();
        foreach (CollectableMessageItem curItem in Items)
		{
            curItem.sprite = spriteStorage.GetSprite(curItem.CollectedKey);
            curItem.CollectedMessage = msgStorage.GetString(curItem.CollectedKey);
        }
		showInfo = GetComponent<OVRShowInfo>();
    }

	[PunRPC]
	void ShowCollectedMessage(Collectable objType)
	{
		CollectableMessageItem item = Items.First(i => i.ObjectType == objType);
		showInfo.displayMsg(item.CollectedMessage, MsgTime, (int)Priority,item.sprite);
	}
}
