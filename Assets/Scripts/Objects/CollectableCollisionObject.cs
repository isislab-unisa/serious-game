using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CollectableCollisionObject : CollisionManager {
    public enum UnableToCollectType {Able, AlreadyOwned, Obsolete}
    public Collectable ObjectType;

	void Update ()
	{
        if (PhotonNetwork.isMasterClient)
        {
            PlayerInventory winnerPlayer = GetObjectWinner();
            if (winnerPlayer != null)
            {
                winnerPlayer.photonView.RPC("SetObjectState", PhotonTargets.AllBuffered, ObjectType, CollectableState.Owned);
                winnerPlayer.photonView.RPC("ShowPlayerObject", PhotonTargets.AllBuffered, ObjectType, true);
				winnerPlayer.photonView.RPC("ShowCollectedMessage", PhotonTargets.All, ObjectType);
				foreach(GameObject curObject in collidersInsideArea)
                {
                    if(curObject != winnerPlayer.gameObject)
                        curObject.GetPhotonView().RPC("SetObjectState", PhotonTargets.AllBuffered, ObjectType, CollectableState.None);
                }
                photonView.RPC("ClearPlayersInsideArea", PhotonTargets.All);
				PhotonNetwork.Destroy(this.photonView);
			}
		}
    }

    PlayerInventory GetObjectWinner()
    {
        for (int i = 0; i < collidersInsideArea.Count; i++)
        {
            GameObject curPlayer = collidersInsideArea[i];
            PlayerInventory curCoinEnabled = curPlayer.GetComponent<PlayerInventory>();
            if (curCoinEnabled.GetObjectState(ObjectType) == CollectableState.Enabled)
                return curCoinEnabled;
        }
        return null;
    }

    public UnableToCollectType IsPlayerAbleToCollect(PlayerInventory inventory, Collectable obj)
    {
        if (inventory.GetObjectState(obj) == CollectableState.Owned)
            return UnableToCollectType.AlreadyOwned;
        if (obj == Collectable.Quadrante && inventory.GetObjectState(Collectable.Asse) == CollectableState.Owned)
            return UnableToCollectType.Obsolete;
        return UnableToCollectType.Able;
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);
		if(!collidersInsideArea.Contains(collider.gameObject))
			return;
        PlayerInventory playerCoin = collider.GetComponent<PlayerInventory>();
        UnableToCollectType unableType = IsPlayerAbleToCollect(playerCoin, ObjectType);
        if (unableType == UnableToCollectType.Able)
            playerCoin.photonView.RPC("SetObjectState", PhotonTargets.AllBuffered, ObjectType, CollectableState.Active);
    }

    protected override void OnTriggerExit(Collider collider)
    {
        GameObject player = collider.gameObject;
        if (collidersInsideArea.Contains(player))
        {
            PlayerInventory playerCoin = collider.GetComponent<PlayerInventory>();
            if (playerCoin.GetObjectState(ObjectType) != CollectableState.Owned)
            {
                playerCoin.photonView.RPC("SetObjectState", PhotonTargets.AllBuffered, ObjectType, CollectableState.None);
            }
        }
		base.OnTriggerExit(collider);
	}

}