using UnityEngine;

/// <summary>
/// ���Ʒ������ķ����Լ���ʻ���ڵ��ӽǡ�
/// </summary>
public class FlightController : MonoBehaviour
{
    [Header("�����ٶ�����")]
    [Tooltip("������ǰ���ٶȡ�")]
    public float forwardSpeed = 25f;

    [Tooltip("������������̧ͷ/��ͷ�����ٶȡ�")]
    public float pitchSpeed = 100f;

    [Tooltip("��������ת������/���ҹ�ת�����ٶȡ�")]
    public float rollSpeed = 100f;

    [Tooltip("������ƫ��������/����ת�䣩���ٶȡ�")]
    public float yawSpeed = 100f;

    [Tooltip("����������̧�����ٶȡ�")]
    public float upSpeed = 50f;

    [Header("�ӽǿ�������")]
    [Tooltip("��������ȡ�")]
    public float mouseSensitivity = 200f;

    [Tooltip("�������ֱ���򣨸����������Ƕȡ�")]
    public float maxPitchAngle = 80f;

    // ˽�б���
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
        // �Զ����Ҽ�ʻ���µ��������
        // ��ȷ�����Ĳ㼶�ṹ�ǣ��ɻ�(���ش˽ű�) -> Cockpit -> Main Camera
        _cameraTransform = transform.Find("Main Camera");

        if (_cameraTransform == null)
        {
            Debug.LogError("������ 'Cockpit' ��������û���ҵ���Ϊ 'Main Camera' �������������㼶�ṹ�����ơ�");
        }
        else
        {
            // ��������������꣬�Ի�ø��õ�����
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // ��ȡ���п�������
        GetFlightInputs();

        // ���洦��
        HandleInput();

        // ��������ƶ�
        HandleMovement();

        // �����ӽ�ת��
        if (_cameraTransform != null)
        {
            HandleCameraRotation();
        }
    }

    /// ��ȡ�������ڷ��е�������롣
    void GetFlightInputs()
    {
        // ����Ҫ��Unity����������������� "Vertical", "Horizontal", "Yaw", �� "Thrust" ��
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
        //������������棬�����ٶȣ��Ƕȵ���Ϣ���õ����º�ĸ����ٶ���Ϣ
        //���������ֱ�����Ը�ϸ�ڵ����ֿ�������С
        DeviceController device = GetComponent<DeviceController>();
        if (_pitchInput == 0 && _rollInput == 0)
        {
            device.handle_up(0, 0);//���Ƹ�������
            // TODO:����DampsEngine����ȡ��Ϣ
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
            device.power_push(0);//��������̤��
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

    /// ÿһ֡ͨ��transform�򵥸��·ɻ���̬λ�ã����������źŵĴ�����һ���ӿڣ�����ֻ���ܷɻ��ĸ����ٶ���Ϣ������Ӧ
    void HandleMovement()
    {
        // ����ֱ�Ӵ�dampsEngine�л�ȡλ����Ϣ
        // Ӧ��������ǰ�ƶ�
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);

        // Ӧ�ôӻ������ϵ�����
        transform.Translate(Vector3.up * upSpeed * Time.deltaTime, Space.Self);

        // Ӧ�ø�����������ת��
        transform.Rotate(Vector3.right, _pitchInput * pitchSpeed * Time.deltaTime, Space.Self);

        // Ӧ�÷�ת��������б��
        transform.Rotate(Vector3.forward, _rollInput * rollSpeed * Time.deltaTime, Space.Self);

        // Ӧ��ƫ��������ת�䣩
        transform.Rotate(Vector3.up, _yawInput * yawSpeed * Time.deltaTime, Space.Self);
    }

    /// ���������������������ӽ�ת����
    void HandleCameraRotation()
    {
        // ��ȡ�������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �ۼ��������ƫ���͸���ֵ
        _cameraYaw += mouseX;
        _cameraPitch -= mouseY; // ��ȥmouseY����Ϊ���Y������Ĭ���Ƿ���

        // ����ֱ����ĸ����Ƕ�������-maxPitchAngle��+maxPitchAngle��֮�䣬��ֹ�������ת
        _cameraPitch = Mathf.Clamp(_cameraPitch, -maxPitchAngle, maxPitchAngle);

        // Ӧ����ת���⽫ֻ��ת���������ʹ���ӽ�����ڷɻ����ı䡣
        _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
    }

    //���ø��Ƕ��ٶȴ�С�Ľӿ�
    public void SetForwordSpeed(float s){forwardSpeed = s;}
    public void SetYawSpeed(float s) { yawSpeed = s; }
    public void SetUpSpeed(float s) { upSpeed = s; }
    public void SetRollSpeed(float s) { rollSpeed = s; }
    public void SetPitchSpeed(float s) { pitchSpeed = s; }

}