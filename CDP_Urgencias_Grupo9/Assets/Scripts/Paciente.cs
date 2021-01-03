using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Paciente : MonoBehaviour
{

    // PROVISIONAL ENFERMERO

    public TargetUrgencias targetBanho;
    // PROVISIONAL ENFERMERO
    //referencias
    public Image emoticono;
    public Sprite emoMuerto;
    private Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    public bool urgente;
    public bool estoyVivo = true;
    public TargetUrgencias targetUrgencias;
    public int targetUrgenciasID;
    public int timerOrina = 5;
    public int timerAnimacionMorir = 5;
    int idPasoActual;
    Paso pasoActual;
    Mundo mundo;
    //Variables para muerte
    bool morirse;
    bool fade=true;
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
    State salirVivo;

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
        mundo = FindObjectOfType<Mundo>();



        myFSM = new StateMachineEngine(false);
        myFSMVivo = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaFuera = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaDentro = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMAnalisisOrina = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMEsperandoSala = new StateMachineEngine(BehaviourEngine.IsASubmachine);

        //Create states
        casa = myFSM.CreateEntryState("casa");//, () => Debug.Log("encasa"));
        muerto = myFSM.CreateState("muerto", () => morirse = true);
        salirVivo= myFSM.CreateState("salirVivo", muertoAction);
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); Destroy(gameObject); });

        //Submáquina vivo


        yendoCentro = myFSMVivo.CreateEntryState("yendoCentro", () => { targetUrgenciasID = mundo.targetColaFuera.Length; });
        llegadaCentro = myFSMVivo.CreateState("llegadaCentro");
        entrandoCentro = myFSMVivo.CreateState("entrandoCentro");
        siendoAtendidoQuirofano = myFSMVivo.CreateState("siendoAtendidoQuirofano");
        siendoAtendidoConsulta = myFSMVivo.CreateState("siendoAtendidoConsulta");
        yendoSala = myFSMVivo.CreateState("yendoSala");
        yendoUCI = myFSMVivo.CreateState("yendoUCI");
        llegadaUCI = myFSMVivo.CreateState("llegadaUCI");
        yendoCasa = myFSMVivo.CreateState("yendoCasa");


        //No urgentes
        siendoAtendidoCelador = myFSMVivo.CreateState("siendoAtendidoCelador", () => { GoTo(mundo.targetMostradorPaciente[0]); mundo.targetMostradorPaciente[0].libre = false; });
        yendoSalaEspera = myFSM.CreateState("yendoSalaEspera");

        //Urgentes
        acudirCeladorSala = myFSMVivo.CreateState("acudirCeladorSala");
        esperandoSalaUrgente = myFSMVivo.CreateState("esperandoSalaUrgente");
        
        //Cola fuera
      
        esperandoCola = myFSMColaFuera.CreateEntryState("esperandoCola");
        avanzandoCola = myFSMColaFuera.CreateState("avanzandoCola", avanzandoColaAction);

        //Cola dentro
      
        esperandoColaDentro = myFSMColaDentro.CreateEntryState("esperandoCola",()=>targetUrgenciasID= mundo.targetColaDentro.Length);
        avanzandoColaDentro = myFSMColaDentro.CreateState("avanzandoCola", avanzandoColaDentroAction);

        //Analisis de orina

        yendoBaño = myFSMAnalisisOrina.CreateEntryState("yendoBaño");
        tomandoMuestra = myFSMAnalisisOrina.CreateState("tomandoMuestra");
        volviendoSitio = myFSMAnalisisOrina.CreateState("volviendoSitio");

        //Esperando sala espera
  
        esperandoDePie = myFSMEsperandoSala.CreateEntryState("esperandoDePie");
        ocupandoAsiento = myFSMEsperandoSala.CreateState("ocupandoAsiento");


        Perception estoyVivoPerception = myFSM.CreatePerception<ValuePerception>(() => estoyVivo);
        Perception soyUrgente = myFSMVivo.CreatePerception<ValuePerception>(()=> enfermedad != null && enfermedad.urgente);
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

        Perception noSoyUrgente = myFSMVivo.CreatePerception<ValuePerception>(() => enfermedad!=null&&!enfermedad.urgente);
        Perception hayHueco = myFSMColaFuera.CreatePerception<ValuePerception>(() =>  targetUrgenciasID != 0&&mundo.targetColaFuera[targetUrgenciasID-1].libre );
        Perception heAvanzado = myFSMColaFuera.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception noHayAforo = myFSMVivo.CreatePerception<ValuePerception>(() => targetUrgenciasID == 0 && mundo.hayAforo());

        Perception hayHuecoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => targetUrgenciasID!=0&&mundo.targetColaDentro[targetUrgenciasID - 1].libre);
        Perception heAvanzadoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Añadir para ver si hay algún mostrador libre
        Perception hayMostradorLibre = myFSMColaDentro.CreatePerception<ValuePerception>(() => mundo.targetMostradorPaciente[0].libre);
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



        //Submaquinas
        esperandoSalaEspera = myFSMVivo.CreateSubStateMachine("esperandoSalaEspera", myFSMEsperandoSala);
        vivo = myFSM.CreateSubStateMachine("vivo", myFSMVivo, yendoCentro);
        haciendoAnalisisOrina = myFSMVivo.CreateSubStateMachine("haciendoAnalisisOrina", myFSMAnalisisOrina);
        haciendoColaDentro = myFSMVivo.CreateSubStateMachine("haciendoColaDentro", myFSMColaDentro);
        haciendoColaFuera = myFSMVivo.CreateSubStateMachine("haciendoColaFuera", myFSMColaFuera, esperandoCola);
        //Transiciones
        myFSM.CreateTransition("aparecer", casa, estoyVivoPerception, vivo);
        myFSMVivo.CreateTransition("he llegado centro y soy urgente", yendoCentro, soyUrgente, entrandoCentro);
        myFSMVivo.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);
        //myFSM.CreateTransition("he llegado centro y soy urgente", yendoCentro, soyUrgente, entrandoCentro);
        //myFSM.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);


        myFSMVivo.CreateTransition("acudiendo celador",entrandoCentro, celadorLibreSala, acudirCeladorSala);    
        myFSMVivo.CreateTransition("ser atendido celador", acudirCeladorSala, heLlegado, siendoAtendidoCelador);    
        myFSMVivo.CreateTransition("esperar sala libre", siendoAtendidoCelador, heSidoAtendido, esperandoSalaUrgente);    
        myFSMVivo.CreateTransition("acudir quirofano", esperandoSalaUrgente, SalaAsignadaLibre, yendoSala);    
        myFSMVivo.CreateTransition("llegado quirofano", yendoSala, heLlegadoSala, siendoAtendidoQuirofano);    
        myFSMVivo.CreateTransition("acudir a la UCI", siendoAtendidoQuirofano, soyGrave, yendoUCI);    
        myFSMVivo.CreateTransition("acudir a casa", siendoAtendidoQuirofano, soyLeve, yendoCasa);    
        myFSMVivo.CreateTransition("he llegado a UCI", yendoUCI, heLlegado, casaFin);  
        //Mirar si hay problemas entre dos estados de máquinas diferentes
        myFSMVivo.CreateTransition("he llegado a casa", yendoCasa, heLlegado, casaFin);    

        myFSM.CreateTransition("morirse", vivo, heMuerto, salirVivo);   
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

    public void GoTo( TargetUrgencias transform)// cambio de estado etc y al llegar al punto se cambia otra vez de estado
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
        myFSM.Update();
        myFSMAnalisisOrina.Update();
        myFSMColaDentro.Update();
        myFSMColaFuera.Update();
        myFSMEsperandoSala.Update();
        myFSMVivo.Update();
        

        if (morirse)
        {
            personaje.Morirse();
            StartCoroutine(Die());
            if (fade == true)
            {
                //Se coge el material y se va bajando el alpha hasta que llegue a cero, entonces se detiene con el booleano
                Color oColor = GetComponentInChildren<SkinnedMeshRenderer>().material.color;
                float fadeAmount = oColor.a - (0.5f * Time.deltaTime);
                oColor = new Color(oColor.r, oColor.g, oColor.b, fadeAmount);
                GetComponentInChildren<SkinnedMeshRenderer>().material.color = oColor;
                if (oColor.a <= 0)
                {
                    fade = false;
                    morirse = false;
                }
            }
        }
        
        //Debug.Log("fsm" + myFSM.GetCurrentState().Name);
        //Debug.Log("fsmVivo " + myFSMVivo.GetCurrentState().Name);
        //Debug.Log("fsmColaFuera " + myFSMColaFuera.GetCurrentState().Name);
        //Debug.Log("fsmColadentro " + myFSMColaDentro.GetCurrentState().Name);
    }

    private void muertoAction()
    {
        emoticono.sprite = emoMuerto;
        personaje.myAgent.Stop();
        personaje.muerto = true;
        personaje.myAgent.enabled = false;
        myFSMVivo.Fire(myFSMVivo.CreateExitTransition("salir vivo", myFSMVivo.GetCurrentState(), myFSMVivo.CreatePerception<ValuePerception>(() => morirse == true), muerto));
    }
    public void setEnfermedad(Enfermedad en, Sprite emo)
    {
        emoticono.sprite = emo;
        enfermedad = en;
        idPasoActual = 0;
        pasoActual = enfermedad.pasos[idPasoActual];
        urgente = enfermedad.urgente;
    }
    private void avanzandoColaAction()
    {
        //Debug.Log("avanzandocolaaction");
        if (targetUrgencias != null)
        {
            targetUrgencias.libre = true;
        }
        targetUrgencias = mundo.targetColaFuera[targetUrgenciasID-1];
        targetUrgenciasID--;
        targetUrgencias.libre = false;
        GoTo(targetUrgencias);
    }

    private void avanzandoColaDentroAction()
    {
        //Debug.Log("avanzandocolaadentroction");
        targetUrgencias.libre = true;
        targetUrgencias = mundo.targetColaDentro[targetUrgenciasID-1];
        targetUrgenciasID--;
        targetUrgencias.libre = false;
        GoTo(targetUrgencias);
    }
    
    IEnumerator Die()
    {
        //Los dos primeros segundos son maomenos el tiempo que tarde en segundos en caer al suelo durante la animación de morirse
        yield return new WaitForSeconds(1);
        //Se coge la posición para ir modificándola
        var t = transform.position;
        //Se le hace ascender a esa velocidad
        t.y += 1.5f * Time.deltaTime;
        transform.position = t;
        //Después de este tiempo se indica que ha acabado, para dar tiempo a desvanecer al personaje
        yield return new WaitForSeconds(2.5f);
        myFSM.Fire("desaparecer");
    }
}
