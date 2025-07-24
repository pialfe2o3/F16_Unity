using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class testF16Controller : MonoBehaviour
{
    // Start is called before the first frame update
    private IntPtr worldPtr = IntPtr.Zero;
    private IntPtr aircraftPtr = IntPtr.Zero;

    private bool _pitchUpPressed = false;
    private bool _pitchDownPressed = false;
    private bool _rollLeftPressed = false;
    private bool _rollRightPressed = false;
    private bool _thrustPressed = false;
    private bool _yawLeftPressed = false;
    private bool _yawRightPressed = false;

    private float _pitchInput;
    private float _rollInput;
    private float _yawInput;
    private float _thrustInput;
    private float _upInput;

    private float[] _pidIntegral = new float[3];
    private float[] _pidPrevError = new float[3];

    //帧计数器
    private int frameCount = 0;

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr RigidWorld_Create();

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Destroy(IntPtr world);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Load(IntPtr world, string filePath);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Save(IntPtr world, string filePath);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Update(IntPtr world, float dt);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern int RigidWorld_GetRigidbodyCount(IntPtr world);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr RigidWorld_GetRigidbody(IntPtr world, int idx);

    //战机rigidworld基础部分导入
    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr AircraftWorld_Create();

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AircraftWorld_Destroy(IntPtr world);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AircraftWorld_Load(IntPtr world, [MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AircraftWorld_Update(IntPtr world, float dt);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr AircraftWorld_GetAircraft(IntPtr world);

    //战机控制接口
    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr F16_Create();

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_Destroy(IntPtr f16);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_ColdStart(IntPtr f16);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_HotStart(IntPtr f16);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetCurrentStateWorld(
        IntPtr f16, float x, float y, float z, float vx, float vy, float vz,
        float roll_RPS, float yaw_RPS, float pitch_RPS,
        float roll, float yaw, float pitch);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetAtomosphere(
        IntPtr f16, double h, double t, double a, double ro, double p,
        double wind_vx, double wind_vy, double wind_vz);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetEngineThrottle(IntPtr f16, float throttle);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetRollControl(IntPtr f16, float aileron);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetPitchControl(IntPtr f16, float elevator);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetYawControl(IntPtr f16, float rudder);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_SetZero(IntPtr f16);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetPosition(IntPtr f16, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetRotation(IntPtr f16, out float x, out float y, out float z, out float w);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetVelocity(IntPtr f16, out float vx, out float vy, out float vz);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetAngularVelocity(IntPtr f16, out float vx, out float vy, out float vz);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void getParam(IntPtr f16, uint index, out float tmp);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_Update(IntPtr f16, float dt);


    // DLL导入刚体位置/旋转
    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetPosition(IntPtr rigidbody, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetRotation(IntPtr rigidbody, out float x, out float y, out float z, out float w);
    void Start()
    {
        //接入物理引擎
        // 1. 创建世界
        worldPtr = AircraftWorld_Create();

        // 2. 加载场景（如有需要）
        AircraftWorld_Load(worldPtr, "Assets/Plugins/scenes/aircraft.xml");

        // 3. 获取飞机对象
        aircraftPtr = AircraftWorld_GetAircraft(worldPtr);

        F16_SetCurrentStateWorld(aircraftPtr, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        F16_SetRollControl(aircraftPtr, 0);

        F16_SetPitchControl(aircraftPtr, 0);
    }


    public static void ApplyPIDControl(
    IntPtr f16Instance,
    bool hasManualInput,
    ref float[] integral,
    ref float[] prevError)
    {
        // PID参数配置
        float[] Kp = { 5.0f, 4.0f, 2.0f };
        float[] Ki = { 0.1f, 0.05f, 0.01f };
        float[] Kd = { 0.5f, 0.3f, 0.1f };
        const float dt = 0.02f;

        if (!hasManualInput)
        {
            // 获取当前角速度
            F16_GetAngularVelocity(f16Instance, out float p, out float q, out float r);
            float[] rates = { p, q, r };
            float[] commands = new float[3];

            for (int i = 0; i < 3; i++)
            {
                float error = -rates[i];
                integral[i] = Math.Clamp(integral[i] + error * dt, -1f, 1f);
                float derivative = (error - prevError[i]) / dt;
                prevError[i] = error;

                commands[i] = Math.Clamp(
                    Kp[i] * error + Ki[i] * integral[i] + Kd[i] * derivative,
                    -1f, 1f);
            }

            // 应用控制指令
            Debug.Log(commands[0]);
            F16_SetRollControl(f16Instance, commands[0]);
            F16_SetPitchControl(f16Instance, commands[1]);
            //F16_SetRollControl(f16Instance, commands[0]);
        }
        else
        {
            Array.Clear(integral, 0, 3);
            Array.Clear(prevError, 0, 3);
        }
    }

    void HandleInput()
    {
        // 重置控制（无输入时）
        if (!_pitchUpPressed && !_pitchDownPressed &&
            !_rollLeftPressed && !_rollRightPressed &&
            !_yawLeftPressed && !_yawRightPressed)
        {
            ApplyPIDControl(aircraftPtr, false, ref _pidIntegral, ref _pidPrevError);
        }

        // 俯仰控制（Pitch）
        if (_pitchUpPressed)
        {
            F16_SetPitchControl(aircraftPtr, 1);  // 机头上仰
        }
        if (_pitchDownPressed)
        {
            F16_SetPitchControl(aircraftPtr, -1); // 机头下俯
        }

        // 横滚控制（Roll）
        if (_rollLeftPressed)
        {
            F16_SetRollControl(aircraftPtr, -1);  // 左滚
        }
        if (_rollRightPressed)
        {
            F16_SetRollControl(aircraftPtr, 1);  // 右滚
        }

        // 油门控制
        if (_thrustPressed)
        {
            F16_SetEngineThrottle(aircraftPtr, 1); // 最大油门
        }
        else
        {
            F16_SetEngineThrottle(aircraftPtr, 0); // 无油门
        }

        // 偏航控制（Yaw）
        if (_yawLeftPressed)
        {
            F16_SetYawControl(aircraftPtr, -1);  // 左偏航
        }
        if (_yawRightPressed)
        {
            F16_SetYawControl(aircraftPtr, 1);   // 右偏航
        }
    }


    void GetFlightInputs()
    {
        // 检测按键按下状态（Key Press）
        _pitchUpPressed = Input.GetKey(KeyCode.A);
        _pitchDownPressed = Input.GetKey(KeyCode.D);
        _rollLeftPressed = Input.GetKey(KeyCode.S);
        _rollRightPressed = Input.GetKey(KeyCode.W);
        _thrustPressed = Input.GetKey(KeyCode.Space);
        _yawLeftPressed = Input.GetKey(KeyCode.Q);
        _yawRightPressed = Input.GetKey(KeyCode.E);
    }
    // Update is called once per frame
    void Update()
    {
        GetFlightInputs();

        HandleInput();

        if (frameCount++ >= 20)
        {

            float dt = Time.deltaTime * 20;
            //Debug.Log(dt);

            // 1. 更新世界
            AircraftWorld_Update(worldPtr, dt);

            float x, y, z;
            F16_GetPosition(aircraftPtr, out x, out y, out z);
            float qx, qy, qz, qw;
            F16_GetRotation(aircraftPtr, out qx, out qy, out qz, out qw);

            // 3. 更新Unity中飞机模型的位置和旋转
            //Debug.Log(qx);
            //Debug.Log(qy);
            //Debug.Log(qz);
            //Debug.Log(qw);
            transform.position = new Vector3(x, y, z);
            Quaternion y90 = Quaternion.Euler(0, 0, 0);
            Quaternion model = new Quaternion(qz, qy, qx, qw);
            //Debug.Log(model);
            transform.rotation = y90 * model;

            frameCount = 0;
        }
    }
}
