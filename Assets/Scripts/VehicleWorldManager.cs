// VehicleWorldManager.cs
// Unity车辆世界管理器，对应C++的VehicleWorld类
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class VehicleWorldManager : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public string sceneFilePath = "Assets/Plugins/scenes/tank.xml";
    public GameObject vehicleGameObject;

    [Header("Controls")]
    [SerializeField] private float throttleInput = 0f;
    [SerializeField] private float steeringInput = 0f;
    [SerializeField] private float brakeInput = 0f;
    [SerializeField] private int currentGear = 0;

    // DLL导入声明
    private IntPtr worldPtr;
    private IntPtr vehiclePtr;

    // DLL函数声明
    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr VehicleWorld_Create();

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void VehicleWorld_Destroy(IntPtr world);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void VehicleWorld_Load(IntPtr world, string filePath);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void VehicleWorld_Update(IntPtr world, float dt);
        
    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr VehicleWorld_GetVehicle(IntPtr world, int index);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_SetThrottleInput(IntPtr vehicle, float throttle);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_SetSteeringInput(IntPtr vehicle, float steering);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_SetBrakeInput(IntPtr vehicle, float brake);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_SetGear(IntPtr vehicle, int gear);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern int Vehicle_GetGear(IntPtr vehicle);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_GetPosition(IntPtr vehicle, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Vehicle_GetRotation(IntPtr vehicle, out float x, out float y, out float z, out float w);

    void Start()
    {
        InitializeVehicleWorld();
    }

    void InitializeVehicleWorld()
    {
        // 创建车辆世界
        worldPtr = VehicleWorld_Create();
        if (worldPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to create VehicleWorld");
            return;
        }

        // 加载场景
        try
        {
            VehicleWorld_Load(worldPtr, sceneFilePath);
            Debug.Log($"Loaded scene: {sceneFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
            return;
        }

        // 获取第一辆车
        vehiclePtr = VehicleWorld_GetVehicle(worldPtr, 0);
        if (vehiclePtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to get vehicle");
            return;
        }

        Debug.Log("Vehicle world initialized successfully");
    }

    void Update()
    {
        if (worldPtr == IntPtr.Zero || vehiclePtr == IntPtr.Zero)
            return;

        // 处理输入
        HandleInput();

        // 更新物理世界
        VehicleWorld_Update(worldPtr, Time.deltaTime);

        // 同步Unity GameObject位置
        SyncVehicleTransform();
    }

    void HandleInput()
    {
        // 数字键切换档位
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                currentGear = i;
                Vehicle_SetGear(vehiclePtr, currentGear);
                Debug.Log($"Gear changed to: {currentGear}");
            }
        }

        // WASD控制
        float newThrottleInput = 0f;
        float newSteeringInput = 0f;
        float newBrakeInput = 0f;

        // 油门控制
        if (Input.GetKey(KeyCode.W))
            newThrottleInput = 1f;
        else if (Input.GetKey(KeyCode.S))
            newThrottleInput = -1f;

        // 转向控制
        if (Input.GetKey(KeyCode.A))
            newSteeringInput = 20f;
        else if (Input.GetKey(KeyCode.D))
            newSteeringInput = -20f;

        // 刹车控制
        if (Input.GetKey(KeyCode.E))
            newBrakeInput = 1f;

        // 应用输入
        if (newThrottleInput != throttleInput)
        {
            throttleInput = newThrottleInput;
            Vehicle_SetThrottleInput(vehiclePtr, throttleInput);
        }

        if (newSteeringInput != steeringInput)
        {
            steeringInput = newSteeringInput;
            Vehicle_SetSteeringInput(vehiclePtr, steeringInput);
        }

        if (newBrakeInput != brakeInput)
        {
            brakeInput = newBrakeInput;
            Vehicle_SetBrakeInput(vehiclePtr, brakeInput);
        }
    }

    void SyncVehicleTransform()
    {
        if (vehicleGameObject == null)
            return;

        // 获取车辆位置
        Vehicle_GetPosition(vehiclePtr, out float x, out float y, out float z);
        vehicleGameObject.transform.position = new Vector3(x, y, z);

        // 获取车辆旋转
        Vehicle_GetRotation(vehiclePtr, out float qx, out float qy, out float qz, out float qw);
        vehicleGameObject.transform.rotation = new Quaternion(qx, qy, qz, qw);
    }

    void OnDestroy()
    {
        if (worldPtr != IntPtr.Zero)
        {
            VehicleWorld_Destroy(worldPtr);
            worldPtr = IntPtr.Zero;
        }
    }

    // GUI显示信息
    void OnGUI()
    {
        if (vehiclePtr != IntPtr.Zero)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Current Gear: {Vehicle_GetGear(vehiclePtr)}");
            GUILayout.Label($"Throttle: {throttleInput:F2}");
            GUILayout.Label($"Steering: {steeringInput:F2}");
            GUILayout.Label($"Brake: {brakeInput:F2}");
            GUILayout.EndArea();
        }

        // 显示控制说明
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Label("Controls:");
        GUILayout.Label("W/S - Throttle Forward/Backward");
        GUILayout.Label("A/D - Steering Left/Right");
        GUILayout.Label("E - Brake");
        GUILayout.Label("0-9 - Change Gear");
        GUILayout.EndArea();
    }
}
