using System;
using UnityEngine;
using Object = System.Object;

namespace FunnelAlgorithm
{
    public struct NavVector3
    {
        public float x;
        public float y;
        public float z;

        public NavVector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public NavVector3(float x, float z) {
            this.x = x;
            this.y = 0;
            this.z = z;
        }

        public NavVector3(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public static NavVector3 Zero {
            get {
                return new NavVector3(0, 0, 0);
            }
        }
        public static NavVector3 One {
            get {
                return new NavVector3(1, 1, 1);
            }
        }
        public static NavVector3 operator +(NavVector3 v1, NavVector3 v2) {
            return new NavVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static NavVector3 operator -(NavVector3 v1, NavVector3 v2) {
            return new NavVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static NavVector3 operator *(NavVector3 v, float value) {
            return new NavVector3(v.x * value, v.y * value, v.z * value);
        }
        public static NavVector3 operator /(NavVector3 v, float value) {
            return new NavVector3(v.x / value, v.y / value, v.z / value);
        }
        public static bool operator ==(NavVector3 v1, NavVector3 v2) {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }
        public static bool operator !=(NavVector3 v1, NavVector3 v2) {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        public static float DotXZ(NavVector3 v1, NavVector3 v2) {
            return v1.x * v2.x + v1.z * v2.z;
        }
        public static NavVector3 CrossXYZ(NavVector3 v1, NavVector3 v2) {
            return new NavVector3() {
                x = v1.y * v2.z - v1.z * v2.y,
                y = v1.z * v2.x - v1.x * v2.z,
                z = v1.x * v2.y - v1.y * v2.x
            };
        }
        public static float CrossXZ(NavVector3 v1, NavVector3 v2) {
            return v1.x * v2.z - v2.x * v1.z;
        }
        public static NavVector3 NormalXZ(NavVector3 v) {
            float len = MathF.Sqrt(v.x * v.x + v.z * v.z);
            NavVector3 nor = new NavVector3 {
                x = v.x / len,
                y = 0,
                z = v.z / len
            };
            return nor;
        }
        public static float AngleXZ(NavVector3 v1, NavVector3 v2) {
            float dot = DotXZ(v1, v2);
            float angle = MathF.Acos(dot);
            NavVector3 cross = CrossXYZ(v1, v2);
            if(cross.y < 0) {
                angle = -angle;
            }
            return angle;
        }
        public static float Distance(NavVector3 v1, NavVector3 v2) {
            return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
        }

        public override bool Equals(object obj) {
            return obj is NavVector3 vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }
        public override int GetHashCode() {
            return HashCode.Combine(x, y, z);
        }

        public Vector3 ConvertToUnityVector() {
            return new Vector3(x, y, z);
        }

        public override string ToString() {
            return $"[{x},{z}]";
        }
    }
}