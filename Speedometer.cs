using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    [Header("speedometer")]
    public Rigidbody carRb;
    public float maxSpeed;
    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    public Text speedLabel;
    public RectTransform arrowImg;

    private float speed;

    void Update()
    {
        if (carRb == null) return;

        speed = carRb.linearVelocity.magnitude * 3.6f;

        if (speedLabel != null)
            speedLabel.text = ((int)speed) + " km/h";

        if (arrowImg != null)
        {
            float speedRatio = Mathf.Clamp01(speed / maxSpeed);
            arrowImg.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speedRatio));
        }
    }
}