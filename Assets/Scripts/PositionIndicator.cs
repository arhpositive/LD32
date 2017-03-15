using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionIndicator : MonoBehaviour
{

	[Range(0, 2)] public int StatsIndex;
	private Player _playerScript;

	private void Start()
	{
		_playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}

	// Update is called once per frame
	void Update ()
	{
		transform.position = _playerScript.AllPlayerStats[StatsIndex].PlayerAveragePosition;
	}
}
