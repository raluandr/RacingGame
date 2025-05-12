using UnityEngine;

public class LapSystem : MonoBehaviour
{
    public int maxLaps;
    private int currentLap;

    private void OnTriggerEnter(Collider other) 
    {
        OpponentCar opponentCar = other.GetComponent<OpponentCar>();
        CarController playerCar = other.GetComponent<CarController>();

        if(opponentCar != null)
        {
            opponentCar.IncreaseLap();
            CheckRaceCompletion(opponentCar);

        }

        if(playerCar != null)
        {
            playerCar.IncreaseLap();
            CheckRaceCompletion(playerCar);
        }
    }

    private void CheckRaceCompletion(OpponentCar opponentCar)
    {
        if(opponentCar.currentLap == maxLaps)
        {
            EndMission(false);
        }
    }

    private void CheckRaceCompletion(CarController playerCar)
    {
        if(playerCar.currentLap == maxLaps)
        {
            EndMission(true);
        }
    }


    void EndMission(bool success)
    {
        if(success)
        {
            Debug.Log("Player Wins");
        }
        else
        {
            Debug.Log("Player Lose");
        }
    }
}
