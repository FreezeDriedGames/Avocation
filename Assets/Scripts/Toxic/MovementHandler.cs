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
	/*
		The intention is to have a mediator for issuing movement commands.
		Basically, if we're connected to a server, issue command to server.
		Otherwise, we are in a singleplayer game or are the server, do it locally.
	*/
	[AddComponentMenu("Toxic/Movement/Movement Handler")]
	public class MovementHandler : MonoBehaviour
	{
		public delegate void MoveTowardsCallback(Vector3 dir);
		public delegate void JumpCallback(bool jump);
		public delegate void FixedUpdateCallback();

		private Toxic.NetworkManager _net_mgr = null;
		private MovementControllerBase _mc = null;

		public FixedUpdateCallback fixedUpdate;
		public MoveTowardsCallback moveTowards;
		public JumpCallback jumpCB;

		void Start()
		{
			_net_mgr = Toxic.NetworkManager.FindNetMgrInstance();
			_mc = GetComponent<MovementControllerBase>();

			// Update locally
			if (!_net_mgr || _net_mgr.isSingleplayer || _net_mgr.isServer) {
				fixedUpdate += LocalFixedUpdate;
				moveTowards += LocalMoveTowards;
				jumpCB += LocalJump;

				_net_mgr = null;

			// We are a client, so the server will update our position
			} else {
				// If there are no callbacks, add empty ones so we don't throw exceptions.
				if (fixedUpdate == null) {
					fixedUpdate += () => {};
				}

				if (moveTowards == null) {
					moveTowards += (Vector3 dir) => {};
				}

				if (jumpCB == null) {
					jumpCB += (bool jump) => {};
				}
			}
		}

		void FixedUpdate()
		{
			fixedUpdate();
		}

		public bool ControlsOrientation()
		{
			return _mc.ControlsOrientation();
		}

		// Dir is world space direction.
		public void MoveTowards(Vector3 dir)
		{
			moveTowards(dir);
		}

		public void Jump(bool jump)
		{
			jumpCB(jump);
		}

		private void LocalFixedUpdate()
		{
			_mc.FixedUpdateImpl();
		}

		private void LocalMoveTowards(Vector3 dir)
		{
			_mc.MoveTowards(dir);
		}

		private void LocalJump(bool jump)
		{
			_mc.Jump(jump);
		}
	}

}
