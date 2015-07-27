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

[AddComponentMenu("Toxic/Movement/Movement Commander")]
[RequireComponent(typeof(CameraController))]
public class MovementCommander : MonoBehaviour
{
	private IMovementController _mc;
	private CameraController _cc;

	private Vector3 _prev_dir = Vector3.zero;
	private bool _prev_jump = false;

	void Start()
	{
		_cc = GetComponent<CameraController>();
	}

	void Update()
	{
		if (Input.GetButtonDown("Possess")) {
			if (_cc.IsPossessing()) {
				_cc.UnPossess();
				_mc = null;

			} else {
				RaycastHit hitInfo = new RaycastHit();

				if (Physics.Raycast(transform.position, transform.forward, out hitInfo)) {
					_mc = hitInfo.transform.gameObject.GetComponent<IMovementController>();

					if (_mc != null) {
						_cc.Possess(hitInfo.transform.gameObject);
					}
				}
			}
		}

		// For some reason this doesn't update immediately. There is some stutter when moving the camera.
		if (_mc != null) {
			bool jump = Input.GetButtonDown("Jump");

			// Don't spam the network with redundant data.
			if (jump != _prev_jump) {
				_mc.Jump(jump);
				_prev_jump = jump;
			}

			if (!_mc.ControlsOrientation()) {
				Vector3 dir = transform.forward;
				dir.y = 0.0f;

				_mc.GetGameObject().transform.LookAt(_mc.GetGameObject().transform.position + dir);
			}
		}
	}

	void FixedUpdate()
	{
		if (_mc != null) {
			Vector3 dir = transform.TransformDirection(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

			// Don't spam the network with redundant data.
			if (dir != _prev_dir) {
				_mc.MoveTowards(dir);
				_prev_dir = dir;
			}
		}
	}
}

}
