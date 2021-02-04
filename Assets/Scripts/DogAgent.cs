using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DogAgent : Unity.MLAgents.Agent
{

    public Transform body;
    public List<Transform> LegParts;

    private GameObject CurrentDogBody;

    private Arena ParentArena;

    private float distance;
    //private float height;
    private int layerMask;

    private Vector3 Target;
    private Vector3 boxSize;

    void Start()
    {
        ParentArena = transform.parent.GetComponent<Arena>();

        layerMask = 1 << 8;
        layerMask = ~layerMask;

        boxSize = new Vector3(1.0f, 0.1f, 1.0f);
    }

    public Arena GetParentArena()
    {
        return ParentArena;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
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

        float newDistance = GetDistance();
        //Debug.Log("distance " + NewDistance);
        if (newDistance < distance)
        {
            AddReward((distance - newDistance)/10f);
            //Debug.Log("reward added " + (distance - newDistance) / 10f);
            distance = newDistance;
        }
        if (newDistance < 0.5f)
        {
            SetRandomTarget();
        }

        /*if (StepCount >= 10)
        {
            AddReward(body.position.y - height);
            //Debug.Log(body.position.y - height);
        }
        height = body.position.y;*/
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localTarget = transform.InverseTransformPoint(Target);
        Vector3 localTargetNorm = localTarget.normalized;

        sensor.AddObservation(Mathf.Atan(localTarget.x));
        sensor.AddObservation(Mathf.Atan(localTarget.y));
        sensor.AddObservation(Mathf.Atan(localTarget.z));
        sensor.AddObservation(localTargetNorm.x);
        sensor.AddObservation(localTargetNorm.y);
        sensor.AddObservation(localTargetNorm.z);
        sensor.AddObservation(Mathf.Atan(localTarget.magnitude));
    }

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
        body.GetComponent<BodyScript>().SetArenaAndReward(this, gameObject, -1);
        //height = body.position.y;
        SetRandomTarget();
        float angle = Mathf.Atan2(Target.z - transform.position.z, Target.x - transform.position.x) * Mathf.Rad2Deg;
        Debug.Log("rotating to angle" + angle);
        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    private void SetRandomTarget()
    {
        for(int i = 0; i < 50; i++)
        {
            Debug.Log("looping");
            Vector3 v = transform.parent.position;
            v += new Vector3(Random.Range(-9f, 9f), 0.5f, Random.Range(-9f, 9f));
            if (!Physics.CheckBox(v, boxSize, Quaternion.identity, layerMask))
            {
                Debug.Log("setting target "+v);
                SetTarget(v);
                //drawBox(v, boxSize, Color.green);
                return;
            }
            else
            {

                //drawBox(v, boxSize, Color.red);
            }
        }
        Debug.Log("setting target to zero");
        SetTarget(Vector3.zero);
        
    }

    private void drawBox(Vector3 center, Vector3 halfSize, Color color)
    {
        Debug.DrawLine(center, center + new Vector3(halfSize.x,halfSize.y,halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(-halfSize.x, halfSize.y, halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(halfSize.x, halfSize.y, -halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), color, 1.0f);
    }

    private void SetTarget(Vector3 LocalTarget)
    {
        Target = LocalTarget;
        distance = GetDistance();
    }

    private float GetDistance()
    {
        //return transform.Find("Body").position.x;
        float localDistance = Vector3.Distance(body.position, Target);
        //Debug.Log("localdistance " + localDistance);
        return localDistance;
    }
}
