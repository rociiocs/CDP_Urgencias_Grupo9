using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Celador : MonoBehaviour
{
    //Variables
    float velGiro;
    float velMovement;
    int timeJornada = 2000;
    int timeAtender = 5;
    int timeTurno = 1000;
    public bool turnoSala;
    Vector3 puestoTrabajo;
    Sala sala;
    Paciente paciente;
    Enfermedad enfermedad;
    Mundo mundo;
    //Maquina de estados
    StateMachineEngine myFSM;

    //Estados
    State casa;
    State irPuestoTrabajo;
    State irCasa;
    State esperarPaciente;
    State atendiendoPaciente;
    State esperandoCompañero;
    State atendiendoUrgente;
    State paseandoSala;

    // Start is called before the first frame update
    void Start()
    {
        myFSM = new StateMachineEngine();

        //Create states
        casa = myFSM.CreateEntryState("casa", casaAction);
        irPuestoTrabajo = myFSM.CreateEntryState("irPuestoTrabajo", irPuestoTrabajoAction);//Se emplea no solo al llegar sino para cambiar de turno
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", esperarPacienteAction);
        atendiendoPaciente = myFSM.CreateState("atendiendoPaciente", atendiendoPacienteAction);
        esperandoCompañero = myFSM.CreateState("esperandoCompañero", esperandoCompañeroAction); ;
        atendiendoUrgente = myFSM.CreateState("atendiendoUrgente", atendiendoUrgenteAction);
        paseandoSala = myFSM.CreateState("paseandoSala", paseandoSalaAction);

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si hay un paciente urgente que atender
        Perception urgenteAtender = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si se produce cambio de turno
        Perception cambioTurno = myFSM.CreatePerception<TimerPerception>(timeTurno);
        //Si hay un puesto libre donde voy a cambiar
        Perception huecoLibre = myFSM.CreatePerception<WatchingPerception>();
        //Si hay un puesto libre y voy a cambiar de turno 
        Perception huecoYTurno = myFSM.CreatePerception<ValuePerception>();
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si el puesto de trabajo está libre, ir hacia él
        Perception comienzaJornada = myFSM.CreatePerception<ValuePerception>();
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarAtender = myFSM.CreatePerception<TimerPerception>(timeAtender);//puede que sea value si se usa animación
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarUrgente = myFSM.CreatePerception<TimerPerception>(timeAtender);//se usa el mismo timer para atender en mostrador y en sala, dependerá de la animación
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<PushPerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<PushPerception>();

        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPuestoTrabajo);
        myFSM.CreateTransition("llegar puesto trabajo", irPuestoTrabajo, llegadaPuesto, esperarPaciente);
        myFSM.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, atendiendoPaciente);
        myFSM.CreateTransition("llega urgente", paseandoSala, urgenteAtender, atendiendoUrgente);
        myFSM.CreateTransition("atencion completada", atendiendoPaciente, terminarAtender, esperarPaciente);
        myFSM.CreateTransition("urgente completada", atendiendoUrgente, terminarUrgente, paseandoSala);
        myFSM.CreateTransition("cambio de turnoMS", esperarPaciente, cambioTurno, esperandoCompañero);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("cambio de turnoSM", paseandoSala, cambioTurno, esperandoCompañero);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("hueco libre", esperandoCompañero, huecoLibre, irPuestoTrabajo);
        myFSM.CreateTransition("cambio directoMS", esperarPaciente, huecoYTurno, paseandoSala);//No se si hay alguna forma de hacerlo bidireccional
        myFSM.CreateTransition("cambio directoSM", paseandoSala, huecoYTurno, esperarPaciente);
        myFSM.CreateTransition("terminada jornada", esperarPaciente, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casa);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto
        myFSM.Fire("llegar puesto trabajo");
    }

    private void irCasaAction()
    {
        //Go to target casa
        myFSM.Fire("llegada casa");
    }

    private void esperarPacienteAction()
    {
        //Si hay paciente
        myFSM.Fire("llega paciente");
        //Si acaba la jornada
        myFSM.Fire("terminada jornada");
    }
    private void paseandoSalaAction()
    {
        //Si hay urgente
        myFSM.Fire("llega urgente");
        //Si acaba la jornada
        myFSM.Fire("terminada jornada");
    }
    private void esperandoCompañeroAction()
    {
        //Si hay hueco libre
        myFSM.Fire("hueco libre");
        //cambio el tipo de turno
        turnoSala = !turnoSala;
    }
    private void atendiendoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        //Do animacion examinar/esperar fin timer
        myFSM.Fire("atencion completada");
    }
    private void atendiendoUrgenteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        //Do animacion examinar/esperar fin timer
        myFSM.Fire("urgente completada");
    }
    private void casaAction()
    {
        //Si el puesto de trabajo está libre
        myFSM.Fire("comienza jornada");
    }
}
