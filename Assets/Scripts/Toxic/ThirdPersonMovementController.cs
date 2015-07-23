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

[AddComponentMenu("Toxic/Movement/Third Person Movement Controller")]
[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonMovementController : MonoBehaviour, IMovementController
{
	public Vector3 gravityDirection = Vector3.down;
	public float gravity = 10.0f;

	public float angularSpeed = 500.0f;
	public float speed = 10.0f;

	public float snapAngle = 10.0f;

	public bool moveDirSeparateFromAngle = false;

	private Rigidbody _rb;

	public void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_rb.maxAngularVelocity = float.PositiveInfinity;
	}

	public void FixedUpdate()
	{

		// We apply gravity manually for more tuning control
		_rb.AddForce(gravityDirection * gravity * _rb.mass);
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}
	
	public bool ControlsOrientation()
	{
		return true;
	}
	
	// Dir is world space direction.
	public void MoveTowards(Vector3 dir)
	{
		// We are not holding a button, so stop rotating.
		if (dir == Vector3.zero) {
			_rb.angularVelocity = dir;
			_rb.velocity = dir;
			return;
		}

		dir.y = 0.0f;
		dir.Normalize();

		Vector3 forward = transform.forward;
		Vector3 right = transform.right;

		forward.y = right.y = 0.0f;
		forward.Normalize();
		right.Normalize();

		float angle = Vector3.Angle(forward, dir);
		float direction = (Vector3.Dot(right, dir) > 0.0f) ? 1.0f : -1.0f;

		if (angle < snapAngle) {
			// If I use Mathf.Deg2Rad here, I get some stuttering, even though Vector3.Angle() returns degrees. :/
			_rb.angularVelocity = new Vector3(0.0f, angle * direction, 0.0f);
		} else {
			_rb.angularVelocity = new Vector3(0.0f, angularSpeed * direction * Mathf.Deg2Rad, 0.0f);
		}

		if (moveDirSeparateFromAngle) {
			_rb.velocity = dir * speed;
		} else {
			_rb.velocity = transform.forward * speed;
		}
	}

	public void Jump()
	{

	}
}

}
