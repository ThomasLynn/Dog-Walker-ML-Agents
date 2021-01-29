using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject dogPrefab;

    public Transform obstacle;

    public void ResetEnv(GameObject ToDelete)
    {
        Destroy(ToDelete);

        obstacle.localPosition = new Vector3(-5, 1, Random.Range(-3.5f, 3.5f));

        GameObject go = (Instantiate(dogPrefab, transform.position + new Vector3(8,0, Random.Range(-3f, 3f)), Quaternion.identity) as GameObject);
        go.transform.parent = transform;
    }
}
