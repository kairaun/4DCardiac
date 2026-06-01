using GaussianSplatting.Editor; // 引用原作者的命名空間
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class BatchGaussianImporter : EditorWindow
{
    private string plyDirectory = "Assets/PLY/gaussian_pertimestamp";
    private string outputDirectory = "Assets/PLY/BatchOutput";

    [MenuItem("Tools/批次導入 4DGS PLY 序列")]
    public static void ShowWindow()
    {
        GetWindow<BatchGaussianImporter>("批次導入 PLY");
    }

    void OnGUI()
    {
        GUILayout.Label("批次轉換 PLY 為 Unity Asset", EditorStyles.boldLabel);
        plyDirectory = EditorGUILayout.TextField("輸入資料夾 (PLY)", plyDirectory);
        outputDirectory = EditorGUILayout.TextField("輸出資料夾 (Asset)", outputDirectory);

        if (GUILayout.Button("開始自動批次轉換", GUILayout.Height(40)))
        {
            BatchProcess(plyDirectory, outputDirectory);
        }
    }

    private void BatchProcess(string inputFolder, string outputFolder)
    {
        if (!Directory.Exists(inputFolder))
        {
            Debug.LogError($"找不到輸入資料夾: {inputFolder}");
            return;
        }

        string[] plyFiles = Directory.GetFiles(inputFolder, "*.ply");
        if (plyFiles.Length == 0)
        {
            Debug.LogWarning("資料夾中沒有找到任何 .ply 檔案！");
            return;
        }

        // 動態生成原作者的轉換視窗實例
        GaussianSplatAssetCreator creator = ScriptableObject.CreateInstance<GaussianSplatAssetCreator>();

        // 透過 Reflection (反射) 獲取原腳本中的私有變數與方法
        var type = typeof(GaussianSplatAssetCreator);
        var inputFileField = type.GetField("m_InputFile", BindingFlags.NonPublic | BindingFlags.Instance);
        var outputFolderField = type.GetField("m_OutputFolder", BindingFlags.NonPublic | BindingFlags.Instance);
        var createAssetMethod = type.GetMethod("CreateAsset", BindingFlags.NonPublic | BindingFlags.Instance);

        if (inputFileField == null || outputFolderField == null || createAssetMethod == null)
        {
            Debug.LogError("無法獲取核心轉換函式，請確認 aras-p 腳本結構是否被修改過。");
            return;
        }

        int count = 0;
        foreach (string file in plyFiles)
        {
            count++;
            // 將路徑的斜線統一，避免 Unity 讀取錯誤
            string safeFilePath = file.Replace("\\", "/");

            // 替原本的腳本注入參數
            inputFileField.SetValue(creator, safeFilePath);
            outputFolderField.SetValue(creator, outputFolder);

            Debug.Log($"[{count}/{plyFiles.Length}] 正在轉換: {Path.GetFileName(safeFilePath)}");

            // 觸發 CreateAsset 按鈕的功能
            createAssetMethod.Invoke(creator, null);
        }

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>批次轉換完成！共處理 {plyFiles.Length} 個檔案。</color>");
    }
}