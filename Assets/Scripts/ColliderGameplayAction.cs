using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class ColliderActionItem
{
	public string[] TagFilters;
	public UnityEvent[] Conditions;
	public UnityEvent Action;
}

public class ColliderGameplayAction : CollisionManager
{
	public LevelManager LevelMan;
	public ColliderActionItem[] OnTriggerEnterActions;
	public ColliderActionItem[] OnTriggerExitActions;
    public ColliderActionItem[] OnPressedButtonActions;
    public float MessageTime;
    public float DelayTime;
	public MessagePriority MsgPriority;
	private PrefabInstancer Instancer;
	private MessageStorage MsgStorage;
	private GameObject in_Collidee;
	private bool out_Condition;
    
    private Dictionary<String, Color>  colors = new Dictionary< String, Color>();
   
	void Awake()
	{
		Instancer = FindObjectOfType<PrefabInstancer>();
		MsgStorage = Instancer.GetPrefabComponent<MessageStorage>();
        InitializeColor();
	}

    void InitializeColor()
    {
        colors["blue"] = Color.blue;
        colors["red"] = Color.red;
        colors["white"] = Color.white;
    }

	bool IsConditionMet(UnityEvent condition)
	{
		out_Condition = false;
		condition.Invoke();
		return out_Condition;
	}

	void DoActionIfConditionMet(Collider collider, ColliderActionItem action)
	{
		if(action.TagFilters.Length == 0 || action.TagFilters.Count(f => f.Equals(collider.tag)) > 0)
		{
			in_Collidee = collider.gameObject;
			foreach(UnityEvent curCondition in action.Conditions)
			{
				if(!IsConditionMet(curCondition))
					return;
			}
			action.Action.Invoke();
		}
	}

	protected override void OnTriggerEnter(Collider collider)
	{
		base.OnTriggerEnter(collider);
		foreach(ColliderActionItem curAction in OnTriggerEnterActions)
		{
			DoActionIfConditionMet(collider, curAction);
		}
	}

	protected override void OnTriggerExit(Collider collider)
	{
		foreach(ColliderActionItem curAction in OnTriggerExitActions)
		{
			DoActionIfConditionMet(collider, curAction);
		}
		base.OnTriggerExit(collider);
	}

    private void OnButtonPressed(Collider collider)
    {
        foreach (ColliderActionItem curAction in OnPressedButtonActions)
        {
            DoActionIfConditionMet(collider, curAction);
        }
    }

    void Update()
    {
        foreach (GameObject player in collidersInsideArea)
        {
            if (player.GetPhotonView().isMine && player.GetComponent<ActionObjects>().getAction().Equals(Actions.allAction))
            {
                OnButtonPressed(player.GetComponent<Collider>());
            }
        }
    }

#region Conditions
    public void Condition_CollideeHasObject(int objIndex)
	{
		PlayerInventory playerInv = in_Collidee.GetComponent<PlayerInventory>();
		Collectable obj = (Collectable)objIndex;
		out_Condition = playerInv.Objects.Count(o => o.Object == obj && o.State == CollectableState.Owned) > 0;
	}

	public void Condition_CollideeHasntObject(int objIndex)
	{
		PlayerInventory playerInv = in_Collidee.GetComponent<PlayerInventory>();
		Collectable obj = (Collectable)objIndex;
		out_Condition = playerInv.Objects.Count(o => o.Object == obj && o.State == CollectableState.Owned) == 0;
	}

	public void Condition_AllPlayersInsideCollider()
	{
		out_Condition = collidersInsideArea.Count == PhotonNetwork.playerList.Count();
	}

	public void Condition_NotAllPlayersInsideColliderLocal()
	{
		out_Condition = localCollidersInsideArea.Count < PhotonNetwork.playerList.Count();
	}

	public void Condition_IsMasterClient()
	{
		out_Condition = PhotonNetwork.isMasterClient;
	}
#endregion

#region Actions

