using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SimpleCollectableCollisionObject : Photon.MonoBehaviour {

    public Collectable ObjectType;
    public string colliderTag;
    public string CatchMessage;
    public float MessageTime;
    public Sprite sprite;
    int priority = 0;


    public void OnTriggerEnter(Collider collider)
    {
        //simply example of catch object - FIFO logic
        if (collider.tag.Equals(colliderTag))
        {
            //set state into player inventory
            collider.gameObject.GetPhotonView().RPC("SetObjectState", PhotonTargets.AllBuffered, ObjectType, CollectableState.Owned);
            //show object on the head - sims like :)
            collider.gameObject.GetPhotonView().RPC("ShowPlayerObject", PhotonTargets.AllBuffered, ObjectType, true);
            //show message
            collider.gameObject.GetComponent<OVRShowInfo>().displayMsg(CatchMessage, MessageTime, priority, sprite);
            //destroy object
            photonView.RPC("Destroy",PhotonTargets.AllBuffered);
            
        }
    }

    [PunRPC]public void Destroy()
    {
        PhotonNetwork.Destroy(this.photonView);
    }

}