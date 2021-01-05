using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Limpiador : MonoBehaviour
{
    Mundo mundo;
    Personaje personaje;
    TargetUrgencias targetUrgencias;
    int targetUrgenciasID;
    
    public bool ocupado = false; // o que este en la sala de limpiadores, esta pa que no de errores
    public bool jornadaFlag;
    public Sala salaLimpiando;
    public int timeJornada = 2000;

    public Image emoticono;
    public Sprite emoLimpiando, emoCasa, emoConsultarPantalla;
    //Maquina de estados
    StateMachineEngine myFSM;

    //Estados
    State casa;
    State casaFin;
    State irCasa;
    State irPantalla;
    State irSala;
    State irQuirofano;
    State esperandoConsultarPantalla;
    State consultandoPantalla;
    State limpiandoQuirofano;
    State limpiandoSala;
    Perception haySalaLimpiar;

    // Start is called before the first frame update
    void Start()
    {
        mundo = FindObjectOfType<Mundo>();
        personaje = GetComponent<Personaje>();
        myFSM = new StateMachineEngine();
        jornadaFlag = true;

        //Create states
        casa = myFSM.CreateEntryState("casa");
        casaFin = myFSM.CreateState("casaFin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje);mundo.ReemplazarLimpiador(personaje.nombre); Destroy(this.gameObject); });
        irCasa = myFSM.CreateState("irCasa", irCasaAction);
        irPantalla = myFSM.CreateState("irPantalla", irPantallaAction);
        irSala = myFSM.CreateState("irSala", irSalaAction);
        irQuirofano = myFSM.CreateState("irQuirofano", irSalaAction);
        esperandoConsultarPantalla = myFSM.CreateState("esperandoConsultarPantalla", () =>PutEmoji(emoConsultarPantalla));
        consultandoPantalla = myFSM.CreateState("consultandoPantalla", () => PutEmoji(emoConsultarPantalla));
        limpiandoQuirofano = myFSM.CreateState("limpiandoQuirofano", () => PutEmoji(emoLimpiando));
        limpiandoSala = myFSM.CreateState("limpiandoSala", () => PutEmoji(emoLimpiando));


        //Create perceptions
        //Comienza jornada, ir hacia la pantalla
        Perception comienzaJornada = myFSM.CreatePerception<ValuePerception>(() => jornadaFlag); //Mirar
        //Si termina el tiempo de la jornada
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        //Si hay sala que limpiar, el limpiador se dirige a ella
        haySalaLimpiar = myFSM.CreatePerception<PushPerception>();
        //Si hay un quirofano que limpiar
        Perception hayQuirofanoLimpiar = myFSM.CreatePerception<PushPerception>();
        //Si hay un quirofano que limpiar urgente, el mundo llama al limpiador
        Perception salaLimpia = myFSM.CreatePerception<ValuePerception>(() => salaLimpiando.porcentajeSuciedad < 0);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPuesto = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPantallaLibre = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado && targetUrgenciasID==0);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaPantallaOcupado = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado && targetUrgenciasID > 0);
        //Se da la percepción desde el update, comprobando si está en la posición correcta
        Perception llegadaCasa = myFSM.CreatePerception<ValuePerception>(()=> personaje.haLlegado);


        //Create transitions
        myFSM.CreateTransition("comienza jornada", casa, comienzaJornada, irPantalla);
        myFSM.CreateTransition("llegada pantalla",irPantalla , llegadaPantallaLibre, consultandoPantalla);
        myFSM.CreateTransition("llegada esperar pantalla",irPantalla , llegadaPantallaOcupado, esperandoConsultarPantalla);
        myFSM.CreateTransition("consultar pantalla",esperandoConsultarPantalla, llegadaPantallaLibre, consultandoPantalla);
        myFSM.CreateTransition("hay sala limpiar", consultandoPantalla, haySalaLimpiar, irSala);
        myFSM.CreateTransition("hay quirofano limpiar", consultandoPantalla, hayQuirofanoLimpiar, irQuirofano);
        myFSM.CreateTransition("llegada a sala", irSala, llegadaPuesto, limpiandoSala);
        myFSM.CreateTransition("llegada a quirofano", irQuirofano, llegadaPuesto, limpiandoQuirofano);
        myFSM.CreateTransition("limpia sala", limpiandoSala, salaLimpia, irPantalla);
        myFSM.CreateTransition("limpia quirofano", limpiandoQuirofano, salaLimpia, irPantalla);
        myFSM.CreateTransition("hay quirofano urgente", limpiandoSala, hayQuirofanoLimpiar, irQuirofano);
        myFSM.CreateTransition("termina jornada", consultandoPantalla, terminadaJornada, irCasa);
        myFSM.CreateTransition("termina jornada esperar", esperandoConsultarPantalla, terminadaJornada, irCasa);
        myFSM.CreateTransition("llegada casa", irCasa, llegadaCasa, casaFin);

    }

    // Update is called once per frame
    void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals(consultandoPantalla.Name))
        {
            
            if(mundo.cirugiasSucias.Count > 0)
            {
                salaLimpiando = mundo.cirugiasSucias[0];
                mundo.salasSucias.Remove(salaLimpiando);
                if (targetUrgencias != null)
                {
                    targetUrgencias.libre = true;
                }
                targetUrgencias = salaLimpiando.posicionLimpiador;
                myFSM.Fire("hay quirofano limpiar");

            }else if(mundo.salasSucias.Count > 0)
            {
                salaLimpiando = mundo.salasSucias[0];
                mundo.salasSucias.Remove(salaLimpiando);
                if (targetUrgencias != null)
                {
                    targetUrgencias.libre = true;
                }
                targetUrgencias = salaLimpiando.posicionLimpiador;
                haySalaLimpiar.Fire();
                //myFSM.Fire("hay sala limpiar");
            }

        }else if (myFSM.GetCurrentState().Name.Equals(esperandoConsultarPantalla.Name))
        {
            if (targetUrgenciasID > 0)
            {
                if (mundo.targetLimpiadores[targetUrgenciasID - 1].libre)
                {
                    targetUrgencias.libre = true;
                    targetUrgenciasID--;
                    targetUrgencias = mundo.targetLimpiadores[targetUrgenciasID];
                    targetUrgencias.libre = false;
                    personaje.GoTo(targetUrgencias);
                   
                }
            }
            
        }else if (myFSM.GetCurrentState().Name.Equals(limpiandoQuirofano.Name) || myFSM.GetCurrentState().Name.Equals(limpiandoSala.Name))
        {
            salaLimpiando.porcentajeSuciedad = salaLimpiando.porcentajeSuciedad - 0.5f;
            if (salaLimpiando.porcentajeSuciedad < mundo.umbral)
            {
                salaLimpiando.sucio = false;
                salaLimpiando.heLlamadoAlMundo = false;
            }
        }
    }

    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }

    private void irPantallaAction()
    {
        //Nav Mesh ir al target puesto
        //Si el puesto de trabajo está libre
        for (int i = 0; i < mundo.targetLimpiadores.Length; i++)
        {
            if (mundo.targetLimpiadores[i].libre)
            {
                mundo.targetLimpiadores[i].libre = false;
                targetUrgencias = mundo.targetLimpiadores[i];
                targetUrgenciasID = i;
                break;
            }
        }
       // targetUrgencias.libre = false;
        personaje.GoTo(targetUrgencias);
    }

    private void irSalaAction()
    {
       
        personaje.GoTo(targetUrgencias);
    }

    public void limpiarQuirofanoUrgente()
    {
        //Si la sala que estoy limpiando esta sucia, añadirla a la lista
        if (salaLimpiando.sucio)
        {
            mundo.AddSalaSucia(salaLimpiando);
        }
        salaLimpiando = mundo.cirugiasSucias[0];
        mundo.salasSucias.Remove(salaLimpiando);
        targetUrgencias = salaLimpiando.posicionLimpiador;
        myFSM.Fire("hay quirofano urgente");
    }

    private void irCasaAction()
    {
        //Go to target casa
        targetUrgencias.libre = true;
        jornadaFlag = false;
        personaje.GoTo(mundo.casa);
        PutEmoji(emoCasa);
    }
}
