using System;
using System.Collections.Generic;

namespace FunnelAlgorithm
{
    public class NavArea : IComparable<NavArea>
    {
        public readonly int[] indexArr;
        private readonly NavVector3[] pointsArr;
        public readonly int areaID;
        public NavBorder targetBorder;
        public NavVector3 center = NavVector3.Zero;
        public NavVector3 min = new NavVector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public NavVector3 max = new NavVector3(float.MinValue, float.MinValue, float.MinValue);
        public List<NavBorder> borderList;
        
        //A Star
        private NavVector3 start = NavVector3.Zero;
        public float weight;
        public float sumDistance = float.PositiveInfinity;
        public NavArea preArea;

        public NavArea(int id, int[] indexes, NavVector3[] points)
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

            center = new NavVector3(center.x / indexArr.Length, center.y / indexArr.Length, center.z / indexArr.Length);
        }
        
        /// <summary>
        /// Calculate Distance by center
        /// </summary>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        public float CalNavAreaDisCenter(NavArea neighbour)
        {
            return NavVector3.Distance(center, neighbour.center);
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
                var dis = NavVector3.Distance(start, v);
                if (minDis > dis)
                {
                    minDis = dis;
                    neighbour.start = v;    
                }
            }

            return minDis;
        }

        public int CompareTo(NavArea other)
        {
            if (weight< other.weight)
            {
                return -1;
            }
            else if (weight > other.weight)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        
        public void Reset() {
            targetBorder = null;
            start = NavVector3.Zero;
            weight = 0;
            sumDistance = float.PositiveInfinity;
            preArea = null;
        }
        public override string ToString() {
            return $"AreaID:{areaID}";
        }
    }
}