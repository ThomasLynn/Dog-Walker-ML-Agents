using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFollower : MonoBehaviour
{

    public Transform arena;

    public float cameraSpeed;
    public float cameraRotSpeed;

    public float height;
    public float length;

    public bool follow;

    private Vector3 startingPos;
    private Quaternion startingRot;
    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            follow = !follow;
        }
        if (follow)
        {
            foreach (Transform child2 in arena)
            {
                if (child2.tag == "Dog")
                {
                    //print("moving " + transform.position);
                    Vector3 bodyPos = child2.Find("Body").position;
                    Vector3 bodyPosFlatNorm = (new Vector3(bodyPos.x, 0, bodyPos.z)).normalized * length;
                    Vector3 newPos = new Vector3(bodyPos.x, 0, bodyPos.z) + bodyPosFlatNorm + Vector3.up * height;
                    transform.position = Vector3.MoveTowards(transform.position, newPos, cameraSpeed * Time.deltaTime);

                    Quaternion look = Quaternion.LookRotation(bodyPos - transform.position, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, look, cameraRotSpeed * Time.deltaTime);
                    break;
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startingPos, cameraSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, startingRot, cameraRotSpeed * Time.deltaTime);
        }
    }
}
