using System.Collections.Generic;
using FunnelAlgorithm.Utility;
using UnityEngine;

namespace FunnelAlgorithm
{
    public partial class NavMap
    {
#region AStar

        private NavArea startArea;
        private NavArea endArea;

        private readonly PriorityQueue<NavArea> openList = new PriorityQueue<NavArea>(4);
        private readonly List<NavArea> closeList = new List<NavArea>();
        private List<NavArea> pathList = new List<NavArea>();

        public List<NavArea> CalAStarPolyPath(NavArea start, NavArea end)
        {
            startArea = start;
            endArea = end;
            openList.Clear();
            closeList.Clear();
            
            openList.Enqueue(start);
            startArea.sumDistance = 0;
            while (openList.Count>0)
            {
                if (openList.Contains(end))
                {
                    pathList = GetPathNavAreas(end);
                    showPathAreaHandle?.Invoke(pathList);
                    closeList.Add(end);
                    break;
                }

                NavArea detectArea = openList.Dequeue();
                if (!closeList.Contains(detectArea))
                {
                    closeList.Add(detectArea);
                }
                //detect neighbour areas
                for (int i = 0; i < detectArea.borderList.Count; i++)
                {
                    NavBorder border = detectArea.borderList[i];
                    NavArea neighbourArea = detectArea.areaID == border.areaID1
                        ? areaArr[border.areaID2]
                        : areaArr[border.areaID1];
                    DetectNeighbourArea(detectArea,neighbourArea);
                }
            }
            return pathList;
        }

        /// <summary>
        /// calculate priority for each neighbour area
        /// </summary>
        /// <param name="detectArea"></param>
        /// <param name="neighbourArea"></param>
        private void DetectNeighbourArea(NavArea detectArea, NavArea neighbourArea)
        {
            if (!closeList.Contains(neighbourArea))
            {
                float neighborDistance = detectArea.CalNavAreaDis(neighbourArea);
                float newSumDistance = detectArea.sumDistance + neighborDistance;
                //update area's sumdistance
                if (float.IsPositiveInfinity(neighbourArea.sumDistance) || newSumDistance < neighbourArea.sumDistance)
                {
                    neighbourArea.preArea = detectArea;
                    neighbourArea.sumDistance = newSumDistance;
                }
                //if this is a new area
                if (!openList.Contains(neighbourArea))
                {
                    float targetDistance = neighbourArea.CalNavAreaDis(endArea); //cal H
                    neighbourArea.weight = neighbourArea.sumDistance + targetDistance; //f = g + h
                    openList.Enqueue(neighbourArea);
                }
            }
        }

        private List<NavArea> GetPathNavAreas(NavArea end)
        {
            List<NavArea> list = new List<NavArea>();
            list.Add(end);
            NavArea pre = end.preArea;
            while (pre!=null)
            {
                list.Insert(0,pre);
                pre = pre.preArea;
            }

            int startID, endID;
            for (int i = 0; i < list.Count-1; i++)
            {
                startID = list[i].areaID;
                endID = list[i + 1].areaID;
                string key;
                if (startID < endID)
                {
                    key = $"{startID}-{endID}";
                }
                else
                {
                    key = $"{endID}-{startID}";
                }

                list[i].targetBorder = GetBorderByAreaIDKey(key);
            }

            return list;
        }
        
        void ResetAStarData() {
            for(int i = 0; i < closeList.Count; i++) {
                closeList[i].Reset();
            }

            List<NavArea> lst = openList.ToList();
            for(int i = 0; i < lst.Count; i++) {
                lst[i].Reset();
            }
        }

#endregion


#region Funnel
        /// <summary>
        /// 拐点数据
        /// </summary>
        private List<NavVector> posLst = null;
        /// <summary>
        /// 漏斗顶点
        /// </summary>
        private NavVector funnelPos = NavVector.Zero;
        /// <summary>
        /// 当前左侧节点
        /// </summary>
        private int curLeftIndex = -1;
        /// <summary>
        /// 当前右侧节点
        /// </summary>
        private int curRightIndex = -1;
        /// <summary>
        /// 左极限缓存
        /// </summary>
        private int leftLimitIndex = -1;
        /// <summary>
        /// 右极限缓存
        /// </summary>
        private int rightLimitIndex = -1;
        /// <summary>
        /// 左极限向量
        /// </summary>
        private NavVector leftLimitDir=NavVector.Zero;
        /// <summary>
        /// 右极限向量
        /// </summary>
        private NavVector rightLimitDir = NavVector.Zero;
        /// <summary>
        /// 左右监测点和检测向量
        /// </summary>
        private int leftCheckIndex = -1;
        private int rightCheckIndex = -1;
        private NavVector leftCheckDir=NavVector.Zero;
        private NavVector rightCheckDir=NavVector.Zero;

        private List<NavVector> CalFunnelConnerPath(List<NavArea> areaLst,NavVector start, NavVector end)
        {
            posLst = new List<NavVector>(){start};
            funnelPos = start;
            var initAreaIndex = GetNextAreaID(areaLst);
            if (initAreaIndex==-1)
            {
                posLst.Add(end);
                return posLst;
            }
            this.LogCyan($"InitAreaID:{initAreaIndex}");
            FunnelShirkEnum leftFSE, rightFSE;
            for (int initIndex = initAreaIndex+1,count = areaLst.Count; initIndex < count; initIndex++)
            {
                if (initIndex == count - 1)
                {
                    //Todo:
                }
                else
                {
                    //Todo: 执行漏斗移动逻辑
                    CalFunnelAction(areaLst[initIndex]);
                }
            }
            return posLst;
        }

        private int GetNextAreaID(List<NavArea> areaLst)
        {
            var initAreaID = -1;
            if (areaLst.Count==0)
            {
                return initAreaID;
            }

            for (int i = 0; i < areaLst.Count; i++)
            {
                if (IsFunnelInitArea(areaLst[i]))
                {
                    initAreaID = areaLst[i].areaID;
                    break;
                }
            }
            return initAreaID;
        }

        private bool IsFunnelInitArea(NavArea area)
        {
            if (area.targetBorder == null) return false;
            var index1 = area.targetBorder.pointIndex1;
            var index2 = area.targetBorder.pointIndex2;
            var v1 = pointsArr[index1] - funnelPos;
            var v2 = pointsArr[index2] - funnelPos;
            NavView.DebugDrawLine(pointsArr[index1],funnelPos,Color.cyan,5);
            NavView.DebugDrawLine(pointsArr[index2],funnelPos,Color.cyan,5);
            float cross = NavVector.CrossXZ(v1, v2);
            switch (cross)
            {
                case > 0:
                    curLeftIndex = index2;
                    curRightIndex = index1;
                    leftLimitIndex = index2;
                    rightLimitIndex = index1;
                    leftLimitDir = v2;
                    rightLimitDir = v1;
                    return true;
                case < 0:
                    curLeftIndex = index1;
                    curRightIndex = index2;
                    leftLimitIndex = index1;
                    rightLimitIndex = index2;
                    leftLimitDir = v1;
                    rightLimitDir = v2;
                    return true;
                default:
                    return false;
            }
        }

        private void CalFunnelAction(NavArea area)
        {
            
        }
        //Todo:
        public void ResetFunnelArea()
        {
            
        }
#endregion

    }
}