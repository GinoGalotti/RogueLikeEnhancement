/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	//Game Objects
	public AudioSource efxSource;
	public AudioSource musicSource;
	public static SoundManager instance = null;

	//Sound variation variables
	public float lowPitchRange = .95f;
	public float highPitchRange = 1.05f;

	// Use this for initialization
	void Awake () 
	{
		doSingletonMagic ();
		DontDestroyOnLoad (gameObject);
	}

	public void PlaySingle (AudioClip clip)
	{
		efxSource.clip = clip;
		efxSource.Play ();
	}

	public void RandomizeSfx (params AudioClip[] clips)
	{
		int randomIndex = Random.Range (0, clips.Length);
		float randomPitch = Random.Range (lowPitchRange, highPitchRange);

		efxSource.pitch = randomPitch;
		efxSource.clip = clips [randomIndex];
		efxSource.Play ();

	}

	private void doSingletonMagic()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
	}

}
