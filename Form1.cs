using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParetoFrontCursach
{
    public partial class Form1 : Form, IObserver
    {
        Graphics graphics;
        Model model;
        Controller controller;
        public Form1(Model model)
        {
            InitializeComponent();

            this.model = model;
            controller = new Controller(this, model);
            graphics = Graphics.FromHwnd(panelCanvas.Handle);

            model.Register(this);

            model.AddPoint(new PointF(2, 7));
            model.AddPoint(new PointF(3, 1));
            model.AddPoint(new PointF(7, 4));
            model.AddPoint(new PointF(8, 6.3f));
            model.AddPoint(new PointF(5, 7));
            model.AddPoint(new PointF(0, 2.4f));
            model.AddPoint(new PointF(2.8f, 8));
            model.AddPoint(new PointF(9, 8.6f));
            model.AddPoint(new PointF(1, 1));
            model.AddPoint(new PointF(10, 10f));
            model.AddPoint(new PointF(11, 8f));
            model.AddPoint(new PointF(12, 12f));
            model.AddPoint(new PointF(0, 0f));

            fillListView();
        }

        const int X = 30;
        const int Y = 30;
        const int STEP = 40;
        const int DOT_RADIUS = 5;

        private void panelCanvas_Paint(object sender, PaintEventArgs e)
        {
            int x0 = X;
            int y0 = panelCanvas.Height - Y;
            int x1 = panelCanvas.Width - X;
            int y1 = Y;

            graphics.Clear(Color.White);

            DrawGrid(x0, y0, x1, y1);

            graphics.DrawLine(Pens.Blue, x0, y1, x0, y0); // ось абсцисс
            graphics.DrawLine(Pens.Blue, x0, y0, x1, y0); // ось ординат

            // стрелочки
            graphics.FillPolygon(Brushes.Blue, new Point[] { new Point(x0, y1), new Point(x0 - 7, y1 + 20), new Point(x0 + 7, y1 + 20) });
            graphics.FillPolygon(Brushes.Blue, new Point[] { new Point(x1, y0), new Point(x1 - 20, y0 + 7), new Point(x1 - 20, y0 - 7) });
            
            // отрезки на осях
            for (int x = x0 + STEP; x < x1 - 30; x += STEP)
            {
                graphics.DrawLine(Pens.Blue, x, y0 - 2, x, y0 + 2);
            }
            for (int y = y0 - STEP; y > y1 + 30; y -= STEP)
            {
                graphics.DrawLine(Pens.Blue, x0 - 2, y, x0 + 2, y);
            }

            DrawDots(x0, y0);

            // рисование фронтов
            foreach (var front in model.Fronts)
            {
                front.Sort((a, b) => a.X > b.X ? 1 : -1);
                if (front.Count > 0)
                    graphics.FillEllipse(Brushes.Red, front[0].X * STEP + x0 - DOT_RADIUS, -front[0].Y * STEP + y0 - DOT_RADIUS, DOT_RADIUS * 2, DOT_RADIUS * 2);
                for (int i = 0; i < front.Count - 1; i++)
                {
                    float xS = front[i].X * STEP + x0;
                    float yS = -front[i].Y * STEP + y0;
                    float xE = front[i + 1].X * STEP + x0;
                    float yE = -front[i + 1].Y * STEP + y0;
                    graphics.DrawLine(Pens.Red, xS, yS, xE, yE);
                    
                    graphics.FillEllipse(Brushes.Red, xE - DOT_RADIUS, yE - DOT_RADIUS, DOT_RADIUS * 2, DOT_RADIUS * 2);
                }
            }
        }

        // отрисовка точек
        private void DrawDots(int x0, int y0)
        {
            foreach (PointF p in model.Points)
            {
                float x = p.X * STEP + x0;
                float y = -p.Y * STEP + y0;
                graphics.FillEllipse(Brushes.Blue, x - DOT_RADIUS, y - DOT_RADIUS, DOT_RADIUS * 2, DOT_RADIUS * 2);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            graphics = Graphics.FromHwnd(panelCanvas.Handle);
            panelCanvas.Refresh();
        }

        // обновление наблюдателя
        public void UpdateState()
        {
            fillListView();
            panelCanvas.Refresh();
            toolStripProgressBar1.Value = model.Progress;
            toolStripStatusLabel2.Text = $"{model.Progress}%";

            button1.Enabled = !model.Enabled;
            buttonAdd.Enabled = !model.Enabled;
            buttonDelete.Enabled = !model.Enabled;
        }

        // добавление точки
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            new FormAdd(model).ShowDialog();
        }

        // удаление точки
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                controller.RemovePoint(listView1.SelectedIndices[0]);
                fillListView();
            }
            else
            {
                MessageBox.Show("Выберите элемент!");
            }
        }

        // заполнение ListView с точками
        private void fillListView()
        {
            listView1.Items.Clear();
            for (int i = 0; i < model.Points.Count; i++)
            {
                var p = model.Points[i];
                ListViewItem item = new ListViewItem($"{i + 1}");
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, $"{p.X}"));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, $"{p.Y}"));
                listView1.Items.Add(item);
            }
        }

        private void очиститьТочкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.ClearPoints();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!controller.ComputeFronts())
            {
                MessageBox.Show("Добавьте точки!");
            }
        }

        // рисовать ли сетку?
        bool grid = false;

        private void отобразитьСеткуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            отобразитьСеткуToolStripMenuItem.Checked ^= true;
            grid ^= true;
            panelCanvas.Refresh();
        }

        // отрисовка сетки
        private void DrawGrid(int x0, int y0, int x1, int y1)
        {
            if (grid)
            {
                for (int x = x0; x < x1; x += STEP / 5)
                {
                    graphics.DrawLine(Pens.Silver, x, y0, x, y1);
                }
                for (int y = y0; y > y1; y -= STEP / 5)
                {
                    graphics.DrawLine(Pens.Silver, x0, y, x1, y);
                }
                for (int x = x0 + STEP; x < x1 - 30; x += STEP)
                {
                    graphics.DrawLine(Pens.Gray, x, y0, x, y1);
                }
                for (int y = y0 - STEP; y > y1 + 30; y -= STEP)
                {
                    graphics.DrawLine(Pens.Gray, x0, y, x1, y);
                }
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAbout().Show();
        }

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProblem().Show();
        }

        private void описаниеАлгоритмаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAlgorithm().Show();
        }
    }
}
