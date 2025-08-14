using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleController : MonoBehaviour
{
    public Transform needleTransform;
    public Vector3 rotationAxis = Vector3.forward;
    public float minAngle = -135f;
    public float maxAngle = 135f;

    public void SetNeedlePosition(float normalizedValue)
    {
        //Debug.Log(normalizedValue);
        float angle = Mathf.Clamp(normalizedValue, minAngle, maxAngle);
        needleTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        //needleTransform.localPosition =new Vector3(0, 2, 0);
    }
}