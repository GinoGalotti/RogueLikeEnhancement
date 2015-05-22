/* 
 * Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * May 2015
 */

using UnityEngine;
using System.Collections;

public class SplashScreenLoader : MonoBehaviour {
	
	private const float WAIT_ONE_SECOND = 1f;

	void Awake () 
	{
		Invoke ("loadMainLevel", WAIT_ONE_SECOND);
	}

	private void loadMainLevel()
	{
		Application.LoadLevel("Main_Level");
	}
	
}