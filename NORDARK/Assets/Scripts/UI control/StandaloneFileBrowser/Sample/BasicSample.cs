using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using SFB;
using ConsoleApplication1;

public class BasicSample : MonoBehaviour {
    //private string _path;
    public  Button uploadIES;
    public Dropdown ShowIES;
    public string IESfilepath;

    private string SelectedIES;
    private string IESFolderPath;
    private DirectoryInfo IESFolder;
    private List<string> children = new List<string>();
    private Dictionary<Dictionary<double, double>, double> IESfiles;
    //private GameObject SelectedLight;

    void Start(){
        uploadIES.onClick.AddListener(TaskOnClick);

        IESFolderPath = "Assets/IES folder";
        IESFolder = new DirectoryInfo(IESFolderPath);

        foreach (var file in IESFolder.GetFiles("*.ies"))
        {
            children.Add(file.Name);
            IESfilepath = "Assets/IES folder/"+file.Name;
            IESParser parser = new IESParser();
            IESfiles = parser.ParseIES(IESfilepath);
            
            Debug.Log (parser.IESversion);
            
        } 
        
        ShowIES.ClearOptions();
        ShowIES.AddOptions(children);

        int loghtNum = ShowIES.value;
        SelectedIES = ShowIES.options[loghtNum].text;
        //setPosition=false;

        

    }

    private void TaskOnClick() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "ies", false);
        if (paths.Length > 0) {
            for (int i =0; i<paths.Length; i++){
                Debug.Log(paths[i]);
                FileUtil.MoveFileOrDirectory(paths[i], "Assets/IES folder/"+Path.GetFileName(paths[i]));
                AssetDatabase.Refresh();

                children.Add(Path.GetFileName(paths[i]));
                ShowIES.ClearOptions();
                ShowIES.AddOptions(children);
            }
        }
    }

 

    // public void WriteResult(string[] paths) {
    //     if (paths.Length == 0) {
    //         return;
    //     }

    //     _path = "";
    //     foreach (var p in paths) {
    //         _path += p + "\n";
    //     }
    // }

    // public void WriteResult(string path) {
    //     _path = path;
    // }
}
