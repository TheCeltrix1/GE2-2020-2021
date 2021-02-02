using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBoid : MonoBehaviour
{
    
    public Vector3 velocity;
    public float speed;
    public Vector3 acceleration;
    public Vector3 force;
    public float maxSpeed = 5;
    public float maxForce = 10;
    public bool loopPathing = false;

    private List<Vector3> _waypoints = new List<Vector3>();
    private int _waypointLength = 4;
    private int _currentWaypoint;
    public float waypointPos = 20;

    public float mass = 1;

    public bool seekEnabled = true;
    public bool fleeEnabled = false;
    public bool arriveEnabled = false;

    public Transform targetTransform;
    public Vector3 target;

    public float slowingDistance = 10;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + velocity);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + acceleration);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + force * 10);

        if (arriveEnabled)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetTransform.position, slowingDistance);
        }

        for (int i = 0; i < _waypoints.Count; i++)
        {
            Gizmos.color = Color.cyan;
            if (i < _waypoints.Count-1)
            {
                Gizmos.DrawLine(_waypoints[i], _waypoints[i + 1]);
            }
            else if (loopPathing)
            {
                Gizmos.DrawLine(_waypoints[0],_waypoints[_waypoints.Count-1]);
            }
        }

    }

    public void Awake()
    {
        GenerateWaypoints(_waypoints);
        Debug.Log("NYAA");
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        Vector3 desired = toTarget.normalized * maxSpeed;

        return (desired - velocity);
    } 

    public Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;
        float ramped = (dist / slowingDistance) * maxSpeed;
        float clamped = Mathf.Min(ramped, maxSpeed);
        Vector3 desired = (toTarget / dist) * clamped;

        return desired - velocity;
    }

    public Vector3 CalculateForce()
    {
        Vector3 f = Vector3.zero;
        if (seekEnabled)
        {
            if (targetTransform != null)
            {
                target = targetTransform.position;
            }
            f += Seek(target);
        }

        if (arriveEnabled)
        {
            if (targetTransform != null)
            {
                target = targetTransform.position;                
            }
            f += Arrive(target);
        }

        if (fleeEnabled)
        {
            if (targetTransform != null)
            {
                target = targetTransform.position;
            }
            f += Flee(target);
        }

        return f;
    }

    public Vector3 Flee(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        Vector3 desired = -toTarget.normalized * maxSpeed;

        return (desired - velocity);
    }

    public void GenerateWaypoints(List<Vector3> waypointies)
    {
        for (int i = 0; i < _waypointLength; i++)
        {
            Vector3 point = new Vector3(Random.Range(-waypointPos,waypointPos), Random.Range(-waypointPos, waypointPos), Random.Range(-waypointPos, waypointPos));
            _waypoints.Add(point);
        }
        _currentWaypoint = 0;
        target = _waypoints[0];
    }

    void Update()
    {
        force = CalculateForce();
        acceleration = force / mass;
        velocity = velocity + acceleration * Time.deltaTime;
        transform.position = transform.position + velocity * Time.deltaTime;
        speed = velocity.magnitude;
        if (speed > 0)
        {
            transform.forward = velocity;
        }

        if (Vector3.Distance(target,transform.position) <= 0.5f)
        {
            if (_currentWaypoint < _waypointLength - 1) {
                _currentWaypoint += 1;
                target = _waypoints[_currentWaypoint];
            }
            else if (loopPathing)
            {
                target = _waypoints[0];
            }
            else
            {
                target = transform.position;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Avoid")
        {
            target = other.gameObject.transform.position;
            fleeEnabled = true;
            seekEnabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Avoid")
        {
            target = _waypoints[_currentWaypoint];
            fleeEnabled = false;
            seekEnabled = true;
        }
    }
}
