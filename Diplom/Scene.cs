using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Diplom
{
    class Scene
    {

        private Bitmap scene;
        private double[,] zBuffer;
        private int width, height;

        public Scene(int width, int height)
        {
            scene = new Bitmap(width, height);
            zBuffer = new double[width, height];
            this.width = width;
            this.height = height;
        }

        public void Clear()
        {
            scene = new Bitmap(width, height);
            zBuffer = new double[width, height];
        }

        public void drawLine(Color pen, Vector a, Vector b)
        {
            //округление вещественных координат
            int x1 = (int)Math.Round(a.x), y1 = (int)Math.Round(a.y),
                x2 = (int)Math.Round(b.x), y2 = (int)Math.Round(b.y);
            //вычисление приращений
            int dx = x2 - x1,
                dy = y2 - y1;
            //вычислление элементарных шагов
            int sx = Math.Sign(dx), sy = Math.Sign(dy);
            int dFx = (sx > 0) ? dy : -dy,
                dFy = (sy > 0) ? dx : -dx,
                Fx = 0, Fy = 0, F = 0;
            //вычисление координат направляющего вектора для вычисления z
            double p1 = b.x - a.x, p2 = b.y - a.y, p3 = b.z - a.z + 1.5, z = 0;
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                do
                {
                    //проверка на нахождение в области
                    if (x1 >= 0 && y1 >= 0 && x1 < scene.Width && y1 < scene.Height)
                    {
                        //вычисление z по найденным координатам x и y
                        z = (p3 * (x1 - a.x) / p1) + a.z + 0.75;
                        //проверка zбуфера
                        if (zBuffer[x1, y1] <= z)
                        {
                            scene.SetPixel(x1, y1, pen);
                            zBuffer[x1, y1] = z;
                        }
                    }
                    if (x1 == x2) break;
                    //определение следующей точки
                    Fx = F + dFx;
                    F = Fx - dFy;
                    x1 += sx;
                    if (Math.Abs(Fx) < Math.Abs(F)) F = Fx;
                    else y1 += sy;
                } while (true);
            }
            else
            {
                do
                {
                    if (x1 >= 0 && y1 >= 0 && x1 < scene.Width && y1 < scene.Height)
                    {
                        z = (p3 * (y1 - a.y) / p2) + a.z + 0.75;
                        if (zBuffer[x1, y1] <= z)
                        {
                            scene.SetPixel(x1, y1, pen);
                            zBuffer[x1, y1] = z;
                        }
                    }
                    if (y1 == y2) break;
                    Fy = F + dFy;
                    F = Fy - dFx;
                    y1 += sy;
                    if (Math.Abs(Fy) < Math.Abs(F)) F = Fy;
                    else x1 += sx;
                } while (true);
            }
        }

        public void drawLines(Color pen, Vector[] points, bool ring)
        {
            for (int i = 0; i < points.Length - 1; i++)
                drawLine(pen, points[i], points[i + 1]);
            if (ring)
                drawLine(pen, points[points.Length - 1], points[0]);
        }

        public void fillPoligon(Color pen, Vector[] points)
        {
            fillPoligon(pen, points[0], points[1], points[2]);
        }

        public void fillPoligon(Color pen, Vector a, Vector b, Vector c)
        {
            //вычислление коэфициентов для уравнения плоскости
            double coefA = (b.y - a.y) * (c.z - a.z) - (c.y - a.y) * (b.z - a.z),
                   coefB = -((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z)),
                   coefC = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y),
                   coefD = -(a.x * coefA + a.y * coefB + a.z * coefC);
            //округление вещественных координат вершин 
            a = new Vector(Math.Round(a.x), Math.Round(a.y), a.z, 1);
            b = new Vector(Math.Round(b.x), Math.Round(b.y), b.z, 1);
            c = new Vector(Math.Round(c.x), Math.Round(c.y), c.z, 1);
            //сортировка вершшин по возрастанию y 
            if (a.y > b.y)
            {
                Vector tmp = new Vector(a.x, a.y, a.z, 1);
                a = new Vector(b.x, b.y, b.z, 1);
                b = new Vector(tmp.x, tmp.y, tmp.z, 1);
            }
            if (a.y > c.y)
            {
                Vector tmp = new Vector(a.x, a.y, a.z, 1);
                a = new Vector(c.x, c.y, c.z, 1);
                c = new Vector(tmp.x, tmp.y, tmp.z, 1);
            }
            if (b.y > c.y)
            {
                Vector tmp = new Vector(c.x, c.y, c.z, 1);
                c = new Vector(b.x, b.y, b.z, 1);
                b = new Vector(tmp.x, tmp.y, tmp.z, 1);
            }
            //вычисление минималльного и максималльного Y
            double[] yArray = new double[] { a.y, b.y, c.y };
            int yMin = Math.Max((int)yArray.Min(), 0), 
                yMax = Math.Min((int)yArray.Max(), height - 1);
            Vector AB = new Vector(b.x - a.x, b.y - a.y, b.z - a.z, 1);
            Vector AC = new Vector(c.x - a.x, c.y - a.y, c.z - a.z, 1);
            Vector BC = new Vector(c.x - b.x, c.y - b.y, c.z - b.z, 1);
            //цикл прохода по всем строкам
            for (int y = yMin; y < yMax; y++)
            {
                List<double> x = new List<double>();
                //вычислление координат Х точек пересечения для каждого ребра
                if (y >= a.y && y < b.y)
                    x.Add(AB.x * (y - a.y) / AB.y + a.x);
                if (y >= a.y && y < c.y)
                    x.Add(AC.x * (y - a.y) / AC.y + a.x);
                if (y >= b.y && y < c.y)
                    x.Add(BC.x * (y - b.y) / BC.y + b.x);
                //вычисление минималльного и максимального Х из списка точек пересечения
                int xMin = Math.Max(0, (int)Math.Round(x.Min())), 
                    xMax = Math.Min(width - 1, (int)Math.Round(x.Max()));
                //вычисление координат Z1 и Z2 соответствующие Xmin и Xmax 
                double z1 = -(coefA * xMin + coefB * y + coefD) / coefC, 
                    z2 = -(coefA * xMax + coefB * y + coefD) / coefC;
                int dx = xMax - xMin;
                if (dx == 0) dx = 1;
                //вычисление приращения по Z при элементарном шаге по Х 
                double dz = (z2 - z1) / (double)dx;
                //Закрашивание
                do
                {
                    //если Z больше значения в буфере
                    //то закрасить и заменить значение в буфере на z
                    if (zBuffer[xMin, y] < z1)
                    {
                        scene.SetPixel(xMin, y, pen);
                        zBuffer[xMin, y] = z1;
                    }
                    //услловие выхода из цикла
                    if (xMin == xMax) break;
                    //шаг по Х
                    xMin++;
                    //приращение z
                    z1 += dz;
                } while (true);
            }
        }
        

        public Bitmap getImage()
        {
            return scene;
        }
    }
}
