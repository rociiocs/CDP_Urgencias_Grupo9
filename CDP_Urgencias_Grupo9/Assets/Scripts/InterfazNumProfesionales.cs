using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfazNumProfesionales : MonoBehaviour
{
  

    public Dropdown numEnfermeros,numLimpiadores,numMedicos,numCeladores,numCirujano, porcentaje;
    private int minEnfermeros = 2, minLimpiadores = 1, minCeladores = 2, minMedicos = 1, minCirujano = 1;
    public Button empezar;
    public GameObject seleccionador;
    Mundo mundo;
    void Start()
    {
        mundo = FindObjectOfType<Mundo>() ;
        empezar.onClick.AddListener(() => ComienzaSimulacion());
      
      
    }
     void ComienzaSimulacion()
    {
        seleccionador.SetActive(true);
        seleccionador.GetComponentInParent<SeleccionadorCamara>().general = true;
        mundo.numCeladores = numCeladores.value + minCeladores;
        mundo.numCirujanos = numCirujano.value + minCirujano;
        mundo.numEnfermeros = numEnfermeros.value + minEnfermeros;
        mundo.numLimpiadores = numLimpiadores.value + minLimpiadores;
        mundo.numMedicos = numMedicos.value + minMedicos;
        mundo.porcentajeUrgentes = (porcentaje.value + 1) * 10;
        mundo.CrearProfesionales();
        gameObject.SetActive(false);
        

    }
  
}
