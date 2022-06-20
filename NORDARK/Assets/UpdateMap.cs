using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;

public class UpdateMap : MonoBehaviour
{
    private AbstractMap bg_Mapbox;
    private GameObject MainMenu;
    private Dropdown drn_MapStyle;
    private Dropdown drn_Location;
    private Toggle cbx_Terrian;
    private Toggle cbx_Buildings;
    private Toggle cbx_Roads;
    private Toggle cbx_LightPoints;

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
        drn_MapStyle = GameObject.Find("Drn_MapStyle").GetComponent<Dropdown>();
        drn_MapStyle.onValueChanged.AddListener(delegate {
            bg_Mapbox.ImageLayer.SetLayerSource((ImagerySourceType)drn_MapStyle.value);
        });
        drn_Location = GameObject.Find("Drn_Location").GetComponent<Dropdown>();
        drn_Location.onValueChanged.AddListener(delegate {
            if(drn_Location.value == 0)
                bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(62.4676991855481, 6.30334069538369));
            else
                bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(59.8539107, 17.6223242));
            bg_Mapbox.UpdateMap();
        });
        cbx_Terrian = GameObject.Find("Cbx_Terrian").GetComponent<Toggle>();
        cbx_Terrian.onValueChanged.AddListener(delegate {
            if (cbx_Terrian.isOn)
                bg_Mapbox.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
            else
                bg_Mapbox.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
        });
        cbx_Buildings = GameObject.Find("Cbx_Buildings").GetComponent<Toggle>();
        cbx_Buildings.onValueChanged.AddListener(delegate {
            bg_Mapbox.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(cbx_Buildings.isOn);
        });
        cbx_Roads = GameObject.Find("Cbx_Roads").GetComponent<Toggle>();
        cbx_Roads.onValueChanged.AddListener(delegate {
            bg_Mapbox.VectorData.GetFeatureSubLayerAtIndex(1).SetActive(cbx_Roads.isOn);
        });
        cbx_LightPoints = GameObject.Find("Cbx_LightPoints").GetComponent<Toggle>();
        cbx_LightPoints.onValueChanged.AddListener(delegate {
            //Nodes.SetActive(cbxShowNodes.isOn);
        });
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