    public void Action_ChangeColor(String color)
    {
        photonView.RPC("ChangeColorRPC", PhotonTargets.AllBuffered,color);
    }

    [PunRPC]
    void ChangeColorRPC(String color)
    {
        Renderer renderer = GetComponentInParent<Renderer>();
        renderer.material.color= colors[color];
    }

    public void Action_TriggerAnimationBroadcast(string triggerName)
	{
		photonView.RPC("TriggerAnimationRPC", PhotonTargets.AllBuffered, triggerName);
	}

	[PunRPC]
	void TriggerAnimationRPC(string triggerName)
	{
		Animator anim = GetComponent<Animator>();
		anim.SetTrigger(triggerName);
	}

	public void Action_ShowMessage(string key)
	{
		OVRShowInfo showInfo = in_Collidee.GetComponent<OVRShowInfo>();
		string msg = MsgStorage.GetString(key);
		showInfo.displayMsg(msg, MessageTime, (int)MsgPriority,null);
	}

    public void Action_ClearMessage()
    {
        OVRShowInfo showInfo = in_Collidee.GetComponent<OVRShowInfo>();
        showInfo.cleanmsg();
    }

    public void Action_DisableColliderBroadcast()
	{
		photonView.RPC("DisableColliderRPC", PhotonTargets.AllBuffered);
        StartCoroutine(ClearPlayersInsideAreaEndOfFrame());
    }

    IEnumerator ClearPlayersInsideAreaEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        photonView.RPC("ClearPlayersInsideArea", PhotonTargets.AllBuffered);
    }

    [PunRPC]
	void DisableColliderRPC()
	{
		Collider collider = GetComponent<Collider>();
		collider.enabled = false;
	}

    public void Action_EnableAnotherColliderBroadcastWithDelay(GameObject other)
    {
        StartCoroutine(EnableAnotherColliderBroadcastWithDelay(other, DelayTime));
    }

    IEnumerator EnableAnotherColliderBroadcastWithDelay(GameObject other, float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("EnableAnotherColliderBroadcastRPC", PhotonTargets.AllBuffered, other.GetPhotonView().viewID);
    }

    [PunRPC]
    void EnableAnotherColliderBroadcastRPC(int otherID)
    {
        GameObject other = PhotonView.Find(otherID).gameObject;
        other.GetComponent<Collider>().enabled = true;
    }

    public void Action_EnableAnotherGameObjectBroadcast(GameObject other)
    {
        photonView.RPC("EnableAnotherGameObjectBroadcastRPC", PhotonTargets.AllBuffered, other.GetPhotonView().viewID);
    }

    [PunRPC]
    void EnableAnotherGameObjectBroadcastRPC(int otherID)
    {
        GameObject other = PhotonView.Find(otherID).gameObject;
        other.SetActive(true);
    }

    public void Action_EnableAnotherRendererBroadcast(GameObject other)
    {
        photonView.RPC("EnableAnotherRendererBroadcastRPC", PhotonTargets.AllBuffered, other.GetPhotonView().viewID);
    }

    [PunRPC]
    void EnableAnotherRendererBroadcastRPC(int otherID)
    {
        GameObject other = PhotonView.Find(otherID).gameObject;
        other.GetComponent<Renderer>().enabled = true;
    }

    public void Action_DisableAnotherGameObjectBroadcast(GameObject other)
    {
        photonView.RPC("DisableAnotherGameObjectBroadcastRPC", PhotonTargets.AllBuffered, other.GetPhotonView().viewID);
    }

    [PunRPC]
    void DisableAnotherGameObjectBroadcastRPC(int otherID)
    {
        GameObject other = PhotonView.Find(otherID).gameObject;
        other.SetActive(false);
    }

    public void Action_LoadNextLevelBroadcast()
	{
		photonView.RPC("LoadNextLevelRPC", PhotonTargets.AllBuffered);
	}

	[PunRPC]
	void LoadNextLevelRPC()
	{		
		LevelMan.GoToNextLevel();
	}

#endregion

}
