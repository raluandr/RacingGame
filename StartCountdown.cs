using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartCountdown : MonoBehaviour
{
    public Text[] countdownTexts;
    public float countdownTime = 5f;
    private CarController[] playerCars;
    private OpponentCar[] opponentCars;
    private OpponentCarWaypoints[] waypoints;

    void Awake()
    {
        playerCars = FindObjectsByType<CarController>(FindObjectsSortMode.None);
        opponentCars = FindObjectsByType<OpponentCar>(FindObjectsSortMode.None);
        waypoints = FindObjectsByType<OpponentCarWaypoints>(FindObjectsSortMode.None);

        StartCoroutine(StartCountdownRoutine());
    }

    IEnumerator StartCountdownRoutine()
    {
        DisableScripts();

        float currentTime = countdownTime;

        while (currentTime > 0)
        {
            UpdateCountdownText(currentTime);
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        EnableScripts();

        UpdateCountdownText("GO");
        yield return new WaitForSeconds(1f);
        SetCountdownTextActive(false);
    }

    void DisableScripts()
    {
        foreach (OpponentCar opponentCar in opponentCars)
            opponentCar.enabled = false;

        foreach (OpponentCarWaypoints waypoint in waypoints)
            waypoint.enabled = false;

        foreach (CarController playerCar in playerCars)
            playerCar.enabled = false;
    }

    void EnableScripts()
    {
        foreach (OpponentCar opponentCar in opponentCars)
            opponentCar.enabled = true;

        foreach (OpponentCarWaypoints waypoint in waypoints)
            waypoint.enabled = true;

        foreach (CarController playerCar in playerCars)
            playerCar.enabled = true;
    }

    void UpdateCountdownText(string text)
    {
        foreach (Text countdownText in countdownTexts)
            countdownText.text = text;
    }

    void UpdateCountdownText(float time)
    {
        foreach (Text countdownText in countdownTexts)
            countdownText.text = time.ToString("0");
    }

    void SetCountdownTextActive(bool isActive)
    {
        foreach (Text countdownText in countdownTexts)
            countdownText.gameObject.SetActive(isActive);
    }
}
