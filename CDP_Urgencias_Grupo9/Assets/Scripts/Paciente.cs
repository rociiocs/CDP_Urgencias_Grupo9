using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paciente : MonoBehaviour
{
    private Personaje personaje;

    void Start()
    {
        personaje = GetComponent<Personaje>();
    }

    public void GoTo( Transform transform)// cambio de estado etc y al llegar al punto se cambia otra vez de estado
    {
        personaje.GoTo(transform);
    }
    void Update()
    {
        
    }
}
