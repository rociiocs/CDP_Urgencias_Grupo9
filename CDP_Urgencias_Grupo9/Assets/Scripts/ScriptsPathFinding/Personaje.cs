using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje: MonoBehaviour
{
    public string nombre;// para el dropdown de la camara
    NavMeshAgent myAgent;
    Animator animator;
    bool andando = false;
    public bool haLlegado = false;
    float epsilon = 0.1f;
    Transform target;
    TargetUrgencias targetU;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        myAgent = GetComponent<NavMeshAgent>();
        if (nombre.Equals(""))
        {
            nombre = "Unknown";
        }
    }

    public void GoTo(TargetUrgencias targetUrgencias)
    {
        if (targetU != null)
        {
            targetU.libre = true;
        }
        targetU = targetUrgencias;
        targetU.actual = this;
        target = targetUrgencias.transform;
        haLlegado = false;
        myAgent.Resume();
        myAgent.SetDestination(target.position);
        animator.SetBool("Walking", true);
        //andando = true;
        this.target = target;
        StartCoroutine(tiempoEspera());
    }

    void Update()
    {
        //Cuando llega al destino, la animacion se para
        if ((AproximadamenteCero(myAgent.remainingDistance))&& (andando)){

            andando = false;
            animator.SetBool("Walking", false);
            myAgent.Stop();
            transform.rotation = target.rotation;
            haLlegado = true;
            targetU.libre = false;
        }
    }
    
    private bool AproximadamenteCero(float value){

        return value <= epsilon;

    }
    IEnumerator tiempoEspera()// es necesario porque por alguna razon entra en la condicion del update justo despues de comenzar a caminar
    {
        yield return new WaitForSeconds(0.5f);
        andando = true;
    }
}
