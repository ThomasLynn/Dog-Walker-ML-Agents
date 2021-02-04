using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabliser : MonoBehaviour
{

    private Transform body;

    void Start()
    {
        body = transform.parent.Find("Body");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = body.position;
        Vector3 v = new Vector3(0, 0, 0);

        v.y = body.eulerAngles.y - 90;
        //v.z = body.eulerAngles.z;

        //v.z = -body.rotation.x;
        //v.y += -body.rotation.z;
        
        transform.eulerAngles = v;
    }
}
