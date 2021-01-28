using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject DogPrefab;

    public void ResetEnv(GameObject ToDelete)
    {
        Destroy(ToDelete);
        GameObject go = (Instantiate(DogPrefab, transform.position + new Vector3(3,0,0), Quaternion.identity) as GameObject);
        go.transform.parent = transform;
    }
}
