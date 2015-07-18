using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ConnectUI : MonoBehaviour
{
	private ToxicNetworkManager _network_mgr = null;
	private PlayerManager _player_mgr = null;

	public void Start()
	{
		GameObject net_mgr = GameObject.Find("NetworkManager");

		if (!net_mgr) {
			Debug.LogError("Could not find global GameObject with name 'NetworkManager'.");
			return;
		}

		_network_mgr = net_mgr.GetComponent<ToxicNetworkManager>();
		_player_mgr = net_mgr.GetComponent<PlayerManager>();

		if (!_network_mgr) {
			Debug.LogError("Could not find a 'ToxicNetworkManager' on global GameObject 'NetworkManager'.");
		}

		if (!_player_mgr) {
			Debug.LogError("Could not find a 'PlayerManager' on global GameObject 'NetworkManager'.");
		}
	}

	public void StartDedicatedServer()
	{
		string port_string = transform.FindChild("PortInput").GetComponent<InputField>().text;
		_network_mgr.networkPort = System.Convert.ToInt32(port_string);

		if (_network_mgr.StartServer()) {
			Debug.Log("Started dedicated server on port " + port_string);
		} else {
			Debug.Log("Failed to start dedicated server on port " + port_string);
		}
	}

	public void StartListenServer()
	{
		string port_string = transform.FindChild("PortInput").GetComponent<InputField>().text;
		_network_mgr.networkPort = System.Convert.ToInt32(port_string);

		if (_network_mgr.StartHost() != null) {
			Debug.Log("Started listen server on port " + port_string);
			ShowReady();
		} else {
			Debug.Log("Failed to start listen server on port " + port_string);
		}
	}

	public void ConnectToServer()
	{
		string port_string = transform.FindChild("PortInput").GetComponent<InputField>().text;
		string ip = transform.FindChild("IPInput").GetComponent<InputField>().text;
		_network_mgr.networkAddress = ip;
		_network_mgr.networkPort = System.Convert.ToInt32(port_string);

		if (_network_mgr.StartClient() != null) {
			Debug.Log("Connected to server at " + ip + ":" + port_string);
			ShowReady();
		} else {
			Debug.Log("Failed to connect to server at " + ip + ":" + port_string);
		}
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void SetReady()
	{
		Toggle checkbox = transform.FindChild("ReadyCheckbox").GetComponent<Toggle>();
		_player_mgr.SetReady(checkbox.isOn);
	}

	private void ShowReady()
	{
		transform.FindChild("ReadyCheckbox").gameObject.SetActive(true);
	}
}
