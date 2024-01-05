using System.Collections.Generic;
using FunnelAlgorithm.Utility;

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

        private List<NavVector> CalFunnelConnerPath(List<NavArea> areaLst,NavVector start, NavVector end)
        {
            posLst = new List<NavVector>(){start};

            return posLst;
        }

        //Todo:
        public void ResetFunnelArea()
        {
            
        }
#endregion

    }
}