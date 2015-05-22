/* Copyright (C) Luis Galotti Muñoz (Gino) <ginogalotti@gmail.com>
 * Assets are derived from Unity's RogueLike tutorial project
 * May 2015
 */

using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour {

	// Game Shared variables
	public float moveTime = 0.05f;

	private float inverseMoveTime;

	// Game Objects
	public LayerMask blockinglayer;

	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;

	protected virtual void Start () 
	{
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();
		inverseMoveTime = 1f / moveTime;
	}

	protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2 (xDir, yDir);

		boxCollider.enabled = false;
		hit = Physics2D.Linecast (start, end, blockinglayer);
		boxCollider.enabled = true;

		if (hit.transform == null) 
		{
			StartCoroutine(SmoothMovement (end));
			return true;
		}
		return false;
	}

	protected virtual void AttemptMove <T> (int xDir, int yDir)
		where T: Component
	{
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		if (hit.transform != null) 
		{
			T hitComponent = hit.transform.GetComponent<T> ();
			if (!canMove && hitComponent != null)
				OnCantMove (hitComponent);
		}
	}

	protected IEnumerator SmoothMovement (Vector3 end) 
	{
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		while (sqrRemainingDistance > float.Epsilon) 
		{
			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, inverseMoveTime + Time.deltaTime);
			rb2D.MovePosition(newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}
	
	protected abstract void OnCantMove <T> (T component)
		where T : Component;
}
