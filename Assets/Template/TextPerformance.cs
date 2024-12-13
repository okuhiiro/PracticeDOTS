using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class TextPerformance : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextMillisecond;
    [SerializeField] private TextMeshProUGUI TextFps;
    
    int m_AccumulatedFrames;
    float m_AccumulatedTime;

    void Update()
    {
        m_AccumulatedFrames++;
        m_AccumulatedTime += Time.deltaTime;

        if (m_AccumulatedFrames == 20)
        {
            float ms = (m_AccumulatedTime * 1000) / m_AccumulatedFrames;
            TextMillisecond.text = $"{ms:0.00}ms";
            
            var fps  = 1f / Time.deltaTime;
            TextFps.text = $"{fps:0.00}fps";
            
            m_AccumulatedFrames = 0;
            m_AccumulatedTime = 0;
        }
    }
}
