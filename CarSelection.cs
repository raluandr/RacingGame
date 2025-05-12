using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    [SerializeField] private GameObject allCarsContainer;

    private GameObject[] _allCars;
    private int _currentIndex;

    void Start()
    {
        _allCars = new GameObject[allCarsContainer.transform.childCount];

        for (int i = 0; i < allCarsContainer.transform.childCount; i++)
        {
            _allCars[i] = allCarsContainer.transform.GetChild(i).gameObject;
            _allCars[i].SetActive(false);
        }

        if (PlayerPrefs.HasKey("SelectedCarIndex"))
            _currentIndex = PlayerPrefs.GetInt("SelectedCarIndex");

        ShowCurrentCar();
    }

    void ShowCurrentCar()
    {
        foreach (GameObject car in _allCars)
            car.SetActive(false);

        _allCars[_currentIndex].SetActive(true);
    }

    public void NextCar()
    {
        _currentIndex = (_currentIndex + 1) % _allCars.Length;
        ShowCurrentCar();
    }

    public void PreviousCar()
    {
        _currentIndex = (_currentIndex - 1 + _allCars.Length) % _allCars.Length;
        ShowCurrentCar();
    }

    public void OnYesButtonClick(string sceneName)
    {
        PlayerPrefs.SetInt("SelectedCarIndex", _currentIndex);
        PlayerPrefs.Save();

        Debug.Log("Selected Car Saved");
        SceneManager.LoadScene(sceneName);
    }
}