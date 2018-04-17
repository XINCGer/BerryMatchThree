using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Berry.Utils;

public class LevelMap : MonoBehaviour {

    public static LevelMap main;

    public float mapSize = 1024f;

    Camera mapCamera;
    public float spawnOffset = 0.1f;
    Transform content;
    public float friction = 10;
    public MapLocation[] locations;
    Dictionary<int, MapLocation> locationsList = new Dictionary<int, MapLocation>();
    [HideInInspector]
    public int[] locationLevelNumber;

    Vector2 inertion = new Vector2();
    bool drag = false;
    Vector2[] lastPosition = new Vector2[2];
    Vector3 clickInfo = new Vector3();


    void Awake() {
        main = this;
        if (content == null) {
            content = new GameObject("Map").transform;
            content.gameObject.layer = LayerMask.NameToLayer("Map");
        }
        if (mapCamera == null) {
            mapCamera = new GameObject("MapCamera").AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = Color.black;
            mapCamera.cullingMask = 1 << LayerMask.NameToLayer("Map");
            mapCamera.transform.position = new Vector3(0, 0, -10);
        }

        UIAssistant.onScreenResize += UpdateMapParameters;
        UpdateMapParameters();
        mapCamera.orthographicSize = camSizeMax;
        spawnOffset *= Mathf.Max(Screen.width, Screen.height);

        locationLevelNumber = new int[locations.Length];
        locationLevelNumber[0] = 0;
        for (int i = 1; i < locations.Length; i++) {
            locationLevelNumber[i] = locationLevelNumber[i - 1] + locations[i-1].GetLevelCount();
        }

        UIAssistant.onShowPage += (string page) => {if (page == "LevelList") UpdateMap();};
    }

    void UpdateMap() {                
        foreach (LevelButton button in FindObjectsOfType<LevelButton>())
            button.Initialize();
    }

    void Update() {
        Nativation();

         if (drag)
            return;
        Move(inertion);
        float speed = inertion.magnitude;
        speed = Mathf.MoveTowards(speed, 0, Time.unscaledDeltaTime * friction);
        if (speed == 0)
            inertion = Vector2.zero;
        else
            inertion = inertion.normalized * speed;
    }

    public void Refresh() {
        if (gameObject.activeInHierarchy) {
            OnDisable();
            OnEnable();
        }
    }

    void Nativation() {
        if (Application.isMobilePlatform)
            MobileNavigation();
        else
            DesktopNatiovation();
    }

    Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
    void MobileNavigation() {
        if (Input.touchCount == 0) {
            if (drag)
                OnEndDrag();
            return;
        }

        if (!drag)
            OnBeginDrag();

        if (!drag) return;

        Vector2 delta = new Vector2();
        foreach (Touch touch in Input.touches)
            if (touches.ContainsKey(touch.fingerId))
                delta += touch.position - touches[touch.fingerId];
        touches.Clear();
        foreach (Touch touch in Input.touches)
            touches.Add(touch.fingerId, touch.position);

        delta = delta / Input.touchCount;

        if (Input.touches[0].phase != TouchPhase.Ended) {
            inertion = delta;
            lastPosition[0] = Input.touches[0].position;
            Move(inertion);
        }

        if (Input.touchCount >= 2) {
            float last = ((Input.touches[0].position - Input.touches[0].deltaPosition) - (Input.touches[1].position - Input.touches[1].deltaPosition)).magnitude;
            float current = (Input.touches[0].position - Input.touches[1].position).magnitude;

            if (current != last)
                Zoom((200 * mapCamera.orthographicSize) * ((last - current) / last));
        }
    }

    void DesktopNatiovation() {
        if (Input.GetMouseButtonDown(0)) {
            lastPosition[0] = Input.mousePosition;
            OnBeginDrag();
        }

        if (drag) {
            if (Input.GetMouseButton(0)) {
                inertion = Utils.Vector3to2(Input.mousePosition) - lastPosition[0];
                Move(inertion);
                lastPosition[0] = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0)) {
                OnEndDrag();
            }
        }

