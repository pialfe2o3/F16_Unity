using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class F16MPDController : MonoBehaviour
{
    // 显示元素引用
    public RawImage radarDisplay;
    public TextMeshProUGUI headingText;
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI ssText;
    public TextMeshProUGUI angleText;


    // 雷达参数
    [Header("Radar Settings")]
    public float scanSpeed = 30f;      // 度/秒
    public float maxRange = 100f;      // 海里
    public Color radarColor = Color.green;

    // 战术符号预设
    [Header("Tactical Symbols")]
    public GameObject friendlySymbolPrefab;
    public GameObject hostileSymbolPrefab;
    public GameObject waypointSymbolPrefab;

    // 私有变量
    private float currentScanAngle = 0f;
    private Texture2D radarTexture;
    private RectTransform mpdRect;
    private Transform symbolsContainer;

    public GameObject leftl;
    public GameObject rightl;

    void Awake()
    {
        // 初始化组件
        mpdRect = GetComponent<RectTransform>();

        // 创建雷达纹理
        radarTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        radarDisplay.texture = radarTexture;
        ClearRadarTexture();

        // 创建符号容器
        //GameObject container = new GameObject("TacticalSymbols");
        //symbolsContainer = container.transform;
        //symbolsContainer.SetParent(transform, false);
    }

    void Update()
    {
        // 更新雷达扫描
        UpdateRadarScan();

        // 更新飞行数据

        setAngle(transform.parent.rotation.eulerAngles.z);
    }

    #region 雷达系统
    private void UpdateRadarScan()
    {
        // 更新扫描角度
        currentScanAngle = (currentScanAngle + scanSpeed * Time.deltaTime) % 360;

        // 绘制雷达扫描
        DrawRadarSweep();

        // 模拟随机接触点
        //if (Random.Range(0, 100) > 98) // 2%几率生成新接触点
        //{
        //    GenerateRandomContact();
        //}
    }

    private void ClearRadarTexture()
    {
        Color[] pixels = new Color[radarTexture.width * radarTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black; // 黑色背景
        }
        radarTexture.SetPixels(pixels);

        // 绘制固定距离环
        DrawDistanceRings();

        radarTexture.Apply();
    }

    private void DrawRadarSweep()
    {
        // 淡化上一帧的扫描线
        Color[] pixels = radarTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].g > 0.1f)
            {
                pixels[i].g *= 0.95f; // 逐渐淡化
            }
        }
        radarTexture.SetPixels(pixels);

        // 绘制新扫描线
        Vector2 center = new Vector2(radarTexture.width / 2, radarTexture.height / 2);
        float rad = currentScanAngle * Mathf.Deg2Rad;

        for (int r = 0; r < radarTexture.width / 2; r++)
        {
            int x = (int)(center.x + Mathf.Sin(rad) * r);
            int y = (int)(center.y + Mathf.Cos(rad) * r);

            if (x >= 0 && x < radarTexture.width && y >= 0 && y < radarTexture.height)
            {
                radarTexture.SetPixel(x, y, radarColor);
            }
        }

        radarTexture.Apply();
    }

    private void DrawDistanceRings()
    {
        Vector2 center = new Vector2(radarTexture.width / 2, radarTexture.height / 2);
        float maxRadius = radarTexture.width / 2;

        for (int ring = 1; ring <= 5; ring++)
        {
            float radius = maxRadius * ring / 5f;

            for (float angle = 0; angle < 360; angle += 0.5f)
            {
                float rad = angle * Mathf.Deg2Rad;
                int x = (int)(center.x + Mathf.Sin(rad) * radius);
                int y = (int)(center.y + Mathf.Cos(rad) * radius);

                if (x >= 0 && x < radarTexture.width && y >= 0 && y < radarTexture.height)
                {
                    radarTexture.SetPixel(x, y, new Color(0, 0.3f, 0)); // 暗绿色距离环
                }
            }
        }
    }
    #endregion

    #region 战术符号系统
    private void GenerateRandomContact()
    {
        //// 随机选择符号类型
        //GameObject prefab = Random.Range(0, 2) == 0 ? friendlySymbolPrefab : hostileSymbolPrefab;

        //// 实例化符号
        //GameObject symbol = Instantiate(prefab, symbolsContainer);
        //RectTransform rt = symbol.GetComponent<RectTransform>();

        //// 随机位置(极坐标转屏幕坐标)
        //float distance = Random.Range(0.2f, 0.8f);
        //float angle = Random.Range(0, 360f);

        //Vector2 dir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
        //rt.anchoredPosition = dir * distance * mpdRect.rect.width / 2;

        //// 设置符号属性
        //symbol.GetComponent<TacticalSymbol>().Initialize(
        //    Random.Range(1, 99),      // 编号
        //    distance * maxRange,      // 距离(海里)
        //    angle,                    // 方位角
        //    Random.Range(1000, 35000) // 高度(英尺)
        //);
    }

    public void AddWaypoint(Vector2 normalizedPosition, string identifier)
    {
        //GameObject waypoint = Instantiate(waypointSymbolPrefab, symbolsContainer);
        //RectTransform rt = waypoint.GetComponent<RectTransform>();
        //rt.anchoredPosition = normalizedPosition * mpdRect.rect.width / 2;

        //waypoint.GetComponent<TacticalSymbol>().Initialize(
        //    identifier,              // 航点标识
        //    normalizedPosition.magnitude * maxRange, // 距离
        //    Mathf.Atan2(normalizedPosition.x, normalizedPosition.y) * Mathf.Rad2Deg, // 方位角
        //    0                       // 高度(通常不显示)
        //);
    }
    #endregion

    #region 飞行数据显示
    public void UpdateFlightData(float dis,float height,float speed,float thrust)
    {
        //Debug.Log(dis);
        // 模拟航向(0-360度)
        float heading = (currentScanAngle + 90) % 360;
        headingText.text = $"HDG {dis.ToString("000")}°";

        // 模拟高度(千英尺)
        float altitude = 25f + Mathf.Sin(Time.time * 0.1f) * 5f;
        altitudeText.text = $"ALT {height.ToString("000.0")}K";

        // 模拟空速(节)
        float airspeed = 350f + Mathf.Cos(Time.time * 0.15f) * 50f;
        speedText.text = $"SPD {speed.ToString("000")}";

        float e = 500.0f;
        energyText.text = $"FOB {e.ToString("000")}Kg";

        ssText.text = $"THR {thrust.ToString("00")}%";
        typeText.text = "HotStart";
    }
    #endregion

    #region 公共控制方法
    public void ChangeRange(float newRange)
    {
        maxRange = Mathf.Clamp(newRange, 10f, 200f);
        ClearRadarTexture();
    }

    public void ChangeScanSpeed(float newSpeed)
    {
        scanSpeed = Mathf.Clamp(newSpeed, 10f, 60f);
    }

    public void setAngle(float angle)
    {
        
        if (angle > 180) angle -= 360;
        float clampedAngle = Mathf.Clamp(angle, -60f, 60f);
        float normalizedAngle = clampedAngle / 60f;

        leftl.transform.localPosition = new Vector3(0,normalizedAngle,0);
        rightl.transform.localPosition = new Vector3(0, normalizedAngle, 0);
        angleText.text = $"{angle.ToString("000.0")}";
    }
    #endregion
}