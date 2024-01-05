using System;
using UnityEngine;
using Object = System.Object;

namespace FunnelAlgorithm
{
    public struct NavVector
    {
        public float x;
        public float y;
        public float z;

        public NavVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NavVector(NavVector v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public NavVector(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static NavVector Zero => new(0, 0, 0);

        public static NavVector One => new(1, 1, 0);

        public static NavVector operator +(NavVector v1, NavVector v2)
        {
            return new NavVector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static NavVector operator -(NavVector v1, NavVector v2)
        {
            return new NavVector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static NavVector operator *(NavVector v1, float value)
        {
            return new NavVector(v1.x * value, v1.y * value, v1.z * value);
        }

        public static NavVector operator /(NavVector v1, float value)
        {
            return new NavVector(v1.x / value, v1.y / value, v1.z / value);
        }

        public static bool operator ==(NavVector v1, NavVector v2)
        {
            return (Math.Abs(v1.x - v2.x) <= 0.01f && Math.Abs(v1.y - v2.y) <= 0.01f && Math.Abs(v1.z - v2.z) <= 0.01f);
        }

        public static bool operator !=(NavVector v1, NavVector v2)
        {
            return Math.Abs(v1.x - v2.x) > 0.01f || Math.Abs(v1.y - v2.y) > 0.01f || Math.Abs(v1.z - v2.z) > 0.01f;
        }

        public static float Distance(NavVector v1, NavVector v2)
        {
            return (v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
        }

        public Vector3 ConvertToUnityVector()
        {
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// [x1,y1]x[x2,y2]=x1y2-x2y1
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float CrossXZ(NavVector v1, NavVector v2)
        {
            return v1.x * v2.z - v2.x * v1.z;
        }

        public static float DotXZ(NavVector v1, NavVector v2)
        {
            return v1.x * v2.x + v1.z * v2.z;
        }
        
        /// <summary>
        /// judge p is a inner point of p1p2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsSegment(NavVector p1, NavVector p2,NavVector p)
        {
            var v1 = p1 - p;
            var v2 = p2 - p;
            var isInLine = CrossXZ(v1, v2) == 0;
            var isNoProjection = DotXZ(v1, v2) <= 0;

            return isInLine && isNoProjection;
        }

        public override bool Equals(object obj)
        {
            if (obj is NavVector vector)
            {
                return Math.Abs(vector.x - x) < 0.01f && Math.Abs(vector.y - y) < 0.01f &&
                       Math.Abs(vector.z - z) < 0.01f;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }
    }
}