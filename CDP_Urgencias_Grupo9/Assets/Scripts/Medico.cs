using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Medico : MonoBehaviour
{
    //Variables
    public int timeJornada = 2000;
    int timeExaminar = 5;
    int timeDespachar = 5;


    Personaje personaje;
    TargetUrgencias targetUrgencias;
    TargetUrgencias targetPaciente;
    List<Sala> oficinas;

    public Image emoticono;
    public Sprite emoExaminar, emoCasa, emoEsperarPaciente;

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
    State casaFin;

    void Start()
    {
        myFSM = new StateMachineEngine();
        mundo = GetComponentInParent<Mundo>();
        personaje = GetComponent<Personaje>();
        oficinas = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.MEDICO));

        //Create states
        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", () => PutEmoji(emoEsperarPaciente));
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        despacharPaciente = myFSM.CreateState("despacharPaciente", despachandoPacienteAction);
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); Destroy(this.gameObject); });


        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => targetPaciente.ocupado); //Mirar
        //Si hay un paciente delante
        Perception terminarDespachar = myFSM.CreatePerception<TimerPerception>(timeDespachar);//puede que sea value si se usa animación
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si el puesto de trabajo está libre, ir hacia él
        Perception comienzaJornada = myFSM.CreatePerception<PushPerception>();
        //Cuando termina de examinar a un paciente, con un timer,
        Perception terminarExaminar = myFSM.CreatePerception<TimerPerception>(timeExaminar);//puede que sea value si se usa animación
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);

        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPuestoTrabajo);
        myFSM.CreateTransition("llegar puesto trabajo", irPuestoTrabajo, llegadaPuesto, esperarPaciente);
        myFSM.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, examinandoPaciente);
        myFSM.CreateTransition("examinacion completada", examinandoPaciente, terminarExaminar, despacharPaciente);
        myFSM.CreateTransition("paciente despachado", despacharPaciente, terminarDespachar, esperarPaciente);
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
            for (int i = 0; i < oficinas.Count; i++)
            {
                if (oficinas[i].libre)
                {
                    sala = oficinas[i];
                    targetUrgencias = sala.posicionProfesional;
                    targetPaciente = sala.posicionPaciente;
                    myFSM.Fire("comienza jornada");
                    return;
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
        targetUrgencias.libre = false;
        sala.libre = false;
        personaje.GoTo(targetUrgencias);
    }

    private void irCasaAction()
    {
        //Go to target casa
        targetUrgencias.libre = true;
        sala.libre = true;
        personaje.GoTo(mundo.casa);
        PutEmoji(emoCasa);
    }

    private void examinandoPacienteAction()
    {
        //Coger referencia paciente
        enfermedad = paciente.enfermedad;
        PutEmoji(emoExaminar);

    }

    private void despachandoPacienteAction()
    {
        //Enviar paciente a casa/UCI/enfermería, según la enfermedad y el paso dentro de la misma, usando el método del paciente
        paciente.siguientePaso();
    }
}
