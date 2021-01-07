using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoSala
{
    MEDICO,
    ENFERMERIA,
    CIRUGIA,
    ESPERA

}
public class Sala
{
    public TipoSala tipo;
    public int ID;
    public float porcentajeSuciedad;
    public bool libre;
    public bool sucio;
    public bool heLlamadoAlMundo;
    public TargetUrgencias posicionPaciente;
    public TargetUrgencias posicionProfesional;
    public TargetUrgencias posicionLimpiador;
    //private float limiteSuciedad = 100;
    //private float umbral = 65;
    // private float speedSuciedad= 0.01;

    public Sala(TipoSala tipo, int ID)
    {
        this.tipo = tipo;
        libre = false;
        porcentajeSuciedad = 0;
        this.ID = ID;
        sucio = false;
        heLlamadoAlMundo = false;
    }
    public bool OnUpdateMundo(float umbral, float limiteSuciedad, float speedSuciedad)
    {
        if(tipo != TipoSala.ESPERA)
        {
            if ((!libre) && (!posicionProfesional.libre))
            {
                if (porcentajeSuciedad < limiteSuciedad)
                {
                    porcentajeSuciedad += speedSuciedad;
                }
                if (porcentajeSuciedad >= umbral)
                {
                    sucio = true;
                    if (!heLlamadoAlMundo)
                    {
                        heLlamadoAlMundo = true;
                        return true;
                    }
                }

            }
        }
        else
        {
            if (porcentajeSuciedad < limiteSuciedad)
            {
                if(tipo!= TipoSala.CIRUGIA)
                {
                    porcentajeSuciedad += speedSuciedad;
                }
            }
            if (porcentajeSuciedad >= umbral)
            {
                sucio = true;
                if (!heLlamadoAlMundo)
                {
                    heLlamadoAlMundo = true;
                    return true;
                }
            }
        }
        return false;
    }
    public void CirugiaRealizada()// el cirujano llamara al mundo
    {
        porcentajeSuciedad = 100;
        sucio = true;

    }
    public bool OnLimpiadorInterrupted(float umbral)// el limpiador tendra que avisar al mundo para que vuelva a meter la sala en la lista
    { 
        heLlamadoAlMundo = false;
        if (porcentajeSuciedad < umbral)
        {
            sucio = false;
           
        }
        else
        {
            sucio = true;
            

        }
        return sucio;
    }
    public bool OnUpdateLimpiador(float limiteSuciedad,float speedLimpieza)
    {
        if (porcentajeSuciedad > limiteSuciedad)
        {
            porcentajeSuciedad -= speedLimpieza;
            if (porcentajeSuciedad <= limiteSuciedad)
            {
                sucio = false;
                return true;
            }
        }
        return false;
    }

}




