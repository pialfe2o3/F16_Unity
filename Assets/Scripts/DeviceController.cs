using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject handle;
    public GameObject padel;
    public GameObject power;
    private Quaternion targetRotation;
    private Quaternion targetPowerRotation;
    private Vector3 targetPowerPosition;
    public float rotateSpeed = 2f; // ��ת�ٶ�
    private float beginmz,beginmx,beginmy;//���ݸ˳�ʼλ�� 
    void Start()
    {
        Transform fightFather = transform.Find("fight_father");
        if (fightFather == null)
        {
            Debug.LogError("�Ҳ���fight_father");
            return;
        }

        // �ҵ�pivot_handle
        handle = fightFather.Find("pivot_handle").gameObject;
        if (handle == null)
        {
            Debug.LogError("�Ҳ���pivot_handle");
            return;
        }
        padel = fightFather.Find("pivot_padel").gameObject;
        if (padel == null)
        {
            Debug.LogError("�Ҳ���pivot_padel");
            return;
        }
        power = fightFather.Find("pivot_power").gameObject;
        if (power == null)
        {
            Debug.LogError("�Ҳ���pivot_power");
            return;
        }
        beginmz=power.transform.localPosition.z;
        beginmx=power.transform.localPosition.x;
        beginmy=power.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        //����Slerpʵ��ƽ��ת��
        if (handle != null)
        {
            handle.transform.localRotation = Quaternion.Slerp(handle.transform.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
        }
        if (padel != null)
        {
            padel.transform.localRotation = Quaternion.Slerp(padel.transform.localRotation, targetPowerRotation, Time.deltaTime * rotateSpeed);
        }
        if (power != null)
        {
            power.transform.localPosition = Vector3.Slerp(power.transform.localPosition, targetPowerPosition, Time.deltaTime * rotateSpeed);
        }
    }


    //�������ţ�
    public void power_push(int p)
    {
        if (p > 5) { p = 5; }
        if (p < 0) { p = 0; }
        targetPowerPosition =new Vector3(beginmx, beginmy, beginmz - p);
    }
    public void padel_up(int p)
    {
        if (p > 3) { p = 3; }
        if (p < 0) { p = 0; }
        targetPowerRotation=Quaternion.Euler(-p * 10, 0, 0);
    }
    //�����ֱ�ת��
    public void handle_up(int p,int direction)
    {
        if (p > 5) { p= 5; }
        if (p < 0) { p = 0; }
        float angle = p * 10f;
        //Debug.Log(111);
        switch (direction)
        {
            case 0:
                targetRotation=Quaternion.Euler(0,0,0);
                break;
            case 1:
                targetRotation=Quaternion.Euler(-angle,0,0);
                break;

            case 2:
                targetRotation = Quaternion.Euler(angle, 0, 0);
                break;

            case 3:
                targetRotation = Quaternion.Euler(0, 0, angle);
                break;

            case 4:
                targetRotation=Quaternion.Euler(0, 0, -angle);
                break;

        }
    }
}
