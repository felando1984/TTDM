using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private Button btnLoad;
    private Button btnSave;
    private GameObject mainCamera;
    //private GameObjectModifier buildingsModifier;
    private ReplaceFeatureCollectionModifier lightModifier;
    private GameObject LightsCollection;
    List<Node> lightNodes;
    string path = @"D:\Documents\GitHub\Ashkan\HDRP\Assets\";

    // Start is called before the first frame update
    void Start()
    {
        lightNodes = new List<Node>();
        LightsCollection = GameObject.Find("LightsCollection");
        mainCamera = GameObject.Find("Main Camera");
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
        // Load Geojson
        FeatureCollection fCollection = Can_Deserialize();
        string[] layer_coords = bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).coordinates;
        layer_coords = new string[fCollection.Features.Count];
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
                GameObject lightObject = Instantiate(Resources.Load("LightType1") as GameObject);
                lightObject.transform.name = "LightInfraS_" + i;
                lightObject.transform.parent = LightsCollection.transform;
                pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coords.Latitude, coords.Longitude));
                //pos.y = (float)coords.Altitude * bg_Mapbox.WorldRelativeScale;
                lightObject.transform.position = pos;
                lightObject.transform.localScale = new Vector3(1, 1, 1);
                lightNode.obj = lightObject;
                lightNodes.Add(lightNode);
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
        GameObject light = Instantiate(Resources.Load("LightType1") as GameObject);// bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).spawnPrefabOptions.prefab.GetComponentsInChildren<Transform>()[0].gameObject;
        light.transform.name = "LightDemo";
        light = light.transform.Find("Spot Light").gameObject;

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
                bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(59.809179, 17.7043457));
            bg_Mapbox.UpdateMap();
        });
        drn_LightType = GameObject.Find("Drn_LightType").GetComponent<Dropdown>();
        drn_LightType.onValueChanged.AddListener(delegate {
            bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(false);
            string path = @"Assets/Prefabs/StreetLightsPack/";
            bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).spawnPrefabOptions.prefab = Resources.Load(drn_LightType.options[drn_LightType.value].text) as GameObject;
            bg_Mapbox.VectorData.GetPointsOfInterestSubLayerAtIndex(0).SetActive(true);
        });
        drn_IESType = GameObject.Find("IESList").GetComponent<Dropdown>();
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
            Debug.Log("change to" + drn_IESType.options[drn_IESType.value].text);
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

        btnLoad = GameObject.Find("Btn_Load").GetComponent<Button>();
        btnLoad.onClick.AddListener(delegate { LoadFile(); });

        btnSave = GameObject.Find("Btn_Save").GetComponent<Button>();
        btnSave.onClick.AddListener(delegate { SaveFile(); });
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
    public FeatureCollection Can_Deserialize(string filename = "LightPoint1 Lerstadvatnet.geojson")
    {
        var rd = new StreamReader(filename);// ("viktig.Geojson");

        string json = rd.ReadToEnd();

        var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);

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
        FeatureCollection fCollection = Can_Deserialize(outputfileName);

        int num = fCollection.Features.Count - 1;
        if (num > 0)
        {
            DestroyChildren(LightsCollection.name);
            lightNodes.Clear();
            GeoJSON.Net.Geometry.Point mPoint;
            // Create nodes if not exist
            for (int i = 0; i < num; i++)
            {
                mPoint = fCollection.Features[i].Geometry as GeoJSON.Net.Geometry.Point;
                var coords = mPoint.Coordinates;
                {
                    Vector2 latlong;
                    Vector3 pos;
                    Node lightNode = new Node();// LightNode
                    latlong = new Vector2((float)(coords.Latitude), (float)(coords.Longitude));
                    lightNode.GeoVec = latlong;
                    pos = latlong.AsUnityPosition(bg_Mapbox.CenterMercator, bg_Mapbox.WorldRelativeScale);
                    GameObject lightObject = Instantiate(Resources.Load("LightType1") as GameObject);
                    lightObject.transform.name = "LightInfraS_" + i;
                    lightObject.transform.parent = LightsCollection.transform;
                    pos.y = bg_Mapbox.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coords.Latitude, coords.Longitude));
                    lightObject.transform.position = pos;
                    lightObject.transform.localScale = new Vector3(1, 1, 1);
                    lightNode.obj = lightObject;
                    lightNodes.Add(lightNode);
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
            }
        }

        Debug.Log(outputfileName + " loaded");
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
        }
        props[num] = new Dictionary<string, object>();
        props[num].Add("mapCenter", bg_Mapbox.CenterLatitudeLongitude.ToString());
        props[num].Add("cameraPos", mainCamera.transform.position.ToString());
        props[num].Add("cameraAngles", mainCamera.transform.eulerAngles.ToString());
        
        string outputfileName = "1.geojson";
        SaveToGeojson(outputfileName, props, g_image); //g_edges
        Debug.Log(outputfileName + " saved");
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
}
