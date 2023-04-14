using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Logs : MonoBehaviour
{
    private DateTime functionStartTime;
    private bool prefabAppeared = false;

    private void Start()
    {
        functionStartTime = DateTime.Now;
    }

    private void Update()
    {
        if (prefabAppeared)
        {
            Debug.Log("Prefab appeared at: " + DateTime.Now.ToString());
            prefabAppeared = false; // reset the flag
        }
    }

    public void MyFunction()
    {
        Debug.Log("MyFunction() started at: " + functionStartTime.ToString());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MyPrefab"))
        {
            prefabAppeared = true;
        }
    }
}

