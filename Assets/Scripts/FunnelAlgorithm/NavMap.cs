using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;

namespace FunnelAlgorithm
{
    public partial class NavMap
    {
        private readonly List<int[]> indexList;
        private readonly NavVector[] pointsArr;
        private NavArea[] areaArr;
        public static Action<NavVector, int> showAreaIDHandle;
        public static Action<List<NavArea>> showPathAreaHandle;
        
        private Dictionary<string, NavBorder> borderList = new Dictionary<string, NavBorder>();
        private Dictionary<string, NavBorder> areaList = new Dictionary<string, NavBorder>();


        public NavMap(List<int[]> indexList, NavVector[] pointsArr)
        {
            this.indexList = indexList;
            this.pointsArr = pointsArr;
            var count = indexList.Count;
            areaArr = new NavArea[count];
            for (int i = 0; i < count; i++)
            {
                areaArr[i] = new NavArea(i, indexList[i], pointsArr);
                showAreaIDHandle?.Invoke(areaArr[i].center,i);
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
                    if (verticeIndex < indexArr.Length-1)
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
                        areaList.Add(areaKey,navBorder);
                    }
                    else
                    {
                        navBorder = new NavBorder()
                        {
                            areaID1 = areaID,
                            pointIndex1 = startIndex,
                            pointIndex2 = endIndex
                        };
                        borderList.Add(key,navBorder);
                    }
                }
            }

            List<string> singles = new List<string>();
            foreach (var navBorder in borderList)
            {
                if (navBorder.Value.isShare==false)
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
                if (border.Value.areaID1==areaID || border.Value.areaID2==areaID)
                {
                    list.Add(border.Value);
                }
            }

            return list;
        }
    }
}