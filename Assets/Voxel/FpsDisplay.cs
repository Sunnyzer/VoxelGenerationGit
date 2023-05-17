using TMPro;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] TextMeshProUGUI minFpsText;
    [SerializeField] TextMeshProUGUI maxFpsText;
    float minFps = int.MaxValue;
    float maxFps = 0;
    private void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        fps = Mathf.Ceil(fps);
        if(fps < minFps)
            minFps = fps;

        if (fps > maxFps)
            maxFps = fps;
        minFpsText.text = "Min Fps : " + minFps;
        maxFpsText.text = "Max Fps : " + maxFps;
        fpsText.text = "Current Fps : " + Mathf.Ceil(fps);
    }
}
