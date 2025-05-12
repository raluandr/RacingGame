using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionController : MonoBehaviour
{
    public float timeLimitInSeconds = 120f;
    private float _elapsedTime;
    private bool _missionCompleted;

    public Text timerText;
    public Text resultText;

    void Start()
    {
        resultText.text = "";
        UpdateTimerText();
    }

    void Update()
    {
        if(!_missionCompleted)
        {
            _elapsedTime += Time.deltaTime;

            if(_elapsedTime >= timeLimitInSeconds)
            {
                EndMission(false);
            }

            UpdateTimerText();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CarController carController = other.GetComponent<CarController>();

        if(carController != null)
        {
            EndMission(true);
        }
    }

    void UpdateTimerText()
    {
        float remainingTime = Mathf.Max(0f, timeLimitInSeconds - _elapsedTime);
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndMission(bool success)
    {
        _missionCompleted = true;

        if(success)
        {
            resultText.text = "You Win";
        }
        else
        {
            resultText.text = "You Lose";
        }

        Invoke("LoadMainMenuScene", 3f);
    }

    void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
