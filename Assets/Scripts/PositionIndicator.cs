using UnityEngine;

public class PositionIndicator : MonoBehaviour
{

	[Range(0, 2)] public int StatsIndex;
	private Player _playerScript;
	private StatsManager _statsManagerScript;

	private void Start()
	{
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();
	}

	// Update is called once per frame
	void Update ()
	{
		if (_playerScript)
		{
			transform.position = _statsManagerScript.AllPlayerStats[StatsIndex].PlayerAveragePosition;
		}
	}
}
