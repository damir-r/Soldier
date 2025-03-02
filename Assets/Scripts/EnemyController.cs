using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float wanderRadius;
    public float wanderTimer;
    public GameObject player;

    private Vector3 lastKnownPlayerPosition;
    private NavMeshAgent agent;
    private float timer;
    private bool followingPlayer;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            throw new System.Exception("Cannot find player (tag search failed)");
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        Vector3 direction = player.transform.position - transform.position;
        Ray ray = new Ray(transform.position, direction.normalized);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit);
        if (followingPlayer)
        {
            //Debug.Log("Following player");
            if (hitSomething)
            {
                if (hit.transform != player.transform)
                {
                    //Debug.Log("Lost player");
                    followingPlayer = false;
                    if (lastKnownPlayerPosition.magnitude == 0)
                        lastKnownPlayerPosition = player.transform.position;

                    //Debug.DrawRay(transform.position, direction, Color.red);
                }
                else
                {
                    agent.destination = player.transform.position;
                    //Debug.DrawRay(transform.position, direction, Color.green);
                }
            }
            else if (lastKnownPlayerPosition.magnitude == 0)
            {
                //Debug.Log("Lost player");
                followingPlayer = false;
                lastKnownPlayerPosition = player.transform.position;
            }

            timer = 0;
            
        } else if (lastKnownPlayerPosition.magnitude > 0)
        {
            //Debug.Log("Going to last known position");
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 5)
            {
                //Debug.Log("Arrived at last known position");
                if (hitSomething && hit.transform == player.transform)
                {
                    //Debug.Log("Found player at last known position");
                    followingPlayer = true;
                }
                lastKnownPlayerPosition = Vector3.zero;
            }
            agent.destination = lastKnownPlayerPosition;
            timer = 0;
        } else
        {
            if (hitSomething && hit.transform == player.transform)
            {
                //Debug.Log("Found player wandering");
                followingPlayer = true;
            }
            else
            {
                timer += Time.deltaTime;

                if (timer >= wanderTimer)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    agent.destination = newPos;
                    timer = 0;
                }
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
