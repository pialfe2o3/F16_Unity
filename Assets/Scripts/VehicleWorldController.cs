// VehicleController.cs
// 额外的车辆控制器脚本，提供更多Unity相关功能
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem exhaustSmoke;
    public ParticleSystem dustParticles;
    public AudioSource engineSound;

    [Header("Camera")]
    public Camera followCamera;
    public Vector3 cameraOffset = new Vector3(0, 5, -10);

    private VehicleWorldManager vehicleManager;
    private Rigidbody rb;

    void Start()
    {
        vehicleManager = FindObjectOfType<VehicleWorldManager>();
        rb = GetComponent<Rigidbody>();

        // 如果没有Rigidbody，车辆变换由DLL控制
        if (rb != null)
        {
            rb.isKinematic = true; // 设置为运动学，由外部控制
        }

        SetupCamera();
    }

    void SetupCamera()
    {
        if (followCamera != null)
        {
            followCamera.transform.SetParent(transform);
            followCamera.transform.localPosition = cameraOffset;
            followCamera.transform.LookAt(transform);
        }
    }

    void Update()
    {
        UpdateEffects();
    }

    void UpdateEffects()
    {
        // 根据车辆状态更新特效
        float speed = GetVehicleSpeed();

        // 引擎声音
        if (engineSound != null)
        {
            engineSound.pitch = Mathf.Lerp(0.5f, 2f, Mathf.Abs(speed) / 50f);
            engineSound.volume = Mathf.Lerp(0.1f, 0.8f, Mathf.Abs(speed) / 50f);
        }

        // 尾气特效
        if (exhaustSmoke != null)
        {
            var emission = exhaustSmoke.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 50f, Mathf.Abs(speed) / 50f);
        }

        // 灰尘特效
        if (dustParticles != null && speed > 5f)
        {
            if (!dustParticles.isPlaying)
                dustParticles.Play();

            var emission = dustParticles.emission;
            emission.rateOverTime = Mathf.Lerp(20f, 100f, speed / 50f);
        }
        else if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }

    float GetVehicleSpeed()
    {
        // 计算速度 - 可以通过比较前一帧位置获得
        return Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
    }

    private Vector3 lastPosition;

    void LateUpdate()
    {
        lastPosition = transform.position;
    }
}