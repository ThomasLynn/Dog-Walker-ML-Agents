using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyScript : MonoBehaviour
{
    DogAgent ParentAgent;
    GameObject ParentObject;
    float Reward;

    public void SetArenaAndReward(DogAgent LocalParentAgent, GameObject LocalParentObject, float LocalReward)
    {
        ParentAgent = LocalParentAgent;
        ParentObject = LocalParentObject;
        Reward = LocalReward;
    }

    void OnTriggerStay(Collider other)
    {
        ParentAgent.AddReward(1f);
        ParentAgent.EndEpisode();
        ParentAgent.GetParentArena().ResetEnv(ParentObject);
    }
}
