﻿/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * ParallaxManager.cs
 * Handles parallax spawn in the game
 */

using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject[] MeteorPrefabArray;
	public GameObject[] StarPrefabArray;

	[Header("Parallax Counts")]
	public int MeteorCount;
	public int StarCount;

	// Use this for initialization
	void Start ()
	{
		InitialMeteorAndStarSpawn();
	}

	private void InitialMeteorAndStarSpawn()
	{
		for (int i = 0; i < MeteorCount; i++)
		{
			int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
			GameObject selectedMeteor = MeteorPrefabArray[meteorKind];
			BasicMove meteorMoveScript = selectedMeteor.GetComponent<BasicMove>();

			Vector2 meteorPos =
				new Vector2(Random.Range(meteorMoveScript.HorizontalLimits[0], meteorMoveScript.HorizontalLimits[1]),
					Random.Range(meteorMoveScript.VerticalLimits[0], meteorMoveScript.VerticalLimits[1]));
			GameObject instantiatedMeteor = Instantiate(selectedMeteor, meteorPos, Quaternion.identity);
			instantiatedMeteor.GetComponent<SpriteRenderer>().color = 
				Random.ColorHSV(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.3f, 0.3f);
		}

		for (int i = 0; i < StarCount; i++)
		{
			int starKind = Random.Range(0, StarPrefabArray.Length);
			GameObject selectedStar = StarPrefabArray[starKind];
			BasicMove starMoveScript = selectedStar.GetComponent<BasicMove>();

			Vector2 starPos =
				new Vector2(Random.Range(starMoveScript.HorizontalLimits[0], starMoveScript.HorizontalLimits[1]),
					Random.Range(starMoveScript.VerticalLimits[0], starMoveScript.VerticalLimits[1]));
			GameObject instantiatedStar = Instantiate(selectedStar, starPos, Quaternion.identity);
			instantiatedStar.GetComponent<SpriteRenderer>().color = 
				Random.ColorHSV(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.3f, 0.3f);
		}
	}
}
