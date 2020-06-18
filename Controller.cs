using System;
using System.Windows.Forms;
using System.Drawing;

namespace ParetoFrontCursach
{
    public class Controller
    {
        Model model;
        public Controller(Form form, Model model)
        {
            this.model = model;
        }

        // добавить точку
        public bool AddPoint(string x, string y)
        {
            try
            {
                float X = float.Parse(x);
                float Y = float.Parse(y);
                if (X >= 0 && Y >= 0)
                {
                    model.AddPoint(new PointF(X, Y));
                    model.ClearFronts();
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // удалить точку
        public void RemovePoint(int index)
        {
            model.RemovePoint(index);
            model.ClearFronts();
        }

        // очистить все точки
        public void ClearPoints()
        {
            model.ClearPoints();
            model.ClearFronts();
        }

        // вычислить фронты
        public bool ComputeFronts()
        {
            if (model.Points.Count > 0)
            {
                model.StartCalculation();
                return true;
            }
            return false;
        }

        // очистка фронтов
        public void DeleteFronts()
        {
            model.ClearFronts();
        }
    }
}
