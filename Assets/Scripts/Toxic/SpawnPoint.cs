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
TODO: Implement SpawnableEditor GUI for easier selection of spawnables.
************************************************************************************/

using UnityEngine.Networking;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Toxic
{

public class SpawnableEditor : Editor
{
	override public void OnInspectorGUI()
	{
		GameObject net_mgr_obj = GameObject.Find("NetworkManager");
		Toxic.NetworkManager net_mgr = (net_mgr_obj != null) ? net_mgr_obj.GetComponent<Toxic.NetworkManager>() : null;

		// If we find the network manager, display the list of spawnable objects
		if (net_mgr) {

		// Otherwise just let the user manually punch in spawn indices
		} else {
		}

		if (GUI.changed) {
			EditorUtility.SetDirty(target);
		}
	}
}
	
[AddComponentMenu("Toxic/Gameplay/Spawn Point")]
public class SpawnPoint : NetworkStartPosition
{
	public System.Random rng = new System.Random();
	public bool useSeedForRNG = false;
	public int seed = 0;

	public PlayerSpawnMethod spawnSelectionMethod;
	public List<int> spawnables;

	public int prevSpawnIndex
	{
		get { return _prev_spawn_index; }
	}

	private Toxic.NetworkManager _net_mgr = null;
	private int _prev_spawn_index = -1;

	void Start()
	{
		GameObject net_mgr = GameObject.Find("NetworkManager");
		
		if (!net_mgr) {
			Debug.LogError("Could not find global GameObject with name 'NetworkManager'.");
			return;
		}
		
		_net_mgr = net_mgr.GetComponent<Toxic.NetworkManager>();
		
		if (!_net_mgr) {
			Debug.LogError("Could not find a 'ToxicNetworkManager' on global GameObject 'NetworkManager'.");
		}

		if (useSeedForRNG) {
			rng = new System.Random(seed);
		}
	}

	public void spawn(int spawn_index)
	{
		_prev_spawn_index = spawn_index;

		GameObject spawned_obj = (GameObject)Instantiate(_net_mgr.spawnPrefabs[spawn_index], transform.position, transform.rotation);
		
		if (_net_mgr.isServer) {
			NetworkServer.Spawn(spawned_obj);
		}
	}

	public void spawn()
	{
		switch (spawnSelectionMethod) {
			case PlayerSpawnMethod.Random:
				spawn(rng.Next(spawnables.Count));
				break;

			case PlayerSpawnMethod.RoundRobin:
				spawn((_prev_spawn_index + 1) % spawnables.Count);
				break;
		}
	}
}

}
