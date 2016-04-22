using UnityEngine;
using System.Collections;
using System;

public class HaloCollider : CollisionManager
{
    //public bool playerCollider=false;
	//public string[] playerType;
	//PhotonView photonView;
	public string objectName= "HorizontalRing";
    private GameObject ringObject;

	// Use this for initialization
	void Start () {
		//photonView = this.GetComponentInParent<PhotonView> ();
		//playerType = Enum.GetNames(typeof(PlayerType));
        ringObject = transform.parent.FindChild(objectName).gameObject;

    }

    void Update()
    {
        if (ringObject.activeInHierarchy && collidersInsideArea.Count == 0)
            ringObject.SetActive(false);
        else if(!ringObject.activeInHierarchy && collidersInsideArea.Count > 0)
            ringObject.SetActive(true);
    }

    /*
    public void OnTriggerEnter(Collider collider){
		foreach (string playerTag in playerType) {
			if (collider.tag.IndexOf (playerTag) != -1){
				playerCollider=true;
				break;
			}
		}

		if (playerCollider) {
		//player collider 
			this.photonView.RPC("changeObjectState", PhotonTargets.AllBuffered,objectName,true);
		}
	}

	public void OnTriggerExit(Collider Collider){
		foreach (string playerTag in playerType) {
			if (GetComponent<Collider>().tag.IndexOf (playerTag) != -1){
				playerCollider=true;
				break;
			}
		}
		
		if (playerCollider) {
			//player collider 
			this.photonView.RPC("changeObjectState", PhotonTargets.AllBuffered,objectName,false);
		}
	}

	[PunRPC] void changeObjectState(string objectName, bool state) {
		//Behaviour haloBehaviour = (Behaviour)this.gameObject.GetComponent("Halo");
		//haloBehaviour.enabled = state;
		this.gameObject.transform.FindChild(objectName).gameObject.SetActive(state);
	}*/

}