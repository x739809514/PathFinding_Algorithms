using System.Collections.Generic;

namespace FunnelAlgorithm
{
    public class NavArea
    {
        private int[] indexArr;
        private NavVector[] pointsArr;
        public int areaID;
        public NavVector center = NavVector.Zero;
        private NavVector min = new NavVector(float.MaxValue, float.MaxValue, float.MaxValue);
        private NavVector max = new NavVector(float.MinValue, float.MinValue, float.MinValue);
        public List<NavBorder> borderList;
        
        public NavArea(int id,int[] indexes, NavVector[] points)
        {
            areaID = id;
            indexArr = indexes;
            pointsArr = points;
            
            for (int i = 0; i < indexArr.Length; i++)
            {
                var v = pointsArr[indexArr[i]];
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
    }
}