using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class UpdateMap : MonoBehaviour
{
    private AbstractMap bg_Mapbox;
    private GameObject MainMenu;
    // Start is called before the first frame update
    void Start()
    {
        // Build 0063
        bg_Mapbox = GameObject.Find("BG_Mapbox").GetComponent<AbstractMap>();
        bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(62.4676991855481, 6.30334069538369));
        bg_Mapbox.SetZoom(17.45f);
        bg_Mapbox.UpdateMap();
        //62.4676991855481, 6.30334069538369
        //17.45

        initGUIMenu();
    }

    // Build 0055
    public void initGUIMenu()
    {
        MainMenu = GameObject.Find("Canvas_Mainmenu");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    //Debug.Log(keyCode.ToString());
                    switch (keyCode)
                    {
                        case KeyCode.F1:
                            // Help
                            break;
                        case KeyCode.F2:
                            // Hide / show menu
                            MainMenu.SetActive(!MainMenu.active);
                            break;
                        case KeyCode.F3:
                            // Screenshot
                            break;
                        case KeyCode.F4:
                            //
                            break;
                        case KeyCode.F5:
                            // Compute
                            break;
                        case KeyCode.F6:
                            // Save
                            break;
                    }
                }
            }
        }
    }
}
