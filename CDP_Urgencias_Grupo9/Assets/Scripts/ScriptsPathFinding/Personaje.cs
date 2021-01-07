using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Personaje: MonoBehaviour
{
    //Referencias
    Animator animator;
    public NavMeshAgent myAgent;
    Transform target;
    TargetUrgencias targetU;

    //Variables
    public string nombre;// para el dropdown de la camara
    public bool muerto = false;
    bool andando = false;
    public bool haLlegado = false;
    float epsilon = 0.1f;
    Vector3 posicionAnterior;//Auxiliar camillas y sillas
   
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
            targetU.ocupable = true;
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

        if(!animator.GetBool("Bote"))
            animator.SetBool("Walking", true);

        this.target = target;
        StartCoroutine(tiempoEspera());
    }

    void Update()
    {
        if (!muerto)
            
        {
            if (myAgent.enabled == true)
            {
                //Cuando llega al destino, la animacion se para
                if ((AproximadamenteCero(myAgent.remainingDistance)) && (andando))
                {
                    DetenerPersonaje();
                    transform.rotation = target.rotation;
                    targetU.ocupable = false;
                }
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
        myAgent.enabled = false;
        posicionAnterior = transform.position;
        transform.position = targetU.transform.position;
        animator.SetBool("Sitting", true);
 
    }
    public void levantarse()
    {
       
        if (posicionAnterior !=Vector3.zero)
        {
            transform.position = posicionAnterior;
            posicionAnterior = Vector3.zero;
        }
        myAgent.enabled = true;
   
       
        animator.SetBool("Sitting", false);
    }
    public void tumbarse()
    {
        myAgent.enabled = false;
        posicionAnterior = transform.position;
        transform.position = targetU.transform.position;
        animator.SetBool("Tumbarse", true);
    }
    public void limpiando(bool limpiando)
    {
        animator.SetBool("Limpiando", limpiando);
    }
    public void Hablando( bool value)
    {
        animator.SetBool("Hablando", value);
    }
    public void LlevandoBote(bool value)
    {
        animator.SetBool("Bote", value);
    }
    public void levantarseOperacion()
    {
        if (posicionAnterior != Vector3.zero && this.gameObject!=null)
        {
            transform.position = posicionAnterior;
            posicionAnterior = Vector3.zero;
        }
        myAgent.enabled = true;
        animator.SetBool("Tumbarse", false);
    }
}
