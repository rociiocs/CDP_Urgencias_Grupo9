using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Medico : MonoBehaviour
{

    //Variables
    public int timeJornada = 40;
    int timeExaminar = 5;
    int timeDespachar = 1;

    //Referencias
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
        mundo = FindObjectOfType<Mundo>();
        personaje = GetComponent<Personaje>();
        oficinas = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.MEDICO));

        //Create states
        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajo = myFSM.CreateState("irPuestoTrabajo", irPuestoTrabajoAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSM.CreateState("esperarPaciente", EsperarPacienteAction);
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        despacharPaciente = myFSM.CreateState("despacharPaciente", despachandoPacienteAction);
        casaFin = myFSM.CreateState("casaFin", CasaFinAction);


        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => !targetPaciente.ocupable);
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

    void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {

            for (int i = 0; i < oficinas.Count; i++)
            {
                if (oficinas[i].posicionProfesional.libre)
                {
                    oficinas[i].posicionProfesional.libre = false;
                    sala = oficinas[i];
                    targetUrgencias = sala.posicionProfesional;
                    targetPaciente = sala.posicionPaciente;

                    if (targetPaciente.actual == null)
                        targetPaciente.ocupable = true;
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
    private void EsperarPacienteAction()
    {
        PutEmoji(emoEsperarPaciente);
        sala.libre = true;
    }
    private void CasaFinAction()
    {
        FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje);
        mundo.ReemplazarMedico(personaje.nombre);
        Destroy(this.gameObject);
    }
    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto
        targetUrgencias.libre = false;
        sala.libre = false;
        personaje.GoTo(targetUrgencias);
        if (personaje.haLlegado)
        {
            personaje.sentarse();
        }
    }

    private void irCasaAction()
    {
        //Go to target casa
        targetUrgencias.libre = true;
        sala.libre = false;
        personaje.GoTo(mundo.casa);
        PutEmoji(emoCasa);
    }

    private void examinandoPacienteAction()
    {
        personaje.Hablando(true);
        //Coger referencia paciente
        if (targetPaciente.actual != null)
        {
            paciente = targetPaciente.actual.GetComponent<Paciente>();
            paciente.reiniciarTimerMorir();
            enfermedad = paciente.enfermedad;
            PutEmoji(emoExaminar);
            paciente.siguientePaso();
        }
    }


    private void despachandoPacienteAction()
    {
        //Enviar paciente a casa/UCI/enfermería, según la enfermedad y el paso dentro de la misma, usando el método del paciente
        personaje.Hablando(false);
        if (paciente != null)
        {
            if (paciente.pasoActual == Paso.Casa)
            {
                paciente.soyLeve.Fire();
            }
            else if (paciente.pasoActual == Paso.UCI)
            {
                paciente.soyGrave.Fire();
            }
            else
            {
                mandarPacienteListaEspera();
                paciente.todaviaTengoQueSerTratado.Fire();
            }
            paciente = null;
            targetPaciente.ocupable = true;
        }
    }
    private void mandarPacienteListaEspera()
    {
        switch (paciente.pasoActual)
        {
            case Paso.Cirujano:
                mundo.AddPacienteCirugia(paciente);
                break;
            case Paso.Enfermeria:
                mundo.AddPacienteEnfermeria(paciente);
                break;
            case Paso.Medico:
                mundo.AddPacienteMedico(paciente);
                break;
            default:
                break;
        }
    }
}
