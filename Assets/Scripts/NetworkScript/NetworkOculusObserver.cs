using UnityEngine;
using System.Collections;

public class NetworkOculusObserver : Photon.MonoBehaviour
{
	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	private CharacterController characterController;
	private AvatarAnimationController animController;

	void Awake(){
		characterController = GetComponent<CharacterController>();
		animController = GetComponent<AvatarAnimationController>();
	}

	void Update()
	{
		if(!photonView.isMine)
		{
			Vector3 velDir = realPosition - transform.position;
			characterController.Move(velDir);
            transform.rotation = realRotation;
        }

    }
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
            stream.SendNext(animController.CurrentAnimDir);
			stream.SendNext(animController.CurrentSpeed);
		}
		else
		{
			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			animController.CurrentAnimDir = (Vector2)stream.ReceiveNext();
			animController.CurrentSpeed = (float)stream.ReceiveNext();
		}
	}

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!photonView.isMine)
            GetComponent<PlayerObject>().MainCamera.SetActive(false);
    }

}