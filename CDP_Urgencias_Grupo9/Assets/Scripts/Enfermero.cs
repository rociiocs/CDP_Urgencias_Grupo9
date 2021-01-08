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

public class Enfermero : MonoBehaviour
{
  
    //Referencias
    public TargetUrgencias puestoTrabajo;
    public TargetUrgencias asientoPaciente;
    public TargetUrgencias banhoTarget;
    public Image emoticono;
    public Sprite emoEsperando, emoCasa, emoSangre, emoBote, emoOrina, emoPCR, emoLAB, emoSuero, emoEsperandoOrina;
    public Paciente currentPaciente;
    private Mundo mundo;
    Personaje personaje;
    Sala sala;

    //Variables
    StateMachineEngine myFSM;
    float timeJornada = 60, timeAnimacionAtender = 5;
    public bool llevarLabFlag = false, jornadaFlag = false;
    Analisis currentAnalisis;

    void Start()
    {
        personaje = GetComponent<Personaje>();
        mundo = FindObjectOfType<Mundo>();
        CreacionEstados();
    }
    private void CreacionEstados()
    {

        myFSM = new StateMachineEngine();
        State casaFin = myFSM.CreateState("casafin", CasaFinAction);
        State enCasa = myFSM.CreateEntryState("casa");
        State yendoPuesto = myFSM.CreateState("irPuestoTrabajo", IrPuestoAction);
        State yendoCasa = myFSM.CreateState("irCasa", IrCasaAction);
        State esperando = myFSM.CreateState("esperarPaciente",EsperarPacienteAction);// idle
        State analisisSangre = myFSM.CreateState("analisisSangre", SangreAction);//animacion y emoticonillo
        State analisisOrina = myFSM.CreateState("analisisOrina", OrinaAction);//animacion y emoticonillo
        State analisisPCR = myFSM.CreateState("analisisPCR", PCRAction ); //animacion y emoticonillo
        State suero = myFSM.CreateState("meterSuero", SueroAction);//animacion y emoticonillo;
        State laboratorio = myFSM.CreateState("llevarLab", LlevarLabAction);
        State darBote = myFSM.CreateState("darBote", DarBoteAction);//Darle al paciente  animacion y emoticono
        State examinarPaciente = myFSM.CreateState("examinar", ExaminarPacienteAction);//es un  micromomento
        State esperandoPacienteBanho = myFSM.CreateState("esperarBanho",  PacienteABañoAction);

     

        Perception pacienteAtender = myFSM.CreatePerception<ValuePerception>(() => !asientoPaciente.ocupable); //Mirar
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
        myFSM.CreateTransition("enviado lab", laboratorio, llegadoPuesto, yendoPuesto);
        myFSM.CreateTransition("a casa", esperando, terminadaJornada, yendoCasa);
        myFSM.CreateTransition("en casa", yendoCasa, llegadoPuesto, casaFin);


    }
    private void Atender(Sprite emoji)
    {
        PutEmoji(emoji);

    }
    private void PutEmoji(Sprite emoji)
    {
        emoticono.sprite = emoji;
    }
    private void PacienteABañoAction()
    {
        personaje.Hablando(false);
        PutEmoji(emoEsperandoOrina);
        sala.libre = false;
        currentPaciente.targetBanho = banhoTarget;
        currentPaciente.tengoQueHacerAnalisisOrina.Fire();
    }
    private void DarBoteAction()
    {
        Atender(emoBote); 
       
    }


    private void CasaFinAction()
    {
        FindObjectOfType<SeleccionadorCamara>().EliminarProfesional(personaje);
        mundo.ReemplazarEnfermero( personaje.nombre);
        Destroy(gameObject);
    }
    private void IrPuestoAction()
    {
        personaje.GoTo(puestoTrabajo);
        jornadaFlag = false;

    }
    private void IrCasaAction()
    {
        PutEmoji(emoCasa); 
        puestoTrabajo.libre = true;
        sala.libre = false;
        personaje.GoTo(mundo.casa);
       
    }
    private void EsperarPacienteAction()
    {
        personaje.Hablando(false);
        personaje.LlevandoBote(false);
        PutEmoji(emoEsperando);
        sala.libre = true;
        DespacharPaciente();

    }
    private void SangreAction()
    {
        Atender(emoSangre);
    }
    private void PCRAction()
    {
        Atender(emoPCR);
    }
    private void OrinaAction()
    {

        Atender(emoOrina);
    }
    private void SueroAction()
    {
        Atender(emoSuero);
    }
    private void LlevarLabAction()
    {
        DespacharPaciente();
        personaje.LlevandoBote(true);
        PutEmoji(emoLAB);
        personaje.GoTo(mundo.laboratorio);
        puestoTrabajo.libre = false;
    }

    private void ExaminarPacienteAction()
    {
        personaje.Hablando(true);
        currentPaciente = asientoPaciente.actual.gameObject.GetComponent<Paciente>();
        currentPaciente.reiniciarTimerMorir();
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

        personaje.Hablando(false);
      
        currentAnalisis = Analisis.NULO;
       
        if (currentPaciente != null)
        {  
            currentPaciente.siguientePaso();
            switch (currentPaciente.pasoActual)
            {
                case Paso.Cirujano: 
                    mundo.AddPacienteCirugia(currentPaciente);
                    currentPaciente.todaviaTengoQueSerTratado.Fire();
                   
                    break;
                case Paso.Enfermeria:
                    mundo.AddPacienteEnfermeria(currentPaciente);
                    currentPaciente.todaviaTengoQueSerTratado.Fire();
                   
                    break;
                case Paso.Medico:
                    mundo.AddPacienteMedico(currentPaciente);
                    currentPaciente.todaviaTengoQueSerTratado.Fire();
                    
                    break;
                case Paso.Casa:
                    currentPaciente.soyLeve.Fire();
                    break;
                case Paso.UCI:
                    currentPaciente.soyGrave.Fire();
                    break;
                default:
          
                    break;
                 
            }
           
            currentPaciente = null;
            asientoPaciente.ocupable = true;

        }



    }
    private void Update()
    {
        myFSM.Update();
        if (myFSM.GetCurrentState().Name.Equals("casa"))
        {
            List<Sala> enfermeria = mundo.salas.FindAll((s) => s.tipo.Equals(TipoSala.ENFERMERIA));
            //Si el puesto de trabajo está libre
            for (int i = 0; i < mundo.numEnfermeros; i++)
            {
                if (enfermeria[i].posicionProfesional.libre)
                {

                    sala = enfermeria[i];
                    
                    puestoTrabajo = enfermeria[i].posicionProfesional;
                    asientoPaciente = enfermeria[i].posicionPaciente;
                    banhoTarget = mundo.banhos[sala.idBanho];
                    if(asientoPaciente.ocupable==null)
                        asientoPaciente.ocupable = true;
                    jornadaFlag = true;
                    enfermeria[i].posicionProfesional.libre = false;
                    return;
                }
            }
        }
      
    }
}
