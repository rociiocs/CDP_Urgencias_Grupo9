﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyRotation : MonoBehaviour {

    private void Update()
    {
        transform.Rotate(new Vector3(0, 1.5f, 0));
    }
}