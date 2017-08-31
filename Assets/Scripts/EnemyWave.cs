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

	private List<GameObject> _enemyList;
	private List<BasicEnemy> _enemyScripts;
	private int _farthestEnemyIndex;
	private Player _playerScript;
	private StatsManager _statsManagerScript;
	private LineRenderer _waveLineRenderer;
	private RectTransform _mainCanvasTransform;
	private GameObject _waveScoreIndicator;
    private float _leftAnchorXPos;
    private float _rightAnchorXPos;
	private Text _waveScoreText;
	private RectTransform _waveScoreIndicatorTransform;
	private float _initialWidth;
	private bool _enemyCountChanged;
	
	private float _waveScore;
	private int _waveMultiplier;

    public bool EnemyDisplacementChanged { get; private set; }
    public bool SetForDestruction { get; private set; }

    public EnemyWave(LineRenderer waveLineRenderer)
	{
		_enemyList = new List<GameObject>();
		_waveLineRenderer = waveLineRenderer;
		_initialWidth = waveLineRenderer.startWidth;
		_enemyScripts = new List<BasicEnemy>();
		_farthestEnemyIndex = 0;
		_enemyCountChanged = false;
		EnemyDisplacementChanged = false;
		SetForDestruction = false;

		_waveScore = 0.0f;
		_waveMultiplier = 0;
	}

	public void Initialize(Player playerScript, RectTransform mainCanvasTransform, GameObject waveScoreIndicator, float leftAnchorXPos, float rightAnchorXPos)
	{
		_playerScript = playerScript;
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();
		_mainCanvasTransform = mainCanvasTransform;
		_waveScoreIndicator = waveScoreIndicator;
	    _leftAnchorXPos = leftAnchorXPos;
	    _rightAnchorXPos = rightAnchorXPos;
        _waveScoreText = _waveScoreIndicator.GetComponent<Text>();
		_waveScoreIndicatorTransform = _waveScoreIndicator.GetComponent<RectTransform>();
	}

	public void AddNewEnemy(GameObject newEnemy)
	{
		_enemyList.Add(newEnemy);
		_enemyScripts.Add(newEnemy.GetComponent<BasicEnemy>());

		int enemyCount = _enemyList.Count;
		_waveLineRenderer.positionCount = enemyCount;
		_waveLineRenderer.SetPosition(enemyCount - 1, newEnemy.transform.position);
	}

	public void FinalizeWidthNodes()
	{
		for (int i = 1; i < _enemyList.Count; ++i) //0 is already initialized
		{
			float keyTime = (float)i / (_enemyList.Count - 1);
			_waveLineRenderer.widthCurve.AddKey(keyTime, i % 2 == 0 ? _initialWidth : 0.0f);
		}
		_enemyCountChanged = true;
		EnemyDisplacementChanged = true;
	}

	public void Update(float previousWavexPos)
	{
		if (SetForDestruction)
		{
			return;
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

    //TODO LATER perhaps we should separate the UI
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
}
