using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    class Vector
    {

        public double[] vector = new double[4];
        public double x;
        public double y;
        public double z;

        public Vector(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            vector[0] = x;
            vector[1] = y;
            vector[2] = z;
            vector[3] = w;
        }

        //длина вектора
        public double getLenght()
        {
            return Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
        }

        //сумма векторов
        public Vector Add(Vector a, Vector b)
        {
            return new Vector(
                a.vector[0] + b.vector[0],
                a.vector[1] + b.vector[1],
                a.vector[2] + b.vector[2],
                1);
        }

        public Vector Multiple(Vector a, Vector b)
        {
            return new Vector(a.y * b.z - b.y * a.z,
                              a.x * b.z - b.x * a.z,
                              a.x * b.y - b.x * a.y, 
                              1);
        }

        //возвращает нормализованный вектор
        public Vector Normalize()
        {
            double len = this.getLenght();
            vector[0] /= len;
            vector[1] /= len;
            vector[2] /= len;
            x /= len;
            y /= len;
            z /= len;
            return this;
        }

        //проверка на колллениарность
        public bool Collinarity(Vector a)
        {
            Vector res = Multiple(this, a);
            if (res.x == 0 && res.y == 0 && res.z == 0) return true;
            else return false;
        }

        //возвращает косинус угла между векторами
        public double Angle(Vector a, Vector b)
        {
            return (a.x * b.x + a.y * b.y + a.z * b.z) / (a.getLenght() * b.getLenght());
        }

        public Vector MultipleByScalar(double s)
        {
            this.vector[0] *= s;
            this.vector[1] *= s;
            this.vector[2] *= s;
            return this;
        }

    }
}
