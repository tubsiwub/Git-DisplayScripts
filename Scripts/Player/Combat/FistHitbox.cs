﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistHitbox : MonoBehaviour
{
	[SerializeField] Transform followTransform;

	GameObject powEffect;

	float effectCooldown = 0;
	float effectCooldownINIT = 0.4f;

	void Start()
	{
		powEffect = Resources.Load<GameObject>("Effect_Pow!");
	}

	void Update()
	{
		// cooldown timer
		effectCooldown -= Time.deltaTime;

		MoveToHand();
	}

	void LateUpdate()
	{
		MoveToHand();
	}

	void MoveToHand()
	{
		if (followTransform != null)
			transform.position = followTransform.position;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Enemy")
		{
			other.gameObject.GetComponent<Enemy>().DamageEnemy(25);
			HitObject();
		}

		if (other.gameObject.tag == "Cage")
		{
			other.gameObject.GetComponent<CageExplode>().BreakCage();
			HitObject();
		}

		if (other.gameObject.tag == "Balloon")
		{
			//other.gameObject.GetComponent<BalloonExplode>().BreakBalloon();
			HitObject();
		}
	}

	void HitObject()
	{
		if (effectCooldown <= 0)
			SpawnPow();
	}

	void SpawnPow()
	{
		float varValue = Random.Range(-1, 1);
		Vector3 variance = new Vector3(varValue, varValue * 0.5f, varValue);
		Instantiate(powEffect, transform.position + variance, Quaternion.identity);
		effectCooldown = effectCooldownINIT;
	}
}
