using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabRotator : MonoBehaviour {

    public Vector3 RotationSpeed;
	
	void Update () {
        transform.Rotate(RotationSpeed);		
	}

}

