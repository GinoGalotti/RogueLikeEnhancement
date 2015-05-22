/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	//Game objects
	public Sprite dmgSprite;
	public AudioClip chopSound1;
	public AudioClip chopSound2;

	private SpriteRenderer spriteRenderer;

	//Gameplay constants
	public int hp = 4;
	
	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void DamageWall (int loss)
	{
		spriteRenderer.sprite = dmgSprite;
		hp -= loss;
		SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);
		disableIfDead ();

	}

	private void disableIfDead()
	{
		if (hp <= 0)
			gameObject.SetActive (false);
	}
}
