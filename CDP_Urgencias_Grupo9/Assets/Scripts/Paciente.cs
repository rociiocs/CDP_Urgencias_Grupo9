using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ComparadorPrioridad : Comparer<Paciente>
{
    public override int Compare(Paciente x, Paciente y)
    {
        //Mayor tiempo de espera, va antes
        if (x.enfermedad.prioridad == y.enfermedad.prioridad)
        {
            return (int)((int)y.timeEspera-(int)x.timeEspera );
        }
        else
        {
            //menor prioridad, va antes
            return x.enfermedad.prioridad - y.enfermedad.prioridad;
        }
    }
}
public class Paciente : MonoBehaviour
{

    // PROVISIONAL ENFERMERO

    public TargetUrgencias targetBanho;
    // PROVISIONAL ENFERMERO
    //referencias
    public Image emoticono;
    public Sprite emoMuerto;
    public Sprite emoCurado;
    public Sprite emoUCI;
    public Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    public bool urgente;
    public bool estoyVivo = true;
    public bool estoySiendoAtendidoFlag = false;
    public TargetUrgencias targetUrgencias;
    public int targetUrgenciasID;
    public int timerOrina = 5;
    public int timerAnimacionMorir = 5;
    //COLA DE PRIORIDADES
    public float timeEspera = 0;
    //
    int idPasoActual;
    public Paso pasoActual;
    Mundo mundo;
    //Variables para muerte
    bool morirse;
    bool fade=true;
    //Maquina de estados
    public StateMachineEngine myFSM;
    public StateMachineEngine myFSMVivo;
    public StateMachineEngine myFSMColaFuera;
    public StateMachineEngine myFSMColaDentro;
    public StateMachineEngine myFSMColaUrgente;
    public StateMachineEngine myFSMAnalisisOrina;
    public StateMachineEngine myFSMEsperandoSala;

    //Estados
    State casa;
    State vivo; //Submáquina
    State muerto;
    State salirVivo;

    //Urgente
    State acudirCeladorSala;
    State esperandoSalaUrgente;
    State siendoAtendidoCelador;
    State haciendoColaUrgente;

    //No urgente
    State haciendoColaFuera; //Submáquina
    State haciendoColaDentro; //Submáquina
    State siendoAtendidoCeladorMostrador;
    
    State esperandoSalaEspera;
    State yendoSalaEspera;
    State haciendoAnalisisOrina; //Submáquina

    //Comunes
    State yendoCentro;
    State llegadaCentro;
    State entrandoCentro;
    State entrandoCentroUrgente;
    State llegadaMostrador;
    State llegadaSala;
    State siendoAtendidoQuirofano;
    State siendoAtendidoConsulta;
    State yendoSala;
    State yendoUCI;
    State yendoCasa;
    State casaFin;
    State esperarCelador;
    //Subestados haciendo cola fuera, cola dentro y cola de urgentes
    State esperandoCola;
    State avanzandoCola;

    State esperandoColaDentro;
    State avanzandoColaDentro;

    State esperandoColaUrgente;
    State avanzandoColaUrgente;

    //Subestados esperando sala espera
    State esperandoDePie;
    State ocupandoAsiento;

    //Subestados analisis de orina
    State yendoBaño;
    State tomandoMuestra;
    State volviendoSitio;
   
    //Para que se pueda disparar
    public Perception SalaAsignadaLibre;
    public Perception heSidoAtendido;
    public Perception todaviaTengoQueSerTratado;
    public Perception tengoQueHacerAnalisisOrina;
    public Perception soyLeve;
    public Perception soyGrave;
    Perception seMeAcaboElTiempo;
    Perception estoySiendoAtendio;
    Perception noPuedoMorirAun;

    public float timeMorir;

