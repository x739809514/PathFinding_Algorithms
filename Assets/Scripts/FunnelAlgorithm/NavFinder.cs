using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunnelAlgorithm.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        private List<int> leftConnerLst = new List<int>();
        private List<int> rightConnerLst = new List<int>();

        public List<NavArea> CalAStarPolyPath(NavArea start, NavArea end)
        {
            startArea = start;
            endArea = end;
            openList.Clear();
            closeList.Clear();

            openList.Enqueue(start);
            startArea.sumDistance = 0;
            while (openList.Count > 0)
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
                    DetectNeighbourArea(detectArea, neighbourArea);
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
            while (pre != null)
            {
                list.Insert(0, pre);
                pre = pre.preArea;
            }

            int startID, endID;
            for (int i = 0; i < list.Count - 1; i++)
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

        void ResetAStarData()
        {
            for (int i = 0; i < closeList.Count; i++)
            {
                closeList[i].Reset();
            }

            List<NavArea> lst = openList.ToList();
            for (int i = 0; i < lst.Count; i++)
            {
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
        private NavVector leftLimitDir = NavVector.Zero;
        /// <summary>
        /// 右极限向量
        /// </summary>
        private NavVector rightLimitDir = NavVector.Zero;
        /// <summary>
        /// 左右监测点和检测向量
        /// </summary>
        private int leftCheckIndex = -1;
        private int rightCheckIndex = -1;
        private NavVector leftCheckDir = NavVector.Zero;
        private NavVector rightCheckDir = NavVector.Zero;

        private List<NavVector> CalFunnelConnerPath(List<NavArea> areaLst, NavVector start, NavVector end)
        {
            posLst = new List<NavVector>() { start };
            funnelPos = start;
            var initAreaIndex = GetInitAreaIndex(areaLst);
            if (initAreaIndex == -1)
            {
                posLst.Add(end);
                return posLst;
            }

            this.LogCyan($"InitAreaID:{initAreaIndex}");
            FunnelShirkEnum leftFSE, rightFSE;
            for (int initIndex = initAreaIndex + 1, count = areaLst.Count; initIndex < count; initIndex++)
            {
                if (initIndex == count - 1)
                {
                    //Todo:
                }
                else
                {
                    CalCheckFunnel(areaLst[initIndex]);
                    leftFSE = CalLeftFunnelChange();
                    rightFSE = CalRightFunnelChange();
                    if (leftFSE == FunnelShirkEnum.LeftToLeft && leftConnerLst.Contains(leftCheckIndex)==false)
                    {
                        leftConnerLst.Add(leftCheckIndex);
                    }
                    if (rightFSE == FunnelShirkEnum.RightToRight && rightConnerLst.Contains(rightCheckIndex)==false)
                    {
                        rightConnerLst.Add(rightCheckIndex);
                    }

                    switch (leftFSE)
                    {
                        case FunnelShirkEnum.None:
                            leftLimitIndex = leftCheckIndex;
                            break;
                        case FunnelShirkEnum.LeftToCenter:
                            leftLimitDir = leftCheckDir;
                            leftLimitIndex = leftCheckIndex;
                            leftConnerLst.Clear();
                            break;
                        case FunnelShirkEnum.LeftToRight:
                            this.LogGreen($"{areaLst[initIndex].targetBorder.pointIndex1}___{areaLst[initIndex].targetBorder.pointIndex2}");
                            CalLeftToRightFunnel();
                            break;
                        default:    
                            break;
                    }

                    switch (rightFSE)
                    {
                        case FunnelShirkEnum.None:
                            rightLimitIndex = rightCheckIndex;
                            break;
                        case FunnelShirkEnum.RightToCenter:
                            rightLimitDir = rightCheckDir;
                            rightLimitIndex = rightCheckIndex;
                            rightConnerLst.Clear();
                            break;
                        case FunnelShirkEnum.RightToLeft:
                            CalRightToLeftFunnel();
                            break;
                        default:    
                            break;
                    }
                }
            }

            return posLst;
        }

        private void CalLeftToRightFunnel()
        {
            funnelPos = pointsArr[rightLimitIndex];
            posLst.Add(funnelPos);

            var updateLimit = false;
            int connerIndex = 0;
            NavVector rldn = NavVector.NormalXZ(rightLimitDir);
            while (connerIndex<rightConnerLst.Count)
            {
                float rad = float.MaxValue;
                for (int i = connerIndex; i < rightConnerLst.Count; i++)
                {
                    NavVector ckdn = NavVector.NormalXZ(pointsArr[rightConnerLst[i]]-funnelPos);
                    float curRad = Mathf.Abs(NavVector.AngleXZ(rldn, ckdn));
                    if (curRad < rad)
                    {
                        connerIndex = i;
                        rad = curRad;
                    }
                }

                updateLimit = true;
                //update funnel limit pos
                leftLimitIndex = leftCheckIndex;
                leftLimitDir = pointsArr[leftLimitIndex] - funnelPos;
                rightLimitIndex = rightConnerLst[connerIndex];
                rightLimitDir = pointsArr[leftLimitIndex] - funnelPos;

                float cross = NavVector.CrossXZ(leftLimitDir, rightLimitDir);
                if (cross>0)
                {
                    funnelPos = pointsArr[rightLimitIndex];
                    posLst.Add(funnelPos);
                    ++connerIndex;
                    if (connerIndex >= rightConnerLst.Count)
                    {
                        rightLimitIndex = -1;
                        rightCheckDir = NavVector.Zero;

                        leftLimitDir = pointsArr[leftLimitIndex] - funnelPos;
                    }
                }
                else
                {
                    for (int i = 0; i < connerIndex; i++)
                    {
                        rightConnerLst.RemoveAt(0);
                    }
                    break;
                }

            }

            if (updateLimit==false)
            {
                rightLimitIndex = -1;
                rightCheckDir = NavVector.Zero;
                leftLimitIndex = leftCheckIndex;
                leftLimitDir = pointsArr[leftLimitIndex] - funnelPos;
            }
        }

        private void CalRightToLeftFunnel()
        {
           funnelPos = pointsArr[leftLimitIndex];
            posLst.Add(funnelPos);

            bool updateLimit = false;
            int connerIndex = 0;
            NavVector lldn = NavVector.NormalXZ(leftLimitDir);
            while(connerIndex < leftConnerLst.Count) {
                float rad = float.MaxValue;
                for(int i = connerIndex; i < leftConnerLst.Count; i++) {
                    NavVector ckdn = NavVector.NormalXZ(pointsArr[leftConnerLst[i]] - funnelPos);
                    float curRad = MathF.Abs(NavVector.AngleXZ(lldn, ckdn));
                    if(curRad <= rad) {
                        connerIndex = i;
                        rad = curRad;
                    }
                }

                updateLimit = true;
                leftLimitIndex = leftConnerLst[connerIndex];
                leftLimitDir = pointsArr[leftLimitIndex] - funnelPos;

                rightLimitIndex = rightCheckIndex;
                rightLimitDir = pointsArr[rightLimitIndex] - funnelPos;
                float cross = NavVector.CrossXZ(leftLimitDir, rightLimitDir);
                if(cross > 0) {
                    funnelPos = pointsArr[leftLimitIndex];
                    posLst.Add(funnelPos);
                    ++connerIndex;
                    if(connerIndex >= leftConnerLst.Count) {
                        leftLimitIndex = -1;
                        leftLimitDir = NavVector.Zero;

                        rightLimitDir = pointsArr[rightLimitIndex] - funnelPos;
                    }
                }
                else {
                    for(int i = 0; i < connerIndex + 1; i++) {
                        leftConnerLst.RemoveAt(0);
                    }
                    break;
                }
            }

            if(!updateLimit) {
                leftLimitIndex = -1;
                leftLimitDir = NavVector.Zero;

                rightLimitIndex = rightCheckIndex;
                rightLimitDir = pointsArr[rightLimitIndex] - funnelPos;
            }
        }

        private int GetInitAreaIndex(List<NavArea> areaLst)
        {
            var initAreaIndex = -1;
            if (areaLst.Count == 0)
            {
                return initAreaIndex;
            }

            for (int i = 0; i < areaLst.Count; i++)
            {
                if (IsFunnelInitArea(areaLst[i]))
                {
                    initAreaIndex = i;
                    break;
                }
            }

            return initAreaIndex;
        }
        private bool IsFunnelInitArea(NavArea area)
        {
            if (area.targetBorder == null) return false;
            var index1 = area.targetBorder.pointIndex1;
            var index2 = area.targetBorder.pointIndex2;
            var v1 = pointsArr[index1] - funnelPos;
            var v2 = pointsArr[index2] - funnelPos;
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
        private void CalCheckFunnel(NavArea area)
        {
            var checkIndex1 = area.targetBorder.pointIndex1;
            var checkIndex2 = area.targetBorder.pointIndex2;
            var checkVector1 = pointsArr[checkIndex1] - funnelPos;
            var checkVector2 = pointsArr[checkIndex2] - funnelPos;
            //NavView.DebugDrawLine(pointsArr[checkIndex1], funnelPos, Color.cyan, 5);
            //NavView.DebugDrawLine(pointsArr[checkIndex2], funnelPos, Color.cyan, 5);

            var offset = 0;
            var count = area.indexArr.Length;
            for (int i = 0; i < count; i++)
            {
                if (area.indexArr[i] == curLeftIndex)
                {
                    offset = i;
                    break;
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (area.indexArr[(i + offset)% count]  == checkIndex1)
                {
                    leftCheckIndex = checkIndex1;
                    rightCheckIndex = checkIndex2;
                    leftCheckDir = checkVector1;
                    rightCheckDir = checkVector2;
                    break;
                }
                else if (area.indexArr[(i + offset) % count] == checkIndex2)
                {
                    leftCheckIndex = checkIndex2;
                    rightCheckIndex = checkIndex1;
                    leftCheckDir = checkVector2;
                    rightCheckDir = checkVector1;
                    break;
                }
                else
                {
                    Debug.Log($"loop index:{i + offset}");
                }

                //Todo:
                if (leftLimitDir == NavVector.Zero)
                    leftLimitDir = leftCheckDir;
                if (rightLimitDir == NavVector.Zero)
                    rightLimitDir = rightCheckDir;
            }
        }
        private FunnelShirkEnum CalLeftFunnelChange()
        {
            FunnelShirkEnum funnelShirk;
            var ll = NavVector.CrossXZ(leftLimitDir, leftCheckDir);
            if (ll > 0)
            {
                funnelShirk = FunnelShirkEnum.LeftToLeft;
            }else if (ll == 0)
            {
                funnelShirk = FunnelShirkEnum.None;
            }
            else
            {
                var rl = NavVector.CrossXZ(rightLimitDir, leftCheckDir);
                if (rl > 0)
                {
                    funnelShirk = FunnelShirkEnum.LeftToCenter;
                }
                else
                {
                    funnelShirk = FunnelShirkEnum.LeftToRight;
                }
            }

            return funnelShirk;
        }
        private FunnelShirkEnum CalRightFunnelChange()
        {
            FunnelShirkEnum funnelShirk;
            var rr = NavVector.CrossXZ(rightLimitDir, rightCheckDir);
            if (rr < 0)
            {
                funnelShirk = FunnelShirkEnum.RightToRight;
            }else if (rr == 0)
            {
                funnelShirk = FunnelShirkEnum.None;
            }
            else
            {
                var lr = NavVector.CrossXZ(leftLimitDir, rightCheckDir);
                if (lr < 0)
                {
                    funnelShirk = FunnelShirkEnum.LeftToCenter;
                }
                else
                {
                    funnelShirk = FunnelShirkEnum.LeftToRight;
                }
            }

            return funnelShirk;
        }
        
        //Todo:
        public void ResetFunnelArea()
        {
        }

#endregion
    }
}