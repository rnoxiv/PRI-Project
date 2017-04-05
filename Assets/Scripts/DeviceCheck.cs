using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceCheck : MonoBehaviour {

	// Use this for initialization

    private bool checkedSyst = false;
    public GameObject touchManager;

	void Start () {

        if (!checkedSyst)
        {
            Debug.Log(SystemInfo.deviceType);
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                (touchManager.GetComponent("StandardInput") as MonoBehaviour).enabled = false;

            }else
            {
                (touchManager.GetComponent("TuioInput") as MonoBehaviour).enabled = false;
                print("Tablet");
            }
        }
    }
}
