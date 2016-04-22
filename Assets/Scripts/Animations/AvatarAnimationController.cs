using UnityEngine;
using System.Collections;

public class AvatarAnimationController : Photon.MonoBehaviour
{
	public Animator animator;
	public float maxVelocity;
	public float DiagonalDeadZoneCos;
	public float MaxDeltaDir;
	private Vector2 lastDir;
	private Vector2 curDir;
	private float curSpeed;
	private Vector3 lastPosition;

	public Vector2 CurrentAnimDir
	{
		get
		{
			return curDir;
		}

		set
		{
			if(!photonView.isMine)
			{
				curDir = value;
			}
		}
	}

	public float CurrentSpeed
	{
		get
		{
			return curSpeed;
		}

		set
		{
			if(!photonView.isMine)
			{
				curSpeed = value;
			}
		}
	}

	void Awake()
	{
		lastPosition = transform.position;
	}

	void FixedUpdate()
	{
		if(photonView.isMine || !(PhotonNetwork.inRoom))
		{
			Vector3 localVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
			localVelocity = Quaternion.Inverse(transform.rotation) * localVelocity;
			localVelocity.y = 0;
			lastPosition = transform.position;

            float leftAxisX = Input.GetAxis("Horizontal");
            float leftAxisY = Input.GetAxis("Vertical");

            float mag = localVelocity.magnitude;
            curSpeed = mag / maxVelocity;
            curDir.x = leftAxisX * curSpeed;
            curDir.y = leftAxisY * curSpeed;
            if (curDir.y < -DiagonalDeadZoneCos)
			{
				if(Mathf.Abs(curDir.x) > Mathf.Abs(curDir.y))
					curDir.y = Mathf.Clamp(curDir.y, -DiagonalDeadZoneCos, DiagonalDeadZoneCos);
				else
					curDir.x = Mathf.Clamp(curDir.x, -DiagonalDeadZoneCos, DiagonalDeadZoneCos);
			}
			Vector2 deltaDir = curDir - lastDir;
			float deltaMag = deltaDir.magnitude;
			if(deltaMag > MaxDeltaDir)
			{
				float scale = MaxDeltaDir / deltaMag;
				deltaDir.x *= scale;
				deltaDir.y *= scale;
				curDir = lastDir + deltaDir;
			}
			lastDir = curDir;
		}
	}

	void Update()
	{
		animator.SetFloat("VelX", curDir.x);
		animator.SetFloat("VelY", curDir.y);
		animator.SetFloat("Speed", curSpeed);
	}
}
