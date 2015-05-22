/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// Game Shared variables
	[HideInInspector] public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;
	[HideInInspector] public float restartLevelDelay = 1f;
	public const float LEVEL_START_DELAY_IN_SECS = 2f;
	public const float TURN_DELAY_IN_SECS = 0.1f;
	public static int remainingFreezeTurns;
	
	// Game Objects
	public BoardManager boardScript;
	public Player playerScript;
	public static GameManager instance = null;

	private Animator levelImageAnimator;
	private Text levelText;
	private GameObject levelImage;
	private List<Enemy> enemies;

	//GameManager variables
	private int level = 1;

	//Game flow controls
	private static bool enemiesMoving;
	private static bool doingSetup;
	private static bool gameOver;
	private static bool showingButtons;
	private bool showGinoEnhancements = true;


	//Gameplay constants
	private const int LEVELS_WITH_UPGRADE = 2;
	private const float CHOP_DAMAGE_INCREMENTAL_PER_UPGRADE = 1.6f;
	private const float POINTS_PER_FOOD_INCREMENTAL_PER_UPGRADE = 1.2f;
	private const int LUCK_INCREMENTAL_PER_UPGRADE = 5;

	//Text constants
	private const string GINO_ENHANCEMENTS_TEXT = "Roguelike 2D Gino Enhancement Project \n" +
			"Stuff that's been added:\n" +
			". A restart level feature\n" +
			". Upgrade stat each 2 levels\n" +
			". Power ups based on luck stat\n" + 
			". Player status\n" + 
			". Enemy spwn chance depending on type\n" +
			". Enemy probability of random move\n" + 
			". New types of enemies: Runner and Poisoner\n";
	private const int FONT_SIZE_FOR_ENHANCEMENT = 15;
	private const int DEFAULT_FONT_SIZE = 32;
	private const string NEW_LEVEL_TEXT_PREFIX = "Day ";
	private const string GAME_OVER_TEXT_PREFIX = "After ";
	private const string GAME_OVER_TEXT_SUFIX = " days, you starved";


	#if UNITY_STANDALONE || UNITYWEBPLAYER || UNITY_EDITOR
	private const string RESTART_TEXT = "\n Press R to restart";
	private const string SKIP_ENHANCEMENT_TEXT = "\n\n Press any key to start";
	#else
	private const string RESTART_TEXT = "\n Tap to restart";
	private const string SKIP_ENHANCEMENT_TEXT = "\n\n Tap to start";
	#endif

	void Awake () 
	{
		doSingletonMagic ();
		DontDestroyOnLoad (gameObject);
		enemies = new List<Enemy>();
		setDefaultValues ();
		InitGame ();
	}

	private void doSingletonMagic()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
	}

	private void OnLevelWasLoaded (int index)
	{
		if (gameOver) 
		{
			SoundManager.instance.musicSource.Play ();
			restoreInitialValues();
		} else {
			level++;
		}

		InitGame ();
	}

	private void restoreInitialValues()
	{
		level = 1;
		playerFoodPoints = 100;
		setDefaultValues();
		gameOver = false;
	}

	private void setDefaultValues()
	{
		boardScript = FindObjectOfType<BoardManager> ();
		playerScript = FindObjectOfType<Player>();
		playerScript.setDefaultValues();
		boardScript.setDefaultValues();
	}

	void InitGame()
	{
		doingSetup = true;

		levelImage = GameObject.Find ("LevelImage");
		levelImageAnimator = levelImage.GetComponent<Animator>();
		levelText = GameObject.Find("LevelText").GetComponent<Text>();

		if (showGinoEnhancements) {
			showGinoEnhancement();
		} else {

			showLevelText();
			if (isUpgradeLevel())
			{
				showUpgradeButtons();
			} else
			{
				Invoke ("HideLevelImage", LEVEL_START_DELAY_IN_SECS);
			}
		}

		enemies.Clear ();
		boardScript.SetupScene (level);
	}
	
	private bool isUpgradeLevel()
	{
		return (level != 1) && ((level - 1)  % LEVELS_WITH_UPGRADE ) == 0;
	}

	private void showUpgradeButtons()
	{
		showingButtons = true;
		levelImageAnimator.SetBool ("buttonVisible", true);
	}

	private void showGinoEnhancement()
	{
		levelText.fontSize = FONT_SIZE_FOR_ENHANCEMENT;
		levelText.text = GINO_ENHANCEMENTS_TEXT + SKIP_ENHANCEMENT_TEXT;
		levelImage.SetActive (true);
	}

	private void showLevelText()
	{
		levelText.fontSize = DEFAULT_FONT_SIZE;
		levelText.text = NEW_LEVEL_TEXT_PREFIX + level;
		levelImage.SetActive (true);
	}

	private void HideLevelImage()
	{
		levelImage.SetActive (false);
		doingSetup = false;
	}

	public void GameOver()
	{
		levelText.text = GAME_OVER_TEXT_PREFIX + level + GAME_OVER_TEXT_SUFIX + RESTART_TEXT;
		levelImage.SetActive(true);
		gameOver = true;
	}

	void Update () {

		if (isEnemiesTurn ()) {
			StartCoroutine (MoveEnemies ());
		} else if (gameOver) {
			if (playerWantToResetGame())
			{
				Application.LoadLevel (Application.loadedLevel);
			}
		} else if (showingButtons)
		{
			#if UNITY_STANDALONE || UNITYWEBPLAYER || UNITY_EDITOR
			playerPressKeyForButtons();
			#endif
		} else if (showGinoEnhancements) 
		{
			if(playerWantToSkipEnhancement())
			{
				Invoke ("HideLevelImage", LEVEL_START_DELAY_IN_SECS);
				showGinoEnhancements = false;
			}
		}
	}

	private void playerPressKeyForButtons()
	{
		if (Input.GetKey(KeyCode.Alpha1)){
			upgradeClick("Luck");
		} else if (Input.GetKey(KeyCode.Alpha2)) {
			upgradeClick("Chop");
		} else if (Input.GetKey(KeyCode.Alpha3)){
			upgradeClick("Food");
		}
	}

	private bool playerWantToResetGame()
	{
		bool playerWantToReset;
		#if UNITY_STANDALONE || UNITYWEBPLAYER || UNITY_EDITOR
		playerWantToReset = Input.GetKey (KeyCode.R);
		#else
		playerWantToReset = Input.touchCount >=0;
		#endif
		return playerWantToReset;
	}

	private bool playerWantToSkipEnhancement()
	{
		bool playerWantToSkipEnhancement;
		#if UNITY_STANDALONE || UNITYWEBPLAYER || UNITY_EDITOR
		playerWantToSkipEnhancement = Input.anyKeyDown;
		#else
		playerWantToSkipEnhancement = Input.touchCount >=0;
		#endif
		return playerWantToSkipEnhancement;
	}

	public void AddEnemyToList(Enemy script)
	{
		enemies.Add (script);
	}

	private bool isEnemiesTurn()
	{
		return !(playersTurn || enemiesMoving || doingSetup || gameOver);
	}

	IEnumerator MoveEnemies()
	{
		enemiesMoving = true;
		yield return new WaitForSeconds(TURN_DELAY_IN_SECS);

		if (enemies.Count == 0 || remainingFreezeTurns > 0) {
			remainingFreezeTurns = decreaseIfNotZero (remainingFreezeTurns);
			yield return new WaitForSeconds (TURN_DELAY_IN_SECS);
		} else {
			float lastEnemyTimeDelay = 0f;
			foreach(Enemy currentEnemy in enemies)
			{
				currentEnemy.MoveEnemy();
				lastEnemyTimeDelay = currentEnemy.moveTime;
			}
			yield return new WaitForSeconds(lastEnemyTimeDelay);
		}
		playersTurn = true;
		enemiesMoving = false;
	}

	public bool CanMove()
	{
		return !(gameOver || doingSetup);
	}

	public void upgradeClick(string buttonName)
	{
		boardScript = FindObjectOfType<BoardManager> ();
		playerScript = FindObjectOfType<Player>();

		switch (buttonName) 
		{
		case "Food":
			playerScript.upgradePointsPerFood(POINTS_PER_FOOD_INCREMENTAL_PER_UPGRADE);
			break;
		case "Luck":
			boardScript.upgradeLuck(LUCK_INCREMENTAL_PER_UPGRADE);
			break;
		case "Chop":
			playerScript.upgradeChopDamage(CHOP_DAMAGE_INCREMENTAL_PER_UPGRADE);
			break;
		}

		HideButtonsMenu ();

		Invoke ("HideLevelImage", LEVEL_START_DELAY_IN_SECS);
	}

	private void HideButtonsMenu()
	{
		showingButtons = false;
		levelImage = GameObject.Find ("LevelImage");
		levelImageAnimator = levelImage.GetComponent<Animator>();
		levelImageAnimator.SetBool ("buttonVisible", false);
	}

	public void freezeEnemies(int turnsFrozen)
	{
		remainingFreezeTurns = turnsFrozen;
	}

	private int decreaseIfNotZero( int valueToDecrease)
	{
		if (valueToDecrease > 0)
			valueToDecrease--;
		return valueToDecrease;
	}

}
