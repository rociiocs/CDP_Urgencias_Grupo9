using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetUrgencias : MonoBehaviour
{
    public bool libre;
    public bool ocupable;
    public Personaje actual;
    private void Start()
    {
        libre = true;
        ocupable = true;
    }
    public void SetLibre(bool value)
    {
        libre = value;
    }
}
