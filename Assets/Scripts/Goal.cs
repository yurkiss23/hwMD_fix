﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    // Статическое поле, доступное любому другому коду
    static public bool goalMet = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        // Когда в область действия триггера попадает что-то,
        // проверить, является ли это "что-то” снарядом
        if (other.gameObject.tag == "Projectile")
        {
            // Если это снаряд, присвоить полю goalMet значение true
            Goal.goalMet = true;
            // Также изменить альфа-канал цвета, чтобы увеличить непрозрачность
            Material mat = GetComponent<Renderer>().material;
            Color c = mat.color;
            c.a = 1;
            mat.color = c;
        }
    }
}
