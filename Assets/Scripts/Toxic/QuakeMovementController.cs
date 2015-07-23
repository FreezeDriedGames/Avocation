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

/************************************************************************************
TODO: Implement proper land on ground detection.
TODO: Implement slope handling.
************************************************************************************/

using UnityEngine;
using System.Collections;

namespace Toxic
{

[AddComponentMenu("Toxic/Movement/Quake Movement Controller")]
[RequireComponent(typeof(Rigidbody))]
public class QuakeMovementController : MonoBehaviour, IMovementController
{
	public Vector3 gravityDirection = Vector3.down;
	public float acceleration = 12.0f;
	public float friction = 6.0f;
	public float speed = 10.0f;
	public float stopSpeed = 100.0f;

	public float airAcceleration = 4.0f; 
	public float airDeceleration = 1.35f;
	public float airSpeed = 4.0f;

	public float jumpSpeed = 4.0f;
	public float gravity = 10.0f;

	private bool _grounded = false;

	private Rigidbody _rb;

	public void Start()
	{
		_rb = GetComponent<Rigidbody>();
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
		return false;
	}

	public void MoveTowards(Vector3 dir)
	{
		dir.y = 0.0f;
		dir.Normalize();

		// Grounded movement
		if (_grounded) {
			Accelerate(dir, speed, acceleration);
			Friction(stopSpeed, friction, _grounded);

		// Air movement
		} else {
			float s = airSpeed;

			if (Vector3.Dot(dir, _rb.velocity) < 0.0f) {
				s *= airDeceleration;
			}

			Accelerate(dir, s, airAcceleration);
		}
	}

	public void Jump()
	{
		if (_grounded) {
			_rb.AddForce(-gravityDirection * jumpSpeed, ForceMode.VelocityChange);
			_grounded = false;
		}
	}

	// Does not actually determine if you are on the ground. Need some more work to figure this out.
	// Also need to port over code for dealing with velocities on slopes and stuff.
	public void OnCollisionEnter(Collision collision)
	{
		_grounded = true;
	}

	private void Accelerate(Vector3 dir, float speed, float acceleration)
	{
		float add_speed = speed - Vector3.Dot(_rb.velocity, dir);

		if (add_speed <= 0.0f) {
			return;
		}

		float accel_speed = (acceleration * Time.fixedDeltaTime) * speed;
		
		if (accel_speed > add_speed) {
			accel_speed = add_speed;
		}

		_rb.AddForce(accel_speed * dir, ForceMode.VelocityChange);
	}

	private void Friction(float stop_speed, float friction, bool grounded)
	{
		Vector3 vel = _rb.velocity;
		float speed = vel.magnitude;

		if (grounded) {
			float temp = vel.y;
			vel.y = 0.0f;

			speed = vel.magnitude;
			vel.y = temp;
		}
		
		if (speed < 1) {
			vel.x = 0.0f;
			vel.z = 0.0f;
			_rb.velocity = vel;
			return;
		}

		float drop = 0.0f;
		float control = (speed < stop_speed) ? stop_speed : speed;

		drop += control * friction * Time.fixedDeltaTime;

		float new_speed = Mathf.Max(speed - drop, 0.0f); // Don't go below zero.
		new_speed /= speed;

		_rb.velocity = vel * new_speed;
	}
}

}
