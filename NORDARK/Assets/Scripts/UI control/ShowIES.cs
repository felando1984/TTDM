using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShowIES : MonoBehaviour
{
    private string IESFolderPath;
    private DirectoryInfo IESFolder;
    // Start is called before the first frame update
    void Start()
    {
        // IESFolderPath = "Assets/IES folder";
        // IESFolder = new DirectoryInfo(IESFolderPath);

        // List<string> children = new List<string>();
        // Transform[] allChildren = StreetLight.transform.GetComponentsInChildren<Transform>();
        // for (int i =0; i<allChildren.Length;i++){
        //     children.Add(allChildren[i].name);
        // }
        // ShowLight.ClearOptions();
        // ShowLight.AddOptions(children);
        // int loghtNum = ShowLight.value;
        // SelectedLight = ShowLight.options[loghtNum].text;
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //     void LoadSavedGames()
    // {
    //     if (Directory.Exists(Application.persistentDataPath))
    //     {
    //         string worldsFolder = Application.persistentDataPath;
 
    //         DirectoryInfo d = new DirectoryInfo(worldsFolder);
    //         foreach (var file in d.GetFiles("*.sav"))
    //         {
    //             Debug.Log(file);
    //         }
    //     }
    //     else
    //     {
    //         File.Create(Application.persistentDataPath);
    //         return;
    //     }
    // }
}
