using System.Collections.Generic;
using FunnelAlgorithm;
using PEUtils;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// display class
/// </summary>
public class PERoot : MonoBehaviour
{
    public Color wireColor;
    private NavConfig config;
    //private NavVector[] pointsArr;
    //private List<int[]> indexList;
    private NavArea navArea;
    private NavMap navMap;

    public NavVector3 startNav;
    public NavVector3 targetNav;
    public bool canMouseClick;
    private bool autoTest = false;

    void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");
        InitNavConfig();

        var navView = transform.GetComponent<NavView>();
        
        if (navView != null)
        {
            navView.pointsArr = config.navVectors;
            NavMap.showAreaIDHandle += navView.ShowAreaID;
            NavMap.showPathAreaHandle += navView.ShowPathAreas;
            NavMap.showConnerViewHandle += navView.ShowConnerView;
        }

        navMap = new NavMap(config.indexList, config.navVectors);
        navMap.SetBorderList();
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
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 1000))
            {
                if (canMouseClick)
                {
                    var start = GameObject.FindGameObjectWithTag("Start").transform;
                    start.position = hit.point;
                    startNav = new NavVector3(start.position);
                }
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 1000))
            {
                if (canMouseClick)
                {
                    var target = GameObject.FindGameObjectWithTag("Target").transform;
                    target.position = hit.point;
                    targetNav = new NavVector3(target.position);

                    if (startNav != targetNav)
                    {
                        navMap.CalNavPath(startNav, targetNav);
                    }
                }
            }
        }
#region AutoTest

        if(Input.GetKeyDown(KeyCode.T))
            autoTest = !autoTest;
        if(autoTest) {
            var p1 = GetRandPos();
            var p2 = GetRandPos();
            List<NavVector3> cornerLst = navMap.CalNavPath(p1, p2);
            if(cornerLst?.Count > 0) {
                NavVector3 v1, v2;
                v1 = cornerLst[0];
                for(int i = 1; i < cornerLst.Count; i++) {
                    v2 = cornerLst[i];

                    NavVector3 center = (v1 + v2) / 2.0f;
                    int areaID = navMap.GetNavAreaID(center);
                    if(areaID == -1) {
                        string info = "";
                        for(int k = 0; k < cornerLst.Count; k++) {
                            info += $" {cornerLst[k]}";
                        }

                        GameObject go = new GameObject();
                        go.name = center.ToString();
                        go.transform.position = center.ConvertToUnityVector();
                        this.LogCyan($": {p1} to {p2} center:{center} posLst:{info}");
                        break;
                    }

                    v1 = v2;
                }
            }
        }

#endregion
        
    }


#region AutoTest
    
    private NavVector3 GetRandPos()
    {
        while(true) {
            System.Random rd = new System.Random();
            float x = rd.Next(0, 100);
            float z = rd.Next(0, 100);
            Vector3 randPos = new Vector3(x, 0, z);
            int areaID = navMap.GetNavAreaID(new NavVector3(randPos));
            if(areaID != -1) {
                return new NavVector3(randPos);
            }
        }
    }

#endregion"
    

    private void InitNavConfig()
    {
        var map = GameObject.FindGameObjectWithTag("mapRoot");
        if (map==null)
        {
            return;
        }
        var points = map.transform.Find("pointRoot");
        var indexs = map.transform.Find("indexRoot");
        
        config = new NavConfig()
        {
            indexList = new List<int[]>(),
            navVectors = new NavVector3[points.childCount]
        };

        NavVector3[] pointsArr = new NavVector3[points.childCount];
        for (int i = 0; i < points.childCount; i++)
        {
            pointsArr[i] = new NavVector3(points.GetChild(i).transform.position);
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

            config.indexList.Add(indexsArr);
        }
    }
}