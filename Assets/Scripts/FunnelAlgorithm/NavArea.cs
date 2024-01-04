using System.Collections.Generic;

namespace FunnelAlgorithm
{
    public class NavArea
    {
        public readonly int[] indexArr;
        private readonly NavVector[] pointsArr;
        public readonly int areaID;
        public NavVector center = NavVector.Zero;
        private NavVector min = new NavVector(float.MaxValue, float.MaxValue, float.MaxValue);
        private NavVector max = new NavVector(float.MinValue, float.MinValue, float.MinValue);
        public List<NavBorder> borderList;
        
        //A Star
        private NavVector start = NavVector.Zero;
        private float weight;
        private float sumDistance = float.PositiveInfinity;
        private NavArea preArea;

        public NavArea(int id, int[] indexes, NavVector[] points)
        {
            areaID = id;
            indexArr = indexes;
            pointsArr = points;

            foreach (var t in indexArr)
            {
                var v = pointsArr[t];
                if (min.x > v.x) min.x = v.x;
                if (min.y > v.y) min.y = v.y;
                if (min.z > v.z) min.z = v.z;
                if (max.x < v.x) max.x = v.x;
                if (max.y < v.y) max.y = v.y;
                if (max.z < v.z) max.z = v.z;

                center += v;
            }

            center = new NavVector(center.x / 4, center.y / 4, center.z / 4);
        }
        
        /// <summary>
        /// Calculate Distance by center
        /// </summary>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        public float CalNavAreaDisCenter(NavArea neighbour)
        {
            return NavVector.Distance(center, neighbour.center);
        }

        /// <summary>
        /// Calculate distance by vertice
        /// </summary>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        public float CalNavAreaDis(NavArea neighbour)
        {
            var indexes = neighbour.indexArr;
            var minDis = float.MaxValue;
            for (int i = 0; i < indexes.Length; i++)
            {
                var v = pointsArr[indexes[i]];
                var dis = NavVector.Distance(start, v);
                if (minDis > dis)
                {
                    minDis = dis;
                    neighbour.start = v;    
                }
            }

            return minDis;
        }
    }
}