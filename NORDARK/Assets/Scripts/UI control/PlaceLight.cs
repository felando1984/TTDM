using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceLight : MonoBehaviour
{
    public GameObject StreetLight;
    public Dropdown ShowLight;
    public Camera cam;
    string SelectedLight;
    GameObject duplicate;
    bool setPosition = true;
    
    // Start is called before the first frame update
    void Start()
    {
        duplicate  = new GameObject();
        List<string> children = new List<string>{"select light"};
        Transform[] allChildren = StreetLight.transform.GetComponentsInChildren<Transform>();
        for (int i =1; i<allChildren.Length;i++){
            children.Add(allChildren[i].name);
        }
        ShowLight.ClearOptions();
        ShowLight.AddOptions(children);
        int loghtNum = ShowLight.value;
        SelectedLight = ShowLight.options[loghtNum].text;
        setPosition=false;
    }

    // Update is called once per frame
    void Update()
    {
        if(SelectedLight != ShowLight.options[ShowLight.value].text && ShowLight.value!=0){
            SelectedLight = ShowLight.options[ShowLight.value].text;
            GameObject LightSelected =  GameObject.Find(SelectedLight);
            Debug.Log(LightSelected);
            duplicate = Instantiate(LightSelected);
            setPosition =true;
        }

         if (Input.GetMouseButtonDown(1) && setPosition){
            duplicate.transform.position = getMousePosition();  
         }
        
    }

    Vector3 getMousePosition(){
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycast;
        if(Physics.Raycast(ray, out raycast,Mathf.Infinity, 1<<LayerMask.NameToLayer("Ground"))){
            return raycast.point;
        }
        return new Vector3(0,0,0);
    }  
    

}
