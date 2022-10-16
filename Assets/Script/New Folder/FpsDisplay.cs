using TMPro;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] TextMeshProUGUI minFpsText;
    [SerializeField] TextMeshProUGUI maxFpsText;
    float tempDeltaTime;
    float minFps = int.MaxValue;
    float maxFps = 0;
    bool start = false;
    private void Start()
    {
        fpsText.gameObject.SetActive(false);
        minFpsText.gameObject.SetActive(false);
        maxFpsText.gameObject.SetActive(false);
    }
    private void Update()
    {
        if(!start)
        {
            start = FindObjectOfType<Player>().gravityScale != 0;
            return;
        }
        fpsText.gameObject.SetActive(true);
        minFpsText.gameObject.SetActive(true);
        maxFpsText.gameObject.SetActive(true);
        tempDeltaTime += (Time.deltaTime - tempDeltaTime) * 0.1f;
        float fps = 1.0f / tempDeltaTime;
        fps = Mathf.Ceil(fps);
        if(minFps > fps)
            minFps = fps;

        if (maxFps < fps)
        {
            maxFps = fps;
            CancelInvoke();
            Invoke("ResetMaxFps", 5);
        }
        minFpsText.text = "Min Fps :" + minFps;
        maxFpsText.text = "Max Fps :" + maxFps;
        fpsText.text = "Current Fps :" + Mathf.Ceil(fps);
    }
    void ResetMaxFps()
    {
        maxFps = 0;
    }
}
