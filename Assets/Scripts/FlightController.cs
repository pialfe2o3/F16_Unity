using System.Runtime.InteropServices;
using System;
using UnityEngine;

/// <summary>
/// 控制飞行器的飞行以及驾驶舱内的视角。
/// </summary>
public class FlightController : MonoBehaviour
{
    [Header("飞行速度设置")]
    [Tooltip("飞行器前进速度。")]
    public float forwardSpeed = 25f;

    [Tooltip("飞行器俯仰（抬头/低头）的速度。")]
    public float pitchSpeed = 100f;

    [Tooltip("飞行器翻转（向左/向右滚转）的速度。")]
    public float rollSpeed = 100f;

    [Tooltip("飞行器偏航（向左/向右转弯）的速度。")]
    public float yawSpeed = 100f;

    [Tooltip("飞行器向上抬升的速度。")]
    public float upSpeed = 50f;

    [Header("视角控制设置")]
    [Tooltip("鼠标灵敏度。")]
    public float mouseSensitivity = 200f;

    [Tooltip("摄像机垂直方向（俯仰）的最大角度。")]
    public float maxPitchAngle = 80f;

    // 私有变量
    private Transform _cameraTransform;
    private float _pitchInput;
    private float _rollInput;
    private float _yawInput;
    private float _thrustInput;
    private float _upInput;

    private float[] _pidIntegral = new float[3];
    private float[] _pidPrevError = new float[3];

    private float _cameraYaw = 0f;
    private float _cameraPitch = 0f;

    private IntPtr worldPtr = IntPtr.Zero;
    private IntPtr aircraftPtr = IntPtr.Zero;
    //帧计数器
    private int frameCount = 0;
    private bool keep = true;



    //回调函数
    //public delegate void LogCallback(string message);

    //[DllImport("DampsEngineExtern")]
    //private static extern void F16_RegisterLogCallback(LogCallback callback);


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
    public static extern void getParam(IntPtr f16, uint index, out float tmp);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_Update(IntPtr f16, float dt);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetAngularVelocity(IntPtr f16, out float vx, out float vy, out float vz);

