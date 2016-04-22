using UnityEngine;
using System.Collections;

public class OculusActionController : MonoBehaviour {
	void Start () {
	}

	void Update () {
		if(gameObject.GetPhotonView().isMine)
		{
			GetComponent<ActionObjects>().setAction(Actions.noAction);
			if(OVRInput.Get(OVRInput.Button.One))
			{
				GetComponent<ActionObjects>().setAction(Actions.allAction);
			}
		}
	}
}
