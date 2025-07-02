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
        // 1. 定义min和max向量
        double[] min = { 0.0, 0.0, 0.0 };
        double[] max = { 2.0, 3.0, 4.0 };

        // 2. 创建AABBBox对象，获取指针
        IntPtr aabbBoxPtr = AABBBox_Create(min, max);

        if (aabbBoxPtr != IntPtr.Zero)
        {
            // 3. 调用AABBBox_getVolume获取体积
            double volume = AABBBox_getVolume(aabbBoxPtr);
            Debug.Log("AABBBox Volume: " + volume); // 应该输出 24

            // 4. 销毁对象
            AABBBox_Destroy(aabbBoxPtr);
            aabbBoxPtr = IntPtr.Zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
