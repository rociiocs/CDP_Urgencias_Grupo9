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
    private Personaje personaje;
    public Enfermedad enfermedad;
    public bool tieneBote;
    public bool urgente;
    public bool estoyVivo = true;
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

    //Para que se pueda disparar
    public Perception SalaAsignadaLibre;
    public Perception heSidoAtendido;
    public Perception todaviaTengoQueSerTratado;
    public Perception tengoQueHacerAnalisisOrina;
    public Perception soyLeve;
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
        casaFin = myFSM.CreateState("casaFin", () => { mundo.PacienteMenos(personaje);  Destroy(gameObject); });

        //Submáquina vivo


        yendoCentro = myFSMVivo.CreateEntryState("yendoCentro", () => { targetUrgenciasID = mundo.targetColaFuera.Length; });
        llegadaCentro = myFSMVivo.CreateState("llegadaCentro");
        entrandoCentro = myFSMVivo.CreateState("entrandoCentro", entrandoCentroAction);
        entrandoCentroUrgente = myFSMVivo.CreateState("entrandoCentroUrgente");
        siendoAtendidoQuirofano = myFSMVivo.CreateState("siendoAtendidoQuirofano");
        siendoAtendidoConsulta = myFSMVivo.CreateState("siendoAtendidoConsulta",()=> Debug.Log("SIENDO ATENDIDO"));
        yendoSala = myFSMVivo.CreateState("yendoSala",()=>GoTo(targetUrgencias));
        yendoUCI = myFSMVivo.CreateState("yendoUCI");
        llegadaUCI = myFSMVivo.CreateState("llegadaUCI");
        yendoCasa = myFSMVivo.CreateState("yendoCasa");


        //No urgentes
        siendoAtendidoCeladorMostrador = myFSMVivo.CreateState("siendoAtendidoCeladorMostrador");

        yendoSalaEspera = myFSM.CreateState("yendoSalaEspera");

        //Urgentes
        acudirCeladorSala = myFSMVivo.CreateState("acudirCeladorSala");
        siendoAtendidoCelador = myFSMVivo.CreateState("siendoAtendidoCelador");
        esperandoSalaUrgente = myFSMVivo.CreateState("esperandoSalaUrgente");
        
        //Cola fuera
      
        esperandoCola = myFSMColaFuera.CreateEntryState("esperandoCola");
        avanzandoCola = myFSMColaFuera.CreateState("avanzandoCola", avanzandoColaAction);

        //Cola dentro
      
        esperandoColaDentro = myFSMColaDentro.CreateEntryState("esperandoCola");
        avanzandoColaDentro = myFSMColaDentro.CreateState("avanzandoCola", avanzandoColaDentroAction);

        //Analisis de orina

        yendoBaño = myFSMAnalisisOrina.CreateEntryState("yendoBaño", irBanhoAction);
        tomandoMuestra = myFSMAnalisisOrina.CreateState("tomandoMuestra");
        volviendoSitio = myFSMAnalisisOrina.CreateState("volviendoSitio",()=>GoTo(targetUrgencias));

        //Esperando sala espera

        esperandoDePie = myFSMEsperandoSala.CreateEntryState("esperandoDePie",ocupandoDePieAction);//,  () => GoTo(mundo.asientos[0]));//targetUrgencias.libre = true);

        ocupandoAsiento = myFSMEsperandoSala.CreateState("ocupandoAsiento", ocupandoAsientoAction);


        Perception estoyVivoPerception = myFSM.CreatePerception<ValuePerception>(() => estoyVivo);
        Perception soyUrgente = myFSMVivo.CreatePerception<ValuePerception>(()=> enfermedad != null && enfermedad.urgente);
        Perception haEntradoCentro = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception celadorLibreSala = myFSMVivo.CreatePerception<ValuePerception>(); //
        //El celador hace push al personaje
        Perception atendidoCelador = myFSMVivo.CreatePerception<PushPerception>(); 
        //El mundo hace push al paciente cuando hay sala libre
         SalaAsignadaLibre = myFSMVivo.CreatePerception<PushPerception>(); 
        Perception heLlegadoSala = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado); 
        //El cirujano lo manda a la UCI
        Perception soyGrave = myFSMVivo.CreatePerception<PushPerception>(); 
        //El cirujano lo manda a la casa
        soyLeve = myFSMVivo.CreatePerception<PushPerception>(); 
        //Si ha llegado a casa o a la uci
        Perception heLlegado = myFSMVivo.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Si he muerto, timer de la enfermedad
        Perception heMuerto = myFSM.CreatePerception<TimerPerception>(enfermedad.timerEnfermedad);

        Perception noSoyUrgente = myFSMVivo.CreatePerception<ValuePerception>(() => enfermedad!=null&&!enfermedad.urgente);
        Perception hayHueco = myFSMColaFuera.CreatePerception<ValuePerception>(() =>  targetUrgenciasID != 0&&mundo.targetColaFuera[targetUrgenciasID-1].libre );
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

        //El profesional hace push para que vuelvas a sala de espera
         todaviaTengoQueSerTratado = myFSMVivo.CreatePerception<PushPerception>();
        Perception animacionMuerto = myFSM.CreatePerception<TimerPerception>(timerAnimacionMorir);
        esperandoSalaEspera = myFSMVivo.CreateSubStateMachine("esperandoSalaEspera", myFSMEsperandoSala,esperandoDePie);
        vivo = myFSM.CreateSubStateMachine("vivo", myFSMVivo, yendoCentro);
        haciendoAnalisisOrina = myFSMVivo.CreateSubStateMachine("haciendoAnalisisOrina", myFSMAnalisisOrina,yendoBaño);
        haciendoColaDentro = myFSMVivo.CreateSubStateMachine("haciendoColaDentro", myFSMColaDentro);
        haciendoColaFuera = myFSMVivo.CreateSubStateMachine("haciendoColaFuera", myFSMColaFuera, esperandoCola);


        //Submaquinas

        //Transiciones
        myFSM.CreateTransition("aparecer", casa, estoyVivoPerception, vivo);
        myFSMVivo.CreateTransition("he llegado centro y soy urgente", yendoCentro, soyUrgente, entrandoCentroUrgente);
        myFSMVivo.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);
        //myFSM.CreateTransition("he llegado centro y soy urgente", yendoCentro, soyUrgente, entrandoCentro);
        //myFSM.CreateTransition("he llegado centro y no soy urgente", yendoCentro, noSoyUrgente, haciendoColaFuera);


        myFSMVivo.CreateTransition("acudiendo celador",entrandoCentro, celadorLibreSala, acudirCeladorSala);    
        myFSMVivo.CreateTransition("ser atendido celador", acudirCeladorSala, heLlegado, siendoAtendidoCelador);    
        myFSMVivo.CreateTransition("esperar sala libre", siendoAtendidoCelador, heSidoAtendido, esperandoSalaUrgente);    
        myFSMVivo.CreateTransition("acudir quirofano", esperandoSalaUrgente, SalaAsignadaLibre, yendoSala);    
        myFSMVivo.CreateTransition("llegado quirofano", yendoSala, heLlegadoSala, urgente? siendoAtendidoQuirofano: siendoAtendidoConsulta);    
        myFSMVivo.CreateTransition("acudir a la UCI", siendoAtendidoQuirofano, soyGrave, yendoUCI);    
        myFSMVivo.CreateTransition("acudir a casa", siendoAtendidoQuirofano, soyLeve, yendoCasa);    
        myFSMVivo.CreateTransition("he llegado a UCI", yendoUCI, heLlegado, casaFin);  
        //Mirar si hay problemas entre dos estados de máquinas diferentes
        myFSMVivo.CreateTransition("he llegado a casa", yendoCasa, heLlegado, casaFin);    

        myFSM.CreateTransition("morirse", vivo, heMuerto, salirVivo);   
        myFSM.CreateTransition("desaparecer", muerto, animacionMuerto, casaFin);   
        myFSMColaFuera.CreateTransition("hay hueco libre", esperandoCola, hayHueco, avanzandoCola);   
        myFSMColaFuera.CreateTransition("he avanzado", avanzandoCola, heAvanzado, esperandoCola);
        myFSMColaFuera.CreateExitTransition("aforo libre", esperandoCola, noHayAforo, entrandoCentro);
        myFSMVivo.CreateTransition("hacer cola dentro", entrandoCentro, noSoyUrgente, haciendoColaDentro);
        myFSMColaDentro.CreateTransition("hay hueco libre", esperandoColaDentro, hayHuecoDentro, avanzandoColaDentro);
        myFSMColaDentro.CreateTransition("he avanzado", avanzandoColaDentro, heAvanzadoDentro, esperandoColaDentro);
        myFSMColaDentro.CreateExitTransition("hay mostrador libre", esperandoColaDentro, hayMostradorLibre, siendoAtendidoCeladorMostrador);
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
        Debug.Log("al baño");
    }
    void Update()
    {
        /*if (enfermedad.tipoEnfermedad.Equals(TipoEnfermedad.Cistitis))
        {
            Debug.Log(myFSM.GetCurrentState().Name);
            

        }*/
        Debug.Log(myFSMAnalisisOrina.GetCurrentState().Name);
        Debug.Log(myFSMVivo.GetCurrentState().Name);
        myFSM.Update();
        myFSMAnalisisOrina.Update();
        myFSMColaDentro.Update();
        myFSMColaFuera.Update();
        myFSMEsperandoSala.Update();
        myFSMVivo.Update();
        timeEspera += Time.deltaTime;

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
                    morirse = false;
                }
            }
        }

        //Debug.Log("fsm" + myFSM.GetCurrentState().Name);
        //Debug.Log("fsmVivo " + myFSMVivo.GetCurrentState().Name);
        //Debug.Log("fsmColaFuera " + myFSMColaFuera.GetCurrentState().Name);
        //Debug.Log("fsmColadentro " + myFSMColaDentro.GetCurrentState().Name);

        if (myFSMEsperandoSala.GetCurrentState().Name.Equals(ocupandoAsiento.Name))
        {
            if (personaje.haLlegado)
            {
                personaje.sentarse();
            }
        }
        //Debug.Log(myFSMVivo.GetCurrentState().Name);
    }
    private bool ComprobarOcupados()
    {
        int cuantos= Array.FindAll(mundo.asientos, (a) => a.libre).Length;
        return cuantos > 0;
    }
    private void muertoAction()
    {
        emoticono.sprite = emoMuerto;
        if (targetUrgencias != null)
        {
            targetUrgencias.libre = true;
            targetUrgencias.ocupado = true;
        }
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
    private void entrandoCentroAction()
    {
        if (urgente)
        {
            //FALTA MANDARLOS AL TARGET DEL CELADOR DE SALA O ESPERAR
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
                targetUrgencias.ocupado = true;
                return true;
            }
        }
        return false;
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
