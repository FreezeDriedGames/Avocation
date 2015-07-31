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

using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Toxic
{

	[AddComponentMenu("Toxic/Networking/Toxic Network Manager")]
	public class NetworkManager : UnityEngine.Networking.NetworkManager
	{
		public delegate void CallbackDelegate(NetworkConnection conn);
		public delegate void ErrorCallback(NetworkConnection conn, int error_code);
		public delegate void EmptyCallback();

		// Host
		public event EmptyCallback HostStarted = delegate {};

		// Server
		public delegate void ServerAddPlayerCallback(NetworkConnection conn, short player_controller_id);
		public delegate void ServerRemovePlayerCallback(NetworkConnection conn, PlayerController player);
		public delegate void ServerSceneChangedCallback(string scene_name);

		public event EmptyCallback ServerStarted = delegate {};
		public event ServerAddPlayerCallback ServerAddPlayer = delegate {};
		public event CallbackDelegate ServerConnect = delegate {};
		public event CallbackDelegate ServerDisconnect = delegate {};
		public event ErrorCallback ServerError = delegate {};
		public event CallbackDelegate ServerReady = delegate {};
		public event ServerRemovePlayerCallback ServerRemovePlayer = delegate {};
		public event ServerSceneChangedCallback ServerSceneChanged = delegate {};

		// Client
		public delegate void ClientStartCallback(NetworkClient client);

		public event ClientStartCallback ClientStarted = delegate {};
		public event CallbackDelegate ClientConnect = delegate {};
		public event CallbackDelegate ClientDisconnect = delegate {};
		public event ErrorCallback ClientError = delegate {};
		public event CallbackDelegate ClientNotReady = delegate {};
		public event CallbackDelegate ClientSceneChanged = delegate {};

		public List<string> levels;

		public bool isHost
		{
			get { return _is_host; }
		}

		public bool isServer
		{
			get { return _is_server; }
		}

		public bool isClient
		{
			get { return _is_client; }
		}

		public bool isSingleplayer
		{
			get { return _is_singleplayer; }
		}

		private bool _is_host = false;
		private bool _is_server = false;
		private bool _is_client = false;
		private bool _is_singleplayer = true;

		// Load user created asset bundles here and add the scenes to the level list.
		void Start()
		{
		}

		static public NetworkManager FindNetMgrInstance()
		{
			GameObject net_mgr = GameObject.Find("NetworkManager");
		
			if (!net_mgr) {
				Debug.LogError("Could not find global GameObject with name 'NetworkManager'.");
				return null;
			}
		
			NetworkManager nm = net_mgr.GetComponent<Toxic.NetworkManager>();
		
			if (!nm) {
				Debug.LogError("Could not find a 'Toxic.NetworkManager' on global GameObject 'NetworkManager'.");
			}

			return nm;
		}

		override public NetworkClient StartHost()
		{
			NetworkClient ret = base.StartHost();
			HostStarted();
			_is_host = _is_server = _is_client = true;
			_is_singleplayer = false;
			return ret;
		}

		// Server Message Handlers
		override public void OnStartServer()
		{
			base.OnStartServer();
			ServerStarted();
			_is_singleplayer = false;
			_is_server = true;
		}

		override public void OnServerAddPlayer(NetworkConnection conn, short player_controller_id)
		{
			if (playerPrefab != null) {
				base.OnServerAddPlayer(conn, player_controller_id);
			}

			ServerAddPlayer(conn, player_controller_id);
		}
	
		override public void OnServerConnect(NetworkConnection conn)
		{
			base.OnServerConnect(conn);
			ServerConnect(conn);
		}
	
		override public void OnServerDisconnect(NetworkConnection conn)
		{
			base.OnServerDisconnect(conn);
			ServerDisconnect(conn);
		}
	
		override public void OnServerError(NetworkConnection conn, int error_code)
		{
			base.OnServerError(conn, error_code);
			ServerError(conn, error_code);
		}
	
		override public void OnServerReady(NetworkConnection conn)
		{
			base.OnServerReady(conn);
			ServerReady(conn);
		}
	
		override public void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
		{
			base.OnServerRemovePlayer(conn, player);
			ServerRemovePlayer(conn, player);
		}
	
		override public void OnServerSceneChanged(string scene_name)
		{
			base.OnServerSceneChanged(scene_name);
			ServerSceneChanged(scene_name);
		}

		// Client Message Handlers
		override public void OnStartClient(NetworkClient client)
		{
			base.OnStartClient(client);
			ClientStarted(client);
			_is_singleplayer = false;
			_is_client = true;
		}
	
		override public void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);
			ClientConnect(conn);
		}
	
		override public void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);
			ClientDisconnect(conn);
		}
	
		override public void OnClientError(NetworkConnection conn, int error_code)
		{
			base.OnClientError(conn, error_code);
			ClientError(conn, error_code);
		}
	
		override public void OnClientNotReady(NetworkConnection conn)
		{
			base.OnClientNotReady(conn);
			ClientNotReady(conn);
		}
	
		override public void OnClientSceneChanged(NetworkConnection conn)
		{
			base.OnClientSceneChanged(conn);
			ClientSceneChanged(conn);
		}
	}

}
