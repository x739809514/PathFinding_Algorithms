using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace FunnelAlgorithm
{
    public partial class NavMap
    {
        private readonly List<int[]> indexList;
        private readonly NavVector3[] pointsArr;
        private NavArea[] areaArr;
        public static Action<NavVector3, int> showAreaIDHandle;
        public static Action<List<NavArea>> showPathAreaHandle;
        public static Action<List<NavVector3>> showConnerViewHandle;

        private Dictionary<string, NavBorder> borderList = new Dictionary<string, NavBorder>();
        private Dictionary<string, NavBorder> areaList = new Dictionary<string, NavBorder>();


        public NavMap(List<int[]> indexList, NavVector3[] pointsArr)
        {
            this.indexList = indexList;
            this.pointsArr = pointsArr;
            var count = indexList.Count;
            areaArr = new NavArea[count];
            for (int i = 0; i < count; i++)
            {
                areaArr[i] = new NavArea(i, indexList[i], pointsArr);
                showAreaIDHandle?.Invoke(areaArr[i].center, i);
            }
        }

        public void SetBorderList()
        {
            for (int areaID = 0; areaID < indexList.Count; areaID++)
            {
                var indexArr = indexList[areaID];
                for (int verticeIndex = 0; verticeIndex < indexArr.Length; verticeIndex++)
                {
                    var startIndex = indexArr[verticeIndex];
                    var endIndex = 0;
                    if (verticeIndex < indexArr.Length - 1)
                    {
                        endIndex = indexArr[verticeIndex + 1];
                    }
                    else
                    {
                        endIndex = indexArr[0];
                    }

                    //make index in increasing order
                    string key = "";
                    if (startIndex < endIndex)
                    {
                        key = $"{startIndex}-{endIndex}";
                    }
                    else
                    {
                        key = $"{endIndex}-{startIndex}";
                    }

                    if (borderList.TryGetValue(key, out var navBorder))
                    {
                        navBorder.areaID2 = areaID;
                        navBorder.isShare = true;
                        string areaKey = "";
                        if (navBorder.areaID1 < navBorder.areaID2)
                        {
                            areaKey = $"{navBorder.areaID1}-{navBorder.areaID2}";
                        }
                        else
                        {
                            areaKey = $"{navBorder.areaID2}-{navBorder.areaID1}";
                        }

                        areaList.Add(areaKey, navBorder);
                    }
                    else
                    {
                        navBorder = new NavBorder()
                        {
                            areaID1 = areaID,
                            pointIndex1 = startIndex,
                            pointIndex2 = endIndex
                        };
                        borderList.Add(key, navBorder);
                    }
                }
            }

            List<string> singles = new List<string>();
            foreach (var navBorder in borderList)
            {
                if (navBorder.Value.isShare == false)
                {
                    singles.Add(navBorder.Key);
                }
            }

            foreach (var singleKey in singles)
            {
                borderList.Remove(singleKey);
            }

            foreach (var area in areaArr)
            {
                area.borderList = new List<NavBorder>();
                area.borderList = SetBorderListForArea(area.areaID);
            }
        }

        private List<NavBorder> SetBorderListForArea(int areaID)
        {
            List<NavBorder> list = new List<NavBorder>();
            foreach (var border in borderList)
            {
                if (border.Value.areaID1 == areaID || border.Value.areaID2 == areaID)
                {
                    list.Add(border.Value);
                }
            }

            return list;
        }

        private NavBorder GetBorderByAreaIDKey(string key)
        {
            return areaList.TryGetValue(key, out NavBorder border) ? border : null;
        }

        public List<NavVector3> CalNavPath(NavVector3 start, NavVector3 end)
        {
            var startAreaID = GetNavAreaID(start);
            var targetAreaID = GetNavAreaID(end);

            if (startAreaID != -1){}
               // Debug.Log($"startAreaID:{startAreaID}");
            else
            {
                //Debug.LogError("no such Area");
                return null;
            }

            if (targetAreaID != -1){}
               // Debug.Log($"targetAreaID:{targetAreaID}");
            else
            {
                //Debug.LogError("no such Area");
                return null;
            }

            var area1 = areaArr[startAreaID];
            var area2 = areaArr[targetAreaID];
            var areas = CalAStarPolyPath(area1, area2);
            //Todo: calculate conner list
            var connerLst = CalFunnelConnerPath(areas, start, end);
            ResetAStarData();
            ResetFunnelArea();
            showConnerViewHandle?.Invoke(connerLst);
            return connerLst;
        }

        
        bool OnXZSegment(NavVector3 p1, NavVector3 p2, NavVector3 p) {
            NavVector3 v1 = p1 - p;
            NavVector3 v2 = p2 - p;
            bool isCollineation = NavVector3.CrossXZ(v1, v2) == 0;
            bool isNoPrjLen = NavVector3.DotXZ(v1, v2) <= 0;
            return isCollineation && isNoPrjLen;
        }
        
        public bool IsInArea(NavVector3 point, int areaID)
        {
            if (areaID > areaArr.Length) return false;
            var area = areaArr[areaID];
            if (point.x < area.min.x || point.x > area.max.x || point.z < area.min.z || point.z > area.max.z)
                return false;
            var count = area.indexArr.Length;
            var isIN = false;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var p0 = pointsArr[area.indexArr[j]];
                var p1 = pointsArr[area.indexArr[i]];
                if (OnXZSegment(p0,p1,point))
                {
                    return true;
                }
                //PNPoly
                //website: https://wrfranklin.org/Research/Short_Notes/pnpoly.html
                if((p0.z<point.z)!=(p1.z<point.z) && point.x<(p1.x-p0.x)*(point.z-p0.z)/ (p1.z - p0.z)+p0.x)
                {
                    isIN = !isIN;
                }
            }

            return isIN;
        }

        public int GetNavAreaID(NavVector3 pos)
        {
            var areaID = -1;
            foreach (var area in areaArr)
            {
                var checkID = area.areaID;
                if (IsInArea(pos, checkID))
                {
                    areaID = checkID;
                    break;
                }
            }

            return areaID;
        }
    }
}