using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ParetoFrontCursach
{
    public class Model
    {
        List<IObserver> observers;
        public List<PointF> Points { get; private set; }

        public List<List<PointF>> Fronts { get; private set; }

        Timer timer;

        public Model()
        {
            observers = new List<IObserver>();
            Points = new List<PointF>();

            Fronts = new List<List<PointF>>();

            timer = new Timer
            {
                Interval = 600,
            };
            timer.Tick += Operate;
        }

        // идёт ли процесс вычисления?
        public bool Enabled { get; private set; } = false;

        // начать вычисление
        private void Enable()
        {
            Enabled = true;
            timer.Start();
        }

        // закончить вычисление
        private void Disable()
        {
            Enabled = false;
            timer.Stop();
        }

        // добавить точку
        public void AddPoint(PointF p)
        {
            Points.Add(p);
            UpdateObservers();
        }

        // начать вычисление
        public void StartCalculation()
        {
            Enable();
        }

        // проверка доминирования
        public bool Dominates(PointF a, PointF b)
        {
            bool ans = false;
            if (a.X > b.X)
            {
                ans = true;
            }
            else if (b.X > a.X)
            {
                return false;
            }
            if (a.Y > b.Y)
            {
                ans = true;
            }
            else if (b.Y > a.Y)
            {
                return false;
            }
            return ans;
        }

        // прогресс в %
        public int Progress { get; private set; } = 0;

        // главный метод вычисления
        private void Operate(object sender, EventArgs e)
        {
            ArrangeFrontsByRank();
            if (I == 0)
            {
                Disable();
            }
            int len = 0;
            foreach (var el in Fronts)
            {
                len += el.Count;
            }
            Progress = 100 * len / Points.Count;
            UpdateObservers();
        }

        // "пустая" точка
        PointF empty = new PointF(-1f, -1f);

        // вычисление фронта Парето
        private List<PointF> ParetoFront(List<PointF> points)
        {
            // список точек фронта
            List<PointF> front = new List<PointF>();
            
            for (int i = 0; i < points.Count; i++)
            {
                // добавляем точку во фронт
                front.Add(points[i]);
                for (int j = 0; j < front.Count - 1 && front[j] != empty; j++)
                {
                    if (Dominates(front[j], points[i]))
                    {
                        // удаляем, если над ней кто-то доминирует
                        front.Remove(points[i]);
                        break;
                    }
                    else if (Dominates(points[i], front[j]))
                    {
                        // удаляем точку фронта, если над ней доминирует новая
                        front[j] = empty;
                    }
                }
                // очистка "пустых" точек
                // точки заменяются на пустые вместо удаления,
                // чтобы не ломать итерацию по списку (не сдвигать индексы)
                front.RemoveAll((e) => (e.X == empty.X) && (e.Y == empty.Y));
            }
            // возвращаем фронт
            return front;
        }

        // рассчёт фронтов по рангам
        int I = 0;
        // временный буфер для хранения исходных точек
        List<PointF> p = new List<PointF>();
        public void ArrangeFrontsByRank()
        {
            if (I == 0)
            {
                // очищаем список списков фронтов, если это начало
                Fronts.Clear();
                // копируем исходные точки, чтобы не потерять
                foreach (var el in Points)
                {
                    p.Add(el);
                }
            }
            // добавляем в список списков фронтов новый фронт
            Fronts.Add(new List<PointF>());
            Fronts[I] = ParetoFront(p);
            // удаляем из рабочего массива все точки, входящие в этот фронт
            Predicate<PointF> pat = (e) => Fronts[I].Contains(e);
            p.RemoveAll(pat);
            I++;
            // если массив точек пуст, обнуляем индекс
            if (p.Count == 0)
            {
                I = 0;
            }
            // всё происходит не в цикле, чтобы иметь
            // возможность вызывать метод по таймеру
            // и отрисовывать в виде промежуточный результат
        }

        // удаление точки
        public void RemovePoint(int index)
        {
            Points.RemoveAt(index);
            UpdateObservers();
        }

        // очистить точки
        public void ClearPoints()
        {
            Points.Clear();
            UpdateObservers();
        }

        // очистить фронты
        public void ClearFronts()
        {
            Fronts.Clear();
            UpdateObservers();
        }

        // регистрация наблюдателя
        public void Register(IObserver observer)
        {
            observers.Add(observer);
            observer.UpdateState();
        }

        // дерегистрация наблюдателя
        public void Deregister(IObserver observer)
        {
            observers.Remove(observer);
        }

        // отправка сообщений наблюдателям
        private void UpdateObservers()
        {
            foreach (IObserver observer in observers)
            {
                observer.UpdateState();
            }
        }
    }
}
