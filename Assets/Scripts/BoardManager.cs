
/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

	//Board creation variables
	public int columns = 8;
	public int rows = 8;

	// Game Objects
	public Count wallCount;
	public Count foodCount;	
	public GameObject exit;
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] foodTiles;
	public GameObject[] enemyTiles;
	public GameObject[] outerWallTiles;
	public GameObject[] powerUpTiles;
	
	private Transform boardHolder;
	private List<Vector3> gridPositions = new List<Vector3>();
	
	//Board variables
	private int luck = 0;
	
	//Board Gameplay constants
	private const int DEFAULT_LUCK = 0;
	private const float POWER_UP_PROBABILITY = 0.9f;
	private const int ENEMY1_SPAWN_CHANCE = 30;
	private const int ENEMY2_SPAWN_CHANCE = 30;
	private const int ENEMY_RUNNER_SPAWN_CHANCE = 20;
	private const int ENEMY_POISON_SPAWN_CHANCE = 20;
	private const int ENEMY1_TILE_POSITION = 0;
	private const int ENEMY2_TILE_POSITION = 1;
	private const int ENEMY_RUNNER_TILE_POSITION = 2;
	private const int ENEMY_POISON_TILE_POSITION = 3;

	[Serializable]
	public class Count
	{
		public int minimum;
		public int maximum;

		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}

	void InitialiseList()
	{
		gridPositions.Clear ();

		for (int x=1; x< columns - 1; x++) 
		{
			for (int y=1; y<rows-1; y++)
			{
				gridPositions.Add (new Vector3(x,y,0f));
			}
		}
	}

	void BoardSetup()
	{
		boardHolder = new GameObject ("Board").transform;

		for (int x= -1; x < columns + 1; x++) 
		{
			for (int y= -1; y < rows+1; y++)
			{
				GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

				if (IsOuterWallPosition(x,y))
				{
					toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
				}

				GameObject instance = Instantiate(toInstantiate, new Vector3(x,y,0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent(boardHolder);
			}
		}	
	}

	Boolean IsOuterWallPosition(int x, int y)
	{
		return (x == -1 || x == columns || y == -1 || y == rows);
	}

	Vector3 RandomFreePosition()
	{
		int randomIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPosition = gridPositions [randomIndex];
		gridPositions.RemoveAt (randomIndex);
		return randomPosition;
	}

	void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
	{
		int objectCount = Random.Range (minimum, maximum + 1);
		for (int i=0; i< objectCount; i++)
		{
			Vector3 randomPosition = RandomFreePosition();
			GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	void LayoutEnemiestAtRandom (GameObject[] tileArray, int minimum, int maximum)
	{
		int objectCount = Random.Range (minimum, maximum + 1);
		for (int i=0; i< objectCount; i++)
		{
			Vector3 randomPosition = RandomFreePosition();
			int randomTile = randomEnemy(Random.Range (0, 100));
			GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	public void SetupScene(int level)
	{
		BoardSetup ();
		InitialiseList ();
		LayoutWall ();
		LayoutFood ();
		LayoutPowerUps ();
		LayoutEnemy (level);
		LayoutExit ();
	}

	void LayoutPowerUps()
	{
		int powerUpPosibility = Mathf.RoundToInt(luck * POWER_UP_PROBABILITY);
		LayoutObjectAtRandom (powerUpTiles, 0, powerUpPosibility);
	}

	void LayoutWall()
	{
		LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
	}

	void LayoutFood()
	{
		LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);
	}

	void LayoutEnemy(int level)
	{
		int enemyCount = (int)Mathf.Log (level, 2f);
		LayoutEnemiestAtRandom (enemyTiles, enemyCount, enemyCount);
	}

	void LayoutExit()
	{
		Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
	}

	public void setDefaultValues()
	{
		luck = DEFAULT_LUCK;
	}

	public void upgradeLuck(int increment)
	{
		luck += increment;
	}

	private int randomEnemy(int randomNumber)
	{
		int chosenTile = ENEMY1_TILE_POSITION;
		if ((randomNumber -= ENEMY2_SPAWN_CHANCE) <= 0) 
		{
			chosenTile = ENEMY2_TILE_POSITION;
		} else if ((randomNumber -= ENEMY_RUNNER_SPAWN_CHANCE) <= 0) 
		{
			chosenTile = ENEMY_RUNNER_TILE_POSITION;
		} else if ((randomNumber -= ENEMY_POISON_SPAWN_CHANCE) <= 0) 
		{
			chosenTile = ENEMY_POISON_TILE_POSITION;
		}

		return chosenTile;
	}

}
