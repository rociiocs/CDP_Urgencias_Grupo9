using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cirujano : MonoBehaviour
{
    //Variables
    public int timeJornada = 2000;
    int timeOperar = 10;
    int timeExaminar = 5;

    Personaje personaje;
    TargetUrgencias targetUrgencias;
    TargetUrgencias targetPaciente;
    List<Sala> quirofanos;
    public Image emoticono;
    public Sprite emoOperacion,emoExaminar,emoCasa;

    Sala sala;
    Paciente paciente;
    Enfermedad enfermedad;
    Mundo mundo;
    //Maquina de estados
    StateMachineEngine myFSM;

    //Estados
    State casa;
    State casaFin;
    State irPuestoTrabajo;
    State irCasa;
    State esperarPaciente;
    State examinandoPaciente;
    State operandoPaciente;
    State llamarLimpiador;
    State esperarLimpiador;

    void Start()
    {
        
        mundo = GetComponentInParent<Mundo>();
        personaje = GetComponent<Personaje>();
        quirofanos = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.CIRUGIA));
        myFSM = new StateMachineEngine();

        //Create states
        casa = myFSM.CreateEntryState("casa", casaAction);
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", esperarPacienteAction);
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        operandoPaciente = myFSM.CreateState("operandoPaciente", operandoPacienteAction);
        llamarLimpiador = myFSM.CreateState("llamarLimpiador", llamarLimpiadorAction);
        esperarLimpiador = myFSM.CreateState("esperarLimpiador", esperarLimpiadorAction);
        casaFin = myFSM.CreateState("casaFin", casaFinAction);

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si termina el tiempo de operar
        Perception terminarOperar = myFSM.CreatePerception<TimerPerception>(timeOperar);
        //Si el puesto de trabajo está libre, ir hacia el
        Perception comienzaJornada = myFSM.CreatePerception<PushPerception>();
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
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);

    }

    // Update is called once per frame
    void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            //Si el puesto de trabajo está libre
            for (int i = 0; i < mundo.targetCirujano.Length; i++)
            {
                if (mundo.targetCirujano[i].libre)
                {
                    targetUrgencias = mundo.targetCirujano[i];
                    targetPaciente = mundo.targetCirujanoPaciente[i];
                    sala = quirofanos[i];
                    myFSM.Fire("comienza jornada");
                    return;
                }
            }
        }else if (myFSM.GetCurrentState().Name.Equals(irPuestoTrabajo.Name))
        {
            if (personaje.haLlegado)
            {
                myFSM.Fire("llegar puesto trabajo");
            }
        }else if (myFSM.GetCurrentState().Name.Equals(esperarPaciente.Name))
        {
            //Booleano que pone el paciente
            if (targetPaciente.ocupado)
            {
                myFSM.Fire("llega paciente");
            }
        }else if (myFSM.GetCurrentState().Name.Equals(esperarLimpiador.Name))
        {
            if (!sala.sucio)
            {
                myFSM.Fire("sala limpia");
            }
        }else if (myFSM.GetCurrentState().Name.Equals(irCasa.Name))
        {
            if (personaje.haLlegado)
            {
                myFSM.Fire("llegada casa");
            }
        }
    }

    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto
        
        targetUrgencias.libre = false;
        personaje.GoTo(targetUrgencias.transform);
    }

    private void irCasaAction()
    {
        //Go to target casa
        targetUrgencias.libre = true;
        personaje.GoTo(mundo.casa.transform);
        emoticono.sprite = emoCasa;

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
        emoticono.sprite = emoExaminar;
    }

    private void operandoPacienteAction()
    {
        //Do animacion operar
         emoticono.sprite = emoOperacion;
        
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
        
    }

    public void casaFinAction()
    {
        Destroy(this.gameObject);
    }
}
