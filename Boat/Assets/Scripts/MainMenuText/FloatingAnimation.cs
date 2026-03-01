using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatAmount = 10f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) *floatAmount;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}
