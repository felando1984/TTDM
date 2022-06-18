using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class UpdateMap : MonoBehaviour
{
    private AbstractMap bg_Mapbox;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
