using UnityEngine;

public class OpponentCarWaypoints : MonoBehaviour
{
    [Header("Opponent Car")]
    public OpponentCar opponentCar;
    public Waypoint currentWaypoint;

    void Start()
    {
        opponentCar.LocateDestination(currentWaypoint.GetPosition());
    }

    void Update()
    {
        if(opponentCar.destinationReached)
        {
            currentWaypoint = currentWaypoint.nextWaypoint;
            opponentCar.LocateDestination(currentWaypoint.GetPosition());
        }
    }
}
