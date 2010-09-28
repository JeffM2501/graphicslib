/*
    Open Combat/Projekt 2501
    Copyright (C) 2010  Jeffery Allen Myers

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;

using OpenTK;

namespace Math3D
{
    public enum ContainmentType
    {
        Disjoint,
        Contains,
        Intersects,
    }

    public enum PlaneIntersectionType
    {
        Front,
        Back,
        Intersecting,
    }

    public class FloatHelper
    {
        protected static Random rand = new Random();

        public static float DefaultTolerance = 0.0001f;

        public static bool Equals ( float f1, float f2 )
        {
            return Equals(f1,f2,DefaultTolerance);
        }

        public static bool Equals ( float f1, float f2, float tolerance )
        {
            return Math.Abs(f1 - f2) <= tolerance;
        }

        public static float Random ( float max )
        {
            return (float)(rand.NextDouble() * max);
        }

        public static float Random ( float min, float max )
        {
            return Random(max - min) + min;
        }

        public static float CopySign ( float val, float sign )
        {
            return (float)(Math.Abs(val) * (sign / Math.Abs(sign)));
        }
    }

    public class Trig
    {
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI); 
        }

        public static float DegreeToRadian(float angle)
        {
            return (float)Math.PI * angle / 180.0f;
        }

        public static float RadianToDegree(float angle)
        {
            return angle * (180.0f / (float)Math.PI);
        }

        public static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static float Hypot(float x, float y)
        {
            return (float)Math.Sqrt(x * x + y * y);
        }
    }

    public class FloatRand
    {
        static Random rand = new Random();

        public static float RandInRange(float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }

        public static float RandPlusMinus()
        {
            return RandInRange(-1, 1);
        }
    }

    public class FrustumHelper
    {
        public static Plane[] GetPlanes ( BoundingFrustum frustum )
        {
            Plane[] l = new Plane[6];
            l[0] = frustum.Near;
            l[1] = frustum.Left;
            l[2] = frustum.Right;
            l[3] = frustum.Top;
            l[4] = frustum.Bottom;
            l[5] = frustum.Far;

            return l;
        }
    }

    public class PlaneHelper
    {
        // sets a plane from 3 points with out a copy
        public static void Set ( ref Plane plane, Vector3 p1, Vector3 p2, Vector3 p3 )
        {
                        // get normal by crossing v1 and v2 and normalizing
            plane.Normal = Vector3.Cross(p1, p2);
            plane.Normal.Normalize();
            plane.D = -Vector3.Dot(p3, plane.Normal);
        }
    }

    public class MatrixHelper4
    {
        public static Matrix4 FromQuaternion ( Quaternion quat )
        {
            quat.Normalize();
            Matrix4 mat = new Matrix4();
            mat.M11 = 1 - 2 * (quat.Y * quat.Y) - 2 * (quat.Z * quat.Z);
            mat.M12 = 2 * (quat.X * quat.Y) - 2 * (quat.Z * quat.W);
            mat.M13 = 2 * (quat.X * quat.Z) + 2 * (quat.Y * quat.W);
           
            mat.M21 = 2 * (quat.X * quat.Y) + 2 * (quat.Z * quat.W);
            mat.M22 = 1 - 2 * (quat.X * quat.X) - 2 * (quat.Z * quat.Z);
            mat.M23 = 2 * (quat.Y * quat.Z) + 2 * (quat.X * quat.W);

            mat.M31 = 2 * (quat.X * quat.Z) + 2 * (quat.Y * quat.W);
            mat.M32 = 2 * (quat.Y * quat.Z) + 2 * (quat.X * quat.W);
            mat.M32 = 1 - 2 * (quat.X * quat.X) - 2 * (quat.Y * quat.Y);

            mat.Transpose();
            return mat;
        }

        // matrix grid methods
        public static float M11(Matrix4 m) { return m.Row0.X; }
        public static void M11(ref Matrix4 m, float value) { m.Row0.X = value; }

        public static float M12(Matrix4 m) { return m.Row0.Y; }
        public static void M12(ref Matrix4 m, float value) { m.Row0.Y = value; }

        public static float M13(Matrix4 m) { return m.Row0.Z; }
        public static void M13(ref Matrix4 m, float value) { m.Row0.Z = value; }

        public static float M14(Matrix4 m) { return m.Row0.W; }
        public static void M14(ref Matrix4 m, float value) { m.Row0.W = value; }

        public static float M21(Matrix4 m) { return m.Row1.X; }
        public static void M21(ref Matrix4 m, float value) { m.Row1.X = value; }

        public static float M22(Matrix4 m) { return m.Row1.Y; }
        public static void M22(ref Matrix4 m, float value) { m.Row1.Y = value; }

        public static float M23(Matrix4 m) { return m.Row1.Z; }
        public static void M23(ref Matrix4 m, float value) { m.Row1.Z = value; }

        public static float M24(Matrix4 m) { return m.Row1.W; }
        public static void M24(ref Matrix4 m, float value) { m.Row1.W = value; }

        public static float M31(Matrix4 m) { return m.Row2.X; }
        public static void M31(ref Matrix4 m, float value) { m.Row2.X = value; }

        public static float M32(Matrix4 m) { return m.Row2.Y; }
        public static void M32(ref Matrix4 m, float value) { m.Row2.Y = value; }

        public static float M33(Matrix4 m) { return m.Row2.Z; }
        public static void M33(ref Matrix4 m, float value) { m.Row2.Z = value; }

        public static float M34(Matrix4 m) { return m.Row2.W; }
        public static void M34(ref Matrix4 m, float value) { m.Row2.W = value; }

        public static float M41(Matrix4 m) { return m.Row3.X; }
        public static void M41(ref Matrix4 m, float value) { m.Row3.X = value; }

        public static float M42(Matrix4 m) { return m.Row3.Y; }
        public static void M42(ref Matrix4 m, float value) { m.Row3.Y = value; }

        public static float M43(Matrix4 m) { return m.Row3.Z; }
        public static void M43(ref Matrix4 m, float value) { m.Row3.Z = value; }

        public static float M44(Matrix4 m) { return m.Row3.W; }
        public static void M44(ref Matrix4 m, float value) { m.Row3.W = value; }

        // matrix index methods
//         public static float m0(Matrix4 m) { return m.Row0.X; }
//         public static void m0(ref Matrix4 m, float value) { m.Row0.X = value; }
// 
//         public static float m1(Matrix4 m) { return m.Row0.Y; }
//         public static void m1(ref Matrix4 m, float value) { m.Row0.Y = value; }
// 
//         public static float m2(Matrix4 m) { return m.Row0.Z; }
//         public static void m2(ref Matrix4 m, float value) { m.Row0.Z = value; }
//      
//         public static float m3(Matrix4 m) { return m.Row0.W; }
//         public static void m3(ref Matrix4 m, float value) { m.Row0.W = value; }
// 
//         public static float m4(Matrix4 m) { return m.Row1.X; }
//         public static void m4(ref Matrix4 m, float value) { m.Row1.X = value; }
// 
//         public static float m5(Matrix4 m) { return m.Row1.Y; }
//         public static void m5(ref Matrix4 m, float value) { m.Row1.Y = value; }
// 
//         public static float m6(Matrix4 m) { return m.Row1.Z; }
//         public static void m6(ref Matrix4 m, float value) { m.Row1.Z = value; }
// 
//         public static float m7(Matrix4 m) { return m.Row1.W; }
//         public static void m7(ref Matrix4 m, float value) { m.Row1.W = value; }
// 
//         public static float m8(Matrix4 m) { return m.Row2.X; }
//         public static void m8(ref Matrix4 m, float value) { m.Row2.X = value; }
// 
//         public static float m9(Matrix4 m) { return m.Row2.Y; }
//         public static void m9(ref Matrix4 m, float value) { m.Row1.Y = value; }
// 
//         public static float m10(Matrix4 m) { return m.Row2.Z; }
//         public static void m10(ref Matrix4 m, float value) { m.Row2.Z = value; }
// 
//         public static float m11(Matrix4 m) { return m.Row2.W; }
//         public static void m11(ref Matrix4 m, float value) { m.Row2.W = value; }
// 
//         public static float m12(Matrix4 m) { return m.Row3.X; }
//         public static void m12(ref Matrix4 m, float value) { m.Row3.X = value; }
// 
//         public static float m13(Matrix4 m) { return m.Row3.Y; }
//         public static void m13(ref Matrix4 m, float value) { m.Row3.Y = value; }
// 
//         public static float m14(Matrix4 m) { return m.Row3.Z; }
//         public static void m14(ref Matrix4 m, float value) { m.Row3.Z = value; }
//      
//         public static float m15(Matrix4 m) { return m.Row3.W; }
//         public static void m15(ref Matrix4 m, float value) { m.Row3.W = value; }

// col major
        public static float m0(Matrix4 m) { return m.Row0.X; }
        public static void m0(ref Matrix4 m, float value) { m.Row0.X = value; }

        public static float m1(Matrix4 m) { return m.Row1.X; }
        public static void m1(ref Matrix4 m, float value) { m.Row1.X = value; }

        public static float m2(Matrix4 m) { return m.Row2.X; }
        public static void m2(ref Matrix4 m, float value) { m.Row2.X = value; }

        public static float m3(Matrix4 m) { return m.Row3.X; }
        public static void m3(ref Matrix4 m, float value) { m.Row3.X = value; }

        public static float m4(Matrix4 m) { return m.Row0.Y; }
        public static void m4(ref Matrix4 m, float value) { m.Row0.Y = value; }

        public static float m5(Matrix4 m) { return m.Row1.Y; }
        public static void m5(ref Matrix4 m, float value) { m.Row1.Y = value; }

        public static float m6(Matrix4 m) { return m.Row2.Y; }
        public static void m6(ref Matrix4 m, float value) { m.Row2.Y = value; }

        public static float m7(Matrix4 m) { return m.Row3.Y; }
        public static void m7(ref Matrix4 m, float value) { m.Row2.Y = value; }

        public static float m8(Matrix4 m) { return m.Row0.Z; }
        public static void m8(ref Matrix4 m, float value) { m.Row0.Z = value; }

        public static float m9(Matrix4 m) { return m.Row1.Z; }
        public static void m9(ref Matrix4 m, float value) { m.Row1.Z = value; }

        public static float m10(Matrix4 m) { return m.Row2.Z; }
        public static void m10(ref Matrix4 m, float value) { m.Row2.Z = value; }

        public static float m11(Matrix4 m) { return m.Row3.Z; }
        public static void m11(ref Matrix4 m, float value) { m.Row3.Z = value; }

        public static float m12(Matrix4 m) { return m.Row0.W; }
        public static void m12(ref Matrix4 m, float value) { m.Row0.W = value; }

        public static float m13(Matrix4 m) { return m.Row1.W; }
        public static void m13(ref Matrix4 m, float value) { m.Row1.W = value; }

        public static float m14(Matrix4 m) { return m.Row2.W; }
        public static void m14(ref Matrix4 m, float value) { m.Row2.W = value; }

        public static float m15(Matrix4 m) { return m.Row3.W; }
        public static void m15(ref Matrix4 m, float value) { m.Row3.W = value; }


        public static void SetRotationRadians(ref Matrix4 mat, Vector3 angles)
        {
            double cr = Math.Cos(angles.X);
            double sr = Math.Sin(angles.X);
            double cp = Math.Cos(angles.Y);
            double sp = Math.Sin(angles.Y);
            double cy = Math.Cos(angles.Z);
            double sy = Math.Sin(angles.Z);

            mat.M11 = (float)(cp * cy);
            mat.M12 = (float)(cp * sy);
            mat.M13 = (float)(-sp);
            mat.M14 = (float)(0.0f);

            double srsp = sr * sp;
            double crsp = cr * sp;

            mat.M21 = (float)(srsp * cy - cr * sy);
            mat.M22 = (float)(srsp * sy + cr * cy);
            mat.M23 = (float)(sr * cp);

            mat.M31 = (float)(crsp * cy + sr * sy);
            mat.M32 = (float)(crsp * sy - sr * cy);
            mat.M34 = (float)(cr * cp);
        }

        public static void SetTranslation(ref Matrix4 mat, Vector3 translation)
        {
            mat.M41 = translation.X;
            mat.M42 = translation.Y;
            mat.M43 = translation.Z;
        }
    }
    public class QuaternionHelper
    {
        public static Quaternion FromEuler(Vector3 angles)
        {
            double x, y, z, w;

            double c1 = Math.Cos(angles.Y * 0.5f);
            double c2 = Math.Cos(angles.Z * 0.5f);
            double c3 = Math.Cos(angles.X * 0.5f);

            double s1 = Math.Sin(angles.Y * 0.5f);
            double s2 = Math.Sin(angles.Z * 0.5f);
            double s3 = Math.Sin(angles.X * 0.5f);

            w = c1 * c2 * c3 - s1 * s2 * s3;
            x = s1 * s2 * c3 + c1 * c2 * s3;
            y = s1 * c2 * c3 + c1 * s2 * s3;
            z = c1 * s2 * c3 - s1 * c2 * s3;

            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        }

        public static Quaternion FromMatrix (Matrix4 m1)
        {
//             if (false)
//             {
//                 float w = (float)Math.Sqrt(1.0 + m1.M11 + m1.M22 + m1.M33) / 2.0f;
//                 float w4 = (4.0f * w);
//                 float x = (m1.M32 - m1.M23) / w4;
//                 float y = (m1.M13 - m1.M31) / w4;
//                 float z = (m1.M21 - m1.M12) / w4;
// 
//                 return new Quaternion(x, y, z, w);
//             }
            
            float w = (float)Math.Sqrt( Math.Max( 0, 1 + m1.M11 + m1.M22 + m1.M33 ) ) / 2f;
            float x = (float)Math.Sqrt( Math.Max( 0, 1 + m1.M11 - m1.M22 - m1.M33 ) ) / 2f;
            float y = (float)Math.Sqrt( Math.Max( 0, 1 - m1.M11 + m1.M22 - m1.M33 ) ) / 2f;
            float z = (float)Math.Sqrt( Math.Max( 0, 1 - m1.M11 - m1.M22 + m1.M33 ) ) / 2f;
            x = FloatHelper.CopySign( x, m1.M32 - m1.M23 );
            y = FloatHelper.CopySign( y, m1.M12 - m1.M31);
            z = FloatHelper.CopySign( z, m1.M21 - m1.M12);
            return new Quaternion(x, y, z, w);
        }
    }

    public class VectorHelper3
    {
        public static Vector3 Up = new Vector3(0, 0, 1);
        public static Vector3 Down = new Vector3(0, 0, -1);
        public static float Distance(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z));
        }

        public static float DistanceSquared(Vector3 v1, Vector3 v2)
        {
            return (v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z);
        }

        public static Vector3 Subtract(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X-v2.X,v1.Y-v2.Y,v1.Z-v2.Z);
        }

        public static Vector3 Min ( Vector3 v1, Vector3 v2 )
        {
            Vector3 ret = new Vector3(v1);
            if (v2.X < ret.X)
                ret.X = v2.X;
            
            if (v2.Y < ret.Y)
                ret.Y = v2.Y;
            
            if (v2.Z < ret.Z)
                ret.Z = v2.Z;

            return ret;
        }

        public static Vector3 Max(Vector3 v1, Vector3 v2)
        {
            Vector3 ret = new Vector3(v1);
            if (v2.X > ret.X)
                ret.X = v2.X;

            if (v2.Y > ret.Y)
                ret.Y = v2.Y;

            if (v2.Z > ret.Z)
                ret.Z = v2.Z;

            return ret;
        }

        public static Vector3 FromQuaternion ( Quaternion quat )
        {
            quat.Normalize();

            Vector3 angs = new Vector3();
            angs.Y = (float)Math.Asin(2.0f*quat.X*quat.Y+2.0f*quat.Z*quat.W);

            if (quat.X*quat.Y+quat.Z*quat.W == 0.5f)
                angs.X = 2.0f * (float)Math.Atan2(quat.X, quat.W);
            else if (quat.X*quat.Y+quat.Z*quat.W == -0.5f)
                angs.X = -2.0f * (float)Math.Atan2(quat.X, quat.W);
            else
            {
                angs.X = (float)Math.Atan2(2.0f * quat.Y * quat.W - 2.0f * quat.X * quat.Z, 1.0f - 2.0f * (quat.Y * quat.Y) - 2 * (quat.Z * quat.Z));
                angs.Z = (float)Math.Atan2(2.0f * quat.X * quat.W - 2.0f * quat.Y * quat.Z, 1.0f - 2.0f * (quat.X * quat.X) - 2 * (quat.Z * quat.Z));
            }

            return angs;
        }

        public static Vector3 FromAngle( float angle )
        {
            return new Vector3((float)Math.Cos(Trig.DegreeToRadian(angle)), (float)Math.Sin(Trig.DegreeToRadian(angle)), 0);
        }
    }

    public class VectorHelper2
    {
        public static bool Equal(Vector2 v1, Vector2 v2)
        {
            return FloatHelper.Equals(v1.X, v2.X) && FloatHelper.Equals(v1.Y, v2.Y);
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y));
        }

        public static float DistanceSquared(Vector2 v1, Vector2 v2)
        {
            return (v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y);
        }

        public static Vector2 Subtract(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 Subtract(Vector3 v1, Vector3 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 Subtract(Vector2 v1, Vector3 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 Subtract(Vector3 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 FromAngle(float angle)
        {
            return new Vector2((float)Math.Cos(Trig.DegreeToRadian(angle)), (float)Math.Sin(Trig.DegreeToRadian(angle)));
        }

        public static Vector2 FromAngle(double angle)
        {
            return new Vector2((float)Math.Cos(Trig.DegreeToRadian(angle)), (float)Math.Sin(Trig.DegreeToRadian(angle)));
        }

        public static Vector2 FromAngle(float angle, float radius)
        {
            return new Vector2((float)Math.Cos(Trig.DegreeToRadian(angle)) * radius, (float)Math.Sin(Trig.DegreeToRadian(angle))*radius);
        }
    }
}
