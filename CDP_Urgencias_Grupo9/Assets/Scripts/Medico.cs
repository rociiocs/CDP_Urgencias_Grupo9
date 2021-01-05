using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Medico : MonoBehaviour
{
   
    //Variables
    public int timeJornada = 2000;
    int timeExaminar = 2;
    int timeDespachar = 1;


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
        esperarPaciente = myFSM.CreateState("esperarPaciente", () => {PutEmoji(emoEsperarPaciente); sala.libre = true;despachandoPacienteAction(); });
        examinandoPaciente = myFSM.CreateState("examinandoPaciente", examinandoPacienteAction);
        //despacharPaciente = myFSM.CreateState("despacharPaciente", despachandoPacienteAction);
        despacharPaciente = myFSM.CreateState("despacharPaciente");
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje);mundo.ReemplazarMedico(personaje.nombre); Destroy(this.gameObject); });


        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => !targetPaciente.ocupado);
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
        //Debug.Log(myFSM.GetCurrentState().Name);
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            //Si el puesto de trabajo está libre
            for (int i = 0; i < oficinas.Count; i++)
            {
                if (oficinas[i].posicionProfesional.libre)
                {
                    oficinas[i].posicionProfesional.libre = false;
                    sala = oficinas[i];
                    targetUrgencias = sala.posicionProfesional;
                    targetPaciente = sala.posicionPaciente;
                    targetPaciente.ocupado = true;
                    myFSM.Fire("comienza jornada");
                    
                    return;
                }
            }
        }
        if (paciente != null)
        {
            Debug.Log(paciente.myFSMVivo.GetCurrentState().Name);
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
        if (personaje.haLlegado)
        {
            personaje.sentarse();
        }
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
        if (targetPaciente.actual != null)
        {
            Debug.Log(myFSM.GetCurrentState().Name);
            paciente = targetPaciente.actual.GetComponent<Paciente>();
            enfermedad = paciente.enfermedad;
            PutEmoji(emoExaminar);
            paciente.siguientePaso();
        }
    }

    //private void despachandoPacienteAction()
    //{
    //    Debug.Log(myFSM.GetCurrentState().Name);
    //    //Enviar paciente a casa/UCI/enfermería, según la enfermedad y el paso dentro de la misma, usando el método del paciente
    //    if (paciente != null)
    //    {
            
    //        if (paciente.pasoActual == Paso.Casa)
    //        {
    //            paciente.soyLeve.Fire();
    //        }
    //        else
    //        {
    //            mandarPacienteListaEspera();
    //            paciente.todaviaTengoQueSerTratado.Fire();
    //        }
    //        sala.libre = true;
    //        paciente = null;
    //    }
    //}
    private void despachandoPacienteAction()
    {
        Debug.Log(myFSM.GetCurrentState().Name);
        //Enviar paciente a casa/UCI/enfermería, según la enfermedad y el paso dentro de la misma, usando el método del paciente
        if (paciente != null)
        {

            if (paciente.pasoActual == Paso.Casa)
            {
                paciente.soyLeve.Fire();
            }
            else
            {
                mandarPacienteListaEspera();
                paciente.todaviaTengoQueSerTratado.Fire();
            }
            //sala.libre = true;
            paciente = null;
            targetPaciente.ocupado = true;
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
