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
    public Sprite emoOperacion,emoExaminar,emoCasa,emoSucio,emoEsperarPaciente;
    //public bool ponerQuirofanoSucio = false;

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
        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", () => PutEmoji(emoEsperarPaciente));
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        operandoPaciente = myFSM.CreateState("operandoPaciente", operandoPacienteAction);
        llamarLimpiador = myFSM.CreateState("llamarLimpiador", llamarLimpiadorAction);
        esperarLimpiador = myFSM.CreateState("esperarLimpiador", ()=> PutEmoji(emoSucio));
        casaFin = myFSM.CreateState("casaFin", () => Destroy(this.gameObject));

        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => targetPaciente.ocupado); //Mirar
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si termina el tiempo de operar
        Perception terminarOperar = myFSM.CreatePerception<TimerPerception>(timeOperar);
        //Si el puesto de trabajo está libre, ir hacia el
        Perception comienzaJornada = myFSM.CreatePerception<PushPerception>();
        //Cuando termina de examinar a un paciente, con un timer
        Perception terminarExaminar = myFSM.CreatePerception<TimerPerception>(timeExaminar);
        //El limpiador debe llamar a esta función con un Push
        Perception llamadoLimpiador = myFSM.CreatePerception<PushPerception>();
        //El cirujano comprueba si la sala está sucia
        Perception salaLimpia = myFSM.CreatePerception<ValuePerception>(() => !sala.sucio); 
        //Si el personaje ha llegado a la casa
        Perception llegadaCasa = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Si el personaje llega  al puesto de trabajo
        Perception llegadaPuesto = myFSM.CreatePerception<ValuePerception>(()=> personaje.haLlegado);

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
            for (int i = 0; i < quirofanos.Count; i++)
            {
                if (quirofanos[i].libre)
                {
                    sala = quirofanos[i];
                    targetUrgencias = sala.posicionProfesional;
                    targetPaciente = sala.posicionProfesional;
                    myFSM.Fire("comienza jornada");
                    return;
                }
            }
        }
        /*
        if (ponerQuirofanoSucio)
        {
            ponerQuirofanoSucio = false;
            mundo.SalaCirugiaSucia(sala);
        }
        */
    }

    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }

    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto
        targetUrgencias.libre = false;
        sala.libre = false;
        personaje.GoTo(targetUrgencias.transform);
    }

    private void irCasaAction()
    {
        //Go to target casa
        targetUrgencias.libre = true;
        sala.libre = true;
        personaje.GoTo(mundo.casa.transform);
        PutEmoji(emoCasa);

    }

    private void examinandoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        //Do animacion examinar
        PutEmoji(emoExaminar);
    }

    private void operandoPacienteAction()
    {
        //Do animacion operar
        PutEmoji(emoOperacion);
    }

    private void llamarLimpiadorAction()
    {
        mundo.SalaCirugiaSucia(sala);
        myFSM.Fire("llamado limpiador");
    }


}
