using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerPruebaPathFinding : MonoBehaviour
{
    List<Personaje> personajesEscena;
    public Button pos1, pos2, pos3;
    public Transform target1, target2, target3;
    // Start is called before the first frame update
    void Start()
    {
        personajesEscena = new List<Personaje>();
        personajesEscena.AddRange(FindObjectsOfType<Personaje>());
        pos1.onClick.AddListener(() => SetDestination(target1.position));
        pos2.onClick.AddListener(() => SetDestination(target2.position));
        pos3.onClick.AddListener(() => SetDestination(target3.position));
    }
    public void SetDestination(Vector3 pos)
    {
        foreach(Personaje p in personajesEscena)
        {
            p.GoTo(pos);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
