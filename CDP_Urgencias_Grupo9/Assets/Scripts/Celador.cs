using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celador : MonoBehaviour
{
    //Variables
    public int timeJornada = 30;
    int timeAtender = 5;
    int timeTurno = 10;
    int idMostrador;
    int idSala;
    public bool turnoSala;

    Personaje personaje;
    TargetUrgencias targetUrgenciasMostrador;
    TargetUrgencias targetUrgenciasSala;
    TargetUrgencias targetPaciente;
    TargetUrgencias targetPacienteSala;
    SalaEspera sala;

    public Image emoticono;
    public Sprite emoAtender, emoCasa, emoEsperarPaciente;

    Paciente paciente;
    Enfermedad enfermedad;
    Mundo mundo;
    //Maquina de estados
    StateMachineEngine myFSM;
    StateMachineEngine myFSMMostrador;
    StateMachineEngine myFSMSala;

    //Estados
    State casa;
    State mostrador;
    State salaState;
    State irPuestoTrabajo;
    State irCasa;
    State esperarPaciente;
    State atendiendoPaciente;
    State esperandoCompañero;
    State atendiendoUrgente;
    State paseandoSala;
    State casaFin;

    // Start is called before the first frame update
    void Start()
    {
        myFSM = new StateMachineEngine();
        myFSMMostrador = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMSala = new StateMachineEngine(BehaviourEngine.IsASubmachine);

        mundo = FindObjectOfType<Mundo>();
        personaje = GetComponent<Personaje>();
        sala =  (SalaEspera) mundo.salas.Find((s) => s.tipo.Equals(TipoSala.ESPERA));

        //Create states
        mostrador = myFSM.CreateSubStateMachine("mostrador", myFSMMostrador);
        salaState = myFSM.CreateSubStateMachine("salaState", myFSMSala);

        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);//Se emplea no solo al llegar sino para cambiar de turno
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSMMostrador.CreateEntryState("esperarPaciente", () => { PutEmoji(emoEsperarPaciente); turnoSala = false; });
        atendiendoPaciente = myFSMMostrador.CreateState("atendiendoPaciente", atendiendoPacienteAction);
        esperandoCompañero = myFSM.CreateState("esperandoCompañero");
        atendiendoUrgente = myFSMSala.CreateState("atendiendoUrgente", atendiendoUrgenteAction);
        paseandoSala = myFSMSala.CreateEntryState("paseandoSala", () => esperandoPacienteAction());
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); mundo.ReemplazarCelador(personaje.nombre); Destroy(this.gameObject); });

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSMMostrador.CreatePerception<ValuePerception>(() => targetPaciente.ocupado);
        //Si hay un paciente urgente que atender
        Perception urgenteAtender = myFSMSala.CreatePerception<ValuePerception>(() => targetPacienteSala.ocupado);
        //Si se produce cambio de turno
        Perception cambioTurno = myFSM.CreatePerception<TimerPerception>(timeTurno);
        //Si hay un puesto libre donde voy a cambiar
        Perception huecoLibre = myFSM.CreatePerception<ValuePerception>(()=>ComprobarLibre());
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si el puesto de trabajo está libre, ir hacia él
        Perception comienzaJornada = myFSM.CreatePerception<PushPerception>();
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarAtender = myFSMMostrador.CreatePerception<TimerPerception>(timeAtender);//puede que sea value si se usa animación
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarUrgente = myFSMSala.CreatePerception<TimerPerception>(timeAtender);//se usa el mismo timer para atender en mostrador y en sala, dependerá de la animación
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);

        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPuestoTrabajo);
        myFSM.CreateTransition("llegar puesto trabajo mostrador", irPuestoTrabajo, llegadaPuesto, mostrador);
        myFSM.CreateTransition("llegar puesto trabajo sala", irPuestoTrabajo, llegadaPuesto, salaState);
        myFSMMostrador.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, atendiendoPaciente);
        myFSMSala.CreateTransition("llega urgente", paseandoSala, urgenteAtender, atendiendoUrgente);
        myFSMMostrador.CreateTransition("atencion completada", atendiendoPaciente, terminarAtender, esperarPaciente);
        myFSMSala.CreateTransition("urgente completada", atendiendoUrgente, terminarUrgente, paseandoSala);
        myFSM.CreateTransition("cambio de turnoMS", mostrador, cambioTurno, esperandoCompañero);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("cambio de turnoSM", salaState, cambioTurno, esperandoCompañero);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("hueco libre", esperandoCompañero, huecoLibre, irPuestoTrabajo);
        myFSM.CreateTransition("terminada jornada mostrador", mostrador, terminadaJornada, irCasa);
        myFSM.CreateTransition("terminada jornada sala", salaState, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);

    }

    // Update is called once per frame
    void Update()
    {
        myFSM.Update();
        for(int i=0; i<sala.posicionMostradorProfesional.Length; i++)
        {
            //Debug.Log("Mostrador " + i + sala.posicionMostradorProfesional[i].libre);
        }
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            //Si el puesto de trabajo está libre
            //Habría que distinguir de alguna forma los puestos de mostrador y de sala
            if (!turnoSala)
            {
                for (int i = 0; i < sala.posicionMostradorProfesional.Length; i++)
                {
                    if (sala.posicionMostradorProfesional[i].libre)
                    {
                        idMostrador = i;
                        targetUrgenciasMostrador = sala.posicionMostradorProfesional[i];
                        targetPaciente = sala.posicionMostradorPaciente[i];
                        myFSM.Fire("comienza jornada");
                        sala.posicionMostradorProfesional[i].libre = false;
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < sala.posicionSalaProfesional.Length; i++)
                {
                    if (sala.posicionSalaProfesional[i].libre)
                    {
                        idSala = i;
                        targetUrgenciasSala = sala.posicionSalaProfesional[i];
                        targetPacienteSala = sala.posicionSalaPaciente[i];
                        myFSM.Fire("comienza jornada");
                        sala.posicionSalaProfesional[i].libre = false;
                        return;
                    }
                }
            }
        }
    }
    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }
    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto
        sala.libre = false;
        if (!turnoSala)
        {
            targetUrgenciasMostrador.libre = false;
            personaje.GoTo(targetUrgenciasMostrador);
        }
        else
        {
            targetUrgenciasSala.libre = false;
            personaje.GoTo(targetUrgenciasSala);
        }
    }

    private void irCasaAction()
    {
        if (turnoSala) { targetUrgenciasSala.libre = true; } else { targetUrgenciasMostrador.libre = true; }
        
        sala.libre = true;
        personaje.GoTo(mundo.casa);
        PutEmoji(emoCasa);
    }
    //REVISAR
    private void atendiendoPacienteAction()
    {
        paciente = targetPaciente.actual.GetComponent<Paciente>();
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
        //AÑADIR AL PACIENTE A LAS COLAS DEL MUNDO PERTINENTES
    }
    private void esperandoPacienteAction()
    {
        turnoSala = true;
        paciente.myFSMVivo.Fire("esperar sala libre");
        PutEmoji(emoEsperarPaciente);
    }
    private void atendiendoUrgenteAction()
    {
        paciente = targetPacienteSala.actual.GetComponent<Paciente>();
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
        paciente.myFSMVivo.Fire("esperar sala libre");
    }
    //REVISAR, NO ESTÁ ENCONTRANDO LOS HUECOS LIBRES
    private bool ComprobarLibre()
    {

        if (!turnoSala)
        {
            targetUrgenciasMostrador.libre = true;
            sala.posicionMostradorProfesional[idMostrador].libre = true;
            for (int i = 0; i < sala.posicionSalaProfesional.Length; i++)
            {
                if (sala.posicionSalaProfesional[i].libre)
                {
                    
                    targetUrgenciasSala = sala.posicionSalaProfesional[i];
                    targetPacienteSala = sala.posicionSalaPaciente[i];
                    sala.posicionSalaProfesional[i].libre = false;
                    return true;
                }
            }
        }
        else
        {
            targetUrgenciasSala.libre = true;
            sala.posicionSalaProfesional[idSala].libre = true;
            for (int i = 0; i < sala.posicionMostradorProfesional.Length; i++)
            {
                if (sala.posicionMostradorProfesional[i].libre)
                {
                    
                    targetUrgenciasMostrador = sala.posicionMostradorProfesional[i];
                    targetPaciente = sala.posicionMostradorPaciente[i];
                    sala.posicionMostradorProfesional[i].libre = false;
                    return true;
                }
            }
        }
        return false;
    }
}
