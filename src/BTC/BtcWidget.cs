using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BtcWidget : MonoBehaviour
{
    [InitializeOnLoadMethod]
    static void AA() 
    {
        var widget = FindObjectOfType<BtcWidget>();
        if (widget == null)
        {
            var go = new GameObject("BTC_WIDGET");
            go.AddComponent<BtcWidget>();
        }
    }

#if UNITY_EDITOR
    private DateTime lastDraw;

    private Texture2D btc;

    private float priceUsd;

    private void DrawString(string text, Color? colour = null, bool showTexture = true, int offsetY = 0)
    {
        Handles.BeginGUI();

        var restoreColor = GUI.color;
        var viewSize = SceneView.currentDrawingSceneView.position;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(
            view.camera.transform.position + view.camera.transform.forward);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            Handles.EndGUI();
            return;
        }
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 40;
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(40, viewSize.height - 100 - offsetY, size.x, size.y), text, style);
        GUI.color = restoreColor;

        if (showTexture)
        {
            GUI.DrawTexture(
                new Rect(40, viewSize.height - 210, 100, 100), btc,
                ScaleMode.StretchToFill, true, 1.0f, new Color(1, 1, 1, 0.5f), 0, 0);
        }

        Handles.EndGUI();
    }
    void LoadTexture()
    {
        btc = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BTC/btc.png");
    }
    void RefreshRate()
    {
        var www = new WWW("https://api.coinmarketcap.com/v1/ticker/bitcoin/");
        while (www.isDone == false) ;
        if (string.IsNullOrEmpty(www.error) == false)
            return;

        var data = MiniJSON.Json.Deserialize(www.text);
        var btcRateInfo = (Dictionary<string, object>)((List<object>)data)[0];

        priceUsd = float.Parse((string)btcRateInfo["price_usd"]);
    }
    void OnDrawGizmos()
    {
        if (btc == null)
            LoadTexture();

        if ((DateTime.Now - lastDraw) >= TimeSpan.FromSeconds(1))
        {
            RefreshRate();
            lastDraw = DateTime.Now;
        }

        DrawString("BTC/USD: " + priceUsd.ToString() + "USD");
    }
#endif
}

