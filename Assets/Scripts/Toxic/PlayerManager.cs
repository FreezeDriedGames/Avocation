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
//[NetworkSettings(channel=1, sendInterval=1.0f)]
[AddComponentMenu("Toxic/Networking/Player Manager")]
public class PlayerManager : MonoBehaviour
{
	public class PlayerInfoResponseMessage : MessageBase
	{
		public string name;
	}

	public class PlayerInfoUpdateMessage : MessageBase
	{
		public int id;
		public int ping;
		public bool ready;
	}

	public struct PlayerData
	{
		public string name;
		public string ip; // server only
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

	//public float updateInterval = 1.0f;

	// <Connection ID, PlayerData>
	private Dictionary<int, PlayerData> _player_data = new Dictionary<int, PlayerData>();

	private ToxicNetworkManager _network_mgr = null;

	private int _local_player_id = -1;

	//private float _time_since_update = 0.0f;

	// Public API
	public void SetReady(bool ready)
	{
		PlayerData pd = _player_data[_local_player_id];
		pd.ready = ready;
		
		_player_data[_local_player_id] = pd;

		_network_mgr.client.connection.Send(MessageIDs.PlayerReady, new IntegerMessage(System.Convert.ToInt32(ready)));
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
		if (!_network_mgr.isHost && _network_mgr.isClient) {
			return;
		}

		// Update pings

		// Check if all the players are ready. Then initiate a level change.
		if (Application.loadedLevelName == "MainMenu") {
			bool all_ready = _player_data.Count > 0;

			foreach (PlayerData pd in _player_data.Values) {
				if (!pd.ready) {
					all_ready = false;
					break;
				}
			}

			if (all_ready) {
				_network_mgr.ServerChangeScene(_network_mgr.levels[0]);
			}
		}
	}

	private void OnHostStart()
	{
		PlayerData pd = new PlayerData();
		pd.name = _local_player_id.ToString(); // Replace with actual local user name
		pd.ping = 0;
		pd.ready = false;
		
		_player_data.Add(_local_player_id, pd); // Local player ID is -1
	}

	private void OnClientStart(NetworkClient client)
	{
		client.RegisterHandler(MessageIDs.PlayerInfoRequest, OnPlayerInfoRequest);
		client.RegisterHandler(MessageIDs.PlayerInfoUpdate, OnPlayerInfoUpdate);
	}

	private void OnServerStart()
	{
		_network_mgr.ServerConnect += OnServerConnect;
		_network_mgr.ServerDisconnect += OnServerDisconnect;

		NetworkServer.RegisterHandler(MessageIDs.PlayerInfoResponse, OnPlayerInfoResponse);
		NetworkServer.RegisterHandler(MessageIDs.PlayerReady, OnPlayerReady);
	}

	// Server callbacks
	private void OnServerConnect(NetworkConnection conn)
	{
		conn.Send(MessageIDs.PlayerInfoRequest, new IntegerMessage(conn.connectionId));
	}

	private void OnServerDisconnect(NetworkConnection conn)
	{
		_player_data.Remove(conn.connectionId);
	}

	private void OnPlayerInfoResponse(NetworkMessage msg)
	{
		PlayerInfoResponseMessage response = msg.ReadMessage<PlayerInfoResponseMessage>();

		PlayerData pd = new PlayerData();
		pd.name = response.name;
		pd.ip = msg.conn.address;
		pd.ping = 0;
		pd.ready = false;

		_player_data.Add(msg.conn.connectionId, pd);
	}

	private void OnPlayerReady(NetworkMessage msg)
	{
		PlayerData pd = _player_data[msg.conn.connectionId];
		pd.ready = msg.ReadMessage<IntegerMessage>().value != 0;

		_player_data[msg.conn.connectionId] = pd;
	}

	// Client callbacks
	private void OnPlayerInfoRequest(NetworkMessage msg)
	{
		_local_player_id = msg.ReadMessage<IntegerMessage>().value;

		PlayerData pd = new PlayerData();
		pd.name = _local_player_id.ToString(); // Use local player name
		pd.ready = false;

		_player_data.Add(_local_player_id, pd);

		PlayerInfoResponseMessage response = new PlayerInfoResponseMessage();
		response.name = _local_player_id.ToString(); // replace with local player name

		msg.conn.Send(MessageIDs.PlayerInfoResponse, response);
	}

	private void OnPlayerInfoUpdate(NetworkMessage msg)
	{
		PlayerInfoUpdateMessage ud = msg.ReadMessage<PlayerInfoUpdateMessage>();

		PlayerData pd = _player_data[ud.id];
		pd.ping = ud.ping;
		pd.ready = ud.ready;

		_player_data[ud.id] = pd;
	}
}
