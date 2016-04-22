using UnityEngine;
using System.Collections;

public class CoinRotation : MonoBehaviour
{
    public float rotationRate;
	
	void Update ()
    {
        float rotationThisFrame = rotationRate * Time.deltaTime;
        float yRot = transform.localEulerAngles.y;
        yRot += rotationThisFrame;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);
	}
}
