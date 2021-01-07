using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celador : MonoBehaviour
{
    //Variables
    public int timeJornada = 1000;
    int timeAtender = 2;
    int timeTurno = 10;
    int idMostrador;
    int idSala;
    public bool turnoSala;
   
    Personaje personaje;
    TargetUrgencias targetUrgenciasMostrador;
    TargetUrgencias targetUrgenciasSala;
    TargetUrgencias targetPaciente;
    TargetUrgencias targetPacienteSala;
    SalaEspera sala;

    public Image emoticono;
    public Sprite emoAtender, emoCasa, emoEsperarPaciente, emoCambio;

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


    //PRUEBAS DE CELTIA NI CASO
    //public TargetUrgencias mostradorCel;
    //PRUEBAS DE CELTIA NI CASO

     Perception cambioTurnoM;
    Celador siguiente;
    bool heSidoLlamadoTurno = false;
    Perception compañeroLibre;
    Perception timercambioTurnoS;
    Perception cambioTurnoS;
    // Start is called before the first frame update
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
        esperandoCompañeroM = myFSM.CreateState("esperandoCompañeroM",esperandoCompañeroMAction);
        esperandoCompañeroS = myFSM.CreateState("esperandoCompañeroS",esperandoCompañeroSAction);
        atendiendoUrgente = myFSMSala.CreateState("atendiendoUrgente", atendiendoUrgenteAction);
        paseandoSala = myFSMSala.CreateEntryState("paseandoSala",esperandoPacienteAction);
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); mundo.ReemplazarCelador(personaje.nombre,this); Destroy(this.gameObject); });
        mostrador = myFSM.CreateSubStateMachine("mostrador", myFSMMostrador);
        salaState = myFSM.CreateSubStateMachine("salaState", myFSMSala);
        //Create perceptions
        //Si hay un paciente delante


      
        Perception pacienteAtender = myFSMMostrador.CreatePerception<ValuePerception>(() => !targetPaciente.ocupado);


        //Si hay un paciente urgente que atender
        Perception urgenteAtender = myFSMSala.CreatePerception<ValuePerception>(() => !targetPacienteSala.ocupado);
        //Si se produce cambio de turno



        //Perception cambioTurnoM = myFSMMostrador.CreatePerception<TimerPerception>(timeTurno);

        cambioTurnoM = myFSMMostrador.CreatePerception<ValuePerception>(()=>heSidoLlamadoTurno);

        timercambioTurnoS = myFSMSala.CreatePerception<TimerPerception>(timeTurno);
        compañeroLibre = myFSMSala.CreatePerception<ValuePerception>(HayCompanheroLibre);
        //Perception cambioTurnoS = myFSMSala.CreateAndPerception<AndPerception>(compañeroLibre, timercambioTurnoS);
        cambioTurnoS = myFSMSala.CreatePerception<PushPerception>();
        //Si hay un puesto libre donde voy a cambiar
        Perception huecoLibre = myFSM.CreatePerception<ValuePerception>(() => ComprobarLibre());
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
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, turnoSala? irPuestoTrabajoS: irPuestoTrabajoM);
        myFSM.CreateTransition("llegar puesto trabajoM", irPuestoTrabajoM, llegadaPuesto, mostrador);
        myFSM.CreateTransition("llegar puesto trabajoS", irPuestoTrabajoS, llegadaPuesto, salaState);
        myFSMMostrador.CreateTransition("llega paciente", esperarPaciente, pacienteAtender, atendiendoPaciente);
        myFSMSala.CreateTransition("llega urgente", paseandoSala, urgenteAtender, atendiendoUrgente);
        myFSMMostrador.CreateTransition("atencion completada", atendiendoPaciente, terminarAtender, esperarPaciente);
        myFSMSala.CreateTransition("urgente completada", atendiendoUrgente, terminarUrgente, paseandoSala);
        myFSMSala.CreateExitTransition("cambio de turnoSM", paseandoSala , cambioTurnoS, esperandoCompañeroS);
        myFSMMostrador.CreateExitTransition("cambio de turnoMS", esperarPaciente, cambioTurnoM, esperandoCompañeroM);

        myFSM.CreateTransition("hueco libreM", esperandoCompañeroS, huecoLibre, irPuestoTrabajoM);
        myFSM.CreateTransition("hueco libreS", esperandoCompañeroM, huecoLibre, irPuestoTrabajoS);

        myFSMMostrador.CreateExitTransition("terminada jornada mostrador", esperarPaciente, terminadaJornada, irCasa);
        myFSMSala.CreateExitTransition("terminada jornada sala", paseandoSala, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);


      //  myFSM.CreateTransition("Cambio de turno fallido", esperandoCompañeroS, turnofallido, salaState);

    }

    // Update is called once per frame
    void Update()
    {

        myFSM.Update();
        myFSMMostrador.Update();
        myFSMSala.Update();
      
        if (!turnoSala)
        {
            //Debug.Log(myFSMMostrador.GetCurrentState().Name);
        }
        if (myFSM.GetCurrentState().Name.Equals(casa.Name))
        {
            targetUrgenciasSala = sala.posicionSalaProfesional[0];
            targetPacienteSala = sala.posicionSalaPaciente[0];
            //Si el puesto de trabajo está libre
            if (!turnoSala)
            {
                for (int i = 0; i < sala.posicionMostradorProfesional.Length; i++)
                {
                    if (sala.posicionMostradorProfesional[i].libre)
                    {
                        sala.posicionMostradorProfesional[i].libre = false;
                        //Debug.Log("hay hueco libre");
                        //idMostrador = i;
                        targetUrgenciasMostrador = sala.posicionMostradorProfesional[i];
                        targetPaciente = sala.posicionMostradorPaciente[i];
                        if (targetPaciente.actual == null)
                        {
                            targetPaciente.ocupado = true;
                        }
                        
                        //mundo.targetMostradorPaciente[i].libre = true;
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
                        //idSala = i;
                        targetUrgenciasSala = sala.posicionSalaProfesional[i];
                        targetPacienteSala = sala.posicionSalaPaciente[i];
                        if (targetPacienteSala.actual == null)
                        {
                            targetPacienteSala.ocupado = true;
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
           // HayCompanheroLibre();

            Debug.Log(compañeroLibre.Check()&&timercambioTurnoS.Check());
            if(compañeroLibre.Check() && timercambioTurnoS.Check())
            {
                cambioTurnoS.Fire();
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
    //REVISAR
    private void atendiendoPacienteAction()
    {
        //Debug.Log(myFSMMostrador.GetCurrentState().Name);
        personaje.Hablando(true);
       // Debug.Log(targetPaciente);
        paciente = targetPaciente.actual.GetComponent<Paciente>();
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
    }
    private void esperandoPacienteAction()
    {
        personaje.Hablando(false);
        //Debug.Log(myFSMSala.GetCurrentState().Name);
        //turnoSala = true;
        if (paciente != null)
        {
            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
            targetPacienteSala.ocupado = true;
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
        //turnoSala = false;
        if (paciente != null)
        {

            mandarPacienteListaEspera();
            paciente.heSidoAtendido.Fire();
            paciente = null;
            targetPaciente.ocupado = true;
        }
    }
    private void atendiendoUrgenteAction()
    {
        personaje.Hablando(true);
        //Debug.Log(myFSMSala.GetCurrentState().Name);
        paciente = targetPacienteSala.actual.GetComponent<Paciente>();
        enfermedad = paciente.enfermedad;
        PutEmoji(emoAtender);
    }

    //REVISAR, NO ESTÁ ENCONTRANDO LOS HUECOS LIBRES
    private bool ComprobarLibre()
    {
        //Debug.Log("quiero cambiar de turno");
        if (!turnoSala)
        {
            //targetUrgenciasMostrador.libre = true;
            //targetPaciente.ocupado = false;
            if (targetUrgenciasSala.libre)
            {
                turnoSala = true;
                targetUrgenciasSala.libre = false;
                if (targetPacienteSala.actual == null)
                {
                     targetPacienteSala.ocupado = true;

                }
                return true;
            }
            return false;
            /*sala.posicionMostradorProfesional[idMostrador].ocupado = true;
            sala.posicionMostradorProfesional[idMostrador].libre = true;*/
            //for (int i = 0; i < sala.posicionSalaProfesional.Length; i++)
            //{
            //    if (sala.posicionSalaProfesional[i].libre)
            //    {
            //        turnoSala = true;
            //        //sala.posicionSalaProfesional[i].libre = false;
            //        targetUrgenciasSala = sala.posicionSalaProfesional[i];
            //        targetUrgenciasSala.libre = false;
            //        targetPacienteSala = sala.posicionSalaPaciente[i];
            //        targetPacienteSala.ocupado = true;
            //        //sala.posicionSalaProfesional[i].ocupado = false;
            //        sala.posicionSalaProfesional[i].libre = false;
            //        return true;
            //    }
            //}
        }
        else
        {
            //targetUrgenciasSala.libre = true;
            //targetPacienteSala.ocupado = false;
            if (targetUrgenciasMostrador.libre)
            {
                turnoSala = false;
                if (targetPaciente == null)
                {

                    targetPaciente.ocupado = true;
                }
                targetUrgenciasMostrador.libre = false;
                return true;
            }
            return false;

            /*sala.posicionSalaProfesional[idSala].ocupado = true;
            sala.posicionSalaProfesional[idSala].libre = true;*/
            //    for (int i = 0; i < sala.posicionMostradorProfesional.Length; i++)
            //    {
            //        if (sala.posicionMostradorProfesional[i].libre)
            //        {
            //            turnoSala = false;
            //            //sala.posicionMostradorProfesional[i].libre = false;
            //            targetUrgenciasMostrador = sala.posicionMostradorProfesional[i];
            //            targetUrgenciasMostrador.libre = false;
            //            targetPaciente = sala.posicionMostradorPaciente[i];
            //            targetPaciente.ocupado = true;
            //            sala.posicionMostradorProfesional[i].libre = false;
            //            //sala.posicionMostradorProfesional[i].ocupado = false;
            //            return true;
            //        }
            //    }
            //}

        }
    }

    private void esperandoCompañeroMAction()
    {
            heSidoLlamadoTurno = false;
            emoticono.sprite = emoCambio;
            targetUrgenciasMostrador.libre = true;
        
            //targetPaciente.ocupado = false;

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
        //targetPacienteSala.ocupado = false;
        
        emoticono.sprite = emoCambio;
        //Celador[] otros = Array.FindAll(FindObjectsOfType<Celador>(), (c) => !c.Equals(this));
        //Celador siguiente = otros[UnityEngine.Random.Range(0, otros.Length)];

        //while (ComprobarCompañero(siguiente))
        //{
        //     otros = Array.FindAll(FindObjectsOfType<Celador>(), (c) => !c.Equals(this));
        //     siguiente = otros[UnityEngine.Random.Range(0, otros.Length)];
        //}
        //targetUrgenciasMostrador = siguiente.targetUrgenciasMostrador;
        //targetPaciente = siguiente.targetPaciente;
        //    targetUrgenciasSala.libre = true;
        //    //targetPacienteSala.ocupado = false;
        //    siguiente.cambioTurnoM.Fire();
        targetUrgenciasMostrador = siguiente.targetUrgenciasMostrador;
        targetPaciente = siguiente.targetPaciente;
        targetUrgenciasSala.libre = true;
        siguiente.heSidoLlamadoTurno = true;
        //targetPacienteSala.ocupado = false;

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
                //compañeroLibre.Fire();
                return true;
           
            }

        }
        return false;
    
    }
}
