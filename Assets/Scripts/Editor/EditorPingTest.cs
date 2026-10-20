using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;

public class EditorPingTest : EditorWindow
{
    [MenuItem("Tools/Firebase Ping Test")]
    public static void ShowWindow()
    {
        EditorPingTest window = GetWindow<EditorPingTest>("Firebase Ping");
        window.Show();
    }

    private string url = "https://ardataset-586e9-default-rtdb.asia-southeast1.firebasedatabase.app/sessions.json";
    private string output = "";

    void OnGUI()
    {
        GUILayout.Label("Firebase Ping Test", EditorStyles.boldLabel);
        url = EditorGUILayout.TextField("URL:", url);

        if (GUILayout.Button("Send Test Ping"))
        {
            string json = "{\"test\":true,\"lesson_name\":\"Editor Test\",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
            output = "Sending...";
            EditorCoroutine.Start(Post(url, json));
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Result:");
        EditorGUILayout.TextArea(output, GUILayout.Height(100));
    }

    IEnumerator Post(string targetUrl, string json)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(targetUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            var op = request.SendWebRequest();
            while (!op.isDone) yield return null;

            if (request.result == UnityWebRequest.Result.Success)
            {
                output = "SUCCESS!\n" + request.downloadHandler.text;
                Debug.Log("[FirebaseTest] Success: " + request.downloadHandler.text);
            }
            else
            {
                output = "FAILED!\n" + request.error;
                Debug.LogError("[FirebaseTest] Failed: " + request.error);
            }
            Repaint();
        }
    }
}

// Very simple Editor Coroutine runner
public static class EditorCoroutine
{
    public static void Start(IEnumerator routine)
    {
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            if (!routine.MoveNext())
            {
                EditorApplication.update -= callback;
            }
        };
        EditorApplication.update += callback;
    }
}
