using UnityEngine;
using UnityEngine.UI;

public class TacticalSymbol : MonoBehaviour
{
    public Text idLabel;
    public Text dataLabel;

    public void Initialize(object identifier, float distance, float bearing, float altitude)
    {
        // 设置符号标识
        idLabel.text = identifier.ToString();

        // 设置数据标签(格式: 距离/方位/高度)
        dataLabel.text = $"{distance.ToString("0")}nm/{bearing.ToString("000")}°/{altitude.ToString("0")}ft";

        // 根据符号类型设置颜色
        if (CompareTag("Friendly"))
        {
            idLabel.color = Color.green;
            dataLabel.color = Color.green;
        }
        else if (CompareTag("Hostile"))
        {
            idLabel.color = Color.red;
            dataLabel.color = Color.red;
        }
        else // 航点等中性符号
        {
            idLabel.color = Color.cyan;
            dataLabel.color = Color.cyan;
        }
    }
}