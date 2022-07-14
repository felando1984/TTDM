using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Rotate : MonoBehaviour
{
    public float speed = 30;
    public string LabelText = "";
    private Material _material;
    private int count = 0;
    private int step = 1;

    private GameObject Label;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        foreach (Transform child in transform)
        {
            GameObject childGameObject = child.gameObject;
            //if (childGameObject.name == "Label")
            //    Label = childGameObject;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.localRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);
        if (count <= 0)
        {
            step = 1;
        }
        else if (count >= 40)
        {
            step = -1;
        }
        count += step;

        transform.Rotate(0, -25 * Time.deltaTime, 0, Space.World); //Space.Self
        _material.SetFloat("_OutLineWidth", 1f + 0.015f * count);

        //Label.GetComponentInParent<TMPro.TextMeshPro>().text = LabelText;
        //Label.transform.Rotate(0, 25 * Time.deltaTime, 0, Space.World);
    }
}
