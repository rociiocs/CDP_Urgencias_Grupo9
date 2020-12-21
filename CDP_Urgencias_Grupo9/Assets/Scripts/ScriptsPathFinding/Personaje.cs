using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje : MonoBehaviour
{
    NavMeshAgent myAgent;
    Animator animator;
    bool andando = false;
    float epsilon = 0.05f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        myAgent = GetComponent<NavMeshAgent>();
       
    }

    public void GoTo(Vector3 position)
    {
        myAgent.SetDestination(position);
        animator.SetBool("Walking", true);
        StartCoroutine(tiempoEspera());
    }

    void Update()
    {
        //Cuando llega al destino, la animacion se para
        if ((AproximadamenteCero(myAgent.remainingDistance))&& (andando)){

            andando = false;
            animator.SetBool("Walking", false);
            myAgent.Stop();
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
