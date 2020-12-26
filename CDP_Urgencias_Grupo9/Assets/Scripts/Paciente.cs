using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paciente : MonoBehaviour
{
    private Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    void Start()
    {
        personaje = GetComponent<Personaje>();
        enfermedad = new Enfermedad(TipoEnfermedad.Cistitis, false, 9000000, null, 1);// TESTEANDO
    }

    public void GoTo( Transform transform)// cambio de estado etc y al llegar al punto se cambia otra vez de estado
    {
        personaje.GoTo(transform);
    }
    void Update()
    {
        
    }
}
