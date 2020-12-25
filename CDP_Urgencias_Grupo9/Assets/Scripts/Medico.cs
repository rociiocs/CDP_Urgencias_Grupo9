using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medico : MonoBehaviour
{
    //Variables
    float velGiro;
    float velMovement;
    int timeJornada = 2000;
    int timeExaminar = 5;
    int timeDespachar = 5; 
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
    State examinandoPaciente;
    State despacharPaciente;

    void Start()
    {
        myFSM = new StateMachineEngine();

        //Create states
        casa = myFSM.CreateEntryState("casa", casaAction);
        irPuestoTrabajo = myFSM.CreateEntryState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", esperarPacienteAction);
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        despacharPaciente = myFSM.CreateState("despacharPaciente", despachandoPacienteAction);


        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si hay un paciente delante
        Perception terminarDespachar = myFSM.CreatePerception<TimerPerception>(timeDespachar);//puede que sea value si se usa animación
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si el puesto de trabajo está libre, ir hacia él
        Perception comienzaJornada = myFSM.CreatePerception<ValuePerception>();
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarExaminar = myFSM.CreatePerception<TimerPerception>(timeExaminar);//puede que sea value si se usa animación
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<PushPerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<PushPerception>();

        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPuestoTrabajo);
        myFSM.CreateTransition("llegar puesto trabajo", irPuestoTrabajo, llegadaPuesto, esperarPaciente);
        myFSM.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, examinandoPaciente);
        myFSM.CreateTransition("examinacion completada", examinandoPaciente, terminarExaminar, despacharPaciente);
        myFSM.CreateTransition("paciente despachado", despacharPaciente, terminarDespachar, esperarPaciente);
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

    private void examinandoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        //Do animacion examinar/esperar fin timer
        myFSM.Fire("examinacion completada");
    }

    private void despachandoPacienteAction()
    {
        //Enviar paciente a casa/UCI/enfermería, según la enfermedad y el paso dentro de la misma, usando el método del paciente
        //paciente.siguientePaso();
        //Hacer animación de despachar/esperar a fin del timer
        myFSM.Fire("paciente despachado");
    }

    private void casaAction()
    {
        //Si el puesto de trabajo está libre
        myFSM.Fire("comienza jornada");
    }
}