    void Start()
    {
        //Referencias
        personaje = GetComponent<Personaje>();
        mundo = FindObjectOfType<Mundo>();
        
        //Máquinas de estados y submáquinas
        myFSM = new StateMachineEngine(false);
        myFSMVivo = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaFuera = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaDentro = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMColaUrgente = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMAnalisisOrina = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        myFSMEsperandoSala = new StateMachineEngine(BehaviourEngine.IsASubmachine);

        //Create states
        casa = myFSM.CreateEntryState("casa");//, () => Debug.Log("encasa"));
        muerto = myFSM.CreateState("muerto", () => morirse = true);
        salirVivo= myFSM.CreateState("salirVivo", muertoAction);
        casaFin = myFSM.CreateState("casaFin", () => { mundo.PacienteMenos(personaje);  Destroy(gameObject); });

        //Submáquina vivo
        yendoCentro = myFSMVivo.CreateEntryState("yendoCentro", yendoCentroAction);
        llegadaCentro = myFSMVivo.CreateState("llegadaCentro");
        entrandoCentro = myFSMVivo.CreateState("entrandoCentro", entrandoCentroAction);
        entrandoCentroUrgente = myFSMVivo.CreateState("entrandoCentroUrgente", entrandoCentroAction);
        siendoAtendidoQuirofano = myFSMVivo.CreateState("siendoAtendidoQuirofano", ()=> { targetUrgencias.ocupado = true; estoySiendoAtendidoFlag = true;  });
        siendoAtendidoConsulta = myFSMVivo.CreateState("siendoAtendidoConsulta");//,()=> Debug.Log("SIENDO ATENDIDO"));
        yendoSala = myFSMVivo.CreateState("yendoSala",()=> { personaje.levantarse(); GoTo(targetUrgencias);estoySiendoAtendidoFlag = true; });
        yendoUCI = myFSMVivo.CreateState("yendoUCI",()=> { GoTo(mundo.casa); mundo.nUCI++;mundo.nUCIText.text = mundo.nUCI.ToString(); emoticono.sprite = emoUCI;estoySiendoAtendidoFlag = true; });
        yendoCasa = myFSMVivo.CreateState("yendoCasa", () => { emoticono.sprite = emoCurado; mundo.nRecuperados++; mundo.nRecuperadosText.text = mundo.nRecuperados.ToString(); GoTo(mundo.casaPaciente);estoySiendoAtendidoFlag = true; });
        llegadaMostrador = myFSMVivo.CreateState("llegadaMostrador");
        llegadaSala = myFSMVivo.CreateState("llegadaSala");
        esperarCelador = myFSMVivo.CreateState("esperarCelador", esperarCeladorAction); 

        //No urgentes
        siendoAtendidoCeladorMostrador = myFSMVivo.CreateState("siendoAtendidoCeladorMostrador",() => estoySiendoAtendidoFlag = true);
        yendoSalaEspera = myFSM.CreateState("yendoSalaEspera", () => estoySiendoAtendidoFlag = false) ;

        //Urgentes
        acudirCeladorSala = myFSMVivo.CreateState("acudirCeladorSala");
        siendoAtendidoCelador = myFSMVivo.CreateState("siendoAtendidoCelador",() => estoySiendoAtendidoFlag = true);
        esperandoSalaUrgente = myFSMVivo.CreateState("esperandoSalaUrgente",() => estoySiendoAtendidoFlag = false);
        
        //Cola fuera
        esperandoCola = myFSMColaFuera.CreateEntryState("esperandoCola");
        avanzandoCola = myFSMColaFuera.CreateState("avanzandoCola", avanzandoColaAction);

        //Cola dentro
        esperandoColaDentro = myFSMColaDentro.CreateEntryState("esperandoColaDentro");
        avanzandoColaDentro = myFSMColaDentro.CreateState("avanzandoColaDentro", avanzandoColaDentroAction);

        //Cola urgente
        esperandoColaUrgente = myFSMColaUrgente.CreateEntryState("esperandoColaUrgente");
        avanzandoColaUrgente = myFSMColaUrgente.CreateState("avanzandoColaUrgente", avanzandoColaUrgenteAction);

        //Analisis de orina
        yendoBaño = myFSMAnalisisOrina.CreateEntryState("yendoBaño", irBanhoAction);
        tomandoMuestra = myFSMAnalisisOrina.CreateState("tomandoMuestra");
        volviendoSitio = myFSMAnalisisOrina.CreateState("volviendoSitio",()=>GoTo(targetUrgencias));

        //Esperando sala espera
        esperandoDePie = myFSMEsperandoSala.CreateEntryState("esperandoDePie",ocupandoDePieAction);//,  () => GoTo(mundo.asientos[0]));//targetUrgencias.libre = true);
        ocupandoAsiento = myFSMEsperandoSala.CreateState("ocupandoAsiento", ocupandoAsientoAction);


        Perception estoyVivoPerception = myFSM.CreatePerception<ValuePerception>(() => estoyVivo);
        Perception soyUrgenteCeladorLibre = myFSMVivo.CreatePerception<ValuePerception>(()=> enfermedad != null && enfermedad.urgente && mundo.targetSalaPaciente[0].libre);
        Perception soyUrgenteCeladorOcupado = myFSMVivo.CreatePerception<ValuePerception>(()=> enfermedad != null && enfermedad.urgente && !mundo.targetSalaPaciente[0].libre);
        Perception haEntradoCentro = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception celadorLibreSala = myFSMVivo.CreatePerception<ValuePerception>(()=>!targetUrgencias.libre); //
        //El celador hace push al personaje
        Perception atendidoCelador = myFSMVivo.CreatePerception<PushPerception>(); 
        //El mundo hace push al paciente cuando hay sala libre
        SalaAsignadaLibre = myFSMVivo.CreatePerception<PushPerception>(); 
        Perception heLlegadoSala = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado); 
        //El cirujano lo manda a la UCI
         soyGrave = myFSMVivo.CreatePerception<PushPerception>(); 
        //El cirujano lo manda a la casa
        soyLeve = myFSMVivo.CreatePerception<PushPerception>(); 
        //Si ha llegado a casa o a la uci
        Perception heLlegado = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Si he muerto, timer de la enfermedad
        seMeAcaboElTiempo = myFSMVivo.CreatePerception<TimerPerception>(timeMorir);
        estoySiendoAtendio = myFSMVivo.CreatePerception<ValuePerception>(() =>! estoySiendoAtendidoFlag);
        Perception heMuerto = myFSMVivo.CreateAndPerception<AndPerception>(estoySiendoAtendio, seMeAcaboElTiempo);
        // Perception heMuerto = myFSMVivo.CreatePerception<TimerPerception>(enfermedad.timerEnfermedad);
        Perception noSoyUrgente = myFSMVivo.CreatePerception<ValuePerception>(() => enfermedad!=null&&!enfermedad.urgente);
        Perception hayHueco = myFSMColaFuera.CreatePerception<ValuePerception>(() =>  targetUrgenciasID != 0 && mundo.targetColaFuera[targetUrgenciasID-1].libre );
        Perception heAvanzadoUrgente = myFSMColaFuera.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception hayHuecoUrgente = myFSMColaUrgente.CreatePerception<ValuePerception>(() =>  targetUrgenciasID != 0 && mundo.targetColaUrgentes[targetUrgenciasID-1].libre );
        Perception heAvanzado = myFSMColaFuera.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception noHayAforo = myFSMVivo.CreatePerception<ValuePerception>(() => targetUrgenciasID == 0 && mundo.aforo);

