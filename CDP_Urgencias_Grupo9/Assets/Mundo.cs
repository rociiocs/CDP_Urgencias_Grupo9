using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoEnfermedad
{
    Cistitis,
    Gripe,
    Colico,
    Covid,
    ITS,
    Alergia,
    Disparo,
    Traumatismo,
    Apuñalamiento,
    Quemadura,
    ElementosExtraños,
    Embarazo
}

public enum Paso
{
    Enfermeria,
    Medico,
    Cirujano,
    UCI,
    Casa
}

public class Mundo : MonoBehaviour
{
    // Start is called before the first frame update
    List<Enfermedad> enfermedades = new List<Enfermedad>();
    
    void Start()
    {
        //Cistitis
        List<Paso> pasos = new List<Paso>();
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Casa);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Cistitis, false, 1800, pasos,12));

        //Gripe
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Casa);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Gripe, false, 600, pasos,10));

        //Colico
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Casa);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Colico, false, 1700, pasos,11));

        //Covid leve
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Casa);
        enfermedades.Add(new Enfermedad(TipoEnfermedad.Covid, false, 600, pasos,9));

        //Covid grave
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.UCI);
        enfermedades.Add(new Enfermedad(TipoEnfermedad.Covid, true, 120, pasos,7));

        //ITS
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Casa);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.ITS, false, 1800, pasos,13));

        //Alergia
        pasos = new List<Paso>();
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Enfermeria);
        pasos.Add(Paso.Medico);
        pasos.Add(Paso.Casa);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Alergia, false, 300, pasos,8));

        //Disparo
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Disparo, true, 60, pasos,1));

        //Traumatismo
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Traumatismo, true, 90, pasos,3));

        //Apuñalamiento
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Apuñalamiento, true, 60, pasos,2));

        //Quemadura
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Quemadura, true, 120, pasos,5));

        //ElementosExtraños
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.ElementosExtraños, true, 90, pasos,4));

        //Embarazo
        pasos = new List<Paso>();
        pasos.Add(Paso.Cirujano);
        pasos.Add(Paso.UCI);

        enfermedades.Add(new Enfermedad(TipoEnfermedad.Embarazo, true, 1800, pasos,6));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
