using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRippler : MonoBehaviour
{
	private SpaceBender _spaceBender;

	private void Start()
	{
		_spaceBender = GetComponent<SpaceBender>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			_spaceBender.StartRipple(0f, 0f, 2f, 0.1f);
			_spaceBender.StartRipple(1f, 1f, 2f, 0.1f);
		}
	}

}
