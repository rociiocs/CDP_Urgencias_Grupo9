using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeleccionadorCamara : MonoBehaviour
{
    // Start is called before the first frame update
    public Dropdown seleccionador;
    bool general = true;
    List<Personaje> personajes;
    public Vector3 offset = new Vector3(0,2f, -3.62f);
    Vector3 offsetPrimera = new Vector3(0, 2.01f, 0.414f);
    Vector3 offsetCentro = new Vector3(0, 1, 0);
    Vector3 posGeneral;
    Quaternion rotationGeneral;
    Camera main;
    Personaje current;
    void Start()
    {
        main = Camera.main;
        posGeneral = main.transform.position;
        rotationGeneral =main.transform.rotation;
        personajes = new List<Personaje>();
        personajes.AddRange(FindObjectsOfType<Personaje>());
        UpdateDropdown();
    }


    void UpdateDropdown()
    {
        seleccionador.options.Clear();
        List<string> opcionesDropdown = new List<string>();
        opcionesDropdown.Add("General");
        foreach (Personaje p in personajes)
        {
            opcionesDropdown.Add(p.nombre);
        }
        seleccionador.AddOptions(opcionesDropdown);
        seleccionador.onValueChanged.AddListener((value) => ChangeCamera(value));
    }
    void ChangeCamera(int value)
    {
        if (value == 0)
        {
            current = null;
            main.transform.parent = null;
            main.transform.position = posGeneral;
            main.transform.rotation = rotationGeneral;
            general = true;
        }
        else
        {
            general = false;
            Transform padre = personajes[value-1].transform;
            main.transform.parent = padre;
            current = personajes[value - 1];
            //primera persona
            //

            //main.transform.position = padre.TransformPoint(offsetPrimera);
            //main.transform.LookAt(padre.TransformPoint(new Vector3(0, 2.01f, 3)));
            //main.transform.LookAt(padre.forward);

            //
            //primerapersona

            //tercerapersona
            //

            main.transform.position = padre.TransformPoint(offset);
            main.transform.LookAt(padre.TransformPoint(offsetCentro));

            //
            //tercerapersona

        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //tercera persona
        //

       /* if (!general)
            main.transform.RotateAround(main.transform.parent.TransformPoint(offsetCentro), new Vector3(0, 1, 0), 11.5f * Time.deltaTime);
            */

        //tercera persona

    }
    public void EliminarProfesional(Personaje personaje)
    {
        personajes.Remove(personaje);
        UpdateDropdown();
        if (current.Equals(personaje))
        {
            ChangeCamera(0);
        }
    }
}
