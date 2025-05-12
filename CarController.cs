using UnityEngine;

public class CarController : MonoBehaviour
{
    // enum pentru tipurile de tractiune
    public enum CarType { FrontWheelDrive, RearWheelDrive, FourWheelDrive }
    public CarType carType = CarType.FourWheelDrive;

    // enum pentru modul de control
    public enum ControlMode { Keyboard, Button }
    public ControlMode control;

    [Header("wheel gameobject meshes")]
    // referinte la mesh-urile vizuale ale rotilor
    public GameObject frontWheelLeft, frontWheelRight, backWheelLeft, backWheelRight;

    [Header("wheel colliders")]
    // referinte la wheel colliders pentru fizica
    public WheelCollider frontWheelLeftCollider, frontWheelRightCollider, backWheelLeftCollider, backWheelRightCollider;

    [Header("movement, steering and braking")]
    private float currentSpeed;
    public float maximumMotorTorque;
    public float maximumSteeringAngle = 20f;
    public float maximumSpeed;
    public float brakePower;
    public Transform com; // center of mass

    // variabile interne
    float carSpeed, carSpeedConverted, motorTorque, tireAngle;
    float vertical, horizontal;
    bool handBrake, isDrifting;
    Rigidbody carRigidBody;

    [Header("sounds & effects")]
    // efecte vizuale si sunete
    public ParticleSystem[] smokeEffects;
    private bool smokeEffectEnabled;
    public TrailRenderer[] trailRenderers;
    public Transform brakeLightLeft, brakeLightRight;
    Material brakeLightLeftMat, brakeLightRightMat;
    Color brakeColor = new Color32(180, 0, 10, 0);
    public AudioSource engineSound;
    public AudioClip engineClip;

    [Header("lap")]
    public int maxLaps, currentLap;

    void Start()
    {
        // initializare materiale faruri frana
        brakeLightLeftMat = brakeLightLeft.GetComponent<Renderer>().material;
        brakeLightRightMat = brakeLightRight.GetComponent<Renderer>().material;
        brakeLightLeftMat.EnableKeyword("_EMISSION");
        brakeLightRightMat.EnableKeyword("_EMISSION");

        // configurare rigidbody
        carRigidBody = GetComponent<Rigidbody>();
        if (carRigidBody != null)
        {
            carRigidBody.centerOfMass = com.localPosition;
            carRigidBody.angularDamping = 1.0f;
        }

        // configurare sunet motor
        engineSound.loop = true;
        engineSound.playOnAwake = false;
        engineSound.volume = 0f;
        engineSound.pitch = 1f;
        engineSound.Play();
        engineSound.Pause();

        // setare max laps din sistemul extern de laps
        LapSystem lapSystemInstance = FindFirstObjectByType<LapSystem>();
        if (lapSystemInstance != null)
            maxLaps = lapSystemInstance.maxLaps;
    }

    void Update()
    {
        GetInputs();
        CalculateCarMovement();
        CalculateSteering();
        CheckDriftInput();
        ApplyAntiRoll(frontWheelLeftCollider, frontWheelRightCollider);
        ApplyAntiRoll(backWheelLeftCollider, backWheelRightCollider);
        HandleDrift();
        ApplyTransformToWheels();
    }

    public void MoveInput(float input) => vertical = input;
    public void SteeringInput(float input) => horizontal = input;

