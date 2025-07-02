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
        Vector3 center = gameObject.transform.position; // �Էɻ���ǰλ��Ϊ����
        Collider[] hits = Physics.OverlapSphere(center, radius);

        //��������ɵ�
        Transform screen = transform.Find("screen2");
        if (screen != null)
        {
            // ����screen�µ�����������
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
            // ֻȡXZƽ��
            Vector2 offset2D = new Vector2(offset.x, offset.z);

            // ��һ�����״�뾶
            float radarRadius = 3.5f; // UI�״�뾶�����أ�
            Vector2 radarPos = offset2D / radius * radarRadius;

            // ��UI������һ����
            if (hit.gameObject.name == "pivot_handle" || hit.gameObject.name == "pivot_padel") { continue; }
            GameObject dot = Instantiate(point, radarUI.transform);
            dot.GetComponent<RectTransform>().anchoredPosition = radarPos;
        }
    }
}
