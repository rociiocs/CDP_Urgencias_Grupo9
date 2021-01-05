using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje: MonoBehaviour
{
    public string nombre;// para el dropdown de la camara
    public NavMeshAgent myAgent;
    public bool muerto = false;
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
            targetU.ocupado = true;
        }
        targetU = targetUrgencias;
        if (targetU == null)
        {
            int a = 4;
        }
        targetU.actual = this;
        target = targetUrgencias.transform;
        targetU.libre = false;
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
        if (!muerto)
        {
            //Cuando llega al destino, la animacion se para
            if ((AproximadamenteCero(myAgent.remainingDistance)) && (andando))
            {
                DetenerPersonaje();
                transform.rotation = target.rotation;
                targetU.ocupado = false;
            }
        }
    }
    public void DetenerPersonaje()
    {
        andando = false;
        animator.SetBool("Walking", false);
        myAgent.Stop();
        haLlegado = true;

    }
    private bool AproximadamenteCero(float value){

        return value <= epsilon;

    }
    IEnumerator tiempoEspera()// es necesario porque por alguna razon entra en la condicion del update justo despues de comenzar a caminar
    {
        andando = false;
        yield return new WaitForSeconds(0.5f);
        andando = true;
    }
    public void Morirse()
    {
        animator.Play("Morirse");
    }

    public void sentarse()
    {
        animator.SetBool("Sitting", true);
    }
    public void levantarse()
    {
        animator.SetBool("Sitting", false);
    }
}
