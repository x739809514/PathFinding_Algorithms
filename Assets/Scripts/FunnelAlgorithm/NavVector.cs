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

        public override bool Equals(object obj)
        {
            if (obj is NavVector vector)
            {
                return Math.Abs(vector.x - x) < 0.01f && Math.Abs(vector.y - y) < 0.01f && Math.Abs(vector.z - z) < 0.01f;
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