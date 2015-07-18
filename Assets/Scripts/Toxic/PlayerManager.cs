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

using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Send on unreliable channel and only update once every second
[NetworkSettings(channel=1, sendInterval=1.0f)]
public class PlayerManager : NetworkBehaviour
{
	public class PlayerInfoResponse : MessageBase
	{
		public string name;
		public int id;
	}

	public struct PlayerData
	{
		public string name;
		//public string ip; // server only
		public int ping;
		public bool ready;
	}

	public Dictionary<int, PlayerData> playerData
	{
		get
		{
			return _player_data;
		}
	}

	public int localPlayerId
	{
		get
		{
			return _local_player_id;
		}
	}

	[SyncVar]
	private Dictionary<int, PlayerData> _player_data = new Dictionary<int, PlayerData>();
	private Dictionary<int, int> _connection_ids = new Dictionary<int, int>();

	private int _local_player_id = -1;
	private int _next_player_id = 0;

	private ToxicNetworkManager _network_mgr = null;

	// Public API
	[Command]
	public void CmdSetReady(int id, bool ready)
	{
		PlayerData pd = _player_data[id];
		pd.ready = ready;
		
		_player_data[id] = pd;
	}


	// Internal Unity handling API
	void Start()
	{
		DontDestroyOnLoad(this);

		GameObject net_mgr = GameObject.Find("NetworkManager");
		
		if (!net_mgr) {
			Debug.LogError("Could not find global GameObject with name 'NetworkManager'.");
			return;
		}
		
		_network_mgr = net_mgr.GetComponent<ToxicNetworkManager>();
		
		if (!_network_mgr) {
			Debug.LogError("Could not find a 'ToxicNetworkManager' on global GameObject 'NetworkManager'.");
		}

		_network_mgr.ServerStarted += OnServerStart;
		_network_mgr.ClientStarted += OnClientStart;
		_network_mgr.HostStarted += OnHostStart;
	}
	
	// Do ping updates every so many intervals here?
	void Update()
	{
	}

	private void OnHostStart()
	{
		_local_player_id = _next_player_id;
		++_next_player_id;

		_connection_ids.Add(_network_mgr.client.connection.connectionId, _local_player_id);

		PlayerData pd = new PlayerData();
		pd.name = _local_player_id.ToString(); // Replace with actual local user name
		pd.ping = 0;
		pd.ready = false;
		
		_player_data.Add(_local_player_id, pd);
	}

	private void OnClientStart(NetworkClient client)
	{
		client.RegisterHandler(MessageIDs.PlayerInfoRequest, OnPlayerInfoRequest);
	}

	private void OnServerStart()
	{
		_network_mgr.ServerConnect += OnServerConnect;
		_network_mgr.ServerDisconnect += OnServerDisconnect;

		NetworkServer.RegisterHandler(MessageIDs.PlayerInfoResponse, OnPlayerInfoResponse);
	}

	// Server callbacks
	private void OnServerConnect(NetworkConnection conn)
	{
		int id = _next_player_id;
		++_next_player_id;

		conn.Send(MessageIDs.PlayerInfoRequest, new IntegerMessage(id));
	}

	private void OnServerDisconnect(NetworkConnection conn)
	{
		_player_data.Remove(_connection_ids[conn.connectionId]);
		_connection_ids.Remove(conn.connectionId);
	}

	private void OnPlayerInfoResponse(NetworkMessage msg)
	{
		PlayerInfoResponse response = msg.ReadMessage<PlayerInfoResponse>();

		PlayerData pd = new PlayerData();
		pd.name = response.name;
		pd.ping = 0;
		pd.ready = false;

		_connection_ids.Add(msg.conn.connectionId, response.id);
		_player_data.Add(response.id, pd);
	}

	// Client callbacks
	private void OnPlayerInfoRequest(NetworkMessage msg)
	{
		_local_player_id = msg.ReadMessage<IntegerMessage>().value;

		PlayerInfoResponse response = new PlayerInfoResponse();
		response.name = _local_player_id.ToString(); // replace with local player name
		response.id = _local_player_id;

		msg.conn.Send(MessageIDs.PlayerInfoResponse, response);
	}
}
