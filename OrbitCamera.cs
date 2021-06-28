using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {

	[SerializeField, Range(1f, 20f)]
	float distance = 5f;

	[SerializeField, Min(0f)]
	float focusRadius = 1f;

	CharacterController cc;

	Vector3 focusPoint;

	Vector2 orbitAngles = new Vector2(45f, 0f);

	[SerializeField, Range(1f, 360f)]
	float rotationSpeed = 90f;

	[SerializeField, Range(-89f, 89f)]
	float minVerticalAngle = -30f, maxVerticalAngle = 60f;
	
	Quaternion lookRotation;

	void Awake()
	{
		cc = GetComponentInParent<CharacterController>();
		transform.localRotation = Quaternion.Euler(orbitAngles);

	}
	void LateUpdate () {
		//Vector3 focusPoint = new Vector3 (cc.gameObject.transform.position.x, cc.gameObject.transform.position.y + 2f, cc.gameObject.transform.position.z);
		UpdateFocusPoint();
		//Quaternion lookRotation;
		if (ManualRotation()) {
			ConstrainAngles();
			lookRotation = Quaternion.Euler(orbitAngles);
		}
		else {
			lookRotation = transform.localRotation;
		}
		Vector3 lookDirection = lookRotation * Vector3.forward;
		Vector3 lookPosition = focusPoint - lookDirection * distance;
		transform.SetPositionAndRotation(lookPosition, lookRotation);
	}

	void UpdateFocusPoint () {
		Vector3 targetPoint = new Vector3 (cc.gameObject.transform.position.x, cc.gameObject.transform.position.y + 2f, cc.gameObject.transform.position.z);
		if (focusRadius > 0f) {
			float distance = Vector3.Distance(targetPoint, focusPoint);
			if (distance > focusRadius) {
				focusPoint = Vector3.Lerp(
					targetPoint, focusPoint, focusRadius / distance
				);
			}
		}
		else {
			focusPoint = targetPoint;
		}
	}

	bool ManualRotation () {
		Vector2 input = new Vector2(
			Input.GetAxis("Mouse Y")*-1,
			Input.GetAxis("Mouse X")
		);
		const float e = 0.001f;
		if (input.x < e || input.x > e || input.y < e || input.y > e) {
			orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			return true;
		}
		return false;
	}
	void OnValidate () {
		if (maxVerticalAngle < minVerticalAngle) {
			maxVerticalAngle = minVerticalAngle;
		}
	}

	void ConstrainAngles () {
		orbitAngles.x =
			Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (orbitAngles.y < 0f) {
			orbitAngles.y += 360f;
		}
		else if (orbitAngles.y >= 360f) {
			orbitAngles.y -= 360f;
		}
	}

	public Quaternion getLookRotation()
	{
		return lookRotation;
	}

}