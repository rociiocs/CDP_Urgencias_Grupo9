using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public bool libre= true;

    public void setOcupado( bool b)
    {
        libre = b;
    }
}
