using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class CollectedObject
{
    public Collectable Object;
    public CollectableState State;
    public GameObject CollectableGameObject;

    public CollectedObject()
    {
        Object = Collectable.Asse;
        State = CollectableState.None;
    }
}

public class PlayerInventory : Photon.MonoBehaviour {
    public List<CollectedObject> Objects;
    private ActionObjects actionObjects;

    void Start ()
    {
        actionObjects = GetComponent<ActionObjects>();
    }
	
	void Update ()
	{
        CheckForObjectsToEnable();
    }

    void CheckForObjectsToEnable()
    {
        foreach(CollectedObject obj in Objects)
        {
            if (obj.State == CollectableState.Active && actionObjects.playerAction == Actions.allAction)
            {
                photonView.RPC("SetObjectState", PhotonTargets.AllBuffered, obj.Object, CollectableState.Enabled);
                return;
            }
        }
    }

    [PunRPC]
    public void SetObjectState(Collectable obj, CollectableState state)
    {
        CollectedObject collObj = Objects.FirstOrDefault(o => o.Object == obj);
        collObj.State = state;
    }

    public CollectableState GetObjectState(Collectable obj)
    {
        CollectedObject collObj = Objects.FirstOrDefault(o => o.Object == obj);
        return collObj.State;
    }

    [PunRPC]
    void ShowPlayerObject(Collectable obj, bool active)
    {
        CollectedObject collected = Objects.FirstOrDefault(o => o.Object == obj);
        if (collected != null && collected.CollectableGameObject != null)
            collected.CollectableGameObject.SetActive(active);
    }
}
 