using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Reflection;
using TMPro;

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


    [Header("Camera Zoom Settings")]
    [SerializeField] private float _zoomSpeed = 5f;          // 缩放灵敏度
    [SerializeField] private float _minFOV = 30f;           // 最小视角（放大）
    [SerializeField] private float _maxFOV = 70f;           // 最大视角（默认）
    [SerializeField] private float _zoomSmoothTime = 0.2f;  // 缩放平滑时间
    private float _targetFOV;
    private float _currentZoomVelocity;  // 用于平滑阻尼

    private Camera _mainCamera;          // 主摄像机组件

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
    private int countNum;
    private int frameCount = 0;
    private bool keep = true;

    public Transform needle1;
    public Transform needle2;
    public Transform needle3;
    public Transform needle4;
    private GameObject MPD;
    private Vector3 lastVel;


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

    //[DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    //public static extern void F16_SetZero(IntPtr f16);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetPosition(IntPtr f16, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern", CallingConvention = CallingConvention.Cdecl)]
    public static extern void F16_GetRotation(IntPtr f16, out float roll, out float pitch, out float yaw);

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



    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadLibrary(string dllPath);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    void TestDllLoad()
    {
        // 加载 DLL
        IntPtr dllHandle = LoadLibrary("DampsEngineExtern.dll");
        if (dllHandle == IntPtr.Zero)
        {
            Debug.LogError($"DLL 加载失败，错误代码: {Marshal.GetLastWin32Error()}");
            return;
        }

        // 获取函数地址
        IntPtr funcPtr = GetProcAddress(dllHandle, "F16_GetPosition");
        if (funcPtr == IntPtr.Zero)
        {
            Debug.LogError($"函数未找到，错误代码: {Marshal.GetLastWin32Error()}");
        }
        else
        {
            Debug.Log($"函数地址: 0x{funcPtr.ToInt64():X}");
        }
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
            //F16_GetAngularVelocity(f16Instance, out float p, out float q, out float r);
            //float[] rates = { p, q, r };
            float[] rates = { 0, 0, 0 };
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

        needle1 = transform.Find("rpm").Find("neddle");
        needle2 = transform.Find("up").Find("neddle");
        needle3 = transform.Find("direction").Find("neddle");
        needle4 = transform.Find("timer").Find("time");

        MPD = transform.Find("MPD").gameObject;

        if (_cameraTransform == null)
        {
            Debug.LogError("错误：在 'Cockpit' 子物体下没有找到名为 'Main Camera' 的摄像机！请检查层级结构和名称。");
        }
        else
        {
            // 锁定并隐藏鼠标光标，以获得更好的体验
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _mainCamera = _cameraTransform.GetComponent<Camera>();
        }

        //接入物理引擎
        // 1. 创建世界
        worldPtr = AircraftWorld_Create();

        // 2. 加载场景（如有需要）
        AircraftWorld_Load(worldPtr, "Assets/Plugins/scenes/aircraft.xml");

        // 3. 获取飞机对象
        aircraftPtr = AircraftWorld_GetAircraft(worldPtr);

        //F16_SetCurrentStateWorld(aircraftPtr,0,3,0,0,0,0,0,0,0,0,0,0);
        //if (aircraftPtr != null)
        //{
        //    Debug.Log(111);
        //}
        F16_HotStart(aircraftPtr);

        lastVel=new Vector3(0,0,0);
    }

    void Update()
    {
        // 获取飞行控制输入
        GetFlightInputs();

        //// 引擎处理(模拟)
        HandleInput();

        //if (Input.GetKey(KeyCode.P))
        //{
        //    keep = false;
        //}
        //if (Input.GetKey(KeyCode.O))
        //{
        //    keep = true;
        //}

        countNum++;
        //if (frameCount++ >= 2)
        //{
        //    // 处理飞行移动
        HandleMovement();
        //    frameCount = 0;
        //}

        //数据
        //F16MPDController mpdController=MPD.GetComponent<F16MPDController>();
        //mpdController.UpdateFlightData();

        // 处理视角转动
        if (_cameraTransform != null)
        {
            HandleCameraRotation();
        }
    }
    float CalculateYaw(float qx, float qy, float qz, float qw)
    {
        float yaw = (float)Math.Atan2(
            2.0f * (qw * qz + qx * qy),
            1.0f - 2.0f * (qy * qy + qz * qz)
        );
        return yaw;
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
            //ApplyPIDControl(aircraftPtr, false, ref _pidIntegral, ref _pidPrevError);
            rollSpeed = 0;
            pitchSpeed = 0;

        }
        if (_pitchInput <- 0.01f)
        {
            Debug.Log("up");
            device.handle_up(4, 2);
            pitchSpeed = 100f;
            F16_SetPitchControl(aircraftPtr, 0.2f);  // 机头上仰
        }
        if (_pitchInput >0.01f)
        {
            Debug.Log("down");
            device.handle_up(4, 1);
            pitchSpeed = 100f;
            F16_SetPitchControl(aircraftPtr, -0.2f); // 机头下俯
        }
        if (_rollInput <- 0.01f)
        {
            Debug.Log("left");
            F16_SetRollControl(aircraftPtr, 0.2f);  // 左滚
            device.handle_up(4, 4);
            rollSpeed = 100f;
        }
        if (_rollInput>0.01f)
        {
            Debug.Log("right");
            F16_SetRollControl(aircraftPtr, -0.2f);  // 右滚
            device.handle_up(4, 3);
            rollSpeed = 100f;
        }

        if (_thrustInput == 0)
        {
            device.power_push(0);//控制油门踏板
            forwardSpeed = 0;
            F16_SetEngineThrottle(aircraftPtr, 0.0f);
        }
        if (_thrustInput > 0.01f)
        {
            device.power_push(5);
            forwardSpeed = -300f;
            F16_SetEngineThrottle(aircraftPtr, 1.0f);
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
        float dt = Time.deltaTime;
        dt = 0.01f;
        //Debug.Log(dt);

        // 1. 更新世界
        AircraftWorld_Update(worldPtr, dt);

        float x = float.NaN, y = float.NaN, z = float.NaN; // 初始化为非法值
        F16_GetPosition(aircraftPtr, out x, out y, out z);
        float roll,pitch,yaw;
        F16_GetRotation(aircraftPtr, out roll, out pitch, out yaw);

        // 3. 更新Unity中飞机模型的位置和旋转
        Debug.Log("x:"+roll+"y:"+pitch+"z:"+yaw);
        transform.position = new Vector3(x, y, z);
        Quaternion y90 = Quaternion.Euler(0, 0, 0);
        Quaternion model = Quaternion.Euler(
            Mathf.Rad2Deg * roll,  
            Mathf.Rad2Deg * yaw,    
            Mathf.Rad2Deg * pitch   
        );
        transform.rotation = model;

        float vx, vy, vz;
        F16_GetVelocity(aircraftPtr, out vx, out vy, out vz);
        Vector3 speed = new Vector3(vx,vy,vz);
        //Debug.Log("speed" + speed);
        lastVel=transform.position;

        float yawDegrees = 1 * (180.0f / (float)Math.PI);
        //Debug.Log(yawDegrees);
        F16MPDController mpdController = MPD.GetComponent<F16MPDController>();
        float t;
        //getParam(aircraftPtr, 2005, out t);
        t = (_thrustInput+UnityEngine.Random.Range(0f, 0.02f))*90;
        mpdController.UpdateFlightData(yaw,y,speed.magnitude,t);

        NeedleController needleController1 = needle1.GetComponent<NeedleController>();
        needleController1.SetNeedlePosition(speed.x);
        NeedleController needleController2 = needle2.GetComponent<NeedleController>();
        needleController2.SetNeedlePosition(y);
        NeedleController needleController3 = needle3.GetComponent<NeedleController>();
        needleController3.SetNeedlePosition(yawDegrees);
        TextMeshProUGUI tmpText = needle4.GetComponent<TextMeshProUGUI>();
        float time = countNum * 0.02f;
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        //Debug.Log(timeSpan);
        string formattedTime = string.Format("{0:D2}:{1:D2}",
            timeSpan.Minutes,
            timeSpan.Seconds);
        if (tmpText != null)
        {
            tmpText.text = formattedTime; // 修改文本内容
        }

        //Debug.Log(getParam());
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
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // 滚轮向上：缩小FOV（放大视角），向下：增大FOV（缩小视角）
            _targetFOV -= scroll * _zoomSpeed;
            _targetFOV = Mathf.Clamp(_targetFOV, _minFOV, _maxFOV);
        }

        // 平滑过渡到目标FOV
        _mainCamera.fieldOfView = Mathf.SmoothDamp(
            _mainCamera.fieldOfView,
            _targetFOV,
            ref _currentZoomVelocity,
            _zoomSmoothTime
        );

    }

    //设置各角度速度大小的接口
    public void SetForwordSpeed(float s){forwardSpeed = s;}
    public void SetYawSpeed(float s) { yawSpeed = s; }
    public void SetUpSpeed(float s) { upSpeed = s; }
    public void SetRollSpeed(float s) { rollSpeed = s; }
    public void SetPitchSpeed(float s) { pitchSpeed = s; }

}