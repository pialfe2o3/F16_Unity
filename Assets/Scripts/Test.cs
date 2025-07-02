using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Test : MonoBehaviour
{

    [DllImport("DampsEngineExtern.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr AABBBox_Create(double[] min, double[] max);

    [DllImport("DampsEngineExtern.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AABBBox_Destroy(IntPtr box);

    [DllImport("DampsEngineExtern.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern double AABBBox_getVolume(IntPtr box);

    void Start()
    {
        // 1. ����min��max����
        double[] min = { 0.0, 0.0, 0.0 };
        double[] max = { 2.0, 3.0, 4.0 };

        // 2. ����AABBBox���󣬻�ȡָ��
        IntPtr aabbBoxPtr = AABBBox_Create(min, max);

        if (aabbBoxPtr != IntPtr.Zero)
        {
            // 3. ����AABBBox_getVolume��ȡ���
            double volume = AABBBox_getVolume(aabbBoxPtr);
            Debug.Log("AABBBox Volume: " + volume); // Ӧ����� 24

            // 4. ���ٶ���
            AABBBox_Destroy(aabbBoxPtr);
            aabbBoxPtr = IntPtr.Zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
