//using System.Collections;
//using System.Collections.Generic;
//using System.Numerics;
using TMPro;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI screen1;
    public TextMeshProUGUI screen2;
    public GameObject radarUI;
    public GameObject point;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetInformationUI();
        SetRadarUI();
        
    }

    public void SetInformationUI()
    {
        //Debug.Log(gameObject.transform.rotation);
        float position_x = gameObject.transform.position.x;
        float position_y = gameObject.transform.position.y;
        float position_z = gameObject.transform.position.z;
        float rotation_x = gameObject.transform.eulerAngles.x;
        float rotation_y = gameObject.transform.eulerAngles.y;
        float rotation_z = gameObject.transform.eulerAngles.z;
        screen1.text = "position:\n" +
            "x:" + (int)position_x + "\n" +
            "y:" + (int)position_y + "\n" +
            "z:" + (int)position_z;
        screen2.text = "angle:\n" +
            "pitch:" + (int)rotation_x + "\n" +
            "yaw:" + (int)rotation_y + "\n" +
            "roll:" + (int)rotation_z;
    }

    public void SetRadarUI()
    {
        float radius = 3000f;
        Vector3 center = gameObject.transform.position; // 以飞机当前位置为球心
        Collider[] hits = Physics.OverlapSphere(center, radius);

        //先清理掉旧的
        Transform screen = transform.Find("screen2");
        if (screen != null)
        {
            // 遍历screen下的所有子物体
            for (int i = screen.childCount - 1; i >= 0; i--)
            {
                Transform child = screen.GetChild(i);
                if (child.name == "point(Clone)")
                {
                    Destroy(child.gameObject);
                }
            }
        }
        foreach (Collider hit in hits)
        {
            Vector3 offset = hit.transform.position - gameObject.transform.position;
            // 只取XZ平面
            Vector2 offset2D = new Vector2(offset.x, offset.z);

            // 归一化到雷达半径
            float radarRadius = 3.5f; // UI雷达半径（像素）
            Vector2 radarPos = offset2D / radius * radarRadius;

            // 在UI上生成一个点
            if (hit.gameObject.name == "pivot_handle" || hit.gameObject.name == "pivot_padel") { continue; }
            GameObject dot = Instantiate(point, radarUI.transform);
            dot.GetComponent<RectTransform>().anchoredPosition = radarPos;
        }
    }
}
