using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMillRotate : MonoBehaviour
{
    public float ratationSpeed = 25f;

    void Update()
    {
        transform.Rotate(0,0,ratationSpeed * Time.deltaTime);
    }
}
