using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnemyWave
{
	private const float DistanceToBreakConnection = 2.0f;
    private const float ScoreTextDistanceFromWave = 100.0f;

	//script variables
	private Player _playerScript;
	private StatsManager _statsManagerScript;

	//gameplay variables
	private List<GameObject> _enemyList;
	private List<BasicEnemy> _enemyScripts;
	private int _farthestEnemyIndex;
	private bool _enemyCountChanged;
	private float _waveScore;
	private int _waveMultiplier;

	//maneuver variables
	private bool _maneuvers;
	private Vector2 _maneuveringDirection;
	private float _lastManeuveringTime;
	private float _nextManeuveringInterval;
	private const float DefaultManeuveringInterval = 1.0f;
	private float _maneuveringVerticalLength;
	private bool _maneuveringInProcess;

	//ui stuff
	private RectTransform _mainCanvasTransform;

	//wave line renderer stuff
	private LineRenderer _waveLineRenderer;
	private float _waveLineInitialWidth;

	//wave score indicator
	private GameObject _waveScoreIndicator;
    private float _leftAnchorXPos;
    private float _rightAnchorXPos;
	private Text _waveScoreText;
	private RectTransform _waveScoreIndicatorTransform;

    public bool EnemyDisplacementChanged { get; private set; }
    public bool SetForDestruction { get; private set; }

    public EnemyWave(Player playerScript, LineRenderer waveLineRenderer, RectTransform mainCanvasTransform,
	    GameObject waveScoreIndicator, float leftAnchorXPos, float rightAnchorXPos)
	{
		_playerScript = playerScript;
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();

		_enemyList = new List<GameObject>();
		_enemyScripts = new List<BasicEnemy>();
		_farthestEnemyIndex = 0;
		_enemyCountChanged = false;
		_waveScore = 0.0f;
		_waveMultiplier = 0;

		_maneuvers = false;
		_maneuveringDirection = Vector2.zero;
		_lastManeuveringTime = 0.0f;
		_nextManeuveringInterval = 0.0f;
		_maneuveringInProcess = false;

		_mainCanvasTransform = mainCanvasTransform;

		_waveLineRenderer = waveLineRenderer;
		_waveLineInitialWidth = waveLineRenderer.startWidth;

		_waveScoreIndicator = waveScoreIndicator;
		_leftAnchorXPos = leftAnchorXPos;
		_rightAnchorXPos = rightAnchorXPos;
		_waveScoreText = _waveScoreIndicator.GetComponent<Text>();
		_waveScoreIndicatorTransform = _waveScoreIndicator.GetComponent<RectTransform>();

		EnemyDisplacementChanged = false;
		SetForDestruction = false;
	}

	public void AddNewEnemy(GameObject newEnemy, BasicEnemy enemyScript)
	{
		_enemyList.Add(newEnemy);
		_enemyScripts.Add(enemyScript);

		int enemyCount = _enemyList.Count;
		_waveLineRenderer.positionCount = enemyCount;
		_waveLineRenderer.SetPosition(enemyCount - 1, newEnemy.transform.position);
	}

	public void FinalizeAfterWaveIsFilled(bool maneuvers, Vector2 maneuveringDirection, float maneuveringVerticalLength)
	{
		_maneuvers = maneuvers;
		_maneuveringDirection = maneuveringDirection;
		_lastManeuveringTime = Time.time;

		//TODO NEXT we want the wave to move vertically up until the difference reaches this in vertical length
		//make a calculation with enemy speed and determine optimal time to spend doing vertical to reach this length

		float enemySpeed = BasicEnemy.MoveSpeed;

		foreach (GameObject e in _enemyList)
		{
			//assert that every enemy moves with the same speed, and that speed equals the magic number const in BasicEnemy
			Assert.IsTrue(Mathf.Approximately(enemySpeed, e.GetComponent<BasicMove>().MoveSpeed));
		}

		_maneuveringVerticalLength = maneuveringVerticalLength;
		_nextManeuveringInterval = DefaultManeuveringInterval;

		AddWaveLineRendererKeys();
	}

	public void Update(float previousWavexPos)
	{
		if (SetForDestruction)
		{
			return;
		}

		if (_maneuvers)
		{
			bool timeToManeuver = Time.time - _lastManeuveringTime > _nextManeuveringInterval;
			
			if (_maneuveringInProcess)
			{
				//end maneuver immediately if we have a sped up enemy
				if (HasSpedUpEnemy() || timeToManeuver)
				{
					PerformManeuver(true);
				}
			}
			else if (timeToManeuver && !HasStunnedEnemy() && !HasSpedUpEnemy())
			{
				PerformManeuver(false);
			}
		}

		if (_enemyCountChanged)
		{
			_enemyList.RemoveAll(item => item == null);
			_enemyScripts.RemoveAll(item => item == null);
			_waveLineRenderer.positionCount = _enemyList.Count;

			Assert.IsTrue(_enemyList.Count == _enemyScripts.Count);

			if (_enemyList.Count == 0)
			{
				int baseWaveScore = GetBaseWaveScore();
				if (_playerScript)
				{
					_playerScript.TriggerEnemyWaveScoring(baseWaveScore * _waveMultiplier);

					//this score gets calculated as a stat only if player is alive, hence we do this if _playerScript exists
					_statsManagerScript.OnWaveDestruction(baseWaveScore);
				}
				Object.Destroy(_waveScoreIndicator);
				SetForDestruction = true;
				return;
			}
			
			EnemyDisplacementChanged = true;
			_enemyCountChanged = false;
		}

		int enemyCount = _enemyList.Count;
		float lineRendererLength = 0.0f;
		float[] enemyDistancesFromStart = new float[enemyCount];
		for (int i = 0; i < enemyCount; ++i)
		{
			_waveLineRenderer.SetPosition(i, _enemyList[i].transform.position);

			if (i > 0)
			{
				lineRendererLength += (_waveLineRenderer.GetPosition(i) - _waveLineRenderer.GetPosition(i - 1)).magnitude;
			}
			enemyDistancesFromStart[i] = lineRendererLength;
		}

		if (EnemyDisplacementChanged)
		{
			Keyframe[] keys = new Keyframe[enemyCount];

			for (int i = 0; i < _enemyList.Count; ++i)
			{
				float keyTime = enemyDistancesFromStart[i] / lineRendererLength;
				keys[i] = new Keyframe(keyTime, Mathf.Max(0.0f, DistanceToBreakConnection - Mathf.Abs(_enemyScripts[i].DisplacementLength)) / DistanceToBreakConnection);
			}
			_waveLineRenderer.widthCurve = new AnimationCurve(keys);

			UpdateFarthestEnemyIndex();

			EnemyDisplacementChanged = false;
		}
		UpdateWaveScoreIndicator(previousWavexPos);
	}

	private void PerformManeuver(bool resetMoveDir)
	{
		_lastManeuveringTime = Time.time;

		if (resetMoveDir)
		{
			foreach (BasicEnemy e in _enemyScripts)
			{
				e.ResetMoveDir();
			}
			_nextManeuveringInterval = DefaultManeuveringInterval;
			_maneuveringInProcess = false;
			return;
		}

		float enemyVerticalSpeed = 0.0f;
		Assert.IsTrue(_enemyScripts.Count > 0);

		foreach (BasicEnemy e in _enemyScripts)
		{
			Vector2 newDirection = (e.InitialMoveDir + _maneuveringDirection).normalized;
			e.SetMoveDir(newDirection);

			float currentEnemyVerticalSpeed = Mathf.Abs(e.GetMoveVelocity().y);

			if (!Mathf.Approximately(enemyVerticalSpeed, currentEnemyVerticalSpeed))
			{
				enemyVerticalSpeed = currentEnemyVerticalSpeed;
			}
		}

		//determine next maneuver interval
		_nextManeuveringInterval = _maneuveringVerticalLength / enemyVerticalSpeed;
		_maneuveringInProcess = true;

		//reverse direction of maneuver
		_maneuveringDirection *= -1;
		
	}

	private void UpdateFarthestEnemyIndex()
	{
		float farthestXPos = float.MinValue;
		for (int i = 0; i < _enemyList.Count; ++i)
		{
			if (_enemyList[i].transform.position.x > farthestXPos)
			{
				farthestXPos = _enemyList[i].transform.position.x;
				_farthestEnemyIndex = i;
			}
		}
	}

    //TODO LATER we should separate the UI
    private void UpdateWaveScoreIndicator(float previousWavexPos)
	{
		//updating high score UI for the wave
	    float currentLeftAnchor = Mathf.Max(_leftAnchorXPos, previousWavexPos + ScoreTextDistanceFromWave);

		Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(_enemyList[_farthestEnemyIndex].transform.position);
		float newPosX = enemyScreenPos.x - _mainCanvasTransform.sizeDelta.x * 0.5f;
		newPosX = Mathf.Clamp(newPosX, currentLeftAnchor, _rightAnchorXPos);

		int baseWaveScore = GetBaseWaveScore();

		_waveScoreIndicatorTransform.anchoredPosition = new Vector2(newPosX, _waveScoreIndicatorTransform.anchoredPosition.y);
		_waveScoreText.text = baseWaveScore.ToString(CultureInfo.InvariantCulture) + " x " +
							  _waveMultiplier.ToString(CultureInfo.InvariantCulture);

        //compare with best wave score to determine text color
		int playerBestWaveBaseScore = _statsManagerScript.GetAllTimeStats().BestWaveBaseScore;
		if (playerBestWaveBaseScore > 0.0f)
		{
			float scoreColorMultiplier = (float)baseWaveScore / _statsManagerScript.GetAllTimeStats().BestWaveBaseScore;
			Color textColor = new Color(1.0f - scoreColorMultiplier, 1.0f, 1.0f - scoreColorMultiplier);
			_waveScoreText.color = textColor;
		}
	}

	private int GetBaseWaveScore()
	{
		return Mathf.RoundToInt(_waveScore * GameConstants.BaseScoreMultiplier);
	}

	private void AddWaveLineRendererKeys()
	{
		for (int i = 1; i < _enemyList.Count; ++i) //0 is already initialized
		{
			float keyTime = (float)i / (_enemyList.Count - 1);
			_waveLineRenderer.widthCurve.AddKey(keyTime, i % 2 == 0 ? _waveLineInitialWidth : 0.0f);
		}
		_enemyCountChanged = true;
		EnemyDisplacementChanged = true;
	}

	public float GetWaveScorexPos()
    {
        return _waveScoreIndicatorTransform.anchoredPosition.x;
    }

	public void OnEnemyCountChanged()
	{
		_enemyCountChanged = true;
	}

	public void OnEnemyDisplacementChanged()
	{
		EnemyDisplacementChanged = true;
	}

	public Vector3 GetLastEnemyPosition()
	{
		return _enemyList[_enemyList.Count - 1].transform.position;
	}

	public void IncreaseWaveMultiplier()
	{
		++_waveMultiplier;
	}

	public void AddDisplacementScore(float displacementAmount)
	{
		_waveScore += displacementAmount;
	}

	public IEnumerator OnWaveMultiplierIncreased()
	{
		int growthRate = 3;
		_waveScoreText.fontSize += growthRate;
		yield return new WaitForSeconds(0.04f);
		_waveScoreText.fontSize += growthRate;
		yield return new WaitForSeconds(0.04f);
		_waveScoreText.fontSize += growthRate;
		yield return new WaitForSeconds(0.04f);
		_waveScoreText.fontSize -= growthRate;
		yield return new WaitForSeconds(0.04f);
		_waveScoreText.fontSize -= growthRate;
		yield return new WaitForSeconds(0.04f);
		_waveScoreText.fontSize -= growthRate;
	}

    public bool HasSpedUpEnemy()
    {
        return _enemyScripts.Any(enemy => enemy.SpeedBoostIsActive);
    }

	//TODO NEXT check if this works
	private bool HasStunnedEnemy()
	{
		return _enemyScripts.Any(enemy => enemy.IsStunned);
	}
}
