using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diplom
{
    public partial class Form1 : Form
    {

        Color fillColor = Color.Black; //для закрашивания изначально черный
        int winSet = 1; //выбранный вариант троек окон
        //расположение окон
        //  0 1 2
        //  3 4 5
        //  6 7 8
        int[,] triple = new int[,] {{0,1,2}, {0,3,6}, {0,4,8},
                           {2,4,6}, {3,0,1}, {0,1,4},
                           {0,3,4}, {3,4,1}, {0,4,6},
                           {2,4,8}, {3,1,5}, {0,4,2},
                           {0,1,5}, {3,1,2}, {3,4,2},
                           {0,4,5}, {0,3,7}, {6,4,1},
                           {6,3,1}, {0,4,7}, {1,0,4},
                           {3,1,0}, {1,3,4}, {0,4,3},
                           {3,0,4}, {3,1,4}, {0,3,1},
                           {0,4,1}};

        Matrix matrix = new Matrix(); //Класс для работы с матрицами
        double[,]  deplaceSombrero, deplaceSombreroDx, deplaceSombreroDy,
            scale, rotation; //матрицы преобразований
        double scaleNum = 10;    //начальный множитель маштабирования
        Vector[] somPoints, somDxPoints, somDyPoints; //точки поверхностей
        Poligon[] edges; //массив связей точек поверхностей
        double height = 0.4;    //начальный множитель для высоты по z
        double minSom = 0, maxSom = 0, minSomDx = 0, 
            maxSomDx = 0, minSomDy = 0, maxSomDy = 0;
        Scene scene;

        public Form1()
        {
            InitializeComponent();

            scene = new Scene(screen.Width, screen.Height);
            //задание началльный матриц преобразования
            //матрица вращения используется для всех фигур
            rotation = matrix.getRotationMatrix(0, 0, 0);
            //матрица перемещения для фигуры типа сомбреро
            double center = (double)winSize.Value * scaleNum;
            deplaceSombrero = matrix.getDeplacmentMatrix((screen.Width / 4) - center * 1.5, screen.Height / 2, 1000);
            //перемещение производной сомбреро по х
            deplaceSombreroDx = matrix.getDeplacmentMatrix(screen.Width / 2 - center, screen.Height / 2, 1000);
            //перемещение производной сомбреро по y
            deplaceSombreroDy = matrix.getDeplacmentMatrix((3 * screen.Width / 4) - center * 0.5, screen.Height / 2, 1000);
            //маштабирование применяется только для фигур
            scale = matrix.getScaleMatrix(scaleNum, scaleNum, scaleNum);
            screen.MouseWheel += new MouseEventHandler(screen_MouseWheel);
            //расположение подписей для графиков
            label6.Location = new Point((int)(this.Width * 0.25 - label6.Text.Length * label6.Font.Size), label6.Location.Y);
            label7.Location = new Point((int)(this.Width * 0.5 - label7.Text.Length * label7.Font.Size * 0.5), label7.Location.Y);
            label8.Location = new Point((int)(this.Width * 0.75), label8.Location.Y);
            //вычисление весов
            getWeight();
            visualizing();
        }

        //маштабирование фигур при помощи вращения коллеса мыши
        private void screen_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0) scaleNum /= 1.1;
            else if (scaleNum < 15) scaleNum *= 1.1;
            scale = matrix.getScaleMatrix(scaleNum, scaleNum, scaleNum);
            double center = (double)winSize.Value * scaleNum;
            deplaceSombrero = matrix.getDeplacmentMatrix((screen.Width / 4) - center * 0.5, screen.Height / 2, 1000);
            deplaceSombreroDx = matrix.getDeplacmentMatrix(screen.Width / 2, screen.Height / 2, 1000);
            deplaceSombreroDy = matrix.getDeplacmentMatrix((3 * screen.Width / 4) + center * 0.5, screen.Height / 2, 1000);
            visualizing();
        }

        //метод для отображения проекций трехмерных фигур на экран
        private void visualizing()
        {
            scene.Clear();
            Vector Oz = new Vector(0, 0, 1, 1); 
            
            //матрицы преобразований для трех фигур
            double[,] transformSombrero = matrix.Multiple(matrix.Multiple(deplaceSombrero, rotation), scale);
            double[,] transformSombreroDx = matrix.Multiple(matrix.Multiple(deplaceSombreroDx, rotation), scale);
            double[,] transformSombreroDy = matrix.Multiple(matrix.Multiple(deplaceSombreroDy, rotation), scale);
            Vector[] tmpSom = new Vector[somPoints.Length],
                tmpSomDx = new Vector[somDxPoints.Length],
                tmpSomDy = new Vector[somDyPoints.Length];
            for (int i = 0; i < somPoints.Length; i++)
            {
                tmpSom[i] = matrix.MultipleVector(transformSombrero, somPoints[i]);
                tmpSomDx[i] = matrix.MultipleVector(transformSombreroDx, somDxPoints[i]);
                tmpSomDy[i] = matrix.MultipleVector(transformSombreroDy, somDyPoints[i]);
            }
            for (int i = 0; i < edges.Length; i++)
            {
                Vector[] edgesSom = new Vector[] {somPoints[edges[i].points[0]],
                                                    somPoints[edges[i].points[1]],
                                                    somPoints[edges[i].points[2]]};
                Vector[] edgesSomDx = new Vector[] {somDxPoints[edges[i].points[0]],
                                                    somDxPoints[edges[i].points[1]],
                                                    somDxPoints[edges[i].points[2]]};
                Vector[] edgesSomDy = new Vector[] {somDyPoints[edges[i].points[0]],
                                                    somDyPoints[edges[i].points[1]],
                                                    somDyPoints[edges[i].points[2]]};
                //услолвие для отображения z == 0, определяется длля всего полигона
                //если плоскость полигона перпендикулярна z и лежит в 0 по z
                if (enebledZ0.Checked && Oz.Collinarity(edges[i].getNormal(edgesSom)) && getAverage(edgesSom) == 0)
                    continue;

                //отрисовка полигнальной сетки
                if (poligonsGrid.Checked)
                {
                    scene.drawLines(Color.Black, new Vector[] { tmpSom[edges[i].points[0]], tmpSom[edges[i].points[1]], tmpSom[edges[i].points[2]] }, true);
                    scene.drawLines(Color.Black, new Vector[] { tmpSomDx[edges[i].points[0]], tmpSomDx[edges[i].points[1]], tmpSomDx[edges[i].points[2]] }, true);
                    scene.drawLines(Color.Black, new Vector[] { tmpSomDy[edges[i].points[0]], tmpSomDy[edges[i].points[1]], tmpSomDy[edges[i].points[2]] }, true);
                }
                //заливка цветом, если выбрана заливка
                if (fillCheck.Checked)
                {
                    Color colorSom = Color.White;
                    Color colorDx = Color.White;
                    Color colorDy = Color.White;
                    if (normal.Checked)
                    {
                        int brightSom = (int)(255 - Math.Abs(Oz.Angle(Oz, edges[i].getNormal(edgesSom))) * 255);
                        colorSom = Color.FromArgb(brightSom, fillColor.R, fillColor.G, fillColor.B);
                        int brightSomDx = (int)(255 - Math.Abs(Oz.Angle(Oz, edges[i].getNormal(edgesSomDx))) * 255);
                        colorDx = Color.FromArgb(brightSomDx, fillColor.R, fillColor.G, fillColor.B);
                        int brightSomDy = (int)(255 - Math.Abs(Oz.Angle(Oz, edges[i].getNormal(edgesSomDy))) * 255);
                        colorDy = Color.FromArgb(brightSomDy, fillColor.R, fillColor.G, fillColor.B);
                    }
                    if (fillZ.Checked)
                    {
                        double somNormalize = (maxSom - minSom) / 255,
                            somDxNormalize = (maxSomDx - minSomDx) / 255,
                            somDyNormalize = (maxSomDy - minSomDy) / 255;
                        int brightSom = 255 - (int)((getAverage(edgesSom) - minSom) / somNormalize);
                        colorSom = Color.FromArgb(brightSom, fillColor.R, fillColor.G, fillColor.B);
                        int brightSomDx = 255 - (int)((getAverage(edgesSomDx) - minSomDx) / somDxNormalize);
                        colorDx = Color.FromArgb(brightSomDx, fillColor.R, fillColor.G, fillColor.B);
                        int brightSomDy = 255 - (int)((getAverage(edgesSomDy) - minSomDy) / somDyNormalize);
                        colorDy = Color.FromArgb(brightSomDy, fillColor.R, fillColor.G, fillColor.B);
                    }
                    if (fillAbsZ.Checked)
                    {
                        double tmpSomMin = Math.Abs(minSom), tmpSomDxMin = Math.Abs(minSomDx), tmpSomDyMin = Math.Abs(minSomDy),
                            somAverage = Math.Abs(getAverage(edgesSom)), 
                            somDxAverege = Math.Abs(getAverage(edgesSomDx)), 
                            somDyAverage = Math.Abs(getAverage(edgesSomDy));
                        int brightSom, brightSomDx, brightSomDy;
                        if (tmpSomMin > maxSom)
                            brightSom = (int)((tmpSomMin - somAverage) / (tmpSomMin / 255));
                        else
                            brightSom = (int)((maxSom - somAverage) / (maxSom / 255));
                        colorSom = Color.FromArgb(brightSom, fillColor.R, fillColor.G, fillColor.B);
                        if (tmpSomDxMin > maxSomDx)
                            brightSomDx = (int)((tmpSomDxMin - somDxAverege) / (tmpSomDxMin / 255));
                        else
                            brightSomDx = (int)((maxSomDx - somDxAverege) / (maxSomDx / 255));
                        colorDx = Color.FromArgb(brightSomDx, fillColor.R, fillColor.G, fillColor.B);
                        if (tmpSomDyMin > maxSomDy)
                            brightSomDy = (int)((tmpSomDyMin - somDyAverage) / (tmpSomDyMin / 255));
                        else
                            brightSomDy = (int)((maxSomDy - somDyAverage) / (maxSomDy / 255));
                        colorDy = Color.FromArgb(brightSomDy, fillColor.R, fillColor.G, fillColor.B);
                    }
                    scene.fillPoligon(colorSom, new Vector[] { tmpSom[edges[i].points[0]], tmpSom[edges[i].points[1]], tmpSom[edges[i].points[2]] });
                    scene.fillPoligon(colorDx, new Vector[] { tmpSomDx[edges[i].points[0]], tmpSomDx[edges[i].points[1]], tmpSomDx[edges[i].points[2]] });
                    scene.fillPoligon(colorDy, new Vector[] { tmpSomDy[edges[i].points[0]], tmpSomDy[edges[i].points[1]], tmpSomDy[edges[i].points[2]] });                    
                }
            }
            screen.Image = scene.getImage();
        }

        private double getAverage(Vector[] points)
        {
            return (points[0].z + points[1].z + points[2].z) / 3;
        }

        private void getWeight()
        {            
            minSom = 0; 
            maxSom = 0; 
            minSomDx = 0;
            maxSomDx = 0; 
            minSomDy = 0; 
            maxSomDy = 0;
            int full = (int)winSize.Value;
            int half = full / 2;
            int max = full * 2;
            double[,] weightWin1 = new double[full, full];  //веса окна для графика сомбреро
            double[,] weightWin2 = new double[full, full];  //веса окна для графика производной по х сомбреро
            double[,] weightWin3 = new double[full, full];  //веса окна для графика производной по у сомбреро
            //определение весов пикселей для одного окна анализа
            for (int i = 0; i < full; i++)
                for (int j = 0; j < full; j++)
                {
                    int x = 0, y = 0;
                    x = (i < half) ? -i : full - i - 1;
                    y = (j < half) ? -j : full - j - 1; 
                    weightWin1[i, j] = (Math.Abs(x)) * (Math.Abs(y)) * height;
                    weightWin2[i, j] = -Math.Sign(x) * (Math.Abs(y)) * height;
                    weightWin3[i, j] = -(Math.Abs(x)) * Math.Sign(y) * height;
                }
            double[,] weightReg1 = new double[max, max];  //веса области для графика собреро
            double[,] weightReg2 = new double[max, max];  //веса области для графика производной сомбреро по х
            double[,] weightReg3 = new double[max, max];  //веса области для графика производной сомбреро по у
            //определение весов пикселей для всей области анализа
            //по выбранным тройкам анализа
            //winset - выбранная тройка окон анализа
            //индекс k отвечает за номер окна
            //вычисление весов происходит сразу для БФ и его производных
            //номера окон расположены следующим образом
            // 0 1 2
            // 3 4 5
            // 6 7 8
            for (int k = 0; k < 3; k++)
            {
                //зная номер индекс номера окна, можем получить его номер
                //и координаты пикселей начала и конца окна
                int num = triple[winSet - 1, k];    //получаем номер окна
                //вычисляем координаты вершин окна анлиза по его номеру
                //здесь half - размер половины окна анлиза в пикселях
                //full - полный размер окна
                //позиция 0-го окна [0,0] и размером full
                //позиция слледующего по горизонтали окна - [0 + half, 0]
                //следующего по вертикали - [0, 0 + half]
                //следующее окно всегда начинается на расстоянии половины окна
                //Зная это можем определлить координаты каждого окна зная его номер
                int xStart = (num % 3) * half, xEnd = xStart + full, //начало и конец окна по х
                    yStart = (num / 3) * half, yEnd = yStart + full; //начало и конец по y
                //цикл для вычисления ядра БФ
                for (int i = xStart; i < xEnd; i++)
                {
                    for (int j = yStart; j < yEnd; j++)
                    {
                        if (k == 1)
                        {
                            weightReg1[i, j] -= 2 * weightWin1[i - xStart, j - yStart];
                            weightReg2[i, j] -= 2 * weightWin2[i - xStart, j - yStart];
                            weightReg3[i, j] -= 2 * weightWin3[i - xStart, j - yStart];
                        }
                        else
                        {
                            weightReg1[i, j] += weightWin1[i - xStart, j - yStart];
                            weightReg2[i, j] += weightWin2[i - xStart, j - yStart];
                            weightReg3[i, j] += weightWin3[i - xStart, j - yStart];
                        }
                    }
                }
            }            
            int len = max * max;
            somPoints = new Vector[len];
            somDxPoints = new Vector[len];
            somDyPoints = new Vector[len];
            edges = new Poligon[(max - 1) * (max - 1) * 4];
            int count = 0;
            //заполнение массива полгинов
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    if (weightReg1[i, j] > maxSom) maxSom = weightReg1[i, j];
                    if (weightReg1[i, j] < minSom) minSom = weightReg1[i, j];
                    if (weightReg2[i, j] > maxSomDx) maxSomDx = weightReg2[i, j];
                    if (weightReg2[i, j] < minSomDx) minSomDx = weightReg2[i, j];
                    if (weightReg3[i, j] > maxSomDy) maxSomDy = weightReg3[i, j];
                    if (weightReg3[i, j] < minSomDy) minSomDy = weightReg3[i, j];
                    somPoints[count] = new Vector(i - full - 0.5, j - full, weightReg1[i, j], 1);
                    somDxPoints[count] = new Vector(i - full - 0.5, j - full, weightReg2[i, j], 1);
                    somDyPoints[count] = new Vector(i - full - 0.5, j - full, weightReg3[i, j], 1);
                    count++;
                }
            }
            count = 0;
            for (int i = 0; i < max - 1; i++)
            {
                for (int j = 0; j < max - 1; j++)
                {
                    int num = max * j + i;
                    edges[count] = new Poligon(num, num + 1, num + max);
                    count++;
                    edges[count] = new Poligon(num + 1, num + max + 1, num + max);
                    count++;
                    edges[count] = new Poligon(num, num + max + 1, num + max);
                    count++;
                    edges[count] = new Poligon(num, num + 1, num + max + 1);
                    count++;
                }
            }
        }

        //выбор тройки окон анализа по их изображению
        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (winSet != Convert.ToInt32((sender as PictureBox).Tag))
            {
                var pictureBox = this.Controls.Find("T" + winSet, true);//поиск по имени нужного pictureBox'a
                (pictureBox[0] as PictureBox).BackColor = Color.White;  //снятие подсвеченного выбора
                (sender as PictureBox).BackColor = Color.Red;           //визуализация выбора красной рамкой
                winSet = Convert.ToInt32((sender as PictureBox).Tag);   //запись номера тройки
                getWeight();
                visualizing();
            }
        }       

        //вращение при помощи ввода угллов поворота в текстбокс
        private void Angle_TextChanged(object sender, EventArgs e)
        {
            string text = (sender as TextBox).Text;
            int angle;
            if (Int32.TryParse(text, out angle))    //если введенное значение является числом
            {
                rotation = matrix.getRotationMatrix(Int32.Parse(xAngle.Text), Int32.Parse(yAngle.Text), Int32.Parse(zAngle.Text));
                visualizing();
            }
            else System.Media.SystemSounds.Asterisk.Play();
        }

        //сброс вращения
        private void bClear_Click(object sender, EventArgs e)
        {
            xAngle.Text = "0";
            yAngle.Text = "0";
            zAngle.Text = "0";
            rotation = matrix.getRotationMatrix(0, 0, 0);
            visualizing();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != System.Windows.Forms.FormWindowState.Minimized)
            {
                scene = new Scene(screen.Width, screen.Height);
                double center = (double)winSize.Value * scaleNum;
                deplaceSombrero = matrix.getDeplacmentMatrix((screen.Width / 4) - center * 0.5, screen.Height / 2, 1000);
                deplaceSombreroDx = matrix.getDeplacmentMatrix(screen.Width / 2, screen.Height / 2, 1000);
                deplaceSombreroDy = matrix.getDeplacmentMatrix((3 * screen.Width / 4) + center * 0.5, screen.Height / 2, 1000);
                label6.Location = new Point((int)(this.Width * 0.25 - label6.Text.Length * label6.Font.Size), label6.Location.Y);
                label7.Location = new Point((int)(this.Width * 0.5 - label7.Text.Length * label7.Font.Size * 0.5), label7.Location.Y);
                label8.Location = new Point((int)(this.Width * 0.75), label8.Location.Y);
                visualizing();
            }
        }

        //изменение размера окна анализа
        private void winSize_ValueChanged(object sender, EventArgs e)
        {
            getWeight();
            double center = (double)winSize.Value * scaleNum;
            deplaceSombrero = matrix.getDeplacmentMatrix((screen.Width / 4) - center * 0.5, screen.Height / 2, 1000);
            deplaceSombreroDx = matrix.getDeplacmentMatrix(screen.Width / 2, screen.Height / 2, 1000);
            deplaceSombreroDy = matrix.getDeplacmentMatrix((3 * screen.Width / 4) + center * 0.5, screen.Height / 2, 1000);
            visualizing();
        }

        //изменение маштаба высоты по z
        private void heightNum_ValueChanged(object sender, EventArgs e)
        {
            height = 0.04 * (double)heightNum.Value;
            getWeight();
            visualizing();
        }

        //выбор цвета для заливки
        private void ColorTool_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                fillColor = colorDialog1.Color;
            visualizing();
        }
 
        //разрешение на расскрашивание
        private void menuTools_CheckedChanged(object sender, EventArgs e)
        {
            ColorTool.Enabled = fillCheck.Checked;
            visualizing();
        }

        //обработчик кнопок для задания углов поворота графиков по осям
        private void Angle_Click(object sender, EventArgs e)
        {
            int tag = Convert.ToInt32((sender as Button).Tag);
            double rad = matrix.getRadian(10);
            switch (tag)
            {
                case 0:
                    rotation = matrix.Multiple(matrix.xRotation(rad), rotation);
                    xAngle.Text = (Int32.Parse(xAngle.Text) + 10).ToString();
                    break;
                case 1:
                    rotation = matrix.Multiple(matrix.yRotation(rad), rotation);
                    yAngle.Text = (Int32.Parse(yAngle.Text) + 10).ToString();
                    break;
                case 2:
                    rotation = matrix.Multiple( matrix.zRotation(rad), rotation);
                    zAngle.Text = (Int32.Parse(zAngle.Text) + 10).ToString();
                    break;
                case 3:
                    rotation = matrix.Multiple(matrix.xRotation(-rad), rotation);
                    xAngle.Text = (Int32.Parse(xAngle.Text) - 10).ToString();
                    break;
                case 4:
                    rotation = matrix.Multiple(matrix.yRotation(-rad), rotation);
                    yAngle.Text = (Int32.Parse(yAngle.Text) - 10).ToString();
                    break;
                case 5:
                    rotation = matrix.Multiple(matrix.zRotation(-rad), rotation);
                    zAngle.Text = (Int32.Parse(zAngle.Text) - 10).ToString();
                    break;
            }
            visualizing();
        }

        private void fillOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem option = sender as ToolStripMenuItem;
            if (!option.Checked)
            {
                option.Checked = true;
                return;
            }
            normal.Checked = false;
            fillZ.Checked = false;
            fillAbsZ.Checked = false;
            option.Checked = true;
            visualizing();
        }
    }
}
