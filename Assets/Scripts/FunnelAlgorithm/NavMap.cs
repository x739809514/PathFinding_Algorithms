using System.Collections.Generic;

namespace FunnelAlgorithm
{
    public class NavMap
    {
        private readonly List<int[]> indexList;
        private readonly NavVector[] pointsArr;
        private NavArea[] areaArr;

        public NavMap(List<int[]> indexList, NavVector[] pointsArr)
        {
            this.indexList = indexList;
            this.pointsArr = pointsArr;
            var count = indexList.Count;
            areaArr = new NavArea[count];
            for (int i = 0; i < count; i++)
            {
                areaArr[i] = new NavArea(i, indexList[i], pointsArr);
            }
        }
    }
}