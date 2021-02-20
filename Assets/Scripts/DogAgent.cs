using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DogAgent : Unity.MLAgents.Agent
{

    public float bodyRewardLoss;
    public Transform body;
    public List<BodyScript> hitScripts;
    public Transform head;
    public List<Transform> LegParts;
    public List<float> startingAngles;

    public float raycastDistance;
    private int layerMask;

    private int checkpointNumber;

    private GameObject CurrentDogBody;

    private float distance;
    //private float height;
    //private int layerMask;

    private Vector3 Target;
    private Vector3 boxSize;

    void Start()
    {
        //body.GetComponent<BodyScript>().SetArenaAndReward(this, gameObject, bodyRewardLoss);
        foreach(BodyScript w in hitScripts)
        {
            w.SetArenaAndReward(this, gameObject, bodyRewardLoss);
        }

        layerMask = 1 << 9;
        //layerMask = ~layerMask;

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
        return transform.parent.GetComponent<Arena>();
    }

    public void SetCheckpointNumber(int num)
    {
        checkpointNumber = num;
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
            GetParentArena().ResetEnv(gameObject);
        }
        if (startingAngles != null)
        {
            for (int i = 0; i < LegParts.Count; i++)
            {
                //float turnAmount = 0f;

                //float turnTarget = Mathf.Clamp(actionBuffers.ContinuousActions[i], 0f, 1f);


                HingeJoint joint = LegParts[i].GetComponent<HingeJoint>();

                float turnTarget = ((joint.limits.max + joint.limits.min) / 2f) + (actionBuffers.ContinuousActions[i] * (joint.limits.max - joint.limits.min)/2f);

                float currentTurn = UnwrapAngle(getAngleFromJoint(joint) - startingAngles[i]);

                //print(i + " " + LegParts[i].localRotation.eulerAngles.z + " " + startingAngles[i] + " " + UnwrapAngle(LegParts[i].localRotation.eulerAngles.z - startingAngles[i]) + " " + joint.limits.min + " " + joint.limits.max);
                //print("local rotation "+LegParts[i].localRotation.eulerAngles);
                print(i+" rot " + getAngleFromJoint(joint) + " " + startingAngles[i] + " " + (getAngleFromJoint(joint)-startingAngles[i]));
                
                //float currentTurn = (() - joint.limits.min) / (joint.limits.max - +joint.limits.min);

                //float turnAmount = Mathf.Clamp((currentTurn - turnTarget) * -10000f, -200f, 200f);
                float turnAmount = Mathf.Clamp((currentTurn - turnTarget) * -10f, -300f, 300f);
                
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
        AddReward((distance - newDistance)*0.1f); // Scaled to keep the extrinsic value estimate below 1 (should soft-cap at about 0.5
        //Debug.Log("reward added " + (distance - newDistance) / 10f);
        distance = newDistance;
        //}
        
        if (newDistance < 1.0f)
        {
            checkpointNumber = checkpointNumber + 1;
            SetRandomTarget(false);
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
        Vector3 localTarget = body.InverseTransformPoint(Target);
        Vector3 localTargetNorm = localTarget.normalized;

        //sensor.AddObservation(body.rotation.eulerAngles.y);
        float x = body.rotation.eulerAngles.x;
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
        sensor.AddObservation(Mathf.Atan(z/10f));
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

        RaycastHit hit;
        for (int i = -10; i <= 2; i++)
        {
            for (int j = -8; j <= 8; j++)
            {
                Vector3 p1 = body.position;
                Vector3 pt = new Vector3(i * 0.25f, 1, j * 0.25f);
                pt = Quaternion.Euler(0, body.rotation.eulerAngles.y, 0) * pt;
                
                p1 = p1 + pt;
                if (Physics.SphereCast(p1, 0.1f, new Vector3(0,-1,0), out hit, raycastDistance, layerMask))
                {
                    //print(distance + " " + hit.point + " " + hit.distance);
                    //print((float)(hit.distance / raycastDistance) + " " + ((hit.collider.tag == "Lava") ? 1f : 0f) + " " + ((hit.collider.tag == "Obstacle") ? 1f : 0f));
                    //Debug.DrawLine(p1, hit.point, Color.yellow, 0.1f);
                    sensor.AddObservation(hit.distance / raycastDistance);
                    sensor.AddObservation(1f);
                    //sensor.AddObservation(hit.collider.tag == "Climb");
                    //sensor.AddObservation(hit.collider.tag == "Obstacle");
                    //sensor.AddObservation(hit.collider.tag == "Ball");
                }
                else
                {
                    //Debug.DrawLine(p1, p1 + new Vector3(0,-raycastDistance,0), Color.white, 0.1f);
                    sensor.AddObservation(1f);
                    sensor.AddObservation(0f);
                    //sensor.AddObservation(0f);
                    //sensor.AddObservation(0f);
                }
            }
        }
        //print("sensor "+ sensor);

        // 9 + 221 * 2 = 451
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
        SetRandomTarget(true);
    }

    public void SetRandomTarget(bool turn)
    {
        /*for (int i = 0; i < 50; i++)
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
        }*/

        //Debug.Log("setting target to zero");
        //SetTarget(Vector3.zero);
        SetTarget(GetParentArena().checkpoints[(checkpointNumber + 1) % GetParentArena().checkpoints.Count].position, turn);


    }

    private void drawBox(Vector3 center, Vector3 halfSize, Color color)
    {
        Debug.DrawLine(center, center + new Vector3(halfSize.x,halfSize.y,halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(-halfSize.x, halfSize.y, halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(halfSize.x, halfSize.y, -halfSize.z), color, 1.0f);
        Debug.DrawLine(center, center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), color, 1.0f);
    }

    private void SetTarget(Vector3 LocalTarget, bool turn)
    {
        Target = LocalTarget;
        distance = GetDistance();
        if (turn)
        {
            float angle = Mathf.Atan2(Target.x - transform.position.x, Target.z - transform.position.z) * Mathf.Rad2Deg;
            //Debug.Log("rotating to angle" + angle);
            transform.rotation = Quaternion.Euler(new Vector3(0, angle + 90 + Random.Range(-10f, 10f), 0));
        }
        
    }

    private float GetDistance()
    {
        //return transform.Find("Body").position.x;
        float localDistance = Vector3.Distance(body.position, Target);
        //Debug.Log("localdistance " + localDistance);
        return localDistance;
    }
}
