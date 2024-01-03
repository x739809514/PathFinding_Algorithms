using System;
using System.Collections.Generic;
using System.Numerics;
using FunnelAlgorithm;
using PEUtils;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PERoot : MonoBehaviour
{
    public Color wireColor;
    private NavConfig config;
    private NavVector[] pointsArr;
    private List<int[]> indexList;

    void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");
        InitNavConfig();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false || config == null)
        {
            InitNavConfig();
        }

        Gizmos.color = wireColor;
        for (int i = 0; i < config.indexList.Count; i++)
        {
            var indexArr = config.indexList[i];
            int count = indexArr.Length;
            Vector3 v1, v2;
            for (int j = 0, k = count - 1; j < count; k = j++)
            {
                int index1 = indexArr[j];
                int index2 = indexArr[k];
                v1 = config.navVectors[index1].ConvertToUnityVector();
                v2 = config.navVectors[index2].ConvertToUnityVector();
                Gizmos.DrawLine(v1, v2);
            }
        }

        Gizmos.color = Color.white;
    }

    void Update()
    {
    }

    private void InitNavConfig()
    {
        var map = GameObject.FindWithTag("mapRoot");
        var points = map.transform.Find("pointRoot");
        var indexs = map.transform.Find("indexRoot");
        pointsArr = new NavVector[points.childCount];
        indexList = new List<int[]>();
        config = new NavConfig()
        {
            indexList = new List<int[]>(),
            navVectors = new NavVector[points.childCount]
        };

        for (int i = 0; i < points.childCount; i++)
        {
            pointsArr[i] = new NavVector(points.GetChild(i).transform.position);
        }

        config.navVectors = pointsArr;
        for (int i = 0; i < indexs.childCount; i++)
        {
            var VerticeArr = indexs.GetChild(i).name.Split("-");
            int[] indexsArr = new int[VerticeArr.Length];
            for (int j = 0; j < VerticeArr.Length; j++)
            {
                indexsArr[j] = int.Parse(VerticeArr[j]);
            }

            indexList.Add(indexsArr);
        }

        config.indexList = indexList;
    }
}