using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
using SFB;

public class UpdateMap : MonoBehaviour
{
    private AbstractMap bg_Mapbox;
    private GameObject MainMenu;
    private Dropdown drn_MapStyle;
    private Dropdown drn_Location;
    private Dropdown drn_LightType;
    private Dropdown drn_IESType;
    private Toggle cbx_Terrian;
    private Toggle cbx_Buildings;
    private Toggle cbx_Roads;
    private Toggle cbx_LightPoints;
    private Button btn_Load;
    private Button btn_Save;
    private GameObject mainCamera;
    private double t1;
    private double t2;
    private bool IsMoving = false;
    private GameObject SelectedBuilding;
    private GameObject SelectedFlagCube = null;
    private GameObject RefPlane;
    private Button tab_Overview;
    private Button tab_LightObject;
    private Button tab_Camera;
    private GameObject[] TabMenuList;
    private Slider slr_RotationY;
    private Text txt_RotationYValue;
    private Text txt_LightObjectName;
    private Dropdown drn_LightIES;
    private Button btn_uploadIES;
    private Button btn_Move;
    private Button btn_Delete;
    private Dropdown drn_Camera;
    private Button btn_ResetCamera;
    private Button btn_UpdateCamera;

    private string IESFolderPath;
    private DirectoryInfo IESFolder;
    private List<string> IESList = new List<string>();

    private string LightPrefabFolderPath;
    private DirectoryInfo LightPrefabFolder;
    private List<string> LightPrefabList = new List<string>();
    private string strStreetLightLib = "StreetLightLib";

    private bool TestMode = true;
    //private GameObjectModifier buildingsModifier;
    private ReplaceFeatureCollectionModifier lightModifier;
    private GameObject LightsCollection;
    private GameObject CameraCollection;
    private Dictionary<string, int> dictLightIndex;
    private Dictionary<string, object> dictLightIESCookie;
    private Dictionary<string, GameObject> dictCamera;
    List<Node> lightNodes;
    string path = @"D:\Documents\GitHub\Ashkan\HDRP\Assets\";

    // Start is called before the first frame update
    void Start()
    {
        lightNodes = new List<Node>();
        LightsCollection = GameObject.Find("LightsCollection");
        CameraCollection = GameObject.Find("CameraCollection");
        mainCamera = GameObject.Find("Main Camera");
        RefPlane = GameObject.Find("BG_White_H");
        SelectedFlagCube = GameObject.Find("SelectedCube");
        TabMenuList = new GameObject[3];
        TabMenuList[0] = GameObject.Find("LeftMenu");
        TabMenuList[1] = GameObject.Find("TabLightObject");
        TabMenuList[2] = GameObject.Find("TabCamera");

        //buildingsModifier = ScriptableObject.CreateInstance<BuildingsModifier>();
        //lightModifier = ScriptableObject.CreateInstance<ReplaceFeatureCollectionModifier>();
        // Build 0063
        bg_Mapbox = GameObject.Find("BG_Mapbox").GetComponent<AbstractMap>();
        //bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(62.4669465500501, 6.29516909238363));
        //bg_Mapbox.SetZoom(17f);
        bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(62.4676991855481, 6.30334069538369));
        bg_Mapbox.SetZoom(17f);
        //bg_Mapbox.SetZoom(17.45f);
        bg_Mapbox.UpdateMap();
        //62.4676991855481, 6.30334069538369
        //17.45
        LightsCollection.transform.localScale = bg_Mapbox.transform.localScale;

        initGUIMenu();
        ClearSelectedBuilding();

