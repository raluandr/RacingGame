using UnityEngine;

public class OpponentCar : MonoBehaviour
{
    [Header("Car Engine")]
    public float maxSpeed;
    public float currentSpeed;
    public float acceleration = 1f;
    public float turningSpeed = 30f;
    public float breakSpeed = 12f;

    [Header("Destination Var")]
    public Vector3 destination;
    public bool destinationReached;

    private Rigidbody _rb;

    [Header("Respawn")]
    public float respawnTimer;
    public const float RespawnTimeThreshold = 10f;

    [Header("Lap")]
    public int maxLaps;
    public int currentLap;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        maxLaps = FindAnyObjectByType<LapSystem>().maxLaps;

        // Setăm un punct de testare în fața mașinii + viteza inițială random
        LocateDestination(transform.position + transform.forward * 50f);
        ResetAcceleration();
    }

    void Update()
    {
        Drive();

        // Dacă nu am ajuns la destinație, măsurăm timpul blocării
        if (!destinationReached)
        {
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= RespawnTimeThreshold)
            {
                RespawnAtDestination();
            }
        }
        else
        {
            respawnTimer = 0f;
        }
    }

    void Drive()
    {
        if (!destinationReached)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;

            if (distance >= breakSpeed)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);

                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
                _rb.linearVelocity = transform.forward * currentSpeed;
            }
            else
            {
                destinationReached = true;
                _rb.linearVelocity = Vector3.zero;
            }
        }
    }

    void RespawnAtDestination()
    {
        respawnTimer = 0f;
        currentSpeed = 5f;
        transform.position = destination;
        destinationReached = false;
    }

    public void LocateDestination(Vector3 newDestination)
    {
        destination = newDestination;
        destinationReached = false;
    }

    public void ResetAcceleration()
    {
        currentSpeed = Random.Range(38f, 46f);
        acceleration = Random.Range(3.5f, 5f);
    }

    public void IncreaseLap()
    {
        currentLap++;
        Debug.Log($"{gameObject.name} Lap: {currentLap}");
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb2 = collision.rigidbody;
        if (rb2 == null) return;

        float e = 0.5f; // coeficient de elasticitate

        Vector3 v1 = _rb.linearVelocity;
        Vector3 v2 = rb2.linearVelocity;
        float m1 = _rb.mass;
        float m2 = rb2.mass;

        Vector3 normal = collision.GetContact(0).normal;
        Vector3 relativeVelocity = v1 - v2;
        float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

        if (velocityAlongNormal > 0)
            return;

        float impulseMagnitude = -(1 + e) * velocityAlongNormal;
        impulseMagnitude /= (1 / m1 + 1 / m2);

        Vector3 impulse = impulseMagnitude * normal;

        _rb.linearVelocity = v1 + (impulse / m1);
        rb2.linearVelocity = v2 - (impulse / m2);
    }
}
