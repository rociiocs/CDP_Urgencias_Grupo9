using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celador : MonoBehaviour
{
    //Variables
    public int timeJornada = 52;
    int timeAtender = 5;
    public int timeTurno =20;
    public bool turnoSala;

    //Referencias
    Personaje personaje;
    TargetUrgencias targetUrgenciasMostrador;
    TargetUrgencias targetUrgenciasSala;
    TargetUrgencias targetPaciente;
    TargetUrgencias targetPacienteSala;
    SalaEspera sala;
    public Image emoticono;
    public Sprite emoAtender, emoCasa, emoEsperarPaciente, emoCambio;
    Paciente paciente;
    Mundo mundo;
    //Maquina de estados
    StateMachineEngine myFSM;
    StateMachineEngine myFSMMostrador;
    StateMachineEngine myFSMSala;

    //Estados
    State casa;
    State mostrador;
    State salaState;
    State irPuestoTrabajoM;
    State irPuestoTrabajoS;
    State irCasa;
    State esperarPaciente;
    State atendiendoPaciente;
    State esperandoCompañeroM;
    State esperandoCompañeroS;
    State atendiendoUrgente;
    State paseandoSala;
    State casaFin;

    // Variables para el cambio de turno
    Perception cambioTurnoM;
    Celador siguiente;
    bool heSidoLlamadoTurno = false;
    Perception compañeroLibre;
    Perception timercambioTurnoS;
    Perception cambioTurnoS;

    void Start()
    {
        myFSM = new StateMachineEngine();
        myFSMMostrador = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMSala = new StateMachineEngine(BehaviourEngine.IsASubmachine);

        mundo = FindObjectOfType<Mundo>();
        personaje = GetComponent<Personaje>();
        sala = (SalaEspera)mundo.salas.Find((s) => s.tipo.Equals(TipoSala.ESPERA));

        //Create states
        casa = myFSM.CreateEntryState("casa");
        irPuestoTrabajoM = myFSM.CreateState("irPuestoTrabajoM", irPuestoTrabajoAction);//Se emplea no solo al llegar sino para cambiar de turno
        irPuestoTrabajoS = myFSM.CreateState("irPuestoTrabajoS", irPuestoTrabajoAction);//Se emplea no solo al llegar sino para cambiar de turno
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        esperarPaciente = myFSMMostrador.CreateEntryState("esperarPaciente", esperandoPacienteActionMostrador);
        atendiendoPaciente = myFSMMostrador.CreateState("atendiendoPaciente", atendiendoPacienteAction);
        esperandoCompañeroM = myFSM.CreateState("esperandoCompañeroM", esperandoCompañeroMAction);
        esperandoCompañeroS = myFSM.CreateState("esperandoCompañeroS", esperandoCompañeroSAction);
        atendiendoUrgente = myFSMSala.CreateState("atendiendoUrgente", atendiendoUrgenteAction);
        paseandoSala = myFSMSala.CreateEntryState("paseandoSala", esperandoPacienteAction);
        casaFin = myFSM.CreateState("casaFin", CasaFinAction);
        mostrador = myFSM.CreateSubStateMachine("mostrador", myFSMMostrador);
        salaState = myFSM.CreateSubStateMachine("salaState", myFSMSala);
        //Create perceptions
        //Si hay un paciente delante
        Perception pacienteAtender = myFSMMostrador.CreatePerception<ValuePerception>(() => !targetPaciente.ocupable);

        //Si hay un paciente urgente que atender
        Perception urgenteAtender = myFSMSala.CreatePerception<ValuePerception>(() => !targetPacienteSala.ocupable);
        //Si se produce cambio de turno

        cambioTurnoM = myFSMMostrador.CreatePerception<ValuePerception>(() => heSidoLlamadoTurno);
        timercambioTurnoS = myFSMSala.CreatePerception<TimerPerception>(timeTurno);
        compañeroLibre = myFSMSala.CreatePerception<ValuePerception>(HayCompanheroLibre);
        cambioTurnoS = myFSMSala.CreatePerception<PushPerception>();

        Perception huecoLibre = myFSM.CreatePerception<ValuePerception>(() => ComprobarLibre());
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
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, turnoSala ? irPuestoTrabajoS : irPuestoTrabajoM);
        myFSM.CreateTransition("llegar puesto trabajoM", irPuestoTrabajoM, llegadaPuesto, mostrador);
        myFSM.CreateTransition("llegar puesto trabajoS", irPuestoTrabajoS, llegadaPuesto, salaState);
        myFSMMostrador.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, atendiendoPaciente);
        myFSMSala.CreateTransition("llega urgente", paseandoSala, urgenteAtender, atendiendoUrgente);
        myFSMMostrador.CreateTransition("atencion completada", atendiendoPaciente, terminarAtender, esperarPaciente);
        myFSMSala.CreateTransition("urgente completada", atendiendoUrgente, terminarUrgente, paseandoSala);
        myFSMSala.CreateExitTransition("cambio de turnoSM", paseandoSala, cambioTurnoS, esperandoCompañeroS);
        myFSMMostrador.CreateExitTransition("cambio de turnoMS", esperarPaciente, cambioTurnoM, esperandoCompañeroM);
        myFSM.CreateTransition("hueco libreM", esperandoCompañeroS, huecoLibre, irPuestoTrabajoM);
        myFSM.CreateTransition("hueco libreS", esperandoCompañeroM, huecoLibre, irPuestoTrabajoS);
        myFSMMostrador.CreateExitTransition("terminada jornada mostrador", esperarPaciente, terminadaJornada, irCasa);
        myFSMSala.CreateExitTransition("terminada jornada sala", paseandoSala, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);


    }

    // Update is called once per frame
    void Update()
    {

        myFSM.Update();
        myFSMMostrador.Update();
        myFSMSala.Update();

        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            targetUrgenciasSala = sala.posicionSalaProfesional[0];
            targetPacienteSala = sala.posicionSalaPaciente[0];
            //Si el puesto de trabajo está libre
            if (!turnoSala)
            {
                for (int i = 0; i < sala.posicionMostradorProfesional.Length; i++)
                {
                    if (sala.posicionMostradorProfesional[i].libre && sala.posicionMostradorProfesional[i].ocupable)
                    {
                        sala.posicionMostradorProfesional[i].libre = false;
                        targetUrgenciasMostrador = sala.posicionMostradorProfesional[i];
                        targetPaciente = sala.posicionMostradorPaciente[i];
                        if (targetPaciente.actual == null)
                        {
                            targetPaciente.ocupable = true;
                        }

                        myFSM.Fire("comienza jornada");
                        return;
                    }
                }
                turnoSala = true;
            }
            else
            {
                for (int i = 0; i < sala.posicionSalaProfesional.Length; i++)
                {
                    if (sala.posicionSalaProfesional[i].libre)
                    {
                        sala.posicionSalaProfesional[i].libre = false;
                        targetUrgenciasSala = sala.posicionSalaProfesional[i];
                        targetPacienteSala = sala.posicionSalaPaciente[i];
                        if (targetPacienteSala.actual == null)
                        {
                            targetPacienteSala.ocupable = true;
                        }

                        myFSM.Fire("comienza jornada");
                        return;
                    }
                }
                turnoSala = false;
            }
        }
        else if (myFSMSala.GetCurrentState().Name.Equals("paseandoSala"))
        {
            Debug.Log(compañeroLibre.Check() && timercambioTurnoS.Check());
            if (compañeroLibre.Check() && timercambioTurnoS.Check())
            {
                cambioTurnoS.Fire();
            }
        }
    }
    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }
    private void CasaFinAction()
    {
        FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje);
        mundo.ReemplazarCelador(personaje.nombre, this);
        Destroy(this.gameObject);
    }
    private void irPuestoTrabajoAction()
    {
        //Nav Mesh ir al target puesto

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

    private void atendiendoPacienteAction()
    {
        personaje.Hablando(true);
        paciente = targetPaciente.actual.GetComponent<Paciente>();

        PutEmoji(emoAtender);
    }
    private void esperandoPacienteAction()
    {
        personaje.Hablando(false);
        if (paciente != null)
        {
            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
            targetPacienteSala.ocupable = true;
        }
        PutEmoji(emoEsperarPaciente);
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
    private void esperandoPacienteActionMostrador()
    {
        personaje.Hablando(false);
        PutEmoji(emoEsperarPaciente);
        if (paciente != null)
        {

            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
            targetPaciente.ocupable = true;
        }
    }
    private void atendiendoUrgenteAction()
    {
        personaje.Hablando(true);
        paciente = targetPacienteSala.actual.GetComponent<Paciente>();

        PutEmoji(emoAtender);
    }


    private bool ComprobarLibre()
    {

        if (!turnoSala)
        {

            if (targetUrgenciasSala.libre)
            {
                turnoSala = true;
                targetUrgenciasSala.libre = false;
                if (targetPacienteSala.actual == null)
                {
                    targetPacienteSala.ocupable = true;

                }
                return true;
            }
            return false;
        }
        else
        {

            if (targetUrgenciasMostrador.libre)
            {
                turnoSala = false;
                if (targetPaciente == null)
                {
                    targetPaciente.ocupable = true;
                }
                targetUrgenciasMostrador.libre = false;
                return true;
            }
            return false;
        }
    }

    private void esperandoCompañeroMAction()
    {
        heSidoLlamadoTurno = false;
        emoticono.sprite = emoCambio;
        targetUrgenciasMostrador.libre = true;
        if (paciente != null)
        {
            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
        }
    }
    private bool ComprobarCompañero(Celador compi)
    {
        if (compi.myFSMMostrador == null)
        {
            return false;
        }
        if (compi.myFSMMostrador.GetCurrentState().Name.Equals("esperarPaciente"))
        {

            return true;
        }
        if (compi.turnoSala)
        {
            return false;
        }
        return false;
    }
    private void esperandoCompañeroSAction()
    {

        emoticono.sprite = emoCambio;
        timercambioTurnoS.Reset();
        targetUrgenciasMostrador = siguiente.targetUrgenciasMostrador;
        targetPaciente = siguiente.targetPaciente;
        targetUrgenciasSala.libre = true;
        siguiente.heSidoLlamadoTurno = true;

        if (paciente != null)
        {
            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
        }
    }

    private bool HayCompanheroLibre()
    {


        Celador[] otros = Array.FindAll(FindObjectsOfType<Celador>(), (c) => !c.Equals(this));
        Celador siguiente = otros[UnityEngine.Random.Range(0, otros.Length)];
        if (siguiente != null)
        {
            if (ComprobarCompañero(siguiente))
            {
                this.siguiente = siguiente;
                return true;

            }

        }
        return false;

    }
}