        Perception hayHuecoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => targetUrgenciasID!=0&&mundo.targetColaDentro[targetUrgenciasID - 1].libre);
        Perception heAvanzadoDentro = myFSMColaDentro.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Añadir para ver si hay algún mostrador libre
        

        //*HACE FALTA COMPROBAR QUE EL CELADOR DE SALA ESTÉ LIBRE Y HACER SU COLA////


        Perception hayMostradorLibre = myFSMColaDentro.CreatePerception<ValuePerception>(() => mostradorLibre());
        //El celador pone el push de ser atendido
        heSidoAtendido = myFSMVivo.CreatePerception<PushPerception>();
        //Añadir como ver si hay algún target de asiento libre
        Perception asientoLibre = myFSMEsperandoSala.CreatePerception<ValuePerception>(() =>  ComprobarOcupados());
        //Push por el enfermero
         tengoQueHacerAnalisisOrina = myFSMVivo.CreatePerception<PushPerception>();

        Perception hellegadoBaño = myFSMAnalisisOrina.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception heTomadoMuestra = myFSMAnalisisOrina.CreatePerception<TimerPerception>(timerOrina);
        Perception hellegadoSitio = myFSMAnalisisOrina.CreatePerception<ValuePerception>(() => personaje.haLlegado);

        todaviaTengoQueSerTratado = myFSMVivo.CreatePerception<PushPerception>();
        Perception animacionMuerto = myFSM.CreatePerception<TimerPerception>(timerAnimacionMorir);
        Perception hayCelador = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado && !mundo.targetEsperaMostrador[targetUrgenciasID].libre);
        Perception noHayCelador = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado && mundo.targetEsperaMostrador[targetUrgenciasID].libre);
        Perception tengoQueHacerCola = myFSMVivo.CreatePerception<PushPerception>();
        //El profesional hace push para que vuelvas a sala de espera
        esperandoSalaEspera = myFSMVivo.CreateSubStateMachine("esperandoSalaEspera", myFSMEsperandoSala,esperandoDePie);
        vivo = myFSM.CreateSubStateMachine("vivo", myFSMVivo, yendoCentro);
        haciendoAnalisisOrina = myFSMVivo.CreateSubStateMachine("haciendoAnalisisOrina", myFSMAnalisisOrina,yendoBaño);
        haciendoColaDentro = myFSMVivo.CreateSubStateMachine("haciendoColaDentro", myFSMColaDentro);
        haciendoColaFuera = myFSMVivo.CreateSubStateMachine("haciendoColaFuera", myFSMColaFuera, esperandoCola);
        haciendoColaUrgente = myFSMVivo.CreateSubStateMachine("haciendoColaurgente", myFSMColaUrgente);
        

        //Submaquinas

        //Transiciones
        myFSM.CreateTransition("aparecer", casa, estoyVivoPerception, vivo);
        myFSMVivo.CreateTransition("llegado centro, soy urgente y celador libre", yendoCentro, soyUrgenteCeladorLibre, entrandoCentroUrgente);
        myFSMVivo.CreateTransition("llegado centro, soy urgente y hago cola", yendoCentro, soyUrgenteCeladorOcupado, haciendoColaUrgente);
        myFSMColaUrgente.CreateTransition("hay hueco libre", esperandoColaUrgente, hayHuecoUrgente, avanzandoColaUrgente);
        myFSMColaUrgente.CreateTransition("he avanzado urgente", avanzandoColaUrgente, heAvanzadoUrgente, esperandoColaUrgente);
        myFSMColaUrgente.CreateExitTransition("celador libre", esperandoColaUrgente, soyUrgenteCeladorLibre, entrandoCentroUrgente);

        myFSMVivo.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);
        
        myFSMVivo.CreateTransition("acudiendo celador", entrandoCentroUrgente, celadorLibreSala, acudirCeladorSala);    
        myFSMVivo.CreateTransition("ser atendido celador", acudirCeladorSala, heLlegado, siendoAtendidoCelador);
        //myFSMVivo.CreateTransition("esperar sala libre", siendoAtendidoCelador, heSidoAtendido, urgente? esperandoSalaUrgente: esperandoSalaEspera);  
        myFSMVivo.CreateTransition("esperar sala libre", siendoAtendidoCelador, heSidoAtendido,  esperandoSalaEspera);
        // myFSMVivo.CreateTransition("acudir quirofano", esperandoSalaUrgente, SalaAsignadaLibre, yendoSala);    
       // myFSMVivo.CreateTransition("acudir quirofano", esperandoSalaUrgente, QuirofanoLibre, yendoSala);
        myFSMVivo.CreateTransition("llegado quirofano", yendoSala, heLlegadoSala, urgente&&enfermedad.tipoEnfermedad!=TipoEnfermedad.Covid? siendoAtendidoQuirofano: siendoAtendidoConsulta);    
        myFSMVivo.CreateTransition("acudir a la UCI", siendoAtendidoQuirofano, soyGrave, yendoUCI);    
        myFSMVivo.CreateTransition("acudir a casa", siendoAtendidoQuirofano, soyLeve, yendoCasa);    
        myFSMVivo.CreateTransition("he llegado a UCI", yendoUCI, heLlegado, casaFin);  
        //Mirar si hay problemas entre dos estados de máquinas diferentes
        myFSMVivo.CreateTransition("he llegado a casa", yendoCasa, heLlegado, casaFin);    

        myFSMVivo.CreateExitTransition("morirse", vivo, heMuerto,  salirVivo);   
        myFSM.CreateTransition("desaparecer", muerto, animacionMuerto, casaFin);   
        myFSMColaFuera.CreateTransition("hay hueco libre", esperandoCola, hayHueco, avanzandoCola);   
        myFSMColaFuera.CreateTransition("he avanzado", avanzandoCola, heAvanzado, esperandoCola);
        myFSMColaFuera.CreateExitTransition("aforo libre", esperandoCola, noHayAforo, entrandoCentro);
        myFSMVivo.CreateTransition("hacer cola dentro", entrandoCentro, noSoyUrgente, haciendoColaDentro);
        myFSMColaDentro.CreateTransition("hay hueco libre", esperandoColaDentro, hayHuecoDentro, avanzandoColaDentro);
        myFSMColaDentro.CreateTransition("he avanzado", avanzandoColaDentro, heAvanzadoDentro, esperandoColaDentro);
        myFSMColaDentro.CreateExitTransition("hay mostrador libre", esperandoColaDentro, hayMostradorLibre, llegadaMostrador);
        myFSMVivo.CreateTransition("llegada a mostrador", llegadaMostrador, hayCelador, siendoAtendidoCelador);
        myFSMVivo.CreateTransition("llegada a mostrador no hay celador", llegadaMostrador, noHayCelador, esperarCelador);
        myFSMVivo.CreateTransition("llegada a mostrador tras cambiar mostrador", esperarCelador, hayCelador, llegadaMostrador);
        myFSMVivo.CreateTransition("tengo que hacer cola", esperarCelador, tengoQueHacerCola,haciendoColaDentro);
        myFSMVivo.CreateTransition("esperando sala", siendoAtendidoCeladorMostrador, heSidoAtendido, esperandoSalaEspera);
        myFSMEsperandoSala.CreateTransition("hay asiento libre", esperandoDePie, asientoLibre, ocupandoAsiento);
        // myFSMVivo.CreateExitTransition("hay sala libre", ocupandoAsiento, SalaAsignadaLibre, yendoSala);
        myFSMEsperandoSala.CreateExitTransition("hay sala libre", ocupandoAsiento, SalaAsignadaLibre, yendoSala);
        // myFSMVivo.CreateExitTransition("hay sala libre pie", esperandoDePie, SalaAsignadaLibre, yendoSala);
        myFSMEsperandoSala.CreateExitTransition("hay sala libre pie", esperandoDePie, SalaAsignadaLibre, yendoSala);
        myFSMVivo.CreateTransition("llegada a sala y siendo atendido", yendoSala, heLlegadoSala, urgente ? siendoAtendidoQuirofano : siendoAtendidoConsulta);
        myFSMVivo.CreateTransition("ir hacer analisis orina", siendoAtendidoConsulta, tengoQueHacerAnalisisOrina, haciendoAnalisisOrina);
        myFSMAnalisisOrina.CreateTransition("tomando muestra orina", yendoBaño, hellegadoBaño, tomandoMuestra);
        myFSMAnalisisOrina.CreateTransition("volver al sitio", tomandoMuestra, heTomadoMuestra, volviendoSitio);
        myFSMAnalisisOrina.CreateExitTransition("volver a ser atendido", volviendoSitio, hellegadoSitio, siendoAtendidoConsulta);

        //myFSMVivo.CreateTransition("volver a sala espera", siendoAtendidoConsulta, todaviaTengoQueSerTratado, yendoSalaEspera);
        //myFSMVivo.CreateTransition("volver esperando sala", yendoSalaEspera, heLlegado, esperandoSalaEspera);

        myFSMVivo.CreateTransition("volver a sala espera", siendoAtendidoConsulta, todaviaTengoQueSerTratado, esperandoSalaEspera);
        //myFSMVivo.CreateTransition("volver esperando sala", yendoSalaEspera, heLlegado, esperandoSalaEspera);

        myFSMVivo.CreateTransition("acudir a la UCI consulta", siendoAtendidoConsulta, soyGrave, yendoUCI);
        myFSMVivo.CreateTransition("acudir a casa consulta", siendoAtendidoConsulta, soyLeve, yendoCasa);

        //myFSM.CreateTransition("No puedo morir", salirVivo,    , vivo);
  
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
    private void irBanhoAction()
    {
       GoTo(targetBanho);
        //Debug.Log("al baño");
    }
    void Update()
    {
        /*if (enfermedad.tipoEnfermedad.Equals(TipoEnfermedad.Cistitis))
        {
            Debug.Log(myFSM.GetCurrentState().Name);
            

        }*/
        //Debug.Log(myFSMAnalisisOrina.GetCurrentState().Name);
        //Debug.Log(myFSMVivo.GetCurrentState().Name);
        myFSM.Update();
        myFSMAnalisisOrina.Update();
        myFSMColaDentro.Update();
        myFSMColaUrgente.Update();
        myFSMColaFuera.Update();
        myFSMEsperandoSala.Update();
        myFSMVivo.Update();
        timeEspera += Time.deltaTime;
        if (myFSMVivo.GetCurrentState().Name.Equals(siendoAtendidoQuirofano.Name))
        {
            timeMorir += Time.deltaTime;
        }
        if (morirse)
        {
            personaje.Morirse();
          
           
            StartCoroutine(Die());
            if (fade == true)
            {
                //Se coge el material y se va bajando el alpha hasta que llegue a cero, entonces se detiene con el 
                GetComponentInChildren<SkinnedMeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");
                Color oColor = GetComponentInChildren<SkinnedMeshRenderer>().material.color;
                float fadeAmount = oColor.a - (0.5f * Time.deltaTime);
                oColor = new Color(oColor.r, oColor.g, oColor.b, fadeAmount);
                GetComponentInChildren<SkinnedMeshRenderer>().material.color = oColor;
                if (oColor.a <= 0)
                {
                    fade = false;
                    mundo.PacienteMenos(personaje);
                    mundo.nMuertes++;
                    mundo.numMuertosText.text = mundo.nMuertes.ToString();
                    //morirse = false;
                }
            }
        }
        if (urgente)
        {
           //Debug.Log(myFSMVivo.GetCurrentState().Name);
           //Debug.Log(myFSMColaUrgente.GetCurrentState().Name);
        }

        if (myFSMEsperandoSala.GetCurrentState().Name.Equals(ocupandoAsiento.Name))
        {
            if (personaje.haLlegado)
            {
                
                personaje.sentarse();
            }
        }
        //Debug.Log(myFSMVivo.GetCurrentState().Name);
        if (myFSMVivo.GetCurrentState().Name.Equals(siendoAtendidoCelador.Name))
        {
            Debug.Log("SIENDO ATENDIDO POR CELADOR");
        }
    }
    private bool ComprobarOcupados()
    {
        int cuantos= Array.FindAll(mundo.asientos, (a) => a.libre).Length;
        return cuantos > 0;
    }
    private void muertoAction()
    {
        emoticono.sprite = emoMuerto;
        mundo.EliminarListaEspera(this);
        
        if (targetUrgencias != null)
        {
            targetUrgencias.libre = true;
            targetUrgencias.ocupado = true;
        }
        if(personaje.myAgent.enabled)
            personaje.myAgent.Stop();

        personaje.muerto = true;
        personaje.myAgent.enabled = false;
        myFSMVivo.Fire(myFSMVivo.CreateExitTransition("salir vivo", myFSMVivo.GetCurrentState(), myFSMVivo.CreatePerception<ValuePerception>(() => morirse == true), muerto));
    }



    public void setEnfermedad(Enfermedad en, Sprite emo)
    {
        emoticono.sprite = emo;
        enfermedad = en;
        timeMorir = enfermedad.timerEnfermedad;
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
        targetUrgencias = mundo.targetColaFuera[targetUrgenciasID - 1];
        targetUrgenciasID--;
        targetUrgencias.libre = false;
        GoTo(targetUrgencias);
    }

    private void avanzandoColaDentroAction()
    {
        //Debug.Log("avanzandocolaadentroction");
        if (targetUrgencias != null)
        {
            targetUrgencias.libre = true;
        }
        targetUrgencias = mundo.targetColaDentro[targetUrgenciasID - 1];
        targetUrgenciasID--;
        targetUrgencias.libre = false;
        GoTo(targetUrgencias);
    }
    private void avanzandoColaUrgenteAction()
    {
        //Debug.Log("avanzandocolaadentroction");
        if (targetUrgencias != null)
        {
            targetUrgencias.libre = true;
        }
        targetUrgencias = mundo.targetColaUrgentes[targetUrgenciasID - 1];
        targetUrgenciasID--;
        targetUrgencias.libre = false;
        GoTo(targetUrgencias);
    }

    private void entrandoCentroAction()
    {
        if (urgente)
        {
            for(int i=0; i < mundo.targetSalaPaciente.Length; i++)
            {
                targetUrgencias = mundo.targetSalaPaciente[i];
                targetUrgencias.libre = false;
                GoTo(targetUrgencias);
                break;
            }
        }
        else
        {
            for (int i = 0; i < mundo.targetColaDentro.Length; i++)
            {
                if (mundo.targetColaDentro[i].libre)
                {
                    targetUrgencias.libre = true;
                    targetUrgencias = mundo.targetColaDentro[i];
                    targetUrgencias.libre = false;
                    targetUrgenciasID = i;
                    GoTo(targetUrgencias);
                    break;
                }
            }
        }
    }

    private void ocupandoAsientoAction()
    {
        for (int i = 0; i < mundo.asientos.Length; i++)
        {
            if (mundo.asientos[i].libre)
            {
                
                targetUrgencias.libre = true;
                targetUrgencias = mundo.asientos[i];
                targetUrgencias.libre = false;
                targetUrgenciasID = i;
                GoTo(targetUrgencias);
                break;
            }
        }
    }
    private void ocupandoDePieAction()
    {
        estoySiendoAtendidoFlag = false;
        for (int i = 0; i < mundo.dePie.Length; i++)
        {
            if (mundo.dePie[i].libre)
            {
                targetUrgencias.libre = true;
                targetUrgencias = mundo.dePie[i];
                targetUrgencias.libre = false;
                targetUrgenciasID = i;
                GoTo(targetUrgencias);
                break;
            }
        }
    }
    //REVISAR
    private bool mostradorLibre()
    {
        //Debug.Log("hay mostrador libre");
        for(int i=0; i< mundo.targetMostradorPaciente.Length; i++)
        {
            if ((mundo.targetMostradorPaciente[i].ocupado)&&(mundo.targetMostradorPaciente[i].libre))
            {
                mundo.targetMostradorPaciente[i].libre = false;
                mundo.targetMostradorPaciente[i].actual = personaje;
                targetUrgencias.libre = true;
                targetUrgencias = mundo.targetMostradorPaciente[i];
                targetUrgenciasID = i;
                GoTo(targetUrgencias);
                return true;
            }
        }
        return false;
    }

    private void yendoCentroAction()
    {
        if (urgente)
        {
            targetUrgenciasID = mundo.targetColaUrgentes.Length;
            for (int i = 0; i < mundo.targetColaUrgentes.Length; i++)
            {
                if (mundo.targetColaUrgentes[i].libre)
                {
                    targetUrgenciasID = i + 1;
                    return;
                }
            }
        }
        else
        {
            targetUrgenciasID = mundo.targetColaFuera.Length;
            
        }
    }

    private void esperarCeladorAction()
    {
        int i = (targetUrgenciasID + 1) % 2;
        if ((mundo.targetMostradorPaciente[i].ocupado) && (mundo.targetMostradorPaciente[i].libre) && !mundo.targetEsperaMostrador[i].libre)
        {
            mundo.targetMostradorPaciente[i].libre = false;
            mundo.targetMostradorPaciente[i].actual = personaje;
            targetUrgencias.libre = true;
            targetUrgencias = mundo.targetMostradorPaciente[i];
            targetUrgenciasID = i;
            GoTo(targetUrgencias);
        }
        else
        {
            targetUrgenciasID = mundo.targetColaDentro.Length;
            myFSMVivo.Fire("tengo que hacer cola");
        }
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
