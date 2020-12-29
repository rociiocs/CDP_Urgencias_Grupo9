using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paciente : MonoBehaviour
{
    private Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    public bool urgente;
    public bool estoyVivo = true;
    public int targetUrgenciasID;
    public int timerOrina = 5;
    public int timerAnimacionMorir = 5;
    int idPasoActual;
    Paso pasoActual;
    Mundo mundo;
    //Maquina de estados
    StateMachineEngine myFSM;
    StateMachineEngine myFSMVivo;
    StateMachineEngine myFSMColaFuera;
    StateMachineEngine myFSMColaDentro;
    StateMachineEngine myFSMAnalisisOrina;
    StateMachineEngine myFSMEsperandoSala;

    //Estados
    State casa;
    State vivo; //Submáquina
    State muerto;

    //Urgente
    State acudirCeladorSala;
    State esperandoSalaUrgente;
    
    //No urgente
    State haciendoColaFuera; //Submáquina
    State haciendoColaDentro; //Submáquina
    State siendoAtendidoCelador;
    State esperandoSalaEspera;
    State yendoSalaEspera;
    State haciendoAnalisisOrina; //Submáquina

    //Comunes
    State yendoCentro;
    State llegadaCentro;
    State entrandoCentro;
    State siendoAtendidoQuirofano;
    State siendoAtendidoConsulta;
    State yendoSala;
    State yendoUCI;
    State llegadaUCI;
    State yendoCasa;
    State casaFin;

    //Subestados haciendo cola fuera y cola dentro
    State esperandoCola;
    State avanzandoCola;

    State esperandoColaDentro;
    State avanzandoColaDentro;

    //Subestados esperando sala espera
    State esperandoDePie;
    State ocupandoAsiento;

    //Subestados analisis de orina
    State yendoBaño;
    State tomandoMuestra;
    State volviendoSitio;


    void Start()
    {
        personaje = GetComponent<Personaje>();
        mundo = GetComponentInParent<Mundo>();
        enfermedad = new Enfermedad(TipoEnfermedad.Cistitis, false, 9000000, null, 1);// TESTEANDO
        idPasoActual = 0;
        pasoActual = enfermedad.pasos[idPasoActual];

        myFSM = new StateMachineEngine(true);
        myFSMVivo = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaFuera = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaDentro = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMAnalisisOrina = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMEsperandoSala = new StateMachineEngine(BehaviourEngine.IsASubmachine);

        //Create states
        casa = myFSM.CreateEntryState("casa");
        muerto = myFSM.CreateState("muerto");
        casaFin = myFSM.CreateState("casaFin");

        //Submáquina vivo
        vivo = myFSM.CreateSubStateMachine("vivo", myFSMVivo);
        
        yendoCentro = myFSMVivo.CreateState("yendoCentro");
        llegadaCentro = myFSMVivo.CreateState("llegadaCentro");
        entrandoCentro = myFSMVivo.CreateState("entrandoCentro");
        siendoAtendidoQuirofano = myFSMVivo.CreateState("siendoAtendidoQuirofano");
        siendoAtendidoConsulta = myFSMVivo.CreateState("siendoAtendidoConsulta");
        yendoSala = myFSMVivo.CreateState("yendoSala");
        yendoUCI = myFSMVivo.CreateState("yendoUCI");
        llegadaUCI = myFSMVivo.CreateState("llegadaUCI");
        yendoCasa = myFSMVivo.CreateState("yendoCasa");
        

        //No urgentes
        siendoAtendidoCelador = myFSMVivo.CreateState("siendoAtendidoCelador");
        yendoSalaEspera = myFSM.CreateState("yendoSalaEspera");

        //Urgentes
        acudirCeladorSala = myFSMVivo.CreateState("acudirCeladorSala");
        esperandoSalaUrgente = myFSMVivo.CreateState("esperandoSalaUrgente");
        
        //Cola fuera
        haciendoColaFuera = myFSMVivo.CreateSubStateMachine("haciendoColaFuera", myFSMColaFuera);
        esperandoCola = myFSMColaFuera.CreateEntryState("esperandoCola");
        avanzandoCola = myFSMColaFuera.CreateState("avanzandoCola");

        //Cola dentro
        haciendoColaDentro = myFSMVivo.CreateSubStateMachine("haciendoColaDentro", myFSMColaDentro);
        esperandoColaDentro = myFSMColaDentro.CreateEntryState("esperandoCola");
        avanzandoColaDentro = myFSMColaDentro.CreateState("avanzandoCola");

        //Analisis de orina
        haciendoAnalisisOrina = myFSMVivo.CreateSubStateMachine("haciendoAnalisisOrina", myFSMAnalisisOrina);
        yendoBaño = myFSMAnalisisOrina.CreateEntryState("yendoBaño");
        tomandoMuestra = myFSMAnalisisOrina.CreateState("tomandoMuestra");
        volviendoSitio = myFSMAnalisisOrina.CreateState("volviendoSitio");

        //Esperando sala espera
        esperandoSalaEspera = myFSMVivo.CreateSubStateMachine("esperandoSalaEspera", myFSMEsperandoSala);
        esperandoDePie = myFSMEsperandoSala.CreateEntryState("esperandoDePie");
        ocupandoAsiento = myFSMEsperandoSala.CreateState("ocupandoAsiento");

        Perception estoyVivoPerception = myFSM.CreatePerception<ValuePerception>(() => estoyVivo);
        Perception soyUrgente = myFSMVivo.CreatePerception<ValuePerception>(()=> urgente);
        Perception haEntradoCentro = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception celadorLibreSala = myFSMVivo.CreatePerception<ValuePerception>(); //
        //El celador hace push al personaje
        Perception atendidoCelador = myFSMVivo.CreatePerception<PushPerception>(); 
        //El mundo hace push al paciente cuando hay sala libre
        Perception SalaAsignadaLibre = myFSMVivo.CreatePerception<PushPerception>(); 
        Perception heLlegadoSala = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado); 
        //El cirujano lo manda a la UCI
        Perception soyGrave = myFSMVivo.CreatePerception<PushPerception>(); 
        //El cirujano lo manda a la casa
        Perception soyLeve = myFSMVivo.CreatePerception<PushPerception>(); 
        //Si ha llegado a casa o a la uci
        Perception heLlegado = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Si he muerto, timer de la enfermedad
        Perception heMuerto = myFSM.CreatePerception<TimerPerception>(enfermedad.timerEnfermedad);

        Perception noSoyUrgente = myFSMVivo.CreatePerception<ValuePerception>(() => !urgente);
        Perception hayHueco = myFSMColaFuera.CreatePerception<ValuePerception>(() => mundo.targetColaFuera[targetUrgenciasID-1].libre);
        Perception heAvanzado = myFSMColaFuera.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception noHayAforo = myFSMVivo.CreatePerception<ValuePerception>(() => targetUrgenciasID == 0 && mundo.hayAforo());

        Perception hayHuecoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => mundo.targetColaDentro[targetUrgenciasID - 1].libre);
        Perception heAvanzadoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Añadir para ver si hay algún mostrador libre
        Perception hayMostradorLibre = myFSMColaDentro.CreatePerception<ValuePerception>(() => targetUrgenciasID == 0);
        //El celador pone el push de ser atendido
        Perception heSidoAtendido = myFSMVivo.CreatePerception<PushPerception>();
        //Añadir como ver si hay algún target de asiento libre
        Perception asientoLibre = myFSMEsperandoSala.CreatePerception<ValuePerception>();
        //Push por el enfermero
        Perception tengoQueHacerAnalisisOrina = myFSMVivo.CreatePerception<PushPerception>();

        Perception hellegadoBaño = myFSMAnalisisOrina.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception heTomadoMuestra = myFSMAnalisisOrina.CreatePerception<TimerPerception>(timerOrina);
        Perception hellegadoSitio = myFSMAnalisisOrina.CreatePerception<ValuePerception>(() => personaje.haLlegado);

        //El profesional hace push para que vuelvas a sala de espera
        Perception todaviaTengoQueSerTratado = myFSMVivo.CreatePerception<PushPerception>();
        Perception animacionMuerto = myFSM.CreatePerception<TimerPerception>(timerAnimacionMorir);

        //Transiciones
        myFSM.CreateTransition("aparecer", casa, estoyVivoPerception, vivo);    
        myFSMVivo.CreateTransition("he llegado centro y soy urgente", yendoCentro, soyUrgente, entrandoCentro);    
        myFSMVivo.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);    
        myFSMVivo.CreateTransition("acudiendo celador",entrandoCentro, celadorLibreSala, acudirCeladorSala);    
        myFSMVivo.CreateTransition("ser atendido celador", acudirCeladorSala, heLlegado, siendoAtendidoCelador);    
        myFSMVivo.CreateTransition("esperar sala libre", siendoAtendidoCelador, heSidoAtendido, esperandoSalaUrgente);    
        myFSMVivo.CreateTransition("acudir quirofano", esperandoSalaUrgente, SalaAsignadaLibre, yendoSala);    
        myFSMVivo.CreateTransition("acudir quirofano", yendoSala, heLlegadoSala, siendoAtendidoQuirofano);    
        myFSMVivo.CreateTransition("acudir a la UCI", siendoAtendidoQuirofano, soyGrave, yendoUCI);    
        myFSMVivo.CreateTransition("acudir a casa", siendoAtendidoQuirofano, soyLeve, yendoCasa);    
        myFSMVivo.CreateTransition("he llegado a UCI", yendoUCI, heLlegado, casaFin);  
        //Mirar si hay problemas entre dos estados de máquinas diferentes
        myFSMVivo.CreateTransition("he llegado a casa", yendoCasa, heLlegado, casaFin);    

        myFSM.CreateTransition("morirse", vivo, heMuerto, muerto);   
        myFSM.CreateTransition("desaparecer", muerto, animacionMuerto, casaFin);   
        myFSMColaFuera.CreateTransition("hay hueco libre", esperandoCola, hayHueco, avanzandoCola);   
        myFSMColaFuera.CreateTransition("he avanzado", avanzandoCola, heAvanzado, esperandoCola);   
        myFSMVivo.CreateTransition("aforo libre", haciendoColaFuera, noHayAforo, entrandoCentro);   
        myFSMVivo.CreateTransition("hacer cola dentro", entrandoCentro, noSoyUrgente, haciendoColaDentro);
        myFSMColaDentro.CreateTransition("hay hueco libre", esperandoColaDentro, hayHuecoDentro, avanzandoColaDentro);
        myFSMColaDentro.CreateTransition("he avanzado", avanzandoColaDentro, heAvanzadoDentro, esperandoColaDentro);
        myFSMVivo.CreateTransition("hay mostrador libre", haciendoColaDentro, hayMostradorLibre, siendoAtendidoCelador);
        myFSMVivo.CreateTransition("esperando sala", siendoAtendidoCelador, heSidoAtendido, esperandoSalaEspera);
        myFSMEsperandoSala.CreateTransition("hay asiento libre", esperandoDePie, asientoLibre, ocupandoAsiento);
        myFSMVivo.CreateTransition("hay sala libre", esperandoSalaEspera, SalaAsignadaLibre, yendoSala);
        myFSMVivo.CreateTransition("llegada a sala y siendo atendido", yendoSala, heLlegadoSala, siendoAtendidoConsulta);
        myFSMVivo.CreateTransition("ir hacer analisis orina", siendoAtendidoConsulta, tengoQueHacerAnalisisOrina, haciendoAnalisisOrina);
        myFSMAnalisisOrina.CreateTransition("tomando muestra orina", yendoBaño, hellegadoBaño, tomandoMuestra);
        myFSMAnalisisOrina.CreateTransition("volver al sitio", tomandoMuestra, heTomadoMuestra, volviendoSitio);
        myFSMAnalisisOrina.CreateTransition("volver a ser atendido", volviendoSitio, hellegadoSitio, siendoAtendidoConsulta);
        myFSMVivo.CreateTransition("volver a sala espera", siendoAtendidoConsulta, todaviaTengoQueSerTratado, yendoSalaEspera);
        myFSMVivo.CreateTransition("volver esperando sala", yendoSalaEspera, heLlegado, esperandoSalaEspera);
        myFSMVivo.CreateTransition("acudir a la UCI consulta", siendoAtendidoConsulta, soyGrave, yendoUCI);
        myFSMVivo.CreateTransition("acudir a casa consulta", siendoAtendidoConsulta, soyLeve, yendoCasa);

    }

    public void GoTo( Transform transform)// cambio de estado etc y al llegar al punto se cambia otra vez de estado
    {
        personaje.GoTo(transform);
    }
    public void siguientePaso()
    {
        idPasoActual++;
        pasoActual = enfermedad.pasos[idPasoActual];
    }
    void Update()
    {
        
    }

    private void vivoAction()
    {

    }
}
