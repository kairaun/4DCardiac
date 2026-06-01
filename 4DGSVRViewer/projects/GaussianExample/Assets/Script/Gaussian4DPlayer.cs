using GaussianSplatting.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class Gaussian4DPlayer : MonoBehaviour
{
    [Header("請放入場景中設定好的『單一 4DGS 物件』當作模板")]
    public GaussianSplatRenderer templateRenderer;

    [Header("請放入所有轉換好的 4DGS .asset")]
    public GaussianSplatAsset[] sequenceFrames;

    public float targetFPS = 30f;

    private List<GaussianSplatRenderer> renderersPool = new List<GaussianSplatRenderer>();
    private float timer = 0f;
    private int currentFrame = 0;

    void Start()
    {
        // 關閉模板，避免干擾
        templateRenderer.gameObject.SetActive(false);

        Debug.Log("開始預載 132 幀 4DGS 至 VRAM...");

        for (int i = 0; i < sequenceFrames.Length; i++)
        {
            GameObject cloneObj = Instantiate(templateRenderer.gameObject, this.transform);
            cloneObj.name = $"Heart_Frame_{i}";

            GaussianSplatRenderer gsRenderer = cloneObj.GetComponent<GaussianSplatRenderer>();

            // 賦予資料
            gsRenderer.m_Asset = sequenceFrames[i];

            // 設定開關 (只有第一幀是 true)
            gsRenderer.m_IsActiveFrame = (i == 0);

            // 開啟物件！這會觸發 OnEnable，把資料送進 VRAM (但因為 IsActiveFrame 是 false，總管不會畫它)
            cloneObj.SetActive(true);

            renderersPool.Add(gsRenderer);
        }
    }

    void Update()
    {
        if (renderersPool.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / targetFPS)
        {
            timer -= 1f / targetFPS;

            // 1. 關閉當前幀
            renderersPool[currentFrame].m_IsActiveFrame = false;

            // 2. 推進到下一幀
            currentFrame = (currentFrame + 1) % renderersPool.Count;

            // 3. 開啟下一幀
            renderersPool[currentFrame].m_IsActiveFrame = true;
        }
    }
}