        // Load Geojson
        FeatureCollection fCollection = Can_Deserialize();
        string[] layer_coords = bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).coordinates;
        layer_coords = new string[fCollection.Features.Count];
        dictLightIndex= new Dictionary<string, int>();
        dictCamera = new Dictionary<string, GameObject>();
        for (int i = 0; i < drn_Camera.options.Count; i++)
        {
            GameObject newCam = new GameObject();
            newCam.transform.position = mainCamera.transform.position;
            newCam.transform.eulerAngles = mainCamera.transform.eulerAngles;
            newCam.transform.parent = CameraCollection.transform;
            string strCam = drn_Camera.options[i].text;
            newCam.name = strCam;
            dictCamera.Add(strCam, newCam);
        }

        // Create nodes if not exist
        for (int i = 0; i < fCollection.Features.Count; i++)//i < 2; i++)//
        {
            GeoJSON.Net.Geometry.Point mPoint = fCollection.Features[i].Geometry as GeoJSON.Net.Geometry.Point;
            var coords = mPoint.Coordinates;
            {
                var index = 0;
                Vector2 latlong;
                Vector3 pos;
                Node lightNode = new Node();// LightNode
                latlong = new Vector2((float)(coords.Latitude), (float)(coords.Longitude));
                lightNode.GeoVec = latlong;
                layer_coords[i] = coords.Latitude.ToString() + ", " + coords.Longitude.ToString();
                pos = latlong.AsUnityPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
                GameObject lightObject = Instantiate(Resources.Load(strStreetLightLib + "/" + LightPrefabList[0]) as GameObject);
                lightObject.transform.name = "LightInfraS_" + i;
                lightObject.transform.parent = LightsCollection.transform;
                pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coords.Latitude, coords.Longitude));
                //pos.y = (float)coords.Altitude * bg_Mapbox.WorldRelativeScale;
                lightObject.transform.position = pos;
                lightObject.transform.localScale = new Vector3(1, 1, 1);
                lightNode.obj = lightObject;
                lightNode.IESfileName = IESList[0];
                lightNode.LightPrefabName = LightPrefabList[0];//"LightType1";
                lightNodes.Add(lightNode);
                dictLightIndex.Add(lightObject.transform.name, i);
            }
            //fCollection.Features[i].Geometry as GeoJSON.Net.Geometry.LineString;
            //var coords = fCollection.Features[i].Geometry.Coordinates[0].Coordinates;
        }
        bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(false);
        bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).coordinates = layer_coords;
        bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(true);
        VectorSubLayerProperties LightLayer = bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0);
        List<string> list_coords = new List<string>();
        for (int i = 0; i < fCollection.Features.Count; i++)
        {
            list_coords.Add(layer_coords[i]);
        }
        //bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).MeshModifiers
        //lightModifier.features[0].PrefabLocations = list_coords;
        //bg_Mapbox.VectorData.GetFeatureSubLayerAtIndex(2).BehaviorModifiers.AddGameObjectModifier(lightModifier);

        //bg_Mapbox.VectorData.GetFeatureSubLayerAtIndex(0).BehaviorModifiers.AddGameObjectModifier(buildingsModifier);
    }

    // Build 0055
    public void initGUIMenu()
    {
        btn_uploadIES = GameObject.Find("UploadIESFile").GetComponent<Button>();

        btn_uploadIES.onClick.AddListener(TaskOnClick);

        IESFolderPath = "Assets/IES folder";
        IESFolder = new DirectoryInfo(IESFolderPath);
        dictLightIESCookie = new Dictionary<string, object>();
        foreach (var file in IESFolder.GetFiles("*.ies"))
        {
            IESList.Add(file.Name);
            dictLightIESCookie[file.Name] = GetIESCookie(file.Name);
        }

        LightPrefabFolderPath = "Assets/Resources/" + strStreetLightLib;
        LightPrefabFolder = new DirectoryInfo(LightPrefabFolderPath);

        foreach (var file in LightPrefabFolder.GetFiles("*.prefab"))
        {
            LightPrefabList.Add(GetFileName(file.Name));
        }

        GameObject light = Instantiate(Resources.Load(strStreetLightLib + "/"+ LightPrefabList[0]) as GameObject);//Instantiate(Resources.Load("LightType1") as GameObject);// bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).spawnPrefabOptions.prefab.GetComponentsInChildren<Transform>()[0].gameObject;
        light.transform.name = "LightDemo";
        light = light.transform.Find("Spot Light").gameObject;
        GameObject.Find("LightDemo").SetActive(false);

        MainMenu = GameObject.Find("Canvas_Mainmenu");
        drn_MapStyle = GameObject.Find("Drn_MapStyle").GetComponent<Dropdown>();
        drn_MapStyle.onValueChanged.AddListener(delegate {
            bg_Mapbox.ImageLayer.SetLayerSource((ImagerySourceType)drn_MapStyle.value);
        });
        drn_Location = GameObject.Find("Drn_Location").GetComponent<Dropdown>();
        drn_Location.onValueChanged.AddListener(delegate {
            if (drn_Location.value == 0)
                bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(62.4676991855481, 6.30334069538369));
            else
                bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(59.809179, 17.7043457));
            bg_Mapbox.UpdateMap();
        });
        drn_LightType = GameObject.Find("Drn_LightType").GetComponent<Dropdown>();
        drn_LightType.ClearOptions();
        drn_LightType.AddOptions(LightPrefabList);
        drn_LightType.onValueChanged.AddListener(delegate {
            string newLightType = drn_LightType.options[drn_LightType.value].text;
            int light_index = dictLightIndex[SelectedBuilding.name];
            GameObject old_lightObject = lightNodes[light_index].obj;
            if ((SelectedBuilding != null) && (newLightType != lightNodes[light_index].LightPrefabName))
            {
                GameObject lightObject = Instantiate(Resources.Load(strStreetLightLib + "/" + newLightType) as GameObject);
                lightObject.transform.name = old_lightObject.transform.name;
                lightObject.transform.parent = old_lightObject.transform.parent;
                lightObject.transform.position = old_lightObject.transform.position;
                lightObject.transform.eulerAngles = old_lightObject.transform.eulerAngles;
                GameObject light = lightObject.transform.Find("Spot Light").gameObject;
                light.GetComponent<HDAdditionalLightData>().SetCookie(dictLightIESCookie[lightNodes[light_index].IESfileName] as UnityEngine.Texture);


                //UnityEngine.Texture ies2DCookie = GetIESCookie(newIES);
                //GameObject light = SelectedBuilding.transform.Find("Spot Light").gameObject;
                //light.GetComponent<HDAdditionalLightData>().SetCookie(ies2DCookie);
                lightNodes[light_index].obj = lightObject;
                lightNodes[light_index].LightPrefabName = newLightType;
                SelectedBuilding = lightObject;
                Destroy(old_lightObject);
            }
            Debug.Log("E005: " + SelectedBuilding.name + " light prefab model changed to " + newLightType);
        });
        //drn_LightType.onValueChanged.AddListener(delegate {
        //    bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(false);
        //    string path = @"Assets/Prefabs/StreetLightsPack/";
        //    bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).spawnPrefabOptions.prefab = Resources.Load(drn_LightType.options[drn_LightType.value].text) as GameObject;
        //    bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(true);
        //});
        drn_IESType = GameObject.Find("IESList").GetComponent<Dropdown>();
        drn_IESType.ClearOptions();
        drn_IESType.AddOptions(IESList);
        drn_IESType.onValueChanged.AddListener(delegate {
            ////GameObject light = GameObject.Find("Spot Light");
            ////light.GetComponent<HDAdditionalLightData>().SetCookie(GetIESCookie(drn_IESType.options[drn_IESType.value].text));
            ////Debug.Log("change to" + drn_IESType.options[drn_IESType.value].text);
            //light = (Resources.Load("LightType1") as GameObject).transform.Find("Spot Light").gameObject;
            ////bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).spawnPrefabOptions.prefab.transform.Find("Spot Light").gameObject;
            //light.GetComponent<HDAdditionalLightData>().SetCookie(GetIESCookie(drn_IESType.options[drn_IESType.value].text));
            //Debug.Log("change to" + drn_IESType.options[drn_IESType.value].text);

            UnityEngine.Texture ies2DCookie = GetIESCookie(drn_IESType.options[drn_IESType.value].text);
            for (int i = 0; i < lightNodes.Count; i++)
            {
                GameObject light = lightNodes[i].obj.transform.Find("Spot Light").gameObject;
                light.GetComponent<HDAdditionalLightData>().SetCookie(ies2DCookie);
            }
            Debug.Log("E004: " + SelectedBuilding.name + " change to" + drn_IESType.options[drn_IESType.value].text);
        });
        cbx_Terrian = GameObject.Find("Cbx_Terrian").GetComponent<Toggle>();
        cbx_Terrian.onValueChanged.AddListener(delegate
        {
            if (cbx_Terrian.isOn)
            {
                bg_Mapbox.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
                for (int i = 0; i < lightNodes.Count; i++)
                {
                    Vector2 latlong = lightNodes[i].GeoVec;
                    Vector3 pos = latlong.AsUnityPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
                    pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(latlong.x, latlong.y));
                    //pos.y = (float)coords.Altitude * bg_Mapbox.WorldRelativeScale;
                    lightNodes[i].obj.transform.position = pos;
                }
            }
            else
            {
                bg_Mapbox.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
                for (int i = 0; i < lightNodes.Count; i++)
                {
                    Vector3 pos = lightNodes[i].obj.transform.position;
                    pos.y = 0;
                    //pos.y = (float)coords.Altitude * bg_Mapbox.WorldRelativeScale;
                    lightNodes[i].obj.transform.position = pos;
                }
            }
            if (SelectedBuilding != null)
            {
                SelectedFlagCube.transform.position = new Vector3(SelectedBuilding.transform.position.x,
                    SelectedBuilding.transform.position.y
                    , SelectedBuilding.transform.position.z);// + new Vector3(0, 30, 0);
            }
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
            bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(cbx_LightPoints.isOn);
        });

        btn_Load = GameObject.Find("Btn_Load").GetComponent<Button>();
        btn_Load.onClick.AddListener(delegate { LoadFile(); });

        btn_Save = GameObject.Find("Btn_Save").GetComponent<Button>();
        btn_Save.onClick.AddListener(delegate { SaveFile(); });

        tab_Overview = GameObject.Find("Tab_Overview").GetComponent<Button>();
        tab_Overview.onClick.AddListener(delegate { ActivateTab(0); });

        tab_LightObject = GameObject.Find("Tab_LightObject").GetComponent<Button>();
        tab_LightObject.onClick.AddListener(delegate { ActivateTab(1); });

        tab_Camera = GameObject.Find("Tab_Camera").GetComponent<Button>();
        tab_Camera.onClick.AddListener(delegate { ActivateTab(2); });

        txt_LightObjectName = GameObject.Find("Txt_LightObjectName").GetComponent<Text>();

        txt_RotationYValue = GameObject.Find("Txt_RotationYValue").GetComponent<Text>();
        slr_RotationY = GameObject.Find("Slr_RotationY").GetComponent<Slider>();
        slr_RotationY.onValueChanged.AddListener(delegate
        {
            if (SelectedBuilding != null) { 
                Vector3 angles = SelectedBuilding.transform.eulerAngles;
                angles.y = slr_RotationY.value;
                SelectedBuilding.transform.eulerAngles = angles;
                txt_RotationYValue.text = angles.y.ToString();
            }
        });

        drn_LightIES = GameObject.Find("Drn_LightIES").GetComponent<Dropdown>();
        drn_LightIES.ClearOptions();
        drn_LightIES.AddOptions(IESList);
        drn_LightIES.onValueChanged.AddListener(delegate {
            string newIES = drn_LightIES.options[drn_LightIES.value].text;
            int light_index = dictLightIndex[SelectedBuilding.name];
            if ((SelectedBuilding != null) && (newIES != lightNodes[light_index].IESfileName))
            {
                UnityEngine.Texture ies2DCookie = GetIESCookie(newIES);
                GameObject light = SelectedBuilding.transform.Find("Spot Light").gameObject;
                light.GetComponent<HDAdditionalLightData>().SetCookie(ies2DCookie);
                lightNodes[light_index].IESfileName = newIES;
            }
            Debug.Log(SelectedBuilding.name + " ies changed to " + newIES);
        });

        btn_Move = GameObject.Find("Btn_Move").GetComponent<Button>();
        btn_Move.onClick.AddListener(delegate { MoveObject(); });

        btn_Delete = GameObject.Find("Btn_Delete").GetComponent<Button>();
        btn_Delete.onClick.AddListener(delegate { DeleteObject(); });

        drn_Camera = GameObject.Find("Drn_Camera").GetComponent<Dropdown>();
        drn_Camera.onValueChanged.AddListener(delegate { ResetCamera();});

        btn_ResetCamera = GameObject.Find("Btn_ResetCamera").GetComponent<Button>();
        btn_ResetCamera.onClick.AddListener(delegate { ResetCamera(); });

        btn_UpdateCamera = GameObject.Find("Btn_UpdateCamera").GetComponent<Button>();
        btn_UpdateCamera.onClick.AddListener(delegate { UpdateCamera(); });

        ActivateTab(0);
    }

    public void ActivateTab(int tabIndex = 0)
    {
        for (int i = 0; i < TabMenuList.Length; i++)
        {
            TabMenuList[i].SetActive(false);
        }
        TabMenuList[tabIndex].SetActive(true);
    }

    public FeatureCollection Can_Deserialize(string filename = "LightPoint1 Lerstadvatnet.geojson")
    {
        var rd = new StreamReader(filename);// ("viktig.Geojson");

        string json = rd.ReadToEnd();

        var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);

        rd.Close();

        return featureCollection;
    }

    public UnityEngine.Texture GetIESCookie(string filename)
    {
        IESEngine engine = new IESEngine();
        IESMetaData iesMetaData = new IESMetaData();

        engine.TextureGenerationType = TextureImporterType.Default;

        UnityEngine.Texture cookieTexture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        string iesFilePath = path + filename;//Path.Combine(Path.GetDirectoryName(Application.dataPath), ctx.assetPath);
        string errorMessage = engine.ReadFile(iesFilePath);

        if (string.IsNullOrEmpty(errorMessage))
        {
            iesMetaData.FileFormatVersion = "LM-63-2002";// engine.GetFileFormatVersion();
            iesMetaData.IESPhotometricType = engine.GetPhotometricType();
            iesMetaData.Manufacturer = engine.GetKeywordValue("MANUFAC");
            iesMetaData.LuminaireCatalogNumber = engine.GetKeywordValue("LUMCAT");
            iesMetaData.LuminaireDescription = engine.GetKeywordValue("LUMINAIRE");
            iesMetaData.LampCatalogNumber = engine.GetKeywordValue("LAMPCAT");
            iesMetaData.LampDescription = engine.GetKeywordValue("LAMP");

            (iesMetaData.IESMaximumIntensity, iesMetaData.IESMaximumIntensityUnit) = engine.GetMaximumIntensity();

            string warningMessage;

            (warningMessage, cookieTexture2D) = engine.Generate2DCookie(iesMetaData.CookieCompression, iesMetaData.SpotAngle, (int)iesMetaData.iesSize, iesMetaData.ApplyLightAttenuation);
        }
        else
        {
            Debug.Log($"Cannot read IES file '{iesFilePath}': {errorMessage}");
        }

        string iesFileName = iesFilePath;// Path.GetFileNameWithoutExtension(ctx.assetPath);

        return cookieTexture2D;
    }

    public void LoadFile()
    {
        string outputfileName = "1.geojson";

        var paths = StandaloneFileBrowser.OpenFilePanel("Select NORDARK scene file", @"D:\Norway\Master Thesis\TTDM\NORDARK\SceneExample", "geojson", false);
        if (paths.Length > 0)
        {
            outputfileName = paths[0];
        }
        Debug.Log("E001: Load scene file <" + outputfileName + ">");

        FeatureCollection fCollection = Can_Deserialize(outputfileName);

        int num = fCollection.Features.Count - 1;
        if (num > 0)
        {
            DestroyChildren(LightsCollection.name);
            lightNodes.Clear();
            GeoJSON.Net.Geometry.Point mPoint;
            dictLightIndex = new Dictionary<string, int>();
            dictLightIESCookie = new Dictionary<string, object>();
            // Create nodes if not exist
            for (int i = 0; i < num; i++)
            {
                mPoint = fCollection.Features[i].Geometry as GeoJSON.Net.Geometry.Point;
                var coords = mPoint.Coordinates;
                {
                    Vector2 latlong;
                    Vector3 pos;
                    string strLightPrefabName;
                    Node lightNode = new Node();// LightNode
                    latlong = new Vector2((float)(coords.Latitude), (float)(coords.Longitude));
                    lightNode.GeoVec = latlong;
                    pos = latlong.AsUnityPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
                    //GameObject lightObject = Instantiate(Resources.Load("LightType1") as GameObject);
                    if (!fCollection.Features[i].Properties.ContainsKey("LightPrefabName"))
                        strLightPrefabName = LightPrefabList[0];
                    else
                        strLightPrefabName = fCollection.Features[i].Properties["LightPrefabName"] as string;
                    GameObject lightObject = Instantiate(Resources.Load(strStreetLightLib + "/" + strLightPrefabName) as GameObject);
                    lightObject.transform.name = "LightInfraS_" + i;
                    lightObject.transform.parent = LightsCollection.transform;
                    pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coords.Latitude, coords.Longitude));
                    lightObject.transform.position = pos;
                    lightObject.transform.eulerAngles = StrToVector3(fCollection.Features[i].Properties["eulerAngles"] as string);
                    lightObject.transform.localScale = new Vector3(1, 1, 1);
                    string strIES = fCollection.Features[i].Properties["IESfileName"] as string;
                    if (!dictLightIESCookie.ContainsKey(strIES))
                    {
                        dictLightIESCookie[strIES] = GetIESCookie(strIES);        
                    }
                    GameObject light = lightObject.transform.Find("Spot Light").gameObject;
                    light.GetComponent<HDAdditionalLightData>().SetCookie(dictLightIESCookie[strIES] as UnityEngine.Texture);
                    lightNode.IESfileName = strIES;
                    lightNode.LightPrefabName = strLightPrefabName;

                    lightNode.obj = lightObject;
                    lightNodes.Add(lightNode);
                    dictLightIndex.Add(lightObject.transform.name, i);
                }
            }
            mPoint = fCollection.Features[num].Geometry as GeoJSON.Net.Geometry.Point;
            if (mPoint.Coordinates.Latitude == 0)
            {
                //string fCollection.Features[num].Properties["mapCenter"];
                Dictionary<string, object> prop = fCollection.Features[num].Properties;
                bg_Mapbox.SetCenterLatitudeLongitude(StrToVector2d(prop["mapCenter"] as string));
                mainCamera.transform.position = StrToVector3(prop["cameraPos"] as string);
                mainCamera.transform.eulerAngles = StrToVector3(prop["cameraAngles"] as string);

                //
                string[] strCameras = (prop["cameraList"] as string).Split('%');
                if (strCameras.Length > 0)
                {
                    DestroyChildren(CameraCollection.name);
                    dictCamera.Clear();
                    for(int i = 0; i < strCameras.Length; i++)
                    {
                        string[] strCamera = strCameras[i].Split('&');
                        if (strCamera.Length == 3)
                        {
                            GameObject newCam = new GameObject();
                            newCam.transform.position = StrToVector3(strCamera[1]);
                            newCam.transform.eulerAngles = StrToVector3(strCamera[2]);
                            newCam.transform.parent = CameraCollection.transform;
                            string strCam = strCamera[0];
                            newCam.name = strCam;
                            dictCamera.Add(strCam, newCam);
                        }
                    }
                }
            }
        }

        Debug.Log("E003: Load scene file successfully");
        //
    }

    public void SaveFile()
    {
        int num = lightNodes.Count;
        for (int i = 0; i < num; i++)
        {
            // check obj exists or not
            if((lightNodes[i].obj as UnityEngine.Object) == null)
            { 
                lightNodes.RemoveAt(i);
                i--;
                num = lightNodes.Count;
            }
        }
            
        Mapbox.Utils.Vector2d[] g_image = new Mapbox.Utils.Vector2d[num];
        // feed the array and concat to as a string
        Dictionary<string, object>[] props = new Dictionary<string, object>[num + 1];
        for (int i = 0; i < num; i++)
        {
            Mapbox.Utils.Vector2d geopos = lightNodes[i].obj.transform.position.GetGeoPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
            lightNodes[i].GeoVec.x = (float)geopos.x;
            lightNodes[i].GeoVec.y = (float)geopos.y;
            
            g_image[i] = geopos;
            props[i] = new Dictionary<string, object>();
            props[i].Add("name", lightNodes[i].name);
            props[i].Add("eulerAngles", lightNodes[i].obj.transform.eulerAngles.ToString()); // after convert the alpha
            props[i].Add("IESfileName", lightNodes[i].IESfileName);
            props[i].Add("LightPrefabName", lightNodes[i].LightPrefabName);
        }
        props[num] = new Dictionary<string, object>();
        props[num].Add("mapCenter", bg_Mapbox.CenterLatitudeLongitude.ToString());
        props[num].Add("cameraPos", mainCamera.transform.position.ToString());
        props[num].Add("cameraAngles", mainCamera.transform.eulerAngles.ToString());
        //
        string[] strCameras = new string[dictCamera.Count];
        int j = 0;
        foreach (var i in dictCamera.Keys)
        {
            string[] strCamera = new string[3];
            strCamera[0] = i;
            strCamera[1] = dictCamera[i].transform.position.ToString();
            strCamera[2] = dictCamera[i].transform.eulerAngles.ToString();
            strCameras[j] = String.Join("&", strCamera);
            j++;
        }
        props[num].Add("cameraList", String.Join("%", strCameras));
        string outputfileName = "1.geojson";

        var filename = StandaloneFileBrowser.SaveFilePanel("Select NORDARK scene file", @"D:\Norway\Master Thesis\TTDM\NORDARK\SceneExample", "Ex00_Default", "geojson");
        if (filename != "")
        {
            outputfileName = filename;
        }

        SaveToGeojson(outputfileName, props, g_image); //g_edges
        Debug.Log("E002: Save scene file as <" + outputfileName + "> successfully");
        //
    }

    public void SaveToGeojson(string filename, Dictionary<string, object>[] properties, Mapbox.Utils.Vector2d[] geoimage)
    {
        if (geoimage.Length == lightNodes.Count)
        {
            var model = new FeatureCollection();

            GeoJSON.Net.Geometry.Point point;
            for (int i = 0; i < geoimage.Length; i++)
            {
                point = new GeoJSON.Net.Geometry.Point(new Position(geoimage[i].x, geoimage[i].y));

                var feature = new Feature(point, properties[i], i.ToString());
                model.Features.Add(feature);
            }
            point = new GeoJSON.Net.Geometry.Point(new Position(0, 0));
            model.Features.Add(new Feature(point, properties[geoimage.Length], geoimage.Length.ToString()));

            var json = JsonConvert.SerializeObject(model);

            json = json.Replace("\"type\":8", "\"type\":\"FeatureCollection\"");
            json = json.Replace("\"type\":7", "\"type\":\"Feature\"");
            json = json.Replace("\"type\":5", "\"type\":\"MultiPolygon\"");
            json = json.Replace("\"type\":4", "\"type\":\"Polygon\"");
            json = json.Replace("\"type\":3", "\"type\":\"MultiLineString\"");
            json = json.Replace("\"type\":0", "\"type\":\"Point\"");

            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine(json);
            sw.Close();
        }
    }

    public Mapbox.Utils.Vector2d StrToVector2d(string VStr)
    {
        string[] VStrArray = VStr.Split(',');
        if (VStrArray.Length == 2)
        {
            return new Mapbox.Utils.Vector2d(double.Parse(VStrArray[0]), double.Parse(VStrArray[1]));
        }
        else
            return new Mapbox.Utils.Vector2d(0, 0);
    }

    public Vector3 StrToVector3(string VStr)
    {
        string[] VStrArray = Between(VStr, "(", ")").Split(',');
        if (VStrArray.Length == 3)
        {
            return new Vector3(float.Parse(VStrArray[0]), float.Parse(VStrArray[1]), float.Parse(VStrArray[2]));
        }
        else
            return new Vector3(0, 0, 0);
    }

    public string Between(string STR, string FirstString, string LastString)
    {
        string FinalString;
        int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
        int Pos2 = STR.IndexOf(LastString);
        FinalString = STR.Substring(Pos1, Pos2 - Pos1);
        return FinalString;
    }

    public string GetFileName(string STR)
    {
        string FinalString;
        FinalString = STR.Substring(0, STR.IndexOf('.'));
        return FinalString;
    }

    public void DestroyChildren(string parentName)
    {
        //GameObject parent = GameObject.Find(parentName).gameObject;
        //GameObject newParent = Instantiate(parent);
        //newParent.name = parent.name;
        //Destroy(parent);
        //return newParent;
        Transform[] children = GameObject.Find(parentName).GetComponentsInChildren<Transform>();
        for (int i = 1; i < children.Length; i++)
        {
            if (children[i].gameObject != null)
                Destroy(children[i].gameObject);
        }
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
                        case KeyCode.Delete:
                            DeleteObject();
                            break;
                        case KeyCode.M:
                            MoveObject();
                            break;
                        case KeyCode.Insert:
                            InsertObject();
                            break;
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            t2 = Time.realtimeSinceStartup;
            if (t2 - t1 < 0.5f) //<0.5s, considered double click
            {
                if (SelectedBuilding != null)
                {
                    IsMoving = true;
                }
            }
            else
            {
                if (SelectBuilding())
                {
                    txt_LightObjectName.text = "Selected: " + SelectedBuilding.name;
                    if (SelectedBuilding.transform.eulerAngles.y < 0)
                        slr_RotationY.value = 0;
                    else
                        slr_RotationY.value = SelectedBuilding.transform.eulerAngles.y;
                    txt_RotationYValue.text = slr_RotationY.value.ToString();
                    int light_index = dictLightIndex[SelectedBuilding.name];
                    string iesFileName = lightNodes[light_index].IESfileName;
                    if (IESList.Contains(iesFileName))
                        drn_LightIES.value = IESList.FindIndex(x => x.Contains(iesFileName));
                    else
                        drn_LightIES.captionText.text = iesFileName;
                    SelectedFlagCube.SetActive(true);
                    Rotate SelRotate = (Rotate)SelectedFlagCube.GetComponent("Rotate");
                    SelRotate.LabelText = SelectedBuilding.name;
                    float scale = bg_Mapbox.WorldRelativeScale * 4;
                    SelectedFlagCube.transform.localScale = new Vector3(scale, scale, scale);
                    SelectedFlagCube.transform.position = new Vector3(SelectedBuilding.transform.position.x,
                    SelectedBuilding.transform.position.y
                    , SelectedBuilding.transform.position.z);// + new Vector3(0, 30, 0);
                    if (TestMode)
                        Debug.Log("E006: selected building is " + SelectedBuilding.name);
                }

                if (IsMoving)
                {
                    ClearSelectedBuilding();
                }
            }
            t1 = t2;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedBuilding();
        }

        if ((SelectedBuilding != null) && (IsMoving))
        {
            Vector3 posPlane = RefPlane.transform.position;
            posPlane.y = SelectedBuilding.transform.position.y;
            RefPlane.transform.position = posPlane;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2 = new RaycastHit();
            if (Physics.Raycast(ray, out hit2, 10000, 1 << 2))
            {
                Mapbox.Utils.Vector2d geopos = hit2.point.GetGeoPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
                Vector3 pos = SelectedBuilding.transform.position;
                pos.x = hit2.point.x;
                pos.z = hit2.point.z;
                pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(geopos);

                SelectedBuilding.transform.position = pos;
                SelectedFlagCube.transform.position = pos;
                Debug.Log(pos.ToString());
            }
        }
    }

    public void DeleteObject()
    {
        if (SelectedBuilding != null)
        {
            Destroy(SelectedBuilding);
            if (TestMode)
                Debug.Log("E007: selected building " + SelectedBuilding.name + " is deleted");
            ClearSelectedBuilding();
        }
    }
    public void MoveObject() 
    {
        if (SelectedBuilding != null)
        {
            if (IsMoving)
            {
                ClearSelectedBuilding();
            }
            else
                IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }

    public void InsertObject()
    {
        GameObject cloneObj;
        if (SelectedBuilding != null)
        {
            cloneObj = Instantiate(SelectedBuilding) as GameObject;
            cloneObj.name = "LightInfraS_" + lightNodes.Count;
            cloneObj.transform.parent = LightsCollection.transform;

            Node lightNode = new Node();// LightNode
            lightNode.GeoVec = lightNodes[lightNodes.Count - 1].GeoVec;
            lightNode.obj = cloneObj;
            lightNode.IESfileName = lightNodes[lightNodes.Count - 1].IESfileName;
            lightNodes.Add(lightNode);
            dictLightIndex.Add(cloneObj.transform.name, lightNodes.Count - 1);

            ClearSelectedBuilding();
            SelectedBuilding = cloneObj;
            SelectedFlagCube.SetActive(true);
            IsMoving = true;
        }
        else
        {
            //CreateByPredefined = true;
        }
    }

    public bool SelectBuilding()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000);
        if (hit)
        {
            SelectedBuilding = GameObject.Find(hitInfo.collider.name);
            return true;
        }
        else
            return false;
    }

    void ClearSelectedBuilding()
    {
        if (SelectedBuilding != null)
        {
            SelectedBuilding = null;
            SelectedFlagCube.SetActive(false);
        }
        IsMoving = false;
        txt_LightObjectName.text = "No selection";
        slr_RotationY.value = 0;
        txt_RotationYValue.text = "";
    }

    private void TaskOnClick()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "ies", false);
        if (paths.Length > 0)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                Debug.Log(paths[i]);
                FileUtil.MoveFileOrDirectory(paths[i], "Assets/IES folder/");
            }

            //StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }

    private void ResetCamera()
    {
        string strCamera = drn_Camera.options[drn_Camera.value].text;
        if (dictCamera.ContainsKey(strCamera))
        {
            mainCamera.transform.position = dictCamera[strCamera].transform.position;
            mainCamera.transform.eulerAngles = dictCamera[strCamera].transform.eulerAngles;
            Debug.Log("E008: Reset main camera to <" + strCamera + ">");
        }
    }

    private void UpdateCamera()
    {
        string strCamera = drn_Camera.options[drn_Camera.value].text;
        if (dictCamera.ContainsKey(strCamera))
        {
            dictCamera[strCamera].transform.position = mainCamera.transform.position;
            dictCamera[strCamera].transform.eulerAngles = mainCamera.transform.eulerAngles;
            Debug.Log("E009: Update <" + strCamera + "> by main camera");
        }
    }
}
