using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionIndicator : MonoBehaviour
{

	[Range(0, 2)] public int StatsIndex;
	private Player _playerScript;

	private void Start()
	{
		GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
		if (playerGameObject)
		{
			_playerScript = playerGameObject.GetComponent<Player>();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (_playerScript)
		{
			transform.position = _playerScript.AllPlayerStats[StatsIndex].PlayerAveragePosition;
		}
	}
}
