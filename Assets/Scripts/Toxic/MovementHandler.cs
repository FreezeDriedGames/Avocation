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
		Otherwise, we are in a singleplayer game, just do it locally.
	*/
	[AddComponentMenu("Toxic/Movement/Movement Handler")]
	public class MovementHandler : MonoBehaviour
	{
		private Toxic.NetworkManager _net_mgr = null;

		void Start()
		{
			_net_mgr = Toxic.NetworkManager.FindNetMgrInstance();

			if (!_net_mgr || _net_mgr.isSingleplayer) {
				_net_mgr = null;
			}
		}

	}

}
