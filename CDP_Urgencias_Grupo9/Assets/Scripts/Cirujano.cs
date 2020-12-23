using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cirujano : MonoBehaviour
{
    //Variables
    int timeJornada = 2000;
    int timeOperar = 10;
    int timeExaminar = 5;
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
    State operandoPaciente;
    State llamarLimpiador;
    State esperarLimpiador;

    void Start()
    {
        myFSM = new StateMachineEngine();

        //Create states
        casa = myFSM.CreateEntryState("casa", casaAction);
        irPuestoTrabajo = myFSM.CreateEntryState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", esperarPacienteAction);
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        operandoPaciente = myFSM.CreateState("operandoPaciente", operandoPacienteAction);
        llamarLimpiador = myFSM.CreateState("llamarLimpiador", llamarLimpiadorAction);
        esperarLimpiador = myFSM.CreateState("esperarLimpiador", esperarLimpiadorAction);

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si termina el tiempo de operar
        Perception terminarOperar = myFSM.CreatePerception<TimerPerception>(timeOperar);
        //Si el puesto de trabajo está libre, ir hacia el
        Perception comienzaJornada = myFSM.CreatePerception<ValuePerception>();
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarExaminar = myFSM.CreatePerception<TimerPerception>(timeExaminar);
        //El limpiador debe llamar a esta función con un Push
        Perception llamadoLimpiador = myFSM.CreatePerception<PushPerception>();
        //El limpiador debe llamar a esta función con un Push
        Perception salaLimpia = myFSM.CreatePerception<PushPerception>(); 
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<PushPerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<PushPerception>();

        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPuestoTrabajo);
        myFSM.CreateTransition("llegar puesto trabajo", irPuestoTrabajo, llegadaPuesto, esperarPaciente);
        myFSM.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, examinandoPaciente);
        myFSM.CreateTransition("examinacion completada", examinandoPaciente, terminarExaminar, operandoPaciente);
        myFSM.CreateTransition("terminar operar", operandoPaciente, terminarOperar, llamarLimpiador);
        myFSM.CreateTransition("llamado limpiador", llamarLimpiador, llamadoLimpiador, esperarLimpiador);
        myFSM.CreateTransition("sala limpia", esperarLimpiador, salaLimpia, esperarPaciente);
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
    }

    private void irCasaAction()
    {
        //Go to target casa
    }

    private void esperarPacienteAction()
    {
        //Si hay paciente
    }

    private void examinandoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        //Do animacion examinar
    }

    private void operandoPacienteAction()
    {
        //Do animacion operar
    }

    private void esperarLimpiadorAction()
    {
        //El limpiador debe poner push cuando termine de limpiar
    }

    private void llamarLimpiadorAction()
    {
        mundo.SalaCirugiaSucia(sala);
        myFSM.Fire("llamado limpiador");
    }

    private void casaAction()
    {
        //Si el puesto de trabajo está libre
        myFSM.Fire("comienza jornada");
    }
}
