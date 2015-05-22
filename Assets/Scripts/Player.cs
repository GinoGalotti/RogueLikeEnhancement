/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MovingObject {

	//Game objetcs
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSoda1;
	public AudioClip drinkSoda2;
	public AudioClip gameOverSound;
	private Animator animator;
	private SpriteRenderer spriteRenderer;

	//Gameplay constants
	private const int DEFAULT_POINTS_PER_FOOD = 10;
	private const int DEFAULT_POINTS_PER_SODA = 20;
	private const int DEFAULT_WALL_DAMAGE = 1;
	private const int FREEZE_TURNS_DURATION = 4;
	private const int INVISIBILITY_TURNS_DURATION = 4;
	private const int INMORTAL_TURNS_DURATION = 4;
	private enum STATUS_CODE
	{
		INMMORTAL, INVISIBLE, POISON, NUMBEROFTYPES
	};
	private Color INMORTAL_COLOR_MASK = new Color (0.1f, 0.1f, 0.7f, 0f);
	private Color INVISIBLE_COLOR_MASK = new Color (0f, 0f, 0f, 0.3f);
	private Color POISON_COLOR_MASK = new Color (0.6f, 0f, 0.6f, 0f);
	private const string FOOD_TEXT_PREFIX = "Food ";
	
	//Player stats
	private static int wallDamage = 1;
	private static int pointsPerFood = 10;
	private static int pointsPerSoda = 20;
	private int[] activeStatusDuration = new int[(int)STATUS_CODE.NUMBEROFTYPES];
	private int food;

	//Touchscreen handler
	private Vector2 touchOrigin = -Vector2.one;

	protected override void Start () 
	{
		animator = GetComponent<Animator> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
		food = GameManager.instance.playerFoodPoints;

		foodText.text = FOOD_TEXT_PREFIX + food;

		base.Start ();
	}

	private void OnDisable()
	{
		GameManager.instance.playerFoodPoints = food;
	}
	
	// Update is called once per frame
	void Update () {

		if (GameManager.instance.playersTurn) 
		{
			int horizontal = 0;
			int vertical = 1;

			#if UNITY_STANDALONE || UNITYWEBPLAYER || UNITY_EDITOR

			horizontal = (int) (Input.GetAxisRaw("Horizontal"));
			vertical = (int) (Input.GetAxisRaw("Vertical"));

			if (horizontal !=0)
				vertical = 0;

			#else
			if (Input.touchCount > 0)
			{
				Touch myTouch = Input.touches[0];

				if (myTouch.phase == TouchPhase.Began)
				{
					touchOrigin = myTouch.position;
				} else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >=0)
				{
					Vector2 touchEnd = myTouch.position;
					float x = touchEnd.x - touchOrigin.x;
					float y = touchEnd.y - touchOrigin.y;
					touchOrigin.x = -1;

					if (swipePrimallyHorizontal(x,y))
				    {
						horizontal = x > 0 ? 1 : -1;
					} else 
					{
						vertical = y>0 ? 1:-1;
					}
				}
			}
			#endif

			if (horizontal != 0 || vertical != 0 && GameManager.instance.CanMove())
			{
				AttemptMove<Wall> (horizontal, vertical);
			} 
		}
	}

	private bool swipePrimallyHorizontal(float x, float y)
	{
		return Mathf.Abs (x) > Mathf.Abs (y);
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		food--;

		if (activeStatusDuration[(int)STATUS_CODE.POISON]>0)
		{
			food--;
		}

		foodText.text = FOOD_TEXT_PREFIX + food;

		base.AttemptMove<T> (xDir, yDir);

		RaycastHit2D hit;
		if (Move (xDir, yDir, out hit))
		{
			reduceEffect();
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}

		CheckIfGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void reduceEffect()
	{
		for (int i = 0; i < activeStatusDuration.Length; i++) 
		{
			if (activeStatusDuration[i] > 0)
				activeStatusDuration[i] = activeStatusDuration[i] -1;
			if (activeStatusDuration[i] == 0)
				updateVisualEffects();
		}
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		switch (other.tag) {
		case "Exit":
			Invoke ("Restart", GameManager.instance.restartLevelDelay);
			break;

		case "Food":
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " " + FOOD_TEXT_PREFIX + food;
			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
			other.gameObject.SetActive(false);
			break;

		case "Soda":
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " " + FOOD_TEXT_PREFIX + food;
			SoundManager.instance.RandomizeSfx(drinkSoda1, drinkSoda1);
			other.gameObject.SetActive(false);
			break;

		case "PowerUpFreeze":
			GameManager.instance.freezeEnemies(FREEZE_TURNS_DURATION);
			other.gameObject.SetActive(false);
			break;

		case "PowerUpInmortal":
			activeStatusDuration[(int)STATUS_CODE.INMMORTAL] = INMORTAL_TURNS_DURATION;
			other.gameObject.SetActive(false);
			updateVisualEffects();
			break;

		case "PowerUpInvisibility":
			activeStatusDuration[(int)STATUS_CODE.INVISIBLE] = INVISIBILITY_TURNS_DURATION;
			other.gameObject.SetActive(false);
			updateVisualEffects();
			break;
		}
	}

	protected override void OnCantMove <T> (T component)
	{
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);
		animator.SetTrigger ("playerChop");
	}

	public void Restart()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	public void LoseFood (int loss)
	{
		animator.SetTrigger ("playerHit");
		food -= loss;
		foodText.text = "-" + loss + " " + FOOD_TEXT_PREFIX + food;
		CheckIfGameOver ();
	}

	private void CheckIfGameOver()
	{
		if (food <= 0) 
		{
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Pause();
			GameManager.instance.GameOver ();
		}
	}

	public void setDefaultValues()
	{
		pointsPerFood = DEFAULT_POINTS_PER_FOOD;
		pointsPerSoda = DEFAULT_POINTS_PER_SODA;
		wallDamage = DEFAULT_WALL_DAMAGE;
	}

	public void upgradePointsPerFood(float increment)
	{
		pointsPerFood = Mathf.RoundToInt(pointsPerFood * increment);
		pointsPerSoda = Mathf.RoundToInt(pointsPerSoda * increment);
	}

	public void upgradeChopDamage(float increment)
	{
		wallDamage = Mathf.RoundToInt(wallDamage * increment);
	}

	public bool isInmortal()
	{
		return activeStatusDuration[(int)STATUS_CODE.INMMORTAL] > 0;
	}

	public bool isInvisible()
	{
		return activeStatusDuration[(int)STATUS_CODE.INVISIBLE] > 0;
	}

	public void poisonPlayer(int durationOfPoison)
	{
		activeStatusDuration [(int)STATUS_CODE.POISON] = durationOfPoison;
		updateVisualEffects();
	}

	private void updateVisualEffects()
	{
		spriteRenderer.color = new Color (1f, 1f, 1f, 1f);
		Color changedColor = spriteRenderer.color;
		if (activeStatusDuration[(int)STATUS_CODE.INVISIBLE] > 0)
		{
			changedColor.a = INVISIBLE_COLOR_MASK.a;
		}
		if (activeStatusDuration[(int)STATUS_CODE.INMMORTAL] > 0)
		{
			changedColor -= INMORTAL_COLOR_MASK;
		}
		if (activeStatusDuration[(int)STATUS_CODE.POISON] > 0)
		{
			changedColor -= POISON_COLOR_MASK;
		}
		spriteRenderer.color = changedColor;
	}

}
