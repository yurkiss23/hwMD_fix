using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    static private Slingshot S; //a
    static public string firstName, secondName;
    static public bool isFire = false;

    // поля, устанавливаемые в инспекторе Unity
    [Header("Set in Inspector")] // a
    public GameObject prefabProjectile;
    public float velocityMult = 8f; // a
    public float server_fps = 0.5f;

    // поля, устанавливаемые динамически
    [Header("Set Dynamically")]         //a
    public GameObject launchPoint;
    public Vector3 launchPos; // b
    public GameObject projectile; // b
    public bool aimingMode; //b

    private Rigidbody projectileRigidbody; // a

    static public Vector3 LAUNCH_POS
    {
        get
        {
            if (S == null) return Vector3.zero;
            return S.launchPos;
        }
    }

    void Awake()
    {
        S = this;
        Transform launchPointTrans = transform.Find("LaunchPoint"); // a
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false); // b
        launchPos = launchPointTrans.position;
    }

    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true); // b
    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false); // b
    }

    void OnMouseDown()
    {
        if (isFire)
        {
            // Игрок нажал кнопку мыши, когда указатель находился над рогаткой
            aimingMode = true;
            // Создать снаряд
            projectile = Instantiate(prefabProjectile) as GameObject;
            // Поместить в точку launchPoint
            projectile.transform.position = launchPos;
            // Сделать его кинематическим
            projectileRigidbody = projectile.GetComponent<Rigidbody>();
            projectileRigidbody.isKinematic = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("GetRequest", server_fps);
    }

    void GetRequest()
    {
        // PositionCollider positionCollider = Network.GetData().Result;
        //if (!isFire)
        //{
            var pc = Network.GetData(firstName);
            if (pc != null)
            {
                projectile = Instantiate(prefabProjectile) as GameObject;

                // Сделать его кинематическим
                projectile.GetComponent<Rigidbody>().isKinematic = true;
                projectileRigidbody = projectile.GetComponent<Rigidbody>();
                projectileRigidbody.isKinematic = true;

                //????????????
                Vector3 myPos = new Vector3(pc.pos.X, pc.pos.Y, pc.pos.Z); //positionCollider.pos;//
                projectile.transform.position = myPos;

                projectileRigidbody.isKinematic = false;

                //????????????
                Vector3 v = new Vector3(pc.velocity.X, pc.velocity.Y, pc.velocity.Z);//positionCollider.velocity;
                projectileRigidbody.velocity = v;

                FollowCam.POI = projectile;
                projectile = null;

                MissionDemolition.ShotFired(); // a
                ProjectileLine.S.poi = projectile;
                isFire = true;

            }
        //}
        Invoke("GetRequest", server_fps);
    }

    // Update is called once per frame
    void Update()
    {
        // Если рогатка не в режиме прицеливания, не выполнять этот код
        if (!aimingMode) return;

        Vector3 mousePos2D = Input.mousePosition; // с
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Найти разность координат между launchPos и mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;
        // Ограничить mouseDelta радиусом коллайдера объекта Slingshot // d
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // Передвинуть снаряд в новую позицию
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;
        if (Input.GetMouseButtonUp(0))
        {
            // Кнопка мыши отпущена
            aimingMode = false;
            projectileRigidbody.isKinematic = false;
            projectileRigidbody.velocity = -mouseDelta * velocityMult;
            FollowCam.POI = projectile;

            Network.PostData(secondName, projPos, projectileRigidbody.velocity);
            isFire = false;

            projectile = null;
            MissionDemolition.ShotFired();
            ProjectileLine.S.poi = projectile;
        }
    }
}


public class Network
{
    public static Solider GetData(string nick)
    {
        string url = string.Format("http://91.238.103.45:200/api/game/{0}", nick);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        var webResponse = request.GetResponse();
        var webStream = webResponse.GetResponseStream();
        var responseReader = new StreamReader(webStream);
        string response = responseReader.ReadToEnd();
        Solider pc = JsonConvert.DeserializeObject<Solider>(response);
        responseReader.Close();
        return pc;

    }

    public static void PostData(string nick, Vector3 pos, Vector3 velocity)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://91.238.103.45:200/api/game");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            Solider pc = new Solider
            {
                Nick = nick,
                pos = new PosVextor3 { X = pos.x, Y = pos.y, Z = pos.z },
                velocity = new PosVextor3 { X = velocity.x, Y = velocity.y, Z = velocity.z }
            };
            string json = JsonConvert.SerializeObject(pc);
            streamWriter.Write(json);
        }
        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
}

public class Solider
{
    public string Nick { get; set; }
    public PosVextor3 pos { get; set; }
    public PosVextor3 velocity { get; set; }
}

public class PosVextor3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}