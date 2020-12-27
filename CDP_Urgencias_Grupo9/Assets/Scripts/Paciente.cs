using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paciente : MonoBehaviour
{
    private Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    int idPasoActual;
    Paso pasoActual;
    void Start()
    {
        personaje = GetComponent<Personaje>();
        enfermedad = new Enfermedad(TipoEnfermedad.Cistitis, false, 9000000, null, 1);// TESTEANDO
        idPasoActual = 0;
        pasoActual = enfermedad.pasos[idPasoActual];
    }

    public void GoTo( Transform transform)// cambio de estado etc y al llegar al punto se cambia otra vez de estado
    {
        personaje.GoTo(transform);
    }
    public void siguientePaso()
    {
        idPasoActual++;
        pasoActual = enfermedad.pasos[idPasoActual];
    }
    void Update()
    {
        
    }
}
