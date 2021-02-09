using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DogAgent : Unity.MLAgents.Agent
{

    public Transform body;
    public Transform head;
    public List<Transform> LegParts;
    public List<float> startingAngles;

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
        body.GetComponent<BodyScript>().SetArenaAndReward(this, gameObject, -.1f);
        head.GetComponent<BodyScript>().SetArenaAndReward(this, gameObject, -.1f);

        layerMask = 1 << 8;
        layerMask = ~layerMask;

        boxSize = new Vector3(1.0f, 0.1f, 1.0f);

        /*startingAngles = new float[LegParts.Count];

        for (int i = 0; i < LegParts.Count; i++)
        {
            startingAngles[i] = getAngleFromJoint(LegParts[i].GetComponent<HingeJoint>());
            //print("angles " + i + " " +startingAngles[i]);
        }*/
        
    }

    public Arena GetParentArena()
    {
        return ParentArena;
    }

    private float UnwrapAngle(float angle)
    {
        float wAngle = angle % 360f;
        while (wAngle > 180)
        {
            return wAngle - 360f;
        }
        if (wAngle < -180f)
        {
            return wAngle + 360f;
        }
        return wAngle;
    }

    private float getAngleFromJoint(HingeJoint joint)
    {
        //return Quaternion.Angle(joint.transform.rotation, joint.connectedBody.rotation);
        return joint.transform.rotation.eulerAngles.z - joint.connectedBody.rotation.eulerAngles.z;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (StepCount >= MaxStep - 2)
        {
            EndEpisode();
            ParentArena.ResetEnv(gameObject);
        }
        if (startingAngles != null)
        {
            for (int i = 0; i < LegParts.Count; i++)
            {
                //float turnAmount = 0f;

                //float turnTarget = Mathf.Clamp(actionBuffers.ContinuousActions[i], 0f, 1f);
                float turnTarget = actionBuffers.ContinuousActions[i]*45f;


                HingeJoint joint = LegParts[i].GetComponent<HingeJoint>();
                float currentTurn = UnwrapAngle(getAngleFromJoint(joint) - startingAngles[i]);

                //print(i + " " + LegParts[i].localRotation.eulerAngles.z + " " + startingAngles[i] + " " + UnwrapAngle(LegParts[i].localRotation.eulerAngles.z - startingAngles[i]) + " " + joint.limits.min + " " + joint.limits.max);
                //print("local rotation "+LegParts[i].localRotation.eulerAngles);
                //print(i+" rot " + getAngleFromJoint(joint) + " " + startingAngles[i] + " " + (getAngleFromJoint(joint)-startingAngles[i]));
                
                //float currentTurn = (() - joint.limits.min) / (joint.limits.max - +joint.limits.min);

                //float turnAmount = Mathf.Clamp((currentTurn - turnTarget) * -10000f, -200f, 200f);
                float turnAmount = Mathf.Clamp((currentTurn - turnTarget) * -10f, -200f, 200f);
                
                //print(i + " tar " + turnTarget + " " + currentTurn + " " + turnAmount);
                
                //print("diff " + (currentTurn - turnTarget)+ " "+ turnAmount);

                JointMotor motor = joint.motor;
                motor.targetVelocity = turnAmount;
                joint.motor = motor;
            }
        }
        
        // Convert the second action to turning left or right

        float newDistance = GetDistance();
        //Debug.Log("distance " + NewDistance);
        //if (newDistance < distance)
        //{
        AddReward(distance - newDistance);
        //Debug.Log("reward added " + (distance - newDistance) / 10f);
        distance = newDistance;
        //}
        
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

        //sensor.AddObservation(body.rotation.eulerAngles.y);
        /*float x = body.rotation.eulerAngles.x;
        if (x > 180f)
        {
            x -= 360f;
        }
        //Debug.Log("x "+Mathf.Atan(x));
        sensor.AddObservation(Mathf.Atan(x));
        float z = body.rotation.eulerAngles.z - 90;
        if (z > 180f)
        {
            z -= 360;
        }
        //Debug.Log("z "+Mathf.Atan(z / 10f));
        sensor.AddObservation(Mathf.Atan(z/10f));*/
        Vector3 start = body.position;
        Vector3 end = Vector3.MoveTowards(start, Target, 1f);
        Debug.DrawLine(start, end, Color.green, 0.1f);

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
            for (int i = 0; i < LegParts.Count; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = 1;
                actionsOut.ContinuousActions.Array[i] = 1.0f;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            for (int i = 0; i < LegParts.Count; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = -1;
                actionsOut.ContinuousActions.Array[i] = -1f;
            }
        }
        /*else if (Input.GetKey(KeyCode.S))
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
        }*/
        else
        {
            for (int i = 0; i < LegParts.Count; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = 0;
                actionsOut.ContinuousActions.Array[i] = 0f;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        //height = body.position.y;
        SetRandomTarget();
        float angle = Mathf.Atan2(Target.x - transform.position.x, Target.z - transform.position.z) * Mathf.Rad2Deg;
        //Debug.Log("rotating to angle" + angle);
        transform.rotation = Quaternion.Euler(new Vector3(0, angle + 90 + Random.Range(-10f, 10f), 0));
    }

    private void SetRandomTarget()
    {
        for (int i = 0; i < 50; i++)
        {
            //Debug.Log("looping");
            Vector3 v = transform.parent.position;
            v += new Vector3(Random.Range(-24f, 24f), 0.5f, Random.Range(-24f, 24f));
            if (Vector3.Distance(body.position, v) > 6.0f) {
                if (!Physics.CheckBox(v, boxSize, Quaternion.identity, layerMask))
                {
                    //Debug.Log("setting target "+v);
                    SetTarget(v);
                    //drawBox(v, boxSize, Color.green);
                    return;
                }
                else
                {

                    //drawBox(v, boxSize, Color.red);
                }
            }
        }
        //Debug.Log("setting target to zero");
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
