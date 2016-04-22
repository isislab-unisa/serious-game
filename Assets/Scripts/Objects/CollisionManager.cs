using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CollisionManager : Photon.MonoBehaviour
{
    public string colliderTag;
	public List<GameObject> collidersInsideArea = new List<GameObject>();
	public List<GameObject> localCollidersInsideArea = new List<GameObject>();

	protected virtual void OnTriggerEnter(Collider collider)
    {
        if (!collider.tag.Equals(colliderTag))
            return;
        if (PhotonNetwork.isMasterClient)
        {
			GameObject player = collider.gameObject;
			photonView.RPC("AddPlayerInsideArea", PhotonTargets.All, player.GetPhotonView().viewID);
        }
		localCollidersInsideArea.Add(collider.gameObject);

	}

    protected virtual void OnTriggerExit(Collider collider)
    {
        GameObject player = collider.gameObject;
        if (collidersInsideArea.Contains(player))
        {
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("RemovePlayerInsideArea", PhotonTargets.All, player.GetPhotonView().viewID);
            }
        }
		if(localCollidersInsideArea.Contains(player))
		{
			localCollidersInsideArea.Remove(player);
		}
    }

    [PunRPC]
    protected virtual void AddPlayerInsideArea(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        if (!collidersInsideArea.Contains(player))
            collidersInsideArea.Add(player);
    }

    [PunRPC]
    protected virtual void RemovePlayerInsideArea(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        collidersInsideArea.Remove(player);
    }

    [PunRPC]
    protected virtual void ClearPlayersInsideArea()
    {
        collidersInsideArea.Clear();
    }
}
