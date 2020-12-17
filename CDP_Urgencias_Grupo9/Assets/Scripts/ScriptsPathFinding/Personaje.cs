using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje : MonoBehaviour
{
    NavMeshAgent myAgent;
    // Start is called before the first frame update
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        
    }

    public void GoTo(Vector3 position)
    {
        myAgent.SetDestination(position);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
