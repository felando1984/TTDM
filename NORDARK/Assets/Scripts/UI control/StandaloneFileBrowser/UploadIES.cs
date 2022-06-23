using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class UploadIES : MonoBehaviour
{
    public  Button uploadIES;
    public Dropdown ShowIES;

    private string IESFolderPath;
    private DirectoryInfo IESFolder;
    private List<string> children = new List<string>();

    void Start(){
        uploadIES.onClick.AddListener(TaskOnClick);

        IESFolderPath = "Assets/IES folder";
        IESFolder = new DirectoryInfo(IESFolderPath);

        foreach (var file in IESFolder.GetFiles("*.ies"))
        {
            children.Add(file.Name);
        } 
                
        ShowIES.ClearOptions();
        ShowIES.AddOptions(children);
        // int loghtNum = ShowLight.value;
        // SelectedLight = ShowLight.options[loghtNum].text;

    }

    private void TaskOnClick() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "ies", false);
        if (paths.Length > 0) {
            for (int i =0; i<paths.Length; i++){
                Debug.Log(paths[i]);
                FileUtil.MoveFileOrDirectory(paths[i], "Assets/IES folder/");
            }
            
            //StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
}
