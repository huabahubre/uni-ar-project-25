using System;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 rotation;
    public float speed = 10f;

    private void Update()
    {
        transform.Rotate(rotation * speed * Time.deltaTime);
    }
}
