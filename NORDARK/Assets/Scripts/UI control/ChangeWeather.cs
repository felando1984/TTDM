using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeWeather : MonoBehaviour
{
    public Dropdown methodSelection;
    public GameObject Sunny;
    public GameObject PartlyCloudy;
    public GameObject Rainy;
    public GameObject rainyParticle;
    public GameObject Snowy;
    public GameObject snowySun;
    public GameObject snowPlane;

    string method;
    string oldmethod;
    // Start is called before the first frame update
    void Start()
    {
        method = methodSelection.options[methodSelection.value].text;
        Sunny.SetActive(true);
        PartlyCloudy.SetActive(false);
        Rainy.SetActive(false);
        Snowy.SetActive(false);
        snowPlane.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(method != methodSelection.options[methodSelection.value].text){
            method = methodSelection.options[methodSelection.value].text;
            enableMethod(method);
        }
    }

    void enableMethod(string method){
        switch (method){
            case "Sunny":
                Sunny.SetActive(true);
                PartlyCloudy.SetActive(false);
                Rainy.SetActive(false);
                rainyParticle.SetActive(false);
                Snowy.SetActive(false); 
                snowPlane.SetActive(false);
                break;
            case "Cloudy":
                Sunny.SetActive(false);
                PartlyCloudy.SetActive(false);
                Rainy.SetActive(false);
                rainyParticle.SetActive(false);
                Snowy.SetActive(false);
                snowPlane.SetActive(false);
                break;
            case "Partly cloudy":
                Sunny.SetActive(false);
                PartlyCloudy.SetActive(true);
                Rainy.SetActive(false);
                rainyParticle.SetActive(false);
                Snowy.SetActive(false);
                snowPlane.SetActive(false);
                break;
            case "Rainy":
                Sunny.SetActive(false);
                PartlyCloudy.SetActive(false);
                Rainy.SetActive(true);
                rainyParticle.SetActive(true);
                Snowy.SetActive(false);
                snowPlane.SetActive(false);
                break;
            case "Snowy":
                Sunny.SetActive(false);
                PartlyCloudy.SetActive(false);
                Rainy.SetActive(false);
                rainyParticle.SetActive(false);
                Snowy.SetActive(true);
                snowySun.SetActive(true);
                snowPlane.SetActive(true);
                break;
            default:
                break;
        }

    }
}
