using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentLightControl : MonoBehaviour
{
    public Slider lightIntensity;
    public Slider lightTemperature;
    public Text lightIntensityText;
    public Text lighttemperatureText;
    public Light environmentLight;
    // Start is called before the first frame update
    void Start()
    {
        lightIntensity.onValueChanged.AddListener(IntensityChanged);
        lightTemperature.onValueChanged.AddListener(temperatureChanged);
    }

    private void IntensityChanged(float value){
        environmentLight.intensity = value;
    }

    private void temperatureChanged(float value){
        environmentLight.colorTemperature = value;
    }

    //Update is called once per frame
    void Update()
    {
        lightIntensityText.text = "Environment light intensity: " + lightIntensity.value + "Lux";
        lighttemperatureText.text = "Environment light temperature: " + lightTemperature.value + "Kelvin";
    }
}
