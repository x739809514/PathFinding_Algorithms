using System.Collections.Generic;
using FunnelAlgorithm.Utility;

namespace FunnelAlgorithm
{
    public partial class NavMap
    {
        private NavArea startArea;
        private NavArea endArea;

        private readonly PriorityQueue<NavArea> detectQue = new PriorityQueue<NavArea>(4);
        private readonly List<NavArea> finishLst = new List<NavArea>();
        private List<NavArea> pathList = new List<NavArea>();

        public List<NavArea> CalAStarPolyPath(NavArea start, NavArea end)
        {
            startArea = start;
            endArea = end;
            detectQue.Clear();
            finishLst.Clear();
            
            detectQue.Enqueue(start);
            startArea.sumDistance = 0;
            while (detectQue.Count>0)
            {
                if (detectQue.Contains(end))
                {
                    pathList = GetPathNavAreas(end);
                    showPathAreaHandle?.Invoke(pathList);
                    finishLst.Add(end);
                    break;
                }

                NavArea detectArea = detectQue.Dequeue();
                if (!finishLst.Contains(detectArea))
                {
                    finishLst.Add(detectArea);
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
            if (!finishLst.Contains(neighbourArea))
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
                if (!detectQue.Contains(neighbourArea))
                {
                    float targetDistance = neighbourArea.CalNavAreaDis(endArea); //cal H
                    neighborDistance = neighbourArea.sumDistance + targetDistance; //f = g + h
                    detectQue.Enqueue(neighbourArea);
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
            for(int i = 0; i < finishLst.Count; i++) {
                finishLst[i].Reset();
            }

            List<NavArea> lst = detectQue.ToList();
            for(int i = 0; i < lst.Count; i++) {
                lst[i].Reset();
            }
        }
    }
}