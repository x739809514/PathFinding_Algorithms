using System;
using System.Collections.Generic;
using FunnelAlgorithm.Utility;
using UnityEngine;

namespace FunnelAlgorithm
{
    public class NavView : MonoBehaviour
    {
        public Transform areaIDTrans;
        public NavVector[] pointsArr;
        
        public void ShowAreaID(NavVector pos, int areaID)
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
            go.transform.localScale = Vector3.one;
        }

        public void ShowPathAreas(List<NavArea> areas)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                var indexArr = areas[i].indexArr;
                var count = indexArr.Length;
                for (int j = 0, k = count - 1; j < count; k = j++)
                {
                    var v1 = pointsArr[indexArr[j]];
                    var v2 = pointsArr[indexArr[k]];
                    DebugDrawLine(v1,v2,Color.yellow,5f);
                }
            }
        }

        public static void DebugDrawLine(NavVector v1, NavVector v2, Color color, float time = float.MaxValue)
        {
            Debug.DrawLine(v1.ConvertToUnityVector(),v2.ConvertToUnityVector(),color,time);
        }
    }
}