using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    static private Slingshot S; //a

    // поля, устанавливаемые в инспекторе Unity
    [Header("Set in Inspector")] // a
    public GameObject prefabProjectile;
    public float velocityMult = 8f; // a

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

    // Start is called before the first frame update
    void Start()
    {
        
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
            projectile = null;
            MissionDemolition.ShotFired();
            ProjectileLine.S.poi = projectile;
        }
    }
}
