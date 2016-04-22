/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OculusPlayerController : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;

	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;

	/// <summary>
	/// If true, each OVRPlayerController will use the player's physical height.
	/// </summary>
	public bool useProfileData = true;

	public OVRInput.Button runButton = OVRInput.Button.One;
	public OVRInput.Button alternateRunButton = OVRInput.Button.PrimaryThumbstick;

	public float axisDeadZone = 0.1f;
	public bool rotationSnap = false;
	public float eyeHeight;

	protected CharacterController Controller = null;
	protected Transform CameraRig = null;
	protected Transform CenterEye = null;
	protected Transform TrackingSpace = null;

	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	private float InitialYRotation = 0.0f;
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool SkipMouseRotation = true;
	private bool HaltUpdateMovement = false;
	private float SimulationRate = 60f;
	private float PendingRotation = 0;
	private float rotationAnimation = 0;
	private bool animating;
	private float targetYaw = 0;

	void Start()
	{
		// Add eye-depth as a camera offset from the player controller
		var p = CameraRig.transform.localPosition;
		p.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = p;
		OVRPlugin.eyeHeight = eyeHeight;
	}

	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();

		if(Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		CameraRig = transform.Find("OVRCameraRig");
		CenterEye = transform.GetComponentInChildren<Camera>().transform;
		TrackingSpace = CameraRig.Find("TrackingSpace");

		InitialYRotation = transform.rotation.eulerAngles.y;
	}

	protected virtual void FixedUpdate()
	{
		if(useProfileData)
		{
			if(InitialPose == null)
			{
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			p.y = OVRManager.profile.eyeHeight - 0.5f * Controller.height
				+ Controller.center.y;
			p.z = OVRManager.profile.eyeDepth;
			CameraRig.transform.localPosition = p;
		}
		else if(InitialPose != null)
		{
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		UpdateMovement();
		UpdateRotation();

		Vector3 moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.fixedDeltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.fixedDeltaTime;

		// Gravity
		if(Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.fixedDeltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.fixedDeltaTime;

		// Offset correction for uneven ground
		float bumpUpOffset = 0.0f;

		if(Controller.isGrounded && MoveThrottle.y <= 0.001f)
		{
			bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);

		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if(predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.fixedDeltaTime);
	}

	float AngleDifference(float a, float b)
	{
		float diff = (360 + a - b) % 360;
		if(diff > 180)
			diff -= 360;
		return diff;
	}

	public virtual void UpdateMovement()
	{
		bool HaltUpdateMovement = false;
		GetHaltUpdateMovement(ref HaltUpdateMovement);
		if(HaltUpdateMovement)
			return;

		float MoveScaleMultiplier = 1;
		GetMoveScaleMultiplier(ref MoveScaleMultiplier);

		float RotationScaleMultiplier = 1;
		GetRotationScaleMultiplier(ref RotationScaleMultiplier);

		bool SkipMouseRotation = false;
		GetSkipMouseRotation(ref SkipMouseRotation);

		float MoveScale = 1.0f;
		// No positional movement if we are in the air
		if(!Controller.isGrounded)
			MoveScale = 0.0f;

		MoveScale *= SimulationRate * Time.fixedDeltaTime;



		Quaternion playerDirection = ((HmdRotatesY) ? CenterEye.rotation : transform.rotation);
		//remove any pitch + yaw components
		playerDirection = Quaternion.Euler(0, playerDirection.eulerAngles.y, 0);

		Vector3 euler = transform.rotation.eulerAngles;

		bool stepLeft = false;
		bool stepRight = false;
		stepLeft = OVRInput.GetDown(OVRInput.Button.PrimaryShoulder) || Input.GetKeyDown(KeyCode.Q);
		stepRight = OVRInput.GetDown(OVRInput.Button.SecondaryShoulder) || Input.GetKeyDown(KeyCode.E);


		float rotateInfluence = SimulationRate * Time.fixedDeltaTime * RotationAmount * RotationScaleMultiplier;

#if !UNITY_ANDROID
		if(!SkipMouseRotation)
		{
			PendingRotation += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
		}
#endif
		float rightAxisX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
		if(Mathf.Abs(rightAxisX) < axisDeadZone)
			rightAxisX = 0;

		PendingRotation += rightAxisX * rotateInfluence;


		if(rotationSnap)
		{
			if(Mathf.Abs(PendingRotation) > RotationRatchet)
			{
				if(PendingRotation > 0)
					stepRight = true;
				else
					stepLeft = true;
				PendingRotation -= Mathf.Sign(PendingRotation) * RotationRatchet;
			}
		}
		else
		{
			euler.y += PendingRotation;
			PendingRotation = 0;
		}



		if(rotationAnimation > 0 && animating)
		{
			float speed = Mathf.Max(rotationAnimation, 3);

			float diff = AngleDifference(targetYaw, euler.y);
			// float done = AngleDifference(euler.y, animationStartAngle);

			euler.y += Mathf.Sign(diff) * speed * Time.fixedDeltaTime;

			if((AngleDifference(targetYaw, euler.y) < 0) != (diff < 0))
			{
				animating = false;
				euler.y = targetYaw;
			}
		}
		if(stepLeft ^ stepRight)
		{
			float change = stepRight ? RotationRatchet : -RotationRatchet;

			if(rotationAnimation > 0)
			{
				targetYaw = (euler.y + change) % 360;
				animating = true;
				// animationStartAngle = euler.y;
			}
			else
			{
				euler.y += change;
			}
		}

		float moveInfluence = SimulationRate * Time.fixedDeltaTime * Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

		// Run!
		if(OVRInput.Get(runButton) || OVRInput.Get(alternateRunButton) || Input.GetKey(KeyCode.LeftShift))
			moveInfluence *= 2.0f;


		float leftAxisX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
		float leftAxisY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

		if(leftAxisX * leftAxisX + leftAxisY * leftAxisY < axisDeadZone * axisDeadZone)
		{
			leftAxisX = 0;
			leftAxisY = 0;
		}


		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			leftAxisY = 1;
		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			leftAxisX = -1;
		if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			leftAxisX = 1;
		if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			leftAxisY = -1;

		if(leftAxisY > 0.0f)
			MoveThrottle += leftAxisY
			* (playerDirection * (Vector3.forward * moveInfluence));

		if(leftAxisY < 0.0f)
			MoveThrottle += Mathf.Abs(leftAxisY)
			* (playerDirection * (Vector3.back * moveInfluence));

		if(leftAxisX < 0.0f)
			MoveThrottle += Mathf.Abs(leftAxisX)
			* (playerDirection * (Vector3.left * moveInfluence));

		if(leftAxisX > 0.0f)
			MoveThrottle += leftAxisX
			* (playerDirection * (Vector3.right * moveInfluence));

		transform.rotation = Quaternion.Euler(euler);
	}

	public void UpdateRotation()
	{
		if(HmdRotatesY)
		{
			Vector3 prevPos = TrackingSpace.position;
			Quaternion prevRot = TrackingSpace.rotation;

			transform.rotation = Quaternion.Euler(0.0f, CenterEye.rotation.eulerAngles.y, 0.0f);
			TrackingSpace.position = prevPos;
			TrackingSpace.rotation = prevRot;
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if(!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if(HmdResetsY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}

	public void SetRotationSnap(bool value)
	{
		rotationSnap = value;
		PendingRotation = 0;
	}

	public void SetRotationAnimation(float value)
	{
		rotationAnimation = value;
		PendingRotation = 0;
	}

	void Reset()
	{
		// Prefer to not reset Y when HMD position reset
		HmdResetsY = false;
	}
}

