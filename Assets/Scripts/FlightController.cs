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

    private float _cameraYaw = 0f;
    private float _cameraPitch = 0f;

    

    void Start()
    {
        // 自动查找驾驶舱下的主摄像机
        // 请确保您的层级结构是：飞机(挂载此脚本) -> Cockpit -> Main Camera
        _cameraTransform = transform.Find("Main Camera");

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
    }

    void Update()
    {
        // 获取飞行控制输入
        GetFlightInputs();

        // 引擎处理
        HandleInput();

        // 处理飞行移动
        HandleMovement();

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
            rollSpeed = 0;
            pitchSpeed = 0;

        }
        if (_pitchInput > 0.01f)
        {
            device.handle_up(4, 2);
            pitchSpeed = 100f;
        }
        if (_pitchInput < -0.01f)
        {
            device.handle_up(4, 1);
            pitchSpeed = 100f;
        }
        if (_rollInput > 0.01f)
        {
            device.handle_up(4, 3);
            rollSpeed = 100f;
        }
        if (_rollInput < -0.01f)
        {
            device.handle_up(4, 4);
            rollSpeed = 100f;
        }

        if (_thrustInput == 0)
        {
            device.power_push(0);//控制油门踏板
            forwardSpeed = 0;
        }
        if (_thrustInput > 0.01f)
        {
            device.power_push(5);
            forwardSpeed = -300f;
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
        // 应用推力向前移动
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);

        // 应用从机翼向上的升力
        transform.Translate(Vector3.up * upSpeed * Time.deltaTime, Space.Self);

        // 应用俯仰（上下旋转）
        transform.Rotate(Vector3.right, _pitchInput * pitchSpeed * Time.deltaTime, Space.Self);

        // 应用翻转（左右倾斜）
        transform.Rotate(Vector3.forward, _rollInput * rollSpeed * Time.deltaTime, Space.Self);

        // 应用偏航（左右转弯）
        transform.Rotate(Vector3.up, _yawInput * yawSpeed * Time.deltaTime, Space.Self);
    }

    /// 处理基于鼠标输入的摄像机视角转动。
    void HandleCameraRotation()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 累加摄像机的偏航和俯仰值
        _cameraYaw += mouseX;
        _cameraPitch -= mouseY; // 减去mouseY是因为鼠标Y轴输入默认是反的

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