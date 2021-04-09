using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject dogPrefab;

    public int dogCount;

    public List<Transform> checkpoints;

    private int currentDogCount;

    private List<GameObject> dogs;

    private int layerMask;

    public void Start()
    {
        if (Application.isEditor)
        {
            dogCount = 1;
        }
        dogs = new List<GameObject>();
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        Spawn();
    }

    public void ResetEnv(GameObject ToDelete)
    {
        Destroy(ToDelete);
        dogs.Remove(ToDelete);
        Spawn();
    }

    private void Spawn()
    {
        //Debug.Log(currentDogCount+" "+ number);
        for (int j = dogs.Count; j < dogCount; j++)
        {
            //Debug.Log("trying to spawn");
            //for (int i = 0; i < 50; i++)
            //{
            //Debug.Log("finding spawn location");
            int checkpointNumber = Random.Range(0, checkpoints.Count);
            
                //Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-9f, 9f), 1, Random.Range(-9f, 9f));
                //if (!Physics.CheckBox(spawnPosition, new Vector3(1f, 0.7f, 1f), Quaternion.identity, layerMask))
                //{
            GameObject go = Instantiate(dogPrefab, checkpoints[checkpointNumber].position, Quaternion.identity, transform) as GameObject;
            go.GetComponent<DogAgent>().SetCheckpointNumber(checkpointNumber);
            go.GetComponent<DogAgent>().SetRandomTarget(true);
            //go.transform.parent = transform;
            dogs.Add(go);
                //break;
                //}
            //}
        }
        
    }
}
