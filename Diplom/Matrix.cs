using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    class Matrix
    {
        //единичная матрица
        private double[,] getIdentityMatrix()
        {
            return new double[,]{
                {1,0,0,0},
                {0,1,0,0},
                {0,0,1,0},
                {0,0,0,1}
            };
        }
        
        //умножение матриц
        public double[,] Multiple(double[,] a, double[,] b)
        {
            double[,] res = new double[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    res[i, j] = a[i, 0] * b[0, j] +
                      a[i, 1] * b[1, j] +
                      a[i, 2] * b[2, j] +
                      a[i, 3] * b[3, j];
                }
            }
            return res;
        }

        //умножение матрицы на вектор
        public Vector MultipleVector(double[,] m, Vector v)
        {
            return new Vector(
                m[0, 0] * v.vector[0] + m[0, 1] * v.vector[1] + m[0, 2] * v.vector[2] + m[0, 3] * v.vector[3],
                m[1, 0] * v.vector[0] + m[1, 1] * v.vector[1] + m[1, 2] * v.vector[2] + m[1, 3] * v.vector[3],
                m[2, 0] * v.vector[0] + m[2, 1] * v.vector[1] + m[2, 2] * v.vector[2] + m[2, 3] * v.vector[3],
                m[3, 0] * v.vector[0] + m[3, 1] * v.vector[1] + m[3, 2] * v.vector[2] + m[3, 3] * v.vector[3]
            );
        }

        //получить матрицу перемещения по данным значениям перемещения по осям
        public double[,] getDeplacmentMatrix(double dx, double dy, double dz)
        {
            return new double[,] {
                {1, 0, 0, dx},
                {0, 1, 0, dy},
                {0, 0, 1, dz},
                {0, 0, 0, 1}
            };
        }

        //матрица маштабирования по осям
        public double[,] getScaleMatrix(double sx, double sy, double sz)
        {
            return new double[,] {
                {sx,  0,  0, 0},
                { 0, sy,  0, 0},
                { 0,  0, sz, 0},
                { 0,  0,  0, 1}
            };
        }

        //перевод угла в радианы
        public double getRadian(double angle)
        {
            return Math.PI * angle / 180;
        }

        //матрица вращения по трем осям
        public double[,] getRotationMatrix(double xAngle, double yAngle, double zAngle)
        {
            //вращение по х
            double[,] xRot = xRotation(getRadian(xAngle));
            //вращение по у
            double[,] yRot = yRotation(getRadian(yAngle));
            //вращение по z
            double[,] zRot = zRotation(getRadian(zAngle));
            return Multiple(xRot, Multiple(yRot, zRot));
        }

        //матрица вращения по Х
        public double[,] xRotation(double rad)
        {
            double cos = Math.Cos(rad),
                sin = Math.Sin(rad);
            return new double[,] {
                {1,    0,   0, 0},
                {0,  cos, sin, 0},
                {0, -sin, cos, 0},
                {0,    0,   0, 1}
            };
        }

        //Матрица вращения по Y
        public double[,] yRotation(double rad)
        {
            double cos = Math.Cos(rad),
                sin = Math.Sin(rad);
            return new double[,] {
                {cos, 0, -sin, 0},
                {  0, 1,    0, 0},
                {sin, 0,  cos, 0},
                {  0, 0,    0, 1}
            };
        }

        //Матрица вращения по Z
        public double[,] zRotation(double rad)
        {
            double cos = Math.Cos(rad),
                sin = Math.Sin(rad);
            return new double[,] {
                {  cos, sin, 0, 0},
                { -sin, cos, 0, 0},
                {    0,   0, 1, 0},
                {    0,   0, 0, 1}
            };
        }
    }
}
