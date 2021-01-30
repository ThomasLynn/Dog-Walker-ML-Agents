using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DogAgent : Unity.MLAgents.Agent
{

    public GameObject Body;
    public List<GameObject> LegParts;

    private GameObject CurrentDogBody;

    private Arena ParentArena;

    private float Distance;

    void Start()
    {
        ParentArena = transform.parent.GetComponent<Arena>();
    }

    public Arena GetParentArena()
    {
        return ParentArena;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (StepCount > 40)
        {
            Body.GetComponent<Rigidbody>().isKinematic = false;
        }
        if (StepCount >= MaxStep - 2)
        {
            EndEpisode();
            ParentArena.ResetEnv(gameObject);
        }
        for (int i = 0; i < LegParts.Count; i++)
        {
            float turnAmount = 0f;
            if (actionBuffers.DiscreteActions[i] == 1)
            {
                turnAmount = 0.5f;
            }
            else if (actionBuffers.DiscreteActions[i] == 2)
            {
                turnAmount = 1f;
            }
            else if (actionBuffers.DiscreteActions[i] == 3)
            {
                turnAmount = -0.5f;
            }
            else if (actionBuffers.DiscreteActions[i] == 4)
            {
                turnAmount = -1f;
            }
            //float turnAmount = Mathf.Clamp(actionBuffers.ContinuousActions[i], -1f, 1f);

            HingeJoint joint = LegParts[i].GetComponent<HingeJoint>();
            JointMotor motor = joint.motor;
            motor.targetVelocity = turnAmount * 200f;
            joint.motor = motor;
        }
        // Convert the second action to turning left or right

        float NewDistance = transform.Find("Body").position.x;
        //Debug.Log("distance " + NewDistance);
        if (NewDistance < Distance)
        {
            AddReward((Distance - NewDistance)/8f);
            Distance = NewDistance;
        }
    }

    /*public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Mathf.Sin(Time.time));
        sensor.AddObservation(Mathf.Cos(Time.time));

        sensor.AddObservation(Mathf.Sin(Time.time * 2));
        sensor.AddObservation(Mathf.Cos(Time.time * 2));

        sensor.AddObservation(Mathf.Sin(Time.time * 3));
        sensor.AddObservation(Mathf.Cos(Time.time * 3));

        sensor.AddObservation(Mathf.Sin(Time.time * 4));
        sensor.AddObservation(Mathf.Cos(Time.time * 4));
    }*/

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (Input.GetKey(KeyCode.W))
        {
            for (int i = 0; i < 9; i++)
            {
                actionsOut.DiscreteActions.Array[i] = 1;
                //actionsOut.ContinuousActions.Array[i] = 0.5f;
            }
        }
        else if (Input.GetKey(KeyCode.E))
        {
            for (int i = 0; i < 9; i++)
            {
                actionsOut.DiscreteActions.Array[i] = 2;
                //actionsOut.ContinuousActions.Array[i] = 1.0f;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            for (int i = 0; i < 9; i++)
            {
                actionsOut.DiscreteActions.Array[i] = 3;
                //actionsOut.ContinuousActions.Array[i] = -0.5f;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            for (int i = 0; i < 9; i++)
            {
                actionsOut.DiscreteActions.Array[i] = 4;
                //actionsOut.ContinuousActions.Array[i] = -1.0f;
            }
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                actionsOut.DiscreteActions.Array[i] = 0;
                //actionsOut.ContinuousActions.Array[i] = 0f;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        Body.GetComponent<BodyScript>().SetArenaAndReward(this, gameObject, -1);
        Distance = transform.Find("Body").position.x;
    }
}