    void GetInputs()
    {
        // citire input tastatura doar daca e setat keyboard
        if (control == ControlMode.Keyboard)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }
    }

    void CalculateCarMovement()
    {
        carSpeed = carRigidBody.linearVelocity.magnitude;
        carSpeedConverted = Mathf.Round(carSpeed * 3.6f);
        handBrake = Input.GetKey(KeyCode.Space);

        if (handBrake)
        {
            motorTorque = 0;
            ApplyBrake();
            brakeLightLeftMat.SetColor("_EmissionColor", brakeColor);
            brakeLightRightMat.SetColor("_EmissionColor", brakeColor);
            EnableTrailEffect(true);
            if (!smokeEffectEnabled) { EnableSmokeEffect(true); smokeEffectEnabled = true; }
        }
        else
        {
            // logica luminilor de frana
            if (vertical >= 0)
                brakeLightLeftMat.SetColor("_EmissionColor", Color.black);
            else
                brakeLightLeftMat.SetColor("_EmissionColor", brakeColor);

            brakeLightRightMat.SetColor("_EmissionColor", brakeLightLeftMat.GetColor("_EmissionColor"));
            ReleaseBrake();

            // aplicare forta motor
            motorTorque = (carSpeedConverted < maximumSpeed) ? maximumMotorTorque * vertical : 0;
            EnableTrailEffect(false);
            if (smokeEffectEnabled) { EnableSmokeEffect(false); smokeEffectEnabled = false; }

            // logica sunet motor bazata pe viteza
            if (carSpeedConverted > 0 || handBrake)
            {
                engineSound.UnPause();
                float gearRatio = currentSpeed / maximumSpeed;
                int currentGear = Mathf.Clamp(Mathf.FloorToInt(gearRatio * 6) + 1, 1, 6);
                float pitch = Mathf.Lerp(0.5f, 1.0f, 0.5f + 0.5f * (carSpeedConverted / maximumSpeed)) * currentGear;
                float volume = 0.2f + 0.8f * (carSpeedConverted / maximumSpeed);
                engineSound.pitch = pitch;
                engineSound.volume = volume;
            }
            else
            {
                engineSound.UnPause();
                engineSound.pitch = 0.5f;
                engineSound.volume = 0.2f;
            }
        }

        // aplicare forta motor
        ApplyMotorTorque();

        // simulare coliziune (model simplificat de ec de ordin 2)
        // f = m * a => a = f / m
        // se poate extinde cu forta impact = masa * diferenta viteza / timp
        if (carRigidBody.linearVelocity.magnitude > maximumSpeed)
        {
            Vector3 dampingForce = -carRigidBody.linearVelocity.normalized * (0.5f * brakePower);
            carRigidBody.AddForce(dampingForce, ForceMode.Force);

        }
    }

    void CheckDriftInput() => isDrifting = Input.GetKey(KeyCode.LeftShift);

    void HandleDrift()
    {
        float normalGrip = 2.4f, driftGrip = 0.7f;
        WheelFrictionCurve leftFriction = backWheelLeftCollider.sidewaysFriction;
        WheelFrictionCurve rightFriction = backWheelRightCollider.sidewaysFriction;
        float grip = isDrifting ? driftGrip : normalGrip;
        leftFriction.stiffness = grip;
        rightFriction.stiffness = grip;
        backWheelLeftCollider.sidewaysFriction = leftFriction;
        backWheelRightCollider.sidewaysFriction = rightFriction;
        EnableTrailEffect(isDrifting);
        if (isDrifting && !smokeEffectEnabled) { EnableSmokeEffect(true); smokeEffectEnabled = true; }
        else if (!isDrifting && smokeEffectEnabled) { EnableSmokeEffect(false); smokeEffectEnabled = false; }
    }

    void ApplyAntiRoll(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        // logica barei anti-roll bazata pe deplasare suspensie
        WheelHit hit;
        float travelL = 1.0f, travelR = 1.0f;
        bool groundedL = leftWheel.GetGroundHit(out hit);
        if (groundedL) travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        bool groundedR = rightWheel.GetGroundHit(out hit);
        if (groundedR) travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        float antiRollForce = (travelL - travelR) * 3000f;
        if (groundedL) carRigidBody.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (groundedR) carRigidBody.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
    }

    void CalculateSteering()
    {
        tireAngle = maximumSteeringAngle * horizontal;
        frontWheelLeftCollider.steerAngle = tireAngle;
        frontWheelRightCollider.steerAngle = tireAngle;
    }

    void ApplyMotorTorque()
    {
        // aplicare motor in functie de tipul masinii
        if (carType == CarType.FrontWheelDrive)
        {
            frontWheelLeftCollider.motorTorque = motorTorque;
            frontWheelRightCollider.motorTorque = motorTorque;
        }
        else if (carType == CarType.RearWheelDrive)
        {
            backWheelLeftCollider.motorTorque = motorTorque;
            backWheelRightCollider.motorTorque = motorTorque;
        }
        else
        {
            frontWheelLeftCollider.motorTorque = motorTorque;
            frontWheelRightCollider.motorTorque = motorTorque;
            backWheelLeftCollider.motorTorque = motorTorque;
            backWheelRightCollider.motorTorque = motorTorque;
        }
    }

    void ApplyBrake()
    {
        // aplicare frana pe toate rotile
        frontWheelLeftCollider.brakeTorque = brakePower;
        frontWheelRightCollider.brakeTorque = brakePower;
        backWheelLeftCollider.brakeTorque = brakePower;
        backWheelRightCollider.brakeTorque = brakePower;
    }

    void ReleaseBrake()
    {
        // resetare frana
        frontWheelLeftCollider.brakeTorque = 0;
        frontWheelRightCollider.brakeTorque = 0;
        backWheelLeftCollider.brakeTorque = 0;
        backWheelRightCollider.brakeTorque = 0;
    }

    public void ApplyTransformToWheels()
    {
        // aplicare pozitie si rotatie la mesh-uri dupa collider
        frontWheelLeftCollider.GetWorldPose(out Vector3 posL, out Quaternion rotL);
        frontWheelLeft.transform.position = posL;
        frontWheelLeft.transform.rotation = rotL;

        frontWheelRightCollider.GetWorldPose(out Vector3 posR, out Quaternion rotR);
        frontWheelRight.transform.position = posR;
        frontWheelRight.transform.rotation = rotR;

        backWheelLeftCollider.GetWorldPose(out Vector3 posBL, out Quaternion rotBL);
        backWheelLeft.transform.position = posBL;
        backWheelLeft.transform.rotation = rotBL;

        backWheelRightCollider.GetWorldPose(out Vector3 posBR, out Quaternion rotBR);
        backWheelRight.transform.position = posBR;
        backWheelRight.transform.rotation = rotBR;
    }

    private void EnableSmokeEffect(bool enable)
    {
        foreach (ParticleSystem smoke in smokeEffects)
            if (enable) smoke.Play(); else smoke.Stop();
    }

    private void EnableTrailEffect(bool enable)
    {
        foreach (TrailRenderer trail in trailRenderers)
            trail.emitting = enable;
    }

    public void IncreaseLap()
    {
        currentLap++;
        Debug.Log(gameObject.name + " lap: " + currentLap);
    }
}
