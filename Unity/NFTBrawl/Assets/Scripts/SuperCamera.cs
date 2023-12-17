using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCamera : MonoBehaviour
{
    public static SuperCamera instance;
    private Camera camera;

    public Camera Camera { get => camera; set => camera = value; }

    private void Awake()
    {
        instance = this;
        camera = GetComponent<Camera>();
    }

}
