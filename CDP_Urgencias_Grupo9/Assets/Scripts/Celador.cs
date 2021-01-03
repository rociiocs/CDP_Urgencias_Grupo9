using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celador : MonoBehaviour
{
    //Variables
    public int timeJornada = 2000;
    int timeAtender = 5;
    int timeTurno = 1000;
    public bool turnoSala;

    Personaje personaje;
    TargetUrgencias targetUrgenciasMostrador;
    TargetUrgencias targetUrgenciasSala;
    TargetUrgencias targetPaciente;
    List<Sala> puestos;

    public Image emoticono;
    public Sprite emoAtender, emoCasa, emoEsperarPaciente;

    Sala sala;
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
        puestos = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.ESPERA));

        //Create states
        mostrador = myFSM.CreateSubStateMachine("mostrador", myFSMMostrador);
        salaState = myFSM.CreateSubStateMachine("salaState", myFSMSala);

        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);//Se emplea no solo al llegar sino para cambiar de turno
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSMMostrador.CreateEntryState("esperarPaciente", () => PutEmoji(emoEsperarPaciente));
        atendiendoPaciente = myFSMMostrador.CreateState("atendiendoPaciente", atendiendoPacienteAction);
        esperandoCompañero = myFSM.CreateState("esperandoCompañero", esperandoCompañeroAction); ;
        atendiendoUrgente = myFSMSala.CreateState("atendiendoUrgente", atendiendoUrgenteAction);
        paseandoSala = myFSMSala.CreateEntryState("paseandoSala", () => PutEmoji(emoEsperarPaciente));
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); mundo.ReemplazarCelador(personaje.nombre); Destroy(this.gameObject); });

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSMMostrador.CreatePerception<ValuePerception>(() => targetPaciente.ocupado); //Mirar
        //Si hay un paciente urgente que atender
        Perception urgenteAtender = myFSMSala.CreatePerception<ValuePerception>(() => targetPaciente.ocupado); //Necesito un tipo de target para los urgentes
        //Si se produce cambio de turno
        Perception cambioTurno = myFSM.CreatePerception<TimerPerception>(timeTurno);
        //Si hay un puesto libre donde voy a cambiar
        Perception huecoLibre = myFSM.CreatePerception<PushPerception>();//Un target de la sala o del mostrador
        //Si hay un puesto libre y voy a cambiar de turno 
        Perception huecoYTurno = myFSM.CreatePerception<PushPerception>();// () => (targetPaciente.ocupado&&cambioTurno));
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
        myFSM.CreateTransition("cambio de turnoMS", mostrador, cambioTurno, salaState);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("cambio de turnoSM", salaState, cambioTurno, mostrador);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("hueco libre", esperandoCompañero, huecoLibre, irPuestoTrabajo);
        myFSM.CreateTransition("cambio directoMS", mostrador, huecoYTurno, salaState);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("cambio directoSM", salaState, huecoYTurno, mostrador);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("terminada jornada mostrador", mostrador, terminadaJornada, irCasa);
        myFSM.CreateTransition("terminada jornada sala", salaState, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);

    }

    // Update is called once per frame
    void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            //Si el puesto de trabajo está libre
            //Habría que distinguir de alguna forma los puestos de mostrador y de sala
            for (int i = 0; i < puestos.Count; i++)
            {
                if (!turnoSala)
                {
                    if (puestos[i].posicionProfesional.libre)
                    {
                        sala = puestos[i];
                        targetUrgenciasMostrador = sala.posicionProfesional;
                        targetUrgenciasSala = sala.posicionProfesionalSala;
                        targetPaciente = sala.posicionPaciente;
                        myFSM.Fire("comienza jornada");
                        puestos[i].posicionProfesional.libre = false;
                        return;
                    }
                }
                else
                {
                    if (puestos[i].posicionProfesionalSala.libre)
                    {
                        sala = puestos[i];
                        targetUrgenciasMostrador = sala.posicionProfesional;
                        targetUrgenciasSala = sala.posicionProfesionalSala;
                        targetPaciente = sala.posicionPaciente;
                        myFSM.Fire("comienza jornada");
                        puestos[i].posicionProfesionalSala.libre = false;
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

    private void esperandoCompañeroAction()
    {
        //cambio el tipo de turno
        turnoSala = !turnoSala;
    }
    private void atendiendoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
        paciente.myFSMVivo.Fire("esperando sala");

    }
    private void atendiendoUrgenteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
    }
}
