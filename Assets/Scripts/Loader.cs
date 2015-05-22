/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */
/*
* I'm doing this in a way to learn basics of Game design, improve my C# expertise and my general coding skills.
* My mantra is simple: make the project something that I'm proud to show, both as a game and as code itself.
* Obviously, I'm still learning as a programmer, and I've just started as a designer; so probably the things that you
 * are going to see are probably not best industry practices.
* 
* If you have something to add, would change anything or just want to give some feedback; PLEASE, send me an email (ginogalotti@gmail.com). I would thank some advice!
*
*
*/


/*
 * TO IMPROVE LIST:
 * 
 * Add a link in the headers to CODE EXPLANATION. Add code explanation itself
 * 
 * Gameplay:
 * . Improve procedural generation of the world.
 * . Add skills and new upgrades (Jump, Moving faster)
 * . Add new monsters (Ranged, Food seeker, procedural monster (% to be fast, poison, or both), bosses)
 * . Enemy can pick power ups.
 * . Improve monster IA
 * . Add extra objetives per level (catch a monster, something special if you pick all the food)
 * . Penalization if you have more tan 150Food (with upgrade to increasing the amount)
 * 
 * Technical bugs:
 * .Enemies can move to the same position
 * 
 */

using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	public GameObject gameManager;

	void Awake () {
	
		if (GameManager.instance == null) 
		{
			Instantiate(gameManager);
		}
	}

}
