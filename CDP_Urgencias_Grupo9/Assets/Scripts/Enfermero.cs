using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Analisis
{
    NULO,
    PCR,
    ORINA,
    DARBOTE,
    SUERO,
    SANGRE
}

//Meter en la memoria los estados de suero y lo que falte
//Ademas el estado examinar, que aunque sea rápido, se necesita

//quizas hay que cambiar el analisis de orina y hacer un estado más: esperando orina
//revisar
//null despues de enviar a baño
public class Enfermero : MonoBehaviour
{
    //info
    StateMachineEngine myFSM;
    float timeJornada = 10, timeAnimacionAtender = 3;
    public bool llevarLabFlag = false, jornadaFlag = false;
    Analisis currentAnalisis;
    //Referencias
    public TargetUrgencias puestoTrabajo;
    public TargetUrgencias asientoPaciente;
    public TargetUrgencias banhoTarget;
    public Image emoticono;
    public Sprite emoEsperando, emoCasa, emoSangre, emoBote, emoOrina, emoPCR, emoLAB, emoSuero, emoEsperandoOrina;
    public Paciente currentPaciente;
    private Mundo mundo;
    Personaje personaje;


    void Start()
    {
        personaje = GetComponent<Personaje>();
        mundo = FindObjectOfType<Mundo>();
        CreacionEstados();
    }
    private void CreacionEstados()
    {

        myFSM = new StateMachineEngine();
        State casaFin = myFSM.CreateState("casafin", () => { FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje); mundo.ReemplazarEnfermero(banhoTarget, personaje.nombre); Destroy(gameObject); });
        State enCasa = myFSM.CreateEntryState("casa");
        State yendoPuesto = myFSM.CreateState("irPuestoTrabajo", () => { personaje.GoTo(puestoTrabajo); jornadaFlag = false; });
        State yendoCasa = myFSM.CreateState("irCasa", () => { PutEmoji(emoCasa); personaje.GoTo(mundo.casa); });
        State esperando = myFSM.CreateState("esperarPaciente", () => { PutEmoji(emoEsperando); DespacharPaciente(); });// idle
        State analisisSangre = myFSM.CreateState("analisisSangre", () => { Atender(emoSangre); });//animacion y emoticonillo
        State analisisOrina = myFSM.CreateState("analisisOrina", () => Atender(emoOrina));//animacion y emoticonillo
        State analisisPCR = myFSM.CreateState("analisisPCR", () => Atender(emoPCR)); //animacion y emoticonillo
        State suero = myFSM.CreateState("meterSuero", () => Atender(emoSuero));//animacion y emoticonillo;
        State laboratorio = myFSM.CreateState("llevarLab", () => { DespacharPaciente(); PutEmoji(emoLAB); personaje.GoTo(mundo.laboratorio); puestoTrabajo.libre = false; });
        State darBote = myFSM.CreateState("darBote", () => { Atender(emoBote); currentPaciente.targetBanho = banhoTarget; });//Darle al paciente  animacion y emoticono
        State examinarPaciente = myFSM.CreateState("examinar", () => ExaminarPaciente());//es un  micromomento
        State esperandoPacienteBanho = myFSM.CreateState("esperarBanho", () => PacienteABaño());

     


        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => !asientoPaciente.libre); //Mirar
        Perception terminadaJornada = myFSM.CreatePerception<TimerPerception>(timeJornada);
        Perception hayAnalisisSangre = myFSM.CreatePerception<ValuePerception>(() => currentAnalisis.Equals(Analisis.SANGRE));
        Perception hayAnalisisPCR = myFSM.CreatePerception<ValuePerception>(() => currentAnalisis.Equals(Analisis.PCR));
        Perception hayAnalisisDarBote = myFSM.CreatePerception<ValuePerception>(() => currentAnalisis.Equals(Analisis.DARBOTE));
        Perception hayMeterSuero = myFSM.CreatePerception<ValuePerception>(() => currentAnalisis.Equals(Analisis.SUERO));
        Perception hayAnalisisOrina = myFSM.CreatePerception<ValuePerception>(() => currentAnalisis.Equals(Analisis.ORINA));
        Perception atendido = myFSM.CreatePerception<TimerPerception>(timeAnimacionAtender);
        Perception llegadoPuesto = myFSM.CreatePerception<ValuePerception>(() => personaje.haLlegado);
        Perception llevarLab = myFSM.CreatePerception<ValuePerception>(() => llevarLabFlag);
        Perception comienzaJornada = myFSM.CreatePerception<ValuePerception>(() => jornadaFlag);



        myFSM.CreateTransition("comienza jornada", enCasa, comienzaJornada, yendoPuesto);
        myFSM.CreateTransition("llegado a puesto", yendoPuesto, llegadoPuesto, esperando);
        myFSM.CreateTransition("hay Paciente", esperando, pacienteAtender, examinarPaciente);

        myFSM.CreateTransition("hay que hacer analisis sangre", examinarPaciente, hayAnalisisSangre, analisisSangre);
        myFSM.CreateTransition("hay que hacer analisis PCR", examinarPaciente, hayAnalisisPCR, analisisPCR);
        myFSM.CreateTransition("hay que hacer analisis orina", examinarPaciente, hayAnalisisOrina, analisisOrina);
        myFSM.CreateTransition("hay que meter Suero", examinarPaciente, hayMeterSuero, suero);
        myFSM.CreateTransition("hay que dar Bote", examinarPaciente, hayAnalisisDarBote, darBote);

        myFSM.CreateTransition("atendido sangre", analisisSangre, atendido, laboratorio);
        myFSM.CreateTransition("atendido PCR", analisisPCR, atendido, laboratorio);
        myFSM.CreateTransition("atendido orina", analisisOrina, atendido, laboratorio);
        myFSM.CreateTransition("atendido suero", suero, atendido, esperando);
        myFSM.CreateTransition("atendido darBote", darBote, atendido, esperandoPacienteBanho);
        myFSM.CreateTransition("ya tengo bote", esperandoPacienteBanho, pacienteAtender, examinarPaciente);

        myFSM.CreateTransition("enviado lab", laboratorio, llegadoPuesto, yendoPuesto);// nueva transicion

        myFSM.CreateTransition("a casa", esperando, terminadaJornada, yendoCasa);
        myFSM.CreateTransition("en casa", yendoCasa, llegadoPuesto, casaFin);// nueva transicion


    }
    private void Atender(Sprite emoji)
    {
        PutEmoji(emoji);
        currentPaciente = null;// mirar
        // animacion 
    }
    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }
    private void PacienteABaño()
    {
        PutEmoji(emoEsperandoOrina);
    }
    private void ExaminarPaciente()
    {
        currentPaciente = asientoPaciente.actual.gameObject.GetComponent<Paciente>();
        Enfermedad current = currentPaciente.enfermedad;
        if (currentPaciente.tieneBote)
        {
            currentAnalisis = Analisis.ORINA;
        }
        else
        {
            switch (current.tipoEnfermedad)
            {
                case TipoEnfermedad.Cistitis:
                    currentPaciente.tieneBote = true;
                    currentAnalisis = Analisis.DARBOTE;
                    break;
                case TipoEnfermedad.Colico:
                    currentAnalisis = Analisis.SUERO;
                    break;
                case TipoEnfermedad.ITS:
                    currentAnalisis = Analisis.SANGRE;
                    break;
                case TipoEnfermedad.Gripe:
                    currentAnalisis = Analisis.PCR;
                    break;
                case TipoEnfermedad.Covid:
                    currentAnalisis = Analisis.PCR;
                    break;
                case TipoEnfermedad.Alergia:
                    currentAnalisis = Analisis.SANGRE;
                    break;
                default:
                    currentAnalisis = Analisis.SANGRE;
                    break;
            }
        }
    }
    private void DespacharPaciente()
    {

        currentAnalisis = Analisis.NULO;
        if (currentPaciente != null)
        {  //  currentPaciente.Despachar();
           //meterle en la lista segun el paso en el que esté o al baño incluso! mirar
            currentPaciente = null;

        }



    }
    private void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals("casa"))
        {
            List<Sala> enfermeria = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.ENFERMERIA));
            //Si el puesto de trabajo está libre
            for (int i = 0; i < mundo.numEnfermeros; i++)// MAS QUE COUNT, NUMERO DE ENFERMERIAS
            {
                if (enfermeria[i].posicionProfesional.libre)
                {

                    puestoTrabajo = enfermeria[i].posicionProfesional;
                    asientoPaciente = enfermeria[i].posicionPaciente;
                    jornadaFlag = true;
                    enfermeria[i].posicionProfesional.libre = false;
                    return;
                }
            }
        }
     

    }
}
