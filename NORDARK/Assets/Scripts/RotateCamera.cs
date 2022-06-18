using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class RotateCamera : MonoBehaviour
{
    [SerializeField]
    private float lookSpeedH = 5f;

    [SerializeField]
    private float lookSpeedV = 5f;

    [SerializeField]
    private float zoomSpeed = 10f;

    [SerializeField]
    private float dragSpeed = 5f;

    [SerializeField]
    private float dragSpeedY = 5f;

    private float yaw = 0f;
    private float pitch = 0f;

    public Vector3 default_pos;
    public Vector3 default_rot;

    private AbstractMap bg_Mapbox;
    public float MapZoomSpeed = 1.5f;
    public float MapMoveSpeed = 0.001f;
    public float MapZoom;
    public double MapGeoLng;
    public double MapGeoLat;

    // Use this for initialization
    void Start()
    {
        this.yaw = this.transform.eulerAngles.y;
        this.pitch = this.transform.eulerAngles.x;
        default_pos = this.transform.position;
        default_rot = this.transform.eulerAngles;

        bg_Mapbox = GameObject.Find("BG_Mapbox").GetComponent<AbstractMap>();
        MapZoom = bg_Mapbox.Zoom;
        MapGeoLng = bg_Mapbox.CenterLatitudeLongitude.x;
        MapGeoLat = bg_Mapbox.CenterLatitudeLongitude.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(this.dragSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-this.dragSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, 0, -this.dragSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 0, this.dragSpeed * Time.deltaTime));

        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(new Vector3(0, -this.dragSpeedY * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(new Vector3(0, this.dragSpeedY * Time.deltaTime, 0));

        }

        //if (Input.GetKey(KeyCode.LeftControl))
        //{
        //Look around with Left Mouse
        if (Input.GetMouseButton(1))
        {
            this.yaw += this.lookSpeedH * Input.GetAxis("Mouse X");
            this.pitch -= this.lookSpeedV * Input.GetAxis("Mouse Y");

            this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);
        }

        // Build 0063, if press Alt, then zoom the map and move the center position
        //62.4676991855481, 6.30334069538369
        //17.45
        bool updateMap = false;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // the scale of the tile will change, 1,2,4,8
            MapZoom += Input.GetAxis("Mouse ScrollWheel") * MapZoomSpeed;
            Debug.Log(MapZoom);
            if (bg_Mapbox.Zoom != MapZoom)
            {
                bg_Mapbox.SetZoom(MapZoom);
                updateMap = true;

            }
            if (Input.GetMouseButton(2))
            {
                if (MapGeoLng == 0)
                    MapGeoLng = bg_Mapbox.CenterLatitudeLongitude.x;
                if (MapGeoLat == 0)
                    MapGeoLat = bg_Mapbox.CenterLatitudeLongitude.y;
                MapGeoLat -= Input.GetAxisRaw("Mouse X") * Time.deltaTime * MapMoveSpeed / Mathf.Pow(2, MapZoom);
                MapGeoLng -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * MapMoveSpeed / Mathf.Pow(2, MapZoom);
                if ((bg_Mapbox.CenterLatitudeLongitude.x != MapGeoLng) || (bg_Mapbox.CenterLatitudeLongitude.y != MapGeoLat))
                {
                    bg_Mapbox.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(MapGeoLng, MapGeoLat));
                    updateMap = true;
                }
            }
            if (updateMap)
                bg_Mapbox.UpdateMap();
        }
        else
        {
            //drag camera around with Middle Mouse
            if (Input.GetMouseButton(2))
            {
                transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
            }

            this.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * this.zoomSpeed, Space.Self);
        }
    }
}