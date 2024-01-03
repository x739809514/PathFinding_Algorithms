using System;
using UnityEngine;

namespace FunnelAlgorithm
{
    public class NavView : MonoBehaviour
    {
        public Transform areaIDTrans;

        public void ShowAreaID(NavVector pos,int areaID)
        {
            var go = new GameObject { name = $"areaID{areaID}" };
            go.transform.SetParent(areaIDTrans);
            go.transform.position = pos.ConvertToUnityVector();
            go.transform.localEulerAngles = new Vector3(90, 0, 0);
            var comp = go.AddComponent<TextMesh>();
            comp.text = areaID.ToString();
            comp.anchor = TextAnchor.MiddleCenter;
            comp.alignment = TextAlignment.Center;
            comp.color = Color.red;
            comp.fontSize = 45;
            comp.fontStyle = FontStyle.Normal;
            go.transform.localScale=Vector3.one;
        }
    }
}