using TMPro;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] TextMeshProUGUI minFpsText;
    [SerializeField] TextMeshProUGUI maxFpsText;
    [SerializeField] TextMeshProUGUI amountChunkLoad;
    float minFps = int.MaxValue;
    float maxFps = 0;
    float time = 0;
    private void Start()
    {
        amountChunkLoad.text = "ChunkLoad : " + (ChunkManager.Instance.ChunkAmountXZ * ChunkManager.Instance.ChunkAmountXZ).ToString();
    }
    private void Update()
    {
        time += Time.deltaTime;
        float fps = 1.0f / Time.deltaTime;
        fps = Mathf.Ceil(fps);
        if(fps < minFps)
            minFps = fps;

        if (fps > maxFps)
            maxFps = fps;
        if (time < 0.2f) return;
        minFpsText.text = "Min Fps : " + minFps;
        maxFpsText.text = "Max Fps : " + maxFps;
        fpsText.text = "Current Fps : " + Mathf.Ceil(fps);
        time = 0;
    }
}
