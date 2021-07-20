using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    class Poligon
    {

        public int[] points;

        public Poligon(int a, int b, int c)
        {
            points = new int[3];
            points[0] = a;
            points[1] = b;
            points[2] = c;
        }

        public Vector getNormal(Vector[] p)
        {
            Vector AB = new Vector(p[1].x - p[0].x,
                                   p[1].y - p[0].y,
                                   p[1].z - p[0].z, 
                                   1);
            Vector AC = new Vector(p[2].x - p[0].x,
                                   p[2].y - p[0].y,
                                   p[2].z - p[0].z,
                                   1);
            return AB.Multiple(AB, AC);
        }

    }
}
