using UnityEngine;
using System.Collections;
using System.Linq;
using System;

[Serializable]
public class AbilityMessageItem
{
	public PlayerType Player;
	public CollectableCollisionObject.UnableToCollectType Ability;
	public string Key;
	[NonSerialized]
	public string Message;	
}

public class CollectableMessage : MonoBehaviour
{
	public AbilityMessageItem[] Items;
	public int MsgTime;
	public MessagePriority Priority;
	private PrefabInstancer Instancer;
	private MessageStorage MsgStorage;
	private CollectableCollisionObject collisionObj;

	void Start()
	{
		Instancer = FindObjectOfType<PrefabInstancer>();
		MsgStorage = Instancer.GetPrefabComponent<MessageStorage>();
		foreach(AbilityMessageItem curMessage in Items)
		{
			curMessage.Message = MsgStorage.GetString(curMessage.Key);
		}
		collisionObj = GetComponent<CollectableCollisionObject>();
	}

	void OnTriggerEnter(Collider collision)
	{
		if(collision.gameObject.tag.Equals("Player"))
		{
			GameObject player = collision.gameObject;
			PlayerInventory playerInv = player.GetComponent<PlayerInventory>();
			PlayerObject playerObj = player.GetComponent<PlayerObject>();
			CollectableCollisionObject.UnableToCollectType abilityType = collisionObj.IsPlayerAbleToCollect(playerInv, collisionObj.ObjectType);
			AbilityMessageItem message = Items.FirstOrDefault(m => m.Ability == abilityType && m.Player == playerObj.Player);
			if(message == null)
				message = Items.First(m => m.Ability == abilityType);
			player.GetComponent<OVRShowInfo>().displayMsg(message.Message, MsgTime, (int)Priority,null);
		}
	}

	void OnTriggerExit(Collider collision)
	{
		if(collision.gameObject.tag.Equals("Player"))
		{
			GameObject player = collision.gameObject;
			player.GetComponent<OVRShowInfo>().cleanmsg();
		}
	}

}
