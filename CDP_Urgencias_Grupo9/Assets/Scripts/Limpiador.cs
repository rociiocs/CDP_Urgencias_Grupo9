using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limpiador : MonoBehaviour
{
    public bool ocupado = false; // o que este en la sala de limpiadores, esta pa que no de errores
    public Sala salaLimpiando;
    public int timeJornada = 2000;

    //Maquina de estados
    StateMachineEngine myFSM;

    //Estados
    State casa;
    State irCasa;
    State irPantalla;
    State irSala;
    State irQuirofano;
    State irSalaEspera;
    State consultandoPantalla;
    State limpiandoBaños;
    State limpiandoQuirofano;
    State limpiandoSalaEspera;
    State limpiandoSala;


    // Start is called before the first frame update
    void Start()
    {
        myFSM = new StateMachineEngine();

        //Create states
        casa = myFSM.CreateEntryState("casa", casaAction);
        consultandoPantalla = myFSM.CreateState("consultandoPantalla", consultandoPantallaAction);
        limpiandoBaños = myFSM.CreateState("limpiandoBaños", limpiandoBañosAction);
        limpiandoQuirofano = myFSM.CreateState("limpiandoQuirofano", limpiandoQuirofanoAction);
        limpiandoSalaEspera = myFSM.CreateState("limpiandoSalaEspera", limpiandoSalaEsperaAction);
        irPantalla = myFSM.CreateState("irPantalla", irPantallaAction);
        irSala = myFSM.CreateState("irSala", irSalaAction);
        irSalaEspera = myFSM.CreateState("irSalaEspera", irSalaEsperaAction);
        irCasa = myFSM.CreateState("irCasa", irCasaAction);

        //Create perceptions
        //Comienza jornada, ir hacia la pantalla
        Perception comienzaJornada = myFSM.CreatePerception<WatchingPerception>(); //Mirar
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si hay sala que limpiar, el limpiador se dirige a ella
        Perception haySalaLimpiar = myFSM.CreatePerception<PushPerception>();
        //Si hay un quirofano que limpiar, el mundo llama al limpiador
        Perception hayQuirofanoLimpiar = myFSM.CreatePerception<PushPerception>();
        //Si hay que limpiar la sala de espera, el limpiador lo que chequea
        Perception haySalaEsperaLimpiar = myFSM.CreatePerception<PushPerception>();
        //Si la sala que estoy limpiando ya está limpia
        Perception salaLimpia = myFSM.CreatePerception<ValuePerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<PushPerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPantalla = myFSM.CreatePerception<PushPerception>();
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<PushPerception>();


        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPantalla);
        myFSM.CreateTransition("llegada pantalla",irPantalla , llegadaPantalla, consultandoPantalla);
        myFSM.CreateTransition("hay sala limpiar", consultandoPantalla, haySalaLimpiar, irSala);
        myFSM.CreateTransition("hay quirofano limpiar", consultandoPantalla, hayQuirofanoLimpiar, irQuirofano);
        myFSM.CreateTransition("hay sala espera limpiar", consultandoPantalla, haySalaEsperaLimpiar, irSalaEspera);
        myFSM.CreateTransition("llegada a sala", irSala, llegadaPuesto, limpiandoSala);
        myFSM.CreateTransition("llegada a quirofano", irQuirofano, llegadaPuesto, limpiandoQuirofano);
        myFSM.CreateTransition("llegada a sala espera", irSalaEspera, llegadaPuesto, limpiandoSalaEspera);
        myFSM.CreateTransition("limpia sala", limpiandoSala, salaLimpia, irPantalla);
        myFSM.CreateTransition("limpia sala espera", limpiandoSalaEspera, salaLimpia, irPantalla);
        myFSM.CreateTransition("limpia quirofano", limpiandoQuirofano, salaLimpia, irPantalla);
        myFSM.CreateTransition("limpia sala", limpiandoSala, hayQuirofanoLimpiar, irQuirofano);
        myFSM.CreateTransition("limpia sala espera", limpiandoSalaEspera, hayQuirofanoLimpiar, irQuirofano);
        myFSM.CreateTransition("termina jornada", consultandoPantalla, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casa);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void casaAction()
    {

    }

    private void consultandoPantallaAction()
    {

    }

    private void limpiandoBañosAction()
    {

    }

    private void limpiandoQuirofanoAction()
    {

    }

    private void limpiandoSalaEsperaAction()
    {

    }

    private void limpiandoSalaAction()
    {

    }

    private void irPantallaAction()
    {

    }

    private void irSalaEsperaAction()
    {

    }

    private void irSalaAction()
    {

    }

    private void irQuirofanoAction()
    {

    }

    private void irCasaAction()
    {

    }
}
