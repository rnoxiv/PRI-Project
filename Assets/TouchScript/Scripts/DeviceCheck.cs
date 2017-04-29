using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceCheck : MonoBehaviour {

	// Use this for initialization

	void Start () {
        Debug.Log(SystemInfo.deviceType);
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            (GetComponent("StandardInput") as MonoBehaviour).enabled = false;

        }else
        {
            (GetComponent("TuioInput") as MonoBehaviour).enabled = false;
            print("Tablet");
        }
    }
}
