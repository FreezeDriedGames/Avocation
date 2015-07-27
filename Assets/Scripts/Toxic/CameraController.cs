/************************************************************************************
Copyright (C) 2015 by Nicholas LaCroix

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
************************************************************************************/

using UnityEngine;
using System.Collections;

namespace Toxic
{

[AddComponentMenu("Toxic/Camera/Camera Controller")]
public class CameraController : MonoBehaviour
{
	public bool freeLookWhenDetached = true;
	public float freeLookSpeed = 5.0f;

	public bool invertY = false;
	
	public Vector2 sensitivity = new Vector2(10.0f, 10.0f);
	
	// Values are in the range [-360, 360].
	public Vector2 min = new Vector2(-85.0f, -360.0f);
	public Vector2 max = new Vector2(85.0f, 360.0f);

	public float attachOffset = 0.0f;
	public GameObject gameObj; // optional if using free look
	public GameObject attachPoint; // optional

	public void Start()
	{
		// Euler angles only go from [0, 360], so have to convert
		min.x = WrapAround(min.x);
		min.y = WrapAround(min.y);
		max.x = WrapAround(max.x);
		max.y = WrapAround(max.y);

		if (!invertY) {
			sensitivity.y = -sensitivity.y;
		}
	}

	public void Update()
	{
		Vector3 angles = transform.eulerAngles;
		angles.x += Input.GetAxis("Mouse Y") * sensitivity.y * Time.timeScale;
		angles.y += Input.GetAxis("Mouse X") * sensitivity.x * Time.timeScale;

		angles.x = WrapAround(angles.x);
		angles.y = WrapAround(angles.y);

		angles.x = ClampAngle(angles.x, min.x, max.x);
		angles.y = ClampAngle(angles.y, min.y, max.y);

		transform.eulerAngles = angles;

		if (gameObj) {
			Transform attach_point = (attachPoint == null) ? gameObj.transform : attachPoint.transform;
			transform.position = attach_point.position - (transform.forward * attachOffset);

		} else if (freeLookWhenDetached) {
			Vector3 dir = transform.TransformDirection(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

			transform.position += dir * freeLookSpeed * Time.deltaTime;
		}
	}

	public void FixedUpdate()
	{
		// Rotate gameObj towards where we're looking

//		Vector3 dir = gameObj.transform.TransformDirection(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
//		dir.Normalize();
//
//		qmc.MoveTowards(dir);
	}


	public bool IsPossessing()
	{
		return gameObj != null;
	}

	public void Possess(GameObject game_obj, GameObject attach_point = null)
	{
		attachPoint = attach_point;
		gameObj = game_obj;

		Transform ap = (attachPoint == null) ? gameObj.transform : attachPoint.transform;
		transform.position = ap.position - (transform.forward * attachOffset);
		transform.LookAt(ap.position);
	}

	public void UnPossess()
	{
		gameObj = null;
		attachPoint = null;
	}

	private float ClampAngle(float angle, float min, float max)
	{
		// Account for ranges that must wrap around from zero.
		if (min > max) {
			if (angle > max && angle < min) {
				if ((angle - max) < (min - angle)) {
					return max;
				} else {
					return min;
				}
			}

		} else {
			return Mathf.Clamp(angle, min, max);
		}

		return angle;
	}

	private float WrapAround(float angle)
	{
		return (angle < 0.0f) ? 360.0f + angle : angle;
	}
}

}
