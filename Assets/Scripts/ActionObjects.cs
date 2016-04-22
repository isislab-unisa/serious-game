using UnityEngine;
using System.Collections;

public class ActionObjects : Photon.MonoBehaviour {

	public Actions playerAction=Actions.noAction;

	public void setAction(Actions actionNumber){
		playerAction = actionNumber;
	}

	public Actions getAction(){
		return playerAction;
	}

}