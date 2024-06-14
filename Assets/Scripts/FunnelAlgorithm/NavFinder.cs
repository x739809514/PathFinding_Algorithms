using System;
using System.Collections.Generic;
using FunnelAlgorithm.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FunnelAlgorithm
{
    /// <summary>
    /// particular finding logic
    /// </summary>
    public partial class NavMap
    {
#region AStar

        private NavArea startArea;
        private NavArea endArea;

        private readonly PriorityQueue<NavArea> openList = new PriorityQueue<NavArea>(4);
        private readonly List<NavArea> closeList = new List<NavArea>();
        private List<NavArea> pathList = new List<NavArea>();
        /*private List<int> leftConnerLst = new List<int>();
        private List<int> rightConnerLst = new List<int>();*/

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
                // get the area has highest priority, add it to close list
                NavArea detectArea = openList.Dequeue();
                if (!closeList.Contains(detectArea))
                {
                    closeList.Add(detectArea);
                }

                //continue detect neighbors of the above area
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
        
        // output final path
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
                //this.LogGreen($"{list[i].targetBorder.pointIndex1}---{list[i].targetBorder.pointIndex2}");
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
        private List<NavVector3> posLst = null;
        /// <summary>
        /// 漏斗顶点
        /// </summary>
        private NavVector3 funnelPos = NavVector3.Zero;
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
        private NavVector3 leftLimitDir = NavVector3.Zero;
        /// <summary>
        /// 右极限向量
        /// </summary>
        private NavVector3 rightLimitDir = NavVector3.Zero;
        /// <summary>
        /// 左右监测点和检测向量
        /// </summary>
        private int leftCheckIndex = -1;
        private int rightCheckIndex = -1;
        private NavVector3 leftCheckDir = NavVector3.Zero;
        private NavVector3 rightCheckDir = NavVector3.Zero;
        readonly List<int> leftConnerLst = new List<int>();
        readonly List<int> rightConnerLst = new List<int>();

        private List<NavVector3> CalFunnelConnerPath(List<NavArea> areaLst, NavVector3 start, NavVector3 end)
        {
            posLst = new List<NavVector3>() { start };
            funnelPos = start;
            leftConnerLst.Clear();
            rightConnerLst.Clear();
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
                    //直到最后一个节点
                    NavVector3 endVector = end - funnelPos;
                    leftCheckDir = endVector;
                    rightCheckDir = endVector;
                    bool inLeft = NavVector3.CrossXZ(leftLimitDir, endVector)>0;
                    bool inRight = NavVector3.CrossXZ(rightLimitDir, endVector) < 0;
                    if(inLeft)
                        CalcEndConner(leftConnerLst,leftLimitIndex,leftLimitDir,end);
                    else if (inRight)
                        CalcEndConner(rightConnerLst, rightLimitIndex, rightLimitDir, end);
                    else
                        posLst.Add(end);
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
                            //this.LogGreen($"{areaLst[initIndex].targetBorder.pointIndex1}___{areaLst[initIndex].targetBorder.pointIndex2}");
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

                    curLeftIndex = leftCheckIndex;
                    curRightIndex = rightCheckIndex;
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
            NavVector3 rldn = NavVector3.NormalXZ(rightLimitDir);
            while (connerIndex<rightConnerLst.Count)
            {
                float rad = float.MaxValue;
                for (int i = connerIndex; i < rightConnerLst.Count; i++)
                {
                    NavVector3 ckdn = NavVector3.NormalXZ(pointsArr[rightConnerLst[i]]-funnelPos);
                    float curRad = Mathf.Abs(NavVector3.AngleXZ(rldn, ckdn));
                    if (curRad <= rad)
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
                rightLimitDir = pointsArr[rightLimitIndex] - funnelPos;

                float cross = NavVector3.CrossXZ(leftLimitDir, rightLimitDir);
                if (cross>0)
                {
                    funnelPos = pointsArr[rightLimitIndex];
                    posLst.Add(funnelPos);
                    ++connerIndex;
                    if (connerIndex >= rightConnerLst.Count)
                    {
                        rightLimitIndex = -1;
                        rightLimitDir = NavVector3.Zero;

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
                rightLimitDir = NavVector3.Zero;
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
            NavVector3 lldn = NavVector3.NormalXZ(leftLimitDir);
            while(connerIndex < leftConnerLst.Count) {
                float rad = float.MaxValue;
                for(int i = connerIndex; i < leftConnerLst.Count; i++) {
                    NavVector3 ckdn = NavVector3.NormalXZ(pointsArr[leftConnerLst[i]] - funnelPos);
                    float curRad = MathF.Abs(NavVector3.AngleXZ(lldn, ckdn));
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
                float cross = NavVector3.CrossXZ(leftLimitDir, rightLimitDir);
                if(cross > 0) {
                    funnelPos = pointsArr[leftLimitIndex];
                    posLst.Add(funnelPos);
                    ++connerIndex;
                    if(connerIndex >= leftConnerLst.Count) {
                        leftLimitIndex = -1;
                        leftLimitDir = NavVector3.Zero;

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
                leftLimitDir = NavVector3.Zero;

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
            this.LogGreen(index1+"----"+index2);
            var v1 = pointsArr[index1] - funnelPos;
            var v2 = pointsArr[index2] - funnelPos;
            float cross = NavVector3.CrossXZ(v1, v2);
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
        /// <summary>
        /// 计算漏斗边
        /// </summary>
        /// <param name="area"></param>
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
                int curIndex = area.indexArr[(i + offset) % count];
                if (curIndex  == checkIndex1)
                {
                    leftCheckIndex = checkIndex1;
                    rightCheckIndex = checkIndex2;
                    leftCheckDir = checkVector1;
                    rightCheckDir = checkVector2;
                    break;
                }
                else if (curIndex == checkIndex2)
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
            }
            if (leftLimitDir == NavVector3.Zero)
                leftLimitDir = leftCheckDir;
            if (rightLimitDir == NavVector3.Zero)
                rightLimitDir = rightCheckDir;
        }
        private FunnelShirkEnum CalLeftFunnelChange()
        {
            FunnelShirkEnum funnelShirk;
            var ll = NavVector3.CrossXZ(leftLimitDir, leftCheckDir);
            if (ll > 0)
            {
                funnelShirk = FunnelShirkEnum.LeftToLeft;
            }else if (ll == 0)
            {
                funnelShirk = FunnelShirkEnum.None;
            }
            else
            {
                var rl = NavVector3.CrossXZ(rightLimitDir, leftCheckDir);
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
            var rr = NavVector3.CrossXZ(rightLimitDir, rightCheckDir);
            if (rr < 0)
            {
                funnelShirk = FunnelShirkEnum.RightToRight;
            }else if (rr == 0)
            {
                funnelShirk = FunnelShirkEnum.None;
            }
            else
            {
                var lr = NavVector3.CrossXZ(leftLimitDir, rightCheckDir);
                if (lr < 0)
                {
                    funnelShirk = FunnelShirkEnum.RightToCenter;
                }
                else
                {
                    funnelShirk = FunnelShirkEnum.RightToLeft;
                }
            }

            return funnelShirk;
        }
        private void CalcEndConner(List<int> cornerIndexLst, int limitIndex, NavVector3 limitDir, NavVector3 end) {
            funnelPos = pointsArr[limitIndex];
            posLst.Add(funnelPos);
            List<NavVector3> cornerPosLst = new List<NavVector3>();
            for(int i = 0; i < cornerIndexLst.Count; i++) {
                cornerPosLst.Add(pointsArr[cornerIndexLst[i]]);
            }
            bool isExist = false;
            for(int i = 0; i < cornerPosLst.Count; i++) {
                if(cornerPosLst[i] == end) {
                    isExist = true;
                    break;
                }
            }
            if(!isExist) cornerPosLst.Add(end);

            NavVector3 ln = NavVector3.NormalXZ(limitDir);
            int connerIndex = 0;
            while(connerIndex < cornerPosLst.Count) {
                float rad = float.MaxValue;
                for(int j = connerIndex; j < cornerPosLst.Count; j++) {
                    NavVector3 checkVec = cornerPosLst[j] - funnelPos;
                    NavVector3 ckn = NavVector3.NormalXZ(checkVec);
                    float curRad = MathF.Abs(NavVector3.AngleXZ(ln, ckn));
                    if(curRad < rad) {
                        rad = curRad;
                        connerIndex = j;
                    }
                }
                funnelPos = cornerPosLst[connerIndex];
                posLst.Add(funnelPos);
                connerIndex++;
            }
        }
        public void ResetFunnelArea()
        {
            curLeftIndex = -1;
            curRightIndex = -1;
            leftLimitIndex = -1;
            rightLimitIndex = -1;
            leftLimitDir = NavVector3.Zero;
            rightLimitDir = NavVector3.Zero;
            leftCheckIndex = -1;
            rightCheckIndex = -1;
            leftCheckDir = NavVector3.Zero;
            rightCheckDir = NavVector3.Zero;
        }
#endregion
    }
}