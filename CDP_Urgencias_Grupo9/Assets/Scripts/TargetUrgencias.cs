﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetUrgencias : MonoBehaviour
{
    public bool libre;
    public bool ocupado;
    public Personaje actual;
    public void SetLibre(bool value)
    {
        libre = value;
    }
}
