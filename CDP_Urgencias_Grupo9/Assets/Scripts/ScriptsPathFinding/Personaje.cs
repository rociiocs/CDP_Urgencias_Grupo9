using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje : MonoBehaviour
{
    NavMeshAgent myAgent;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        myAgent = GetComponent<NavMeshAgent>();
    }

    public void GoTo(Vector3 position)
    {
        myAgent.SetDestination(position);
        animator.SetBool("Walking", true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
