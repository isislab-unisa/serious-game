using UnityEngine;
using System.Collections;

public class EnvironmentalMessage : MonoBehaviour
{
	public PrefabInstancer Instancer;
	public string Key;
	public string[] MsgParams;
	public int MsgTime;
	public MessagePriority Priority;
	public bool ShowOnce;
	private MessageStorage MsgStorage;
	private SpriteStorage spriteStorage;
	private string message;
    private Sprite sprite;
	private bool msgShown;

	void Start()
	{
		MsgStorage = Instancer.GetPrefabComponent<MessageStorage>();
		spriteStorage = Instancer.GetPrefabComponent<SpriteStorage>();
		message = string.Format(MsgStorage.GetString(Key), MsgParams);
        sprite = spriteStorage.GetSprite(Key);
	}

	void OnTriggerEnter(Collider collision)
	{
		if(ShowOnce && msgShown)
			return;
		if(collision.gameObject.tag.Equals("Player"))
		{
			GameObject player = collision.gameObject;
			if(player.GetPhotonView().isMine)
			{
				player.GetComponent<OVRShowInfo>().displayMsg(message, MsgTime, (int)Priority, sprite);
				msgShown = true;
			}
		}
	}

	void OnTriggerExit(Collider collision)
	{
		if(ShowOnce && msgShown)
			return;
		if(collision.gameObject.tag.Equals("Player"))
		{
			GameObject player = collision.gameObject;
			if(player.GetPhotonView().isMine)
			{
				player.GetComponent<OVRShowInfo>().cleanmsg();
			}
		}
	}
}
