using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFollower : MonoBehaviour
{

    public Vector3 offset;

    public Transform arena;

    public bool follow;

    private Vector3 startingPos;
    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
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
                    Vector3 newPos = new Vector3(bodyPos.x + offset.x, offset.y, bodyPos.z + offset.z);
                    transform.position = newPos;
                    break;
                }
            }
        }
        else
        {
            transform.position = startingPos;
        }
    }
}
