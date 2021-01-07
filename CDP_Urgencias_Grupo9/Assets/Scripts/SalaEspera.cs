using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalaEspera : Sala
{
    public TargetUrgencias[] posicionMostradorProfesional;
    public TargetUrgencias[] posicionMostradorPaciente;
    public TargetUrgencias[] posicionSalaProfesional;
    public TargetUrgencias[] posicionSalaPaciente;
    public SalaEspera(TipoSala tipo, int ID) : base(tipo, ID){}



}
