using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnemyWave
{
	private const float DistanceToBreakConnection = 2.0f;

	private List<GameObject> _enemyList;
	private List<BasicEnemy> _enemyScripts;
	private int _farthestEnemyIndex;
	private Player _playerScript;
	private LineRenderer _waveLineRenderer;
	private RectTransform _mainCanvasTransform;
	private GameObject _waveScoreIndicator;
	private Text _waveScoreText;
	private RectTransform _waveScoreIndicatorTransform;
	private float _initialWidth;
	private bool _enemyCountChanged;
	private bool _enemyDisplacementChanged;
	private bool _setForDestruction;
	private float _waveScore;
	private int _waveMultiplier;

	public EnemyWave(LineRenderer waveLineRenderer)
	{
		_enemyList = new List<GameObject>();
		_waveLineRenderer = waveLineRenderer;
		_initialWidth = waveLineRenderer.startWidth;
		_enemyScripts = new List<BasicEnemy>();
		_farthestEnemyIndex = 0;
		_enemyCountChanged = false;
		_enemyDisplacementChanged = false;
		_setForDestruction = false;

		_waveScore = 0.0f;
		_waveMultiplier = 1;
	}

	public void Initialize(Player playerScript, RectTransform mainCanvasTransform, GameObject waveScoreIndicator)
	{
		_playerScript = playerScript;
		_mainCanvasTransform = mainCanvasTransform;
		_waveScoreIndicator = waveScoreIndicator;
		_waveScoreText = _waveScoreIndicator.GetComponent<Text>();
		_waveScoreIndicatorTransform = _waveScoreIndicator.GetComponent<RectTransform>();
	}

	public bool IsSetForDestruction()
	{
		return _setForDestruction;
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
		_enemyDisplacementChanged = true;
	}

	public void Update()
	{
		if (_setForDestruction)
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
				_playerScript.TriggerEnemyWaveScoring(baseWaveScore, baseWaveScore * _waveMultiplier);
				Object.Destroy(_waveScoreIndicator);
				_setForDestruction = true;
				return;
			}
			
			_enemyDisplacementChanged = true;
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

		if (_enemyDisplacementChanged)
		{
			Keyframe[] keys = new Keyframe[enemyCount];

			for (int i = 0; i < _enemyList.Count; ++i)
			{
				float keyTime = enemyDistancesFromStart[i] / lineRendererLength;
				keys[i] = new Keyframe(keyTime, Mathf.Max(0.0f, DistanceToBreakConnection - Mathf.Abs(_enemyScripts[i].DisplacementLength)) / DistanceToBreakConnection);
			}
			_waveLineRenderer.widthCurve = new AnimationCurve(keys);

			UpdateFarthestEnemyIndex();

			_enemyDisplacementChanged = false;
		}
		UpdateWaveScoreIndicator();
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

	private void UpdateWaveScoreIndicator()
	{
		//updating high score UI for the wave
		//TODO LATER perhaps we should separate the UI
		Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(_enemyList[_farthestEnemyIndex].transform.position);
		float newPosX = enemyScreenPos.x - _mainCanvasTransform.sizeDelta.x * 0.5f;
		newPosX = Mathf.Clamp(newPosX, GameConstants.ScoreTextMinClamp, GameConstants.ScoreTextMaxClamp);

		int baseWaveScore = GetBaseWaveScore();

		_waveScoreIndicatorTransform.anchoredPosition = new Vector2(newPosX, _waveScoreIndicatorTransform.anchoredPosition.y);
		_waveScoreText.text = baseWaveScore.ToString(CultureInfo.InvariantCulture) + " x " +
							  _waveMultiplier.ToString(CultureInfo.InvariantCulture);

		int playerBestWaveBaseScore = _playerScript.GetAllTimeStats().BestWaveBaseScore;
		if (playerBestWaveBaseScore > 0.0f)
		{
			float scoreColorMultiplier = (float)baseWaveScore / _playerScript.GetAllTimeStats().BestWaveBaseScore;
			Color textColor = new Color(1.0f - scoreColorMultiplier, 1.0f, 1.0f - scoreColorMultiplier);
			_waveScoreText.color = textColor;
		}
	}

	private int GetBaseWaveScore()
	{
		return Mathf.RoundToInt(_waveScore * GameConstants.BaseScoreMultiplier);
	}

	public void OnEnemyCountChanged()
	{
		_enemyCountChanged = true;
	}

	public void OnEnemyDisplacementChanged()
	{
		_enemyDisplacementChanged = true;
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
}
