using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardPatrolBehaviour : MonoBehaviour
{
    private enum PatrolTypes
    {
        Looping,
        Linear
    }

    [Tooltip("'Looping' means the guard will move to the 1st waypoint after reaching the last. 'Linear' means the guard goes backwards once reaching the last waypoint.")]
    [SerializeField] private PatrolTypes currentPatrolType = PatrolTypes.Looping;

    private GuardStateMachine myStateMachine;

    private PlayerDetection myPlayerDetection;

    [Tooltip("The waypoints that make up the guard's route. The order here translates to the game. Don't leave any empty list entries.")]
    [SerializeField] private List<Transform> waypoints;

    [Tooltip("How often the guard check whether they see the player while patrolling.")]
    [SerializeField] private float detectionCheckFrequency = 0.1f;

    private int currentWaypoint = 0;

    private bool isWaypointAscending = true;

    private void Awake()
    {
        myStateMachine = GetComponent<GuardStateMachine>();

        myPlayerDetection = GetComponent<PlayerDetection>();
    }

    private void OnEnable()
    {
        myStateMachine.OnSwithToPatrol += GetNewDestination;

        myStateMachine.OnSwithToPatrol += PlayerDetection;
    }

    private void OnDisable()
    {
        myStateMachine.OnSwithToPatrol -= GetNewDestination;

        myStateMachine.OnSwithToPatrol -= PlayerDetection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!myStateMachine.GetCurrentGuardState().Equals(GuardStateMachine.GuardState.Patrolling))
        {
            return;
        }

        if(other.transform.Equals(waypoints[currentWaypoint]))
        {
            GetNewDestination();
        }
    }

    private void PlayerDetection()
    {
        StartCoroutine(PlayerDetectionBehaviour());
    }

    private void GetNewDestination()
    {
        if(waypoints.Count < 0)
        {
            return;
        }

        if(currentPatrolType.Equals(PatrolTypes.Looping))
        {
            if(currentWaypoint + 1 < waypoints.Count)
            {
                currentWaypoint++;
            }
            else
            {
                currentWaypoint = 0;
            }
        }
        else if(currentPatrolType.Equals(PatrolTypes.Linear))
        {
            if(isWaypointAscending)
            {
                if(currentWaypoint + 1 < waypoints.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    isWaypointAscending = false;

                    currentWaypoint--;
                }
            }
            else
            {
                if(currentWaypoint - 1 >= 0)
                {
                    currentWaypoint--;
                }
                else
                {
                    isWaypointAscending = true;

                    currentWaypoint++;
                }
            }
        }

        myStateMachine.GetAgent().SetDestination(waypoints[currentWaypoint].position);
    }

    private IEnumerator PlayerDetectionBehaviour()
    {
        yield return new WaitForSeconds(detectionCheckFrequency);

        if(myPlayerDetection.IsPlayerInSight())
        {
            myStateMachine.SetState(GuardStateMachine.GuardState.Chasing);
        }
        else
        {
            StartCoroutine(PlayerDetectionBehaviour());
        }
    }
}