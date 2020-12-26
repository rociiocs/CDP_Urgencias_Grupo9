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


public class Mundo: MonoBehaviour
{

    public List<Enfermedad> enfermedades = new List<Enfermedad>();
    public List<Sala> salas = new List<Sala>();
    public List<Sala> salasSucias = new List<Sala>();
    public List<Sala> cirugiasSucias = new List<Sala>();
    public List<Paciente> listaEsperaEnfermeria = new List<Paciente>();
    public List<Paciente> listaEsperaMedico = new List<Paciente>();
    public List<Paciente> listaEsperaCirugia = new List<Paciente>();
    public List<Limpiador> listaLimpiadores = new List<Limpiador>();
    public List<TargetUrgencias> mostradores;
    private int numEnfermeria = 3, numMedico = 4, numCirugia = 2;
    private int numEnfermeriaP = 6, numMedicoP = 4, numCirugiaP = 2;
    private float umbral= 65, speedSuciedad=0.01f, limitePorcentaje = 100;
    public TargetUrgencias[] targetMedico;
    public TargetUrgencias[] targetMedicoPaciente;
    public TargetUrgencias[] targetCirujano;
    public TargetUrgencias[] targetCirujanoPaciente;
    public TargetUrgencias[] targetEnfermeria;
    public TargetUrgencias[] targetEnfermeriaPaciente;
    public TargetUrgencias[] targetLimpiadores;
    public TargetUrgencias[] asientos;
    public TargetUrgencias casa;
    private int nMuertes = 0;

    void Awake()
    {

        //Instantiate numPersonajes
        
        //Creacion de salas
        int ID = 0;
        for (int i = 0; i < numEnfermeria; i++)
        {
            Sala nueva = new Sala(TipoSala.ENFERMERIA, ID);
            nueva.posicionPaciente = targetEnfermeriaPaciente[i].transform;
            nueva.posicionProfesional = targetEnfermeria[i].transform;
            salas.Add(nueva);

            ID++;
        }
        for (int i = 0; i < numCirugia; i++)
        {
            Sala nueva = new Sala(TipoSala.CIRUGIA, ID);
            nueva.posicionPaciente = targetCirujanoPaciente[i].transform;
            nueva.posicionProfesional = targetCirujano[i].transform;
            salas.Add(nueva);
      
            ID++;
        }
        for (int i = 0; i <numMedico; i++)
        {
            Sala nueva = new Sala(TipoSala.MEDICO, ID);
            nueva.posicionPaciente = targetMedicoPaciente[i].transform;
            nueva.posicionProfesional = targetMedico[i].transform;
            salas.Add(nueva);
      
            ID++;
        }
        Sala espera = new Sala(TipoSala.ESPERA, ID);
        salas.Add(espera);


        //Creacion de Base de datos de Enfermedades
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
        foreach (Sala s  in salas)
        {
            if (s.OnUpdateMundo(umbral, limitePorcentaje, speedSuciedad))
            {
                salasSucias.Add(s);
            }
        }

        ComprobarLibre(listaEsperaMedico, salas.FindAll((s) => s.tipo.Equals(TipoSala.MEDICO)));
        ComprobarLibre(listaEsperaCirugia, salas.FindAll((s) => s.tipo.Equals(TipoSala.CIRUGIA)));
        ComprobarLibre(listaEsperaEnfermeria, salas.FindAll((s) => s.tipo.Equals(TipoSala.ENFERMERIA)));
        LlamarLimpiadores();
    }

    private void LlamarLimpiadores()
    {
        int cont;
        if (salasSucias.Count != 0)
        {
            foreach(Limpiador l in listaLimpiadores)
            {
                if(!l.ocupado)
                {
                    // llamar limpiador 
                }
            }
        }
    }
    public void SalaCirugiaSucia(Sala sala)
    {
        cirugiasSucias = salasSucias.FindAll((s) => s.tipo.Equals(TipoSala.CIRUGIA));
        if (cirugiasSucias.Count != 0)
        {
            //llamar limpiador
        }
    }
    private void ComprobarLibre(List<Paciente> listaEspera, List<Sala> salas)
    {
        if (listaEspera.Count != 0)
        {
            foreach (Sala s in salas)
            {
                if (s.libre)
                {
                   Paciente llamado= listaEspera[0];
                   listaEspera.RemoveAt(0);
                   llamado.GoTo(s.posicionPaciente);
                }
            }
        }
    }
    public void AddPacienteEnfermeria(Paciente paciente)
    {
        listaEsperaEnfermeria.Add(paciente);
        listaEsperaEnfermeria.Sort();//Con un comparator de prioridad? si misma prioridad-> por tiempo de espera
    }
    public void AddPacienteMedico(Paciente paciente)
    {
        listaEsperaMedico.Add(paciente);
        listaEsperaMedico.Sort();//Con un comparator de prioridad? si misma prioridad-> por tiempo de espera

    }
    public void AddPacienteCirugia(Paciente paciente)
    {
        listaEsperaCirugia.Add(paciente);
        listaEsperaCirugia.Sort();//Con un comparator de prioridad? si misma prioridad-> por tiempo de espera
    }

}
