using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogKiller : MonoBehaviour
{
    public float rewardPenalty;

    void OnCollisionStay(Collision collisionInfo)
    {
        //Debug.Log("colliding");
        if (collisionInfo.transform.tag == "Dog")
        {
            Transform trans = collisionInfo.transform;
            //print(collisionInfo.transform);
            for(int i = 0; i < 3; i++)
            {
                DogAgent agent = trans.GetComponent<DogAgent>();
                //print(agent);
                if (agent != null)
                {
                    //print("dog fall oh no");
                    agent.AddReward(rewardPenalty);
                    agent.EndEpisode();
                    agent.GetParentArena().ResetEnv(trans.gameObject);
                    break;
                }
                else
                {
                    trans = trans.parent;
                }
            }
        }
    }
}