        if (Input.mouseScrollDelta.y != 0)
            Zoom(Input.mouseScrollDelta.y * 200);
    }
    
    public float camSizeMin = 0;
    public float camSizeMax = 0;
    public void UpdateMapParameters() {
        camSizeMax = 0.5f * (mapSize * Screen.height) / (Screen.width * 100f);
        camSizeMin = Mathf.Min(3.5f, camSizeMax);
        mapCamera.orthographicSize = camSizeMax;
    }

    void OnEnable() {
        for (int n = 0; n < locations.Length; n++)
            locations[n].number = n;
        main = this;
        content.gameObject.SetActive(true);
        mapCamera.gameObject.SetActive(true);
        UpdateMapParameters();
        if (content.childCount == 0) {
            int target_level = 1;
            if (LevelProfile.main != null)
                target_level = LevelProfile.main.level;
            else
                target_level = ProfileAssistant.main.local_profile.current_level;

            int location = 0;
            for (int i = 0; i < locationLevelNumber.Length; i++) {
                if (target_level < locationLevelNumber[i])
                    break;
                location = i;
            }

            Transform rect = Transform.Instantiate(locations[location].transform);
            rect.name = locations[0].name;
            rect.parent = content;
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;

            MapLocation map_location = rect.gameObject.GetComponent<MapLocation>();
            map_location.number = location;
            Vector3 position = mapCamera.transform.position;
            position.y = (map_location.nextLocationConnector.position.y + map_location.previousLocationConnector.position.y) / 2;
            mapCamera.transform.position = position;
        }
    }

    void OnDisable() {
        foreach (MapLocation loc in new List<MapLocation>(locationsList.Values))
            if (loc)
                DestroyImmediate(loc.gameObject);
        locationsList.Clear();
        if (content)
            content.gameObject.SetActive(false);
        if (mapCamera)
            mapCamera.gameObject.SetActive(false);
    }

    public void OnBeginDrag() {
        if (EventSystem.current.IsPointerOverGameObject(-1))
            return;
        if (EventSystem.current.IsPointerOverGameObject(0))
            return;

        drag = true;
        inertion = Vector2.zero;
        
        if (Application.isMobilePlatform)
            clickInfo = Input.touches[0].position;
        else 
            clickInfo = Input.mousePosition;

        clickInfo.z = Time.unscaledTime;

        touches.Clear();
        foreach (Touch touch in Input.touches)
            touches.Add(touch.fingerId, touch.position);
    }

    public void OnEndDrag() {
        drag = false;

        if (EventSystem.current.IsPointerOverGameObject(-1))
            return;
        if (EventSystem.current.IsPointerOverGameObject(0))
            return;

        if (Time.unscaledTime - clickInfo.z > 0.3f) return;
        float delta = 0;

        Vector2 point = Application.isMobilePlatform ?
            lastPosition[0] :
            new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        delta = Vector2.Distance(point, new Vector2(clickInfo.x, clickInfo.y));
        delta /= Mathf.Min(Screen.width, Screen.height);

        if (delta > 0.05f) return;

        point = mapCamera.ScreenPointToRay(point).origin;

        RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero);
        if (!hit.transform || hit.transform.tag != "LevelButton")
            return;

        hit.transform.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
    }

    void Zoom(float z) {
        mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize + z * Time.unscaledDeltaTime, camSizeMin, camSizeMax);
    }

    void Move(Vector2 delta) {
        if (locationsList.ContainsKey(0)) {
            float y = mapCamera.WorldToScreenPoint(locationsList[0].previousLocationConnector.transform.position).y;
            if (y + delta.y >= 0) {
                inertion = Vector2.zero;
                delta.y = -y;
            }
        }
        if (locationsList.ContainsKey(locations.Length - 1)) {
            float y = mapCamera.WorldToScreenPoint(locationsList[locations.Length - 1].nextLocationConnector.transform.position).y;
            if (y + delta.y <= Screen.height) {
                inertion = Vector2.zero;
                delta.y = Screen.height - y;
            }
        }
        Vector3 position = mapCamera.transform.position;
        float crop = (camSizeMax - mapCamera.orthographicSize) * mapSize / (100 * 2 * camSizeMax);

        position -= new Vector3(delta.x, delta.y, 0) * 2 * mapCamera.orthographicSize / Screen.height;
        position.x = Mathf.Clamp(position.x, -crop, crop);
        mapCamera.transform.position = position;

        foreach (MapLocation location in new List<MapLocation>(locationsList.Values))
            location.OnPositionChanged();
    }



    public void AddLocation(MapLocation mapLocation) {
        locationsList.Add(mapLocation.number, mapLocation);
    }

    public void RemoveLocation(MapLocation mapLocation) {
        locationsList.Remove(mapLocation.number);
    }

    public int IsVisible(Transform o) {
        float y = mapCamera.WorldToScreenPoint(o.position).y;
        if (y < -spawnOffset)
            return 1;
        if (y >= Screen.height + spawnOffset)
            return -1;
        return 0;
    }

    public MapLocation ShowNextLocation(MapLocation mapLocation) {
        int index = mapLocation.number + 1;

        if (index < 0 || index >= locations.Length)
            return null;

        MapLocation nextLocation;
        if (locationsList.ContainsKey(index))
            nextLocation = locationsList[index];
        else {
            nextLocation = locations[index];
            Transform t = Transform.Instantiate(nextLocation.transform);
            t.name = nextLocation.name;

            t.parent = content;
            t.position = mapLocation.nextLocationConnector.position;
            t.localScale = Vector3.one;
            nextLocation = t.GetComponent<MapLocation>();
            nextLocation.number = index;
        }

        return nextLocation;
    }

    public MapLocation ShowPreviuosLocation(MapLocation mapLocation) {
        int index = mapLocation.number - 1;

        if (index < 0 || index >= locations.Length)
            return null;

        MapLocation previuosLocation;
        if (locationsList.ContainsKey(index))
            previuosLocation = locationsList[index];
        else {
            previuosLocation = locations[index];
            Transform t = Transform.Instantiate(previuosLocation.transform);
            t.name = previuosLocation.name;

            t.parent = content;
            t.position = mapLocation.previousLocationConnector.position;
            previuosLocation = t.GetComponent<MapLocation>();
            previuosLocation.number = index;
            t.localScale = Vector3.one;
            t.position -= previuosLocation.nextLocationConnector.position - t.position;
        }

        return previuosLocation;
    }
}
