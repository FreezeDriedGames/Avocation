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
using System.Collections;

public class TestSpawnScript : MonoBehaviour
{
	// Use Update() instead of Start() so that the spawn points can initialize
	void Update()
	{
		GameObject net_mgr = GameObject.Find("NetworkManager");
		
		if (!net_mgr) {
			Debug.LogError("Could not find global GameObject with name 'NetworkManager'.");
			return;
		}
		
		Toxic.NetworkManager nm = net_mgr.GetComponent<Toxic.NetworkManager>();
		Toxic.PlayerManager pm = net_mgr.GetComponent<Toxic.PlayerManager>();

		if (!nm) {
			Debug.LogError("Could not find a 'Toxic.NetworkManager' on global GameObject 'NetworkManager'.");
			return;
		}

		if (!pm) {
			Debug.LogError("Could not find a 'Toxic.PlayerManager' on global GameObject 'NetworkManager'.");
			return;
		}

		if (nm.isServer) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag("SpawnPoints");

			int spawner = 0;

			foreach (Toxic.PlayerManager.PlayerData pd in pm.playerData.Values) {
				Toxic.SpawnPoint sp = objects[spawner].GetComponent<Toxic.SpawnPoint>();
				GameObject player = sp.spawn();

				NetworkServer.ReplacePlayerForConnection(pd.network_connection, player, pd.player_controller_id); 				

				spawner = (spawner + 1) % objects.Length;
			}
		}

		enabled = false;
	}
}
