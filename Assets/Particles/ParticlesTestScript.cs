﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesTestScript : MonoBehaviour 
{
	public ParticleSystem particles;
	float t = 0f;
	float delta = 0.025f;
	int randomSeedIndex;

	private uint[] randomSeeds = new uint[] { 123, 124, 125, 126, 127, 128 };

	private void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			t += delta;
			if (t >= 1f) { t = 1f; delta = -delta; }
			if (t <= 0f) { t = 0f; delta = -delta; randomSeedIndex = (randomSeedIndex + 1) % randomSeeds.Length;
							particles.randomSeed = randomSeeds[randomSeedIndex];  }
			particles.Simulate(t, true, true);
		}
	}
}
