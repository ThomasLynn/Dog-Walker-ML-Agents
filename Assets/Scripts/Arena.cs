using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject dogPrefab;

    public int dogCount;

    private int currentDogCount;

    int layerMask;

    public void Start()
    {
        currentDogCount = 0;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        SpawnToNumber(dogCount);
    }

    public void ResetEnv(GameObject ToDelete)
    {
        Destroy(ToDelete);
        currentDogCount--;
        SpawnToNumber(dogCount);
    }

    private void SpawnToNumber(int number)
    {
        //Debug.Log(currentDogCount+" "+ number);
        for (int j = currentDogCount; j < number; j++)
        {
            //Debug.Log("trying to spawn");
            for (int i = 0; i < 50; i++)
            {
                //Debug.Log("finding spawn location");
                Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-9f, 9f), 1, Random.Range(-9f, 9f));
                if (!Physics.CheckBox(spawnPosition, new Vector3(1f, 0.7f, 1f), Quaternion.identity, layerMask))
                {
                    GameObject go = Instantiate(dogPrefab, spawnPosition, Quaternion.identity) as GameObject;
                    go.transform.parent = transform;
                    currentDogCount++;
                    break;
                }
            }
        }
        
    }
}
