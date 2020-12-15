using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enfermedad
{
    // Start is called before the first frame update
    public TipoEnfermedad tipoEnfermedad;
    public List<Paso> pasos;
    public bool urgente;
    public float timerEnfermedad;
    public int prioridad;

    public Enfermedad(TipoEnfermedad tipoEnfermedad, bool urgente, float timerEnfermedad, List<Paso> pasos, int prioridad)
    {
        this.tipoEnfermedad = tipoEnfermedad;
        this.urgente = urgente;
        this.timerEnfermedad = timerEnfermedad;
        this.pasos = pasos;
        this.prioridad = prioridad;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
