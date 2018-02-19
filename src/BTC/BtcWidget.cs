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
    static void InjectWidget() 
    {
        EditorApplication.update += OnDefferedCreation;
    }
    static void OnDefferedCreation()
    {
        EditorApplication.update -= OnDefferedCreation;

        var widget = FindObjectOfType<BtcWidget>();
        if (widget == null)
        {
            var go = new GameObject("BTC_WIDGET");
            go.AddComponent<BtcWidget>();
        }
    }

#if UNITY_EDITOR
    private DateTime lastDraw;

    private Texture2D btc, eth, xrp;

    private float btcUsd, ethUsd, xrpUsd;

    private void DrawString()
    {
        Handles.BeginGUI();

        var restoreColor = GUI.color;
        var viewSize = SceneView.currentDrawingSceneView.position;

        GUI.color = Color.white;
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
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 30;
        Vector2 size = style.CalcSize(new GUIContent("BTC/USD: $" + btcUsd.ToString("#")));
        GUI.Label(new Rect(80, viewSize.height - 100 - 0, size.x, size.y),
            "BTC/USD: $" + btcUsd.ToString("#") + "" , style);
        GUI.Label(new Rect(80, viewSize.height - 100 - 40, size.x, size.y),
            "ETH/USD: $" + ethUsd.ToString("#") + "" , style);
        GUI.Label(new Rect(80, viewSize.height - 100 - 80, size.x, size.y),
            "XRP/USD: $" + xrpUsd.ToString("0.##") + "" , style);
        GUI.color = restoreColor;

        // DRAW ICONS
        GUI.DrawTexture(
                new Rect(35, viewSize.height - 100, 35, 35), btc,
                ScaleMode.StretchToFill, true, 1.0f, new Color(1, 1, 1, 0.5f), 0, 0);
        GUI.DrawTexture(
            new Rect(35, viewSize.height - 140, 35, 35), eth,
            ScaleMode.StretchToFill, true, 1.0f, new Color(1, 1, 1, 0.5f), 0, 0);
        GUI.DrawTexture(
            new Rect(35, viewSize.height - 180, 35, 35), xrp,
            ScaleMode.StretchToFill, true, 1.0f, new Color(1, 1, 1, 0.5f), 0, 0);

        Handles.EndGUI();
    }
    void LoadTexture()
    {
        btc = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BTC/btc.png");
        eth = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BTC/eth.png");
        xrp = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BTC/xrp.png");
    }
    void RefreshRate()
    {
        var www = new WWW("https://api.coinmarketcap.com/v1/ticker/");
        while (www.isDone == false) ;
        if (string.IsNullOrEmpty(www.error) == false)
            return;

        var data = MiniJSON.Json.Deserialize(www.text);
        var ticks = (List<object>)data;

        foreach (var _c in ticks)
        {
            var c = (Dictionary<string, object>)_c;

            if ((string)c["symbol"] == "BTC")
                btcUsd = float.Parse((string)c["price_usd"]);
            else if ((string)c["symbol"] == "ETH")
                ethUsd = float.Parse((string)c["price_usd"]);
            else if ((string)c["symbol"] == "XRP")
                xrpUsd = float.Parse((string)c["price_usd"]);
        }
    }
    void OnDrawGizmos()
    {
        if (btc == null || eth == null || xrp == null)
            LoadTexture();

        if ((DateTime.Now - lastDraw) >= TimeSpan.FromSeconds(1))
        {
            RefreshRate();
            lastDraw = DateTime.Now;
        }

        DrawString();
    }
#endif
}

