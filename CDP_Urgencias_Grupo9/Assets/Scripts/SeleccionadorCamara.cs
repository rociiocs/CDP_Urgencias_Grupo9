using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SeleccionadorCamara : MonoBehaviour
{

    //Referencias
    public Dropdown seleccionador;
    List<Personaje> personajes;
    Camera main;
    Personaje current;
    //Variables
    public Vector3 offset = new Vector3(0,2f, -3.62f);
    Vector3 offsetCentro = new Vector3(0, 1, 0);
    Vector3 posGeneral;
    Quaternion rotationGeneral;
    public bool general =false;
    float speed = 1f;
    float speedCamera = 0.02f;
    float yaw= 0.0f, pitch = 0.0f;
    void Start()
    {
        main = Camera.main;
        posGeneral = main.transform.position;
        rotationGeneral =main.transform.rotation;
        personajes = new List<Personaje>();
        personajes.AddRange(FindObjectsOfType<Personaje>());
        UpdateDropdown();
    }

    private void Update()
    {
        if (general)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            yaw += speed * Input.GetAxis("Mouse X");
            pitch += speed * Input.GetAxis("Mouse Y");
            main.transform.eulerAngles = new Vector3(-pitch, yaw, 0.0f);
            
            
            main.transform.position += x*speedCamera*main.transform.right;
            main.transform.position += y*speedCamera*main.transform.forward;
            if (main.transform.position.x < -28)
            {

                main.transform.position = new Vector3(-28, main.transform.position.y, main.transform.position.z);
            }
            else if (main.transform.position.x >6)
            {


                main.transform.position = new Vector3(6, main.transform.position.y, main.transform.position.z);
            }
            if (main.transform.position.z < -23)
            {
                
                    main.transform.position = new Vector3(main.transform.position.x, main.transform.position.y, -23);


            }
            else if (main.transform.position.z > -1)
            {
                main.transform.position = new Vector3(main.transform.position.x, main.transform.position.y,-1);
            }


            if (main.transform.position.y > 17)
            {
               
                
                    main.transform.position = new Vector3(main.transform.position.x, 17, main.transform.position.z);
                

            }
            else if(main.transform.position.y < 0)
            {
                main.transform.position = new Vector3(main.transform.position.x, 0, main.transform.position.z);
            }

        }
        
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
            main.transform.position = padre.TransformPoint(offset);
            main.transform.LookAt(padre.TransformPoint(offsetCentro));

        }
    }

    public void AnhadirProfesional( Personaje p)
    {
        personajes.Add(p);
        UpdateDropdown();
    }
    public void EliminarProfesional(Personaje personaje)
    {
        personajes.Remove(personaje);
        UpdateDropdown();
        if (current != null)
        {
            if (current.Equals(personaje))
            {
                ChangeCamera(0);
            }
        }
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void RecargarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
