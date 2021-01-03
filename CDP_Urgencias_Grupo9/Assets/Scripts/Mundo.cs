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

    //Referencia sa prefabs
    public GameObject prefabEnfermero, prefabPaciente, prefabMedico, prefabCelador, prefabCirujano, prefabLimpiador;

    //referencia al seleccionador de camara
    SeleccionadorCamara sc;
    //
    public Sprite[] emoticonoEnfermedad;
    public List<Enfermedad> enfermedades = new List<Enfermedad>();
    public List<Sala> salas = new List<Sala>();
    public List<Sala> salasSucias = new List<Sala>();
    public List<Sala> cirugiasSucias = new List<Sala>();
    public List<Paciente> listaEsperaEnfermeria = new List<Paciente>();
    public List<Paciente> listaEsperaMedico = new List<Paciente>();
    public List<Paciente> listaEsperaCirugia = new List<Paciente>();
    public List<Limpiador> listaLimpiadores = new List<Limpiador>();
    private int numEnfermeria = 3, numMedico = 4, numCirugia = 2;
    private int numEnfermeriaP = 6, numMedicoP = 4, numCirugiaP = 2;
    public int numEnfermeros = 3, numMedicos = 2, numCirujanos = 1, numCeladores = 2, numLimpiadores = 2;
    public float umbral= 65, speedSuciedad=0.01f, limitePorcentaje = 100;
    public TargetUrgencias[] targetMedico;
    public TargetUrgencias[] targetMedicoPaciente;
    public TargetUrgencias[] targetCirujano;
    public TargetUrgencias[] targetCirujanoPaciente;
    public TargetUrgencias[] targetEnfermeria;
    public TargetUrgencias[] targetEnfermeriaPaciente;
    public TargetUrgencias[] targetLimpiadores;
    public TargetUrgencias[] targetLimpiadoresSala;
    public TargetUrgencias[] targetEspera;
    public TargetUrgencias[] targetMostradorPaciente;
    public TargetUrgencias[] targetColaDentro;
    public TargetUrgencias[] targetColaFuera;
    public TargetUrgencias[] asientos;
    public TargetUrgencias[] banhos;

    public TargetUrgencias laboratorio;
    public TargetUrgencias casa;
    public TargetUrgencias casaPaciente;
    private int nMuertes = 0;
    private int MAX_PACIENTES = 15;
    //private float spawnMaxTime = 15f;
    //private float spawnMinTime = 9f;
    private float spawnMaxTime = 0;
    private float spawnMinTime = 9;
    private int contPacientes = 0;
    public bool aforo;
    public int aforoMax = 4;
    public int asientosOcupados = 0;
    //public bool ponerSalaSucia = false;


    void Awake()
    {
        sc = FindObjectOfType<SeleccionadorCamara>();

        //Instantiate numPersonajes
        
        //Creacion de salas
        int ID = 0;
        for (int i = 0; i < numEnfermeriaP; i++)
        {
            Sala nueva = new Sala(TipoSala.ENFERMERIA, ID);
            //Esto se debería hacer dos veces
            nueva.posicionPaciente = targetEnfermeriaPaciente[i];
            nueva.posicionProfesional = targetEnfermeria[i];
           
            salas.Add(nueva);
            if (ID < numEnfermeria)
            {
                nueva.posicionLimpiador = targetLimpiadoresSala[ID];
                ID++;
            }
         
        }
        for (int i = 0; i < numCirugiaP; i++)
        {
            Sala nueva = new Sala(TipoSala.CIRUGIA, ID);
            nueva.posicionPaciente = targetCirujanoPaciente[i];
            nueva.posicionProfesional = targetCirujano[i];
            salas.Add(nueva);
            if (ID < numCirugia)
            {
                nueva.posicionLimpiador = targetLimpiadoresSala[ID];
                ID++;
            }
           
        }
        for (int i = 0; i <numMedicoP; i++)
        {
            Sala nueva = new Sala(TipoSala.MEDICO, ID);
            nueva.posicionPaciente = targetMedicoPaciente[i];
            nueva.posicionProfesional = targetMedico[i];
          
            salas.Add(nueva);

            if (ID < numMedico)
            {
                nueva.posicionLimpiador = targetLimpiadoresSala[ID];
                ID++;
            }
        }

        Sala espera = new Sala(TipoSala.ESPERA, ID);
        espera.posicionPaciente = targetMostradorPaciente[0];
        espera.posicionProfesional = targetEspera[0];
        espera.posicionProfesionalSala = targetEspera[2];
        espera.posicionLimpiador = targetLimpiadoresSala[targetLimpiadoresSala.Length-1];
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

        cirugiasSucias = salasSucias.FindAll((s) => s.tipo.Equals(TipoSala.CIRUGIA));
        if (cirugiasSucias.Count != 0)
        {
            for (int i = 0; i < targetLimpiadores.Length; i++)
            {
                if (!targetLimpiadores[i].libre)
                {
                    return;
                }
            }

            for (int i = 0; i < listaLimpiadores.Count; i++)
            {
                if (listaLimpiadores[i].salaLimpiando.tipo != TipoSala.CIRUGIA)
                {
                    listaLimpiadores[i].limpiarQuirofanoUrgente();
                    break;
                }
            }
        }

        ComprobarLibre(listaEsperaMedico, salas.FindAll((s) => s.tipo.Equals(TipoSala.MEDICO)));
        ComprobarLibre(listaEsperaCirugia, salas.FindAll((s) => s.tipo.Equals(TipoSala.CIRUGIA)));
        ComprobarLibre(listaEsperaEnfermeria, salas.FindAll((s) => s.tipo.Equals(TipoSala.ENFERMERIA)));
        LlamarLimpiadores();

        aforo = false;
        for (int i = 0; i < targetColaDentro.Length; i++)
        {
            if (targetColaDentro[i].libre)
            {
                aforo = true;
            }
        }
    }

    public void CrearProfesionales()
    {
        for(int i=0; i < numCeladores; i++)
        {
            prefabCelador.GetComponent<Celador>().turnoSala = !prefabCelador.GetComponent<Celador>().turnoSala;
            Personaje nuevo = Instantiate(prefabCelador, casa.transform.position,Quaternion.identity).GetComponent<Personaje>();
            nuevo.nombre = "Celador " + i;
            sc.AnhadirProfesional(nuevo);
           
        }
        for (int i = 0; i < numEnfermeros; i++)
        {
            Enfermero nuevo = Instantiate(prefabEnfermero, casa.transform.position, Quaternion.identity).GetComponent<Enfermero>();
            nuevo.banhoTarget = banhos[i];
            nuevo.gameObject.GetComponent<Personaje>().nombre = "Enfermero " + i;
            sc.AnhadirProfesional(nuevo.GetComponent<Personaje>());
        }
        for (int i = 0; i < numMedicos; i++)
        {
            Personaje nuevo = Instantiate(prefabMedico, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
            nuevo.nombre = "Medico " + i;
            sc.AnhadirProfesional(nuevo);
        }
        for (int i = 0; i < numLimpiadores; i++)
        {
            Personaje nuevo = Instantiate(prefabLimpiador, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
            nuevo.nombre = "Limpiador " + i;
            sc.AnhadirProfesional(nuevo);
        }
        for (int i = 0; i < numCirujanos; i++)
        {
            Personaje nuevo = Instantiate(prefabCirujano, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
            nuevo.nombre = "Cirujano " + i;
            sc.AnhadirProfesional(nuevo);
        }
        StartCoroutine(SpawnPacientes());
    }
    public void ReemplazarEnfermero(TargetUrgencias banho, string nombre)
    {
        Enfermero nuevo = Instantiate(prefabEnfermero, casa.transform.position, Quaternion.identity).GetComponent<Enfermero>();
        nuevo.banhoTarget = banho;
        nuevo.GetComponent<Personaje>().nombre = nombre;
        sc.AnhadirProfesional(nuevo.GetComponent<Personaje>());
    }
    public void ReemplazarLimpiador(string nombre)
    {
        Personaje nuevo = Instantiate(prefabLimpiador, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
        nuevo.nombre = nombre;
        sc.AnhadirProfesional(nuevo);
    }
    public void ReemplazarCelador(string nombre)
    {
        Personaje nuevo = Instantiate(prefabCelador, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
        nuevo.nombre = nombre;
        sc.AnhadirProfesional(nuevo);
    }
    public void ReemplazarMedico(string nombre)
    {
        Personaje nuevo = Instantiate(prefabMedico, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
        nuevo.nombre = nombre;
        sc.AnhadirProfesional(nuevo);
    }
    public void ReemplazarCirujano(string nombre)
    {
        Personaje nuevo = Instantiate(prefabCirujano, casa.transform.position, Quaternion.identity).GetComponent<Personaje>();
        nuevo.nombre = nombre;
        sc.AnhadirProfesional(nuevo);
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
    public void AddSalaSucia(Sala sala)
    {
        salasSucias.Add(sala);
    }
    public void SalaCirugiaSucia(Sala sala)
    {
        sala.sucio = true;
        sala.porcentajeSuciedad = 100;
        
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
    IEnumerator SpawnPacientes()
    {
        while (true)
        {
            float time = Random.Range(spawnMinTime, spawnMaxTime);
            yield return new WaitForSeconds(time);
            yield return new WaitUntil(()=>contPacientes < MAX_PACIENTES);
            contPacientes++;
            Paciente nuevo= Instantiate(prefabPaciente, casaPaciente.transform.position,Quaternion.identity).GetComponent<Paciente>();
            int enfermedad = (int)Random.Range(0, enfermedades.Count-4);
            Enfermedad aux = enfermedades[enfermedad];
            nuevo.setEnfermedad(aux, emoticonoEnfermedad[(int)aux.tipoEnfermedad]);
            
        }
    }
}
