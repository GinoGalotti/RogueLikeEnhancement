/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Enemy : MovingObject {

	//Game Objects
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;
	public bool isPoison = false;
	public bool isRunner = false;

	private Animator animator;
	private Transform target;
	private Player scriptPlayer;
	
	//Gameplay variables
	public int playerDamage;
	public int enemyDelayTurn = 1;

	//Gameplay constants
	private const float RUNNER_SKIP_MOVE_PROBABILITY = 0.15f;
	private const float RANDOM_MOVE_PROBABILITY = 0.2f;
	private const int POISON_DURATION_IN_TURNS = 3;
	
	//Mechanism variables
	private int skipMove;

	protected override void Start () 
	{
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		target = GameObject.FindGameObjectWithTag ("Player").transform;
		scriptPlayer = GameObject.FindObjectOfType<Player>();

		base.Start ();
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		if (skipMove > 0 || GameManager.remainingFreezeTurns > 0) 
		{
			skipMove = decreaseIfNotZero(skipMove);
			return;
		}

		base.AttemptMove<T> (xDir, yDir);

		skipMove = enemyDelayTurn;
		if (isRunner)
			skipMove = Mathf.RoundToInt(Random.Range (0, 1 - RUNNER_SKIP_MOVE_PROBABILITY));
	}

	public void MoveEnemy()
	{
		int xDir = 0;
		int yDir = 0;

		if (scriptPlayer.isInvisible () || isRandomMove()) {
			xDir = randomMovement();
			yDir = randomMovement();
		} else {
			if (isVerticalMove ()) {
				yDir = isPlayerNorth () ? 1 : -1;
			} else {
				xDir = isPlayerEast() ? 1 : -1;
			}
		}

		AttemptMove <Player> (xDir, yDir);
	}

	private bool isRandomMove()
	{
		return Random.Range (0f, 1f) < RANDOM_MOVE_PROBABILITY;
	}

	private int randomMovement()
	{
		return Mathf.RoundToInt (Random.Range (-1, 1));
	}

	protected override void OnCantMove <T>(T component)
	{
		Player hitPlayer = component as Player;
		if (isPoison)
			hitPlayer.poisonPlayer (POISON_DURATION_IN_TURNS);
		if (!scriptPlayer.isInmortal ()) 
		{
			hitPlayer.LoseFood (playerDamage);
		}
		animator.SetTrigger ("enemyAttack");
		SoundManager.instance.RandomizeSfx (enemyAttack1, enemyAttack2);
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		if (isPoison) 
		{
			switch (other.tag) {
			case "Food":
				other.gameObject.SetActive(false);
				break;
			case "Soda":
				other.gameObject.SetActive(false);
				break;
			}
		}
	}

	private bool isVerticalMove()
	{
		return (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon);
	}

	private bool randomAxisMove()
	{
		return (0.5f <= Random.Range(0, 1));
	}

	private bool isPlayerNorth()
	{
		return target.position.y > transform.position.y;
	}

	private bool isPlayerEast()
	{
		return target.position.x > transform.position.x;
	}

	private int decreaseIfNotZero( int valueToDecrease)
	{
		if (valueToDecrease > 0)
			valueToDecrease--;
		return valueToDecrease;
	}

}