    // DLL导入刚体位置/旋转
    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetPosition(IntPtr rigidbody, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetRotation(IntPtr rigidbody, out float x, out float y, out float z, out float w);


    private static void OnDllLog(string message)
    {
        Debug.Log($"[DLL] {message}");
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
            //Debug.Log(commands[0]);
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
    void Start()
    {
        // 自动查找驾驶舱下的主摄像机
        // 请确保您的层级结构是：飞机(挂载此脚本) -> Cockpit -> Main Camera
        _cameraTransform = transform.Find("F16 Camera");

        if (_cameraTransform == null)
        {
            Debug.LogError("错误：在 'Cockpit' 子物体下没有找到名为 'Main Camera' 的摄像机！请检查层级结构和名称。");
        }
        else
        {
            // 锁定并隐藏鼠标光标，以获得更好的体验
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        //接入物理引擎
        // 1. 创建世界
        worldPtr = AircraftWorld_Create();

        // 2. 加载场景（如有需要）
        AircraftWorld_Load(worldPtr, "Assets/Plugins/scenes/aircraft.xml");

        // 3. 获取飞机对象
        aircraftPtr = AircraftWorld_GetAircraft(worldPtr);

        F16_SetCurrentStateWorld(aircraftPtr,0,3,0,0,0,0,0,0,0,0,0,0);

        F16_SetRollControl(aircraftPtr, 0);

        F16_SetPitchControl(aircraftPtr, 0);
    }

    void Update()
    {
        // 获取飞行控制输入
        GetFlightInputs();

        //// 引擎处理(模拟)
        HandleInput();

        if (Input.GetKey(KeyCode.P))
        {
            keep = false;
        }
        if (Input.GetKey(KeyCode.O))
        {
            keep = true;
        }

        if (frameCount++ >= 10)
        {
            // 处理飞行移动
            HandleMovement();
            frameCount = 0;
        }

        // 处理视角转动
        if (_cameraTransform != null)
        {
            HandleCameraRotation();
        }
    }

    /// 获取所有用于飞行的玩家输入。
    void GetFlightInputs()
    {
        // 您需要在Unity的输入管理器中设置 "Vertical", "Horizontal", "Yaw", 和 "Thrust" 轴
        _pitchInput = Input.GetAxis("Vertical");
        _rollInput = Input.GetAxis("Horizontal");
        _yawInput = Input.GetAxis("Yaw");
        _thrustInput = Input.GetAxis("Thrust");
        _upInput = Input.GetAxis("Up");
    }

    void UpdatePlaneInformation(float forwardSpeed1,float upSpeed1,float pitchSpeed1,float yawSpeed1,float rollSpeed1)
    {
        forwardSpeed = forwardSpeed1;
        upSpeed = upSpeed1;
        pitchSpeed = pitchSpeed1;
        yawSpeed = yawSpeed1;
        rollSpeed = rollSpeed1;
    }

    void HandleInput()
    {
        //在这里调用引擎，输入速度，角度等信息，得到更新后的各个速度信息
        //后续接入手柄后可以更细节地区分控制量大小
        DeviceController device = GetComponent<DeviceController>();
        if (_pitchInput == 0 && _rollInput == 0)
        {
            device.handle_up(0, 0);//控制各个舵翼
                                   // TODO:接入DampsEngine，获取信息
            ApplyPIDControl(aircraftPtr, false, ref _pidIntegral, ref _pidPrevError);
            rollSpeed = 0;
            pitchSpeed = 0;

        }
        if (_rollInput > 0.01f)
        {
            device.handle_up(4, 3);
            pitchSpeed = 100f;
            F16_SetPitchControl(aircraftPtr, -1);  // 机头上仰
        }
        if (_rollInput < -0.01f)
        {
            device.handle_up(4, 4);
            pitchSpeed = 100f;
            F16_SetPitchControl(aircraftPtr, 1); // 机头下俯
        }
        if (_pitchInput > 0.01f)
        {
            F16_SetRollControl(aircraftPtr, -1);  // 左滚
            device.handle_up(4, 1);
            rollSpeed = 100f;
        }
        if (_pitchInput < -0.01f)
        {
            F16_SetRollControl(aircraftPtr, 1);  // 右滚
            device.handle_up(4, 2);
            rollSpeed = 100f;
        }

        if (_thrustInput == 0)
        {
            device.power_push(0);//控制油门踏板
            forwardSpeed = 0;
            F16_SetEngineThrottle(aircraftPtr, 0);
        }
        if (_thrustInput > 0.01f)
        {
            device.power_push(5);
            forwardSpeed = -300f;
            F16_SetEngineThrottle(aircraftPtr, 1);
        }

        if (_yawInput == 0)
        {
            yawSpeed = 0;
        }
        if (_yawInput != 0)
        {
            yawSpeed = 60f;
        }                   
    }

    /// 每一帧通过transform简单更新飞机姿态位置，对于输入信号的处理另开一个接口，这里只接受飞机的各个速度信息做出反应
    void HandleMovement()
    {
        // 后续直接从dampsEngine中获取位置信息
        //// 应用推力向前移动
        //transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);

        //// 应用从机翼向上的升力
        //transform.Translate(Vector3.up * upSpeed * Time.deltaTime, Space.Self);

        //// 应用俯仰（上下旋转）
        //transform.Rotate(Vector3.right, _pitchInput * pitchSpeed * Time.deltaTime, Space.Self);

        //// 应用翻转（左右倾斜）
        //transform.Rotate(Vector3.forward, _rollInput * rollSpeed * Time.deltaTime, Space.Self);

        //// 应用偏航（左右转弯）
        //transform.Rotate(Vector3.up, _yawInput * yawSpeed * Time.deltaTime, Space.Self);
        float dt = Time.deltaTime*frameCount;

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
        transform.position = new Vector3(x/5, y, z);
        Quaternion y90 = Quaternion.Euler(0, 0, 0);
        Quaternion model = new Quaternion(qz, qy, qx, qw);
        if(keep)transform.rotation = y90*model;

    }



    /// 处理基于鼠标输入的摄像机视角转动。
    void HandleCameraRotation()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 累加摄像机的偏航和俯仰值
        _cameraYaw += mouseX;
        _cameraPitch -= mouseY;

        // 将垂直方向的俯仰角度限制在-maxPitchAngle到+maxPitchAngle度之间，防止摄像机翻转
        _cameraPitch = Mathf.Clamp(_cameraPitch, -maxPitchAngle, maxPitchAngle);

        // 应用旋转。这将只旋转摄像机本身，使其视角相对于飞机而改变。
        _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
    }

    //设置各角度速度大小的接口
    public void SetForwordSpeed(float s){forwardSpeed = s;}
    public void SetYawSpeed(float s) { yawSpeed = s; }
    public void SetUpSpeed(float s) { upSpeed = s; }
    public void SetRollSpeed(float s) { rollSpeed = s; }
    public void SetPitchSpeed(float s) { pitchSpeed = s; }

}