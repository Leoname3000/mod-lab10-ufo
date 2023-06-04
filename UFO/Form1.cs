using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UFO
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Width = 890;
            Height = 490;

            x1 = 18;
            y1 = 413;
            p1 = new PointF(x1, y1);

            x2 = 785;
            y2 = 26;
            p2 = new PointF(x2, y2);

            precision = 2;
            maxPrecision = 9;
            steps = 10;

            this.Paint += new PaintEventHandler(Form1_Paint);

            precisionLabel.Left = 20;
            precisionLabel.Top = 20;
            Controls.Add(precisionLabel);

            epsilonLabel.Left = (int) x2 - 15;
            epsilonLabel.Top = (int) y2 + 20;
            Controls.Add(epsilonLabel);

            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);

            BuildPath(precision);
            precision++;
            timer.Start();
        }

        int precision;
        int maxPrecision;
        int steps;
        float x1, y1;
        float x2, y2;
        PointF p1, p2;

        List<PointF> points = new List<PointF>();
        List<float> epsilons = new List<float>();
        List<int> precisions = new List<int>();

        Timer timer = new Timer();
        Label precisionLabel = new Label();
        Label epsilonLabel = new Label();

        private void timer_Tick(object sender, EventArgs e)
        {
            if (precision < maxPrecision + 1)
            {
                BuildPath(precision);
                Invalidate();
                precision++;
            }
            else
            {
                List<double> dEpsilons = new List<double>();
                foreach (var f in epsilons) dEpsilons.Add((double) f);
                List<double> dPrecisions = new List<double>();
                foreach (var i in precisions) dPrecisions.Add((double) i);

                ScottPlot.Plot plot = new ScottPlot.Plot();
                plot.AddScatter(dPrecisions.ToArray(), dEpsilons.ToArray());
                plot.Title("Зависимость погрешности от числа членов ряда");
                plot.YLabel("Итоговая погрешность");
                plot.XLabel("Число слагаемых");
                plot.SaveFig("dia.png");
                timer.Stop();
                Close();
            }
        }

        private float Distance(PointF p1, PointF p2)
        {
            return MathF.Sqrt(MathF.Pow(p2.X - p1.X, 2) + MathF.Pow(p2.Y - p1.Y, 2));
        }

        private int Factorial(int n)
        {
            int result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private float Sin(int precision, float x)
        {
            float result = 0;
            for (int n = 1; n <= precision; n++)
            {
                result += MathF.Pow(-1, n - 1) * MathF.Pow(x, 2*n - 1) / Factorial(2*n - 1);
            }
            return result;
        }

        private float Cos(int precision, float x)
        {
            float result = 0;
            for (int n = 1; n <= precision; n++)
            {
                result += MathF.Pow(-1, n - 1) * MathF.Pow(x, 2*n - 2) / Factorial(2*n - 2);
            }
            return result;
        }

        private float Arctg(int precision, float x)
        {
            float result = 0;
            for (int n = 1; n <= precision; n++)
            {
                result += MathF.Pow(-1, n - 1) * MathF.Pow(x, 2*n - 1) / (2*n - 1);
            }
            return result;
        }

        private void BuildPath(int precision)
        {
            float angle = Arctg(precision, MathF.Abs(y2 - y1) / MathF.Abs(x2 - x1));
            float distance = Distance(p1, p2);
            float step = distance / steps;

            points = new List<PointF>();
            points.Add(p1);
            float x = p1.X;
            float y = p1.Y;
            float epsilon = distance;
            float newEpsilon = 0;

            for (int i = 0; i < steps; i++)
            {
                x += step * Cos(precision, angle);
                y -= step * Sin(precision, angle);
                PointF newPoint = new PointF(x, y);
                newEpsilon = Distance(newPoint, p2);

                points.Add(newPoint);
                epsilon = newEpsilon;
            }

            epsilons.Add(epsilon);
            precisions.Add(precision);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int dw = 2;
            Pen pen = new Pen(Color.Red, dw);
            foreach (var p in points)
            {
                g.DrawEllipse(pen, p.X - dw/2, p.Y - dw/2, dw, dw);
            }
            Pen endPen = new Pen(Color.Black, 1);
            g.DrawEllipse(endPen, x1 - dw/2, y1 - dw/2, dw, dw);
            g.DrawEllipse(endPen, x2 - dw/2, y2 - dw/2, dw, dw);
            g.DrawLine(endPen, p1, p2);

            float epsilon = epsilons[epsilons.Count-1];
            g.DrawEllipse(endPen, x2 - epsilon, y2 - epsilon, epsilon*2, epsilon*2);
            epsilonLabel.Text = epsilon.ToString();
            precisionLabel.Text = $"Precision = {precisions[precisions.Count-1]}";
        }
    }
}
