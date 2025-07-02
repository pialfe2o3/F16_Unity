using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class RigidWorldManager : MonoBehaviour
{
    [Header("RigidWorld Settings")]
    public string sceneFilePath = "Assets/Plugins/scenes/capsules.xml";
    public GameObject rigidbodyPrefab;
    public GameObject PlanePrefab;
    public Transform rigidbodiesParent;

    private IntPtr worldPtr = IntPtr.Zero;

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr RigidWorld_Create();

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Destroy(IntPtr world);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Load(IntPtr world, string filePath);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Save(IntPtr world, string filePath);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RigidWorld_Update(IntPtr world, float dt);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern int RigidWorld_GetRigidbodyCount(IntPtr world);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr RigidWorld_GetRigidbody(IntPtr world, int idx);


    // DLL导入刚体位置/旋转
    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetPosition(IntPtr rigidbody, out float x, out float y, out float z);

    [DllImport("DampsEngineExtern 3", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Rigidbody_GetRotation(IntPtr rigidbody, out float x, out float y, out float z, out float w);

    // 你可以根据需要继续添加其它DLL导入声明

    private GameObject[] rigidbodyObjects;

    void Start()
    {
        // 创建物理世界
        worldPtr = RigidWorld_Create();
        if (worldPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to create RigidWorld");
            return;
        }

        // 加载场景
        try
        {
            RigidWorld_Load(worldPtr, sceneFilePath);
            Debug.Log($"Loaded scene: {sceneFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
            return;
        }

        // 创建Unity刚体对象
        int count = RigidWorld_GetRigidbodyCount(worldPtr);
        Debug.Log(count);
        rigidbodyObjects = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                GameObject p = Instantiate(PlanePrefab, rigidbodiesParent);
                p.name = $"RPlane_{i}";
                rigidbodyObjects[i] = p;
            }
            else
            {
                GameObject go = Instantiate(rigidbodyPrefab, rigidbodiesParent);
                go.name = $"RigidBody_{i}";
                rigidbodyObjects[i] = go;
            }
        }

        Debug.Log("Rigid world initialized successfully");
    }

    void Update()
    {
        if (worldPtr == IntPtr.Zero)
            return;

        // 更新物理世界
        RigidWorld_Update(worldPtr, Time.deltaTime);

        // 同步刚体位置（假设你有类似Vehicle_GetPosition的接口）
        for (int i = 0; i < rigidbodyObjects.Length; i++)
        {
            IntPtr rbPtr = RigidWorld_GetRigidbody(worldPtr, i);
            if (rbPtr == IntPtr.Zero) continue;

            // 你需要在C++侧导出Rigidbody_GetPosition和Rigidbody_GetRotation接口
            float x, y, z, qx, qy, qz, qw;
            Rigidbody_GetPosition(rbPtr, out x, out y, out z);
            Rigidbody_GetRotation(rbPtr, out qx, out qy, out qz, out qw);

            rigidbodyObjects[i].transform.position = new Vector3(x, y, z);
            rigidbodyObjects[i].transform.rotation = new Quaternion(qx, qy, qz, qw);
        }
    }

    void OnDestroy()
    {
        if (worldPtr != IntPtr.Zero)
        {
            RigidWorld_Destroy(worldPtr);
            worldPtr = IntPtr.Zero;
        }
    }


    // 可选：GUI显示
    void OnGUI()
    {
        if (worldPtr != IntPtr.Zero)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"RigidBody Count: {rigidbodyObjects?.Length ?? 0}");
            GUILayout.EndArea();
        }
    }
}

