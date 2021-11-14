using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace Paulus.Serial
{
    public class LiveSeries
    {
        public LiveSeries(LiveChart parent, string name, Color? customColor = null)
        {
            _parent = parent;

            Name = name;
            _customColor = customColor;

            initialize();
        }

        #region Properties

        protected LiveChart _parent;
        public LiveChart Parent { get { return _parent; } }

        public Chart Chart { get { return _parent._chart; } }

        public ChartArea ChartArea { get { return _parent?._chartArea; } }

        protected Series _series;
        public Series Series { get { return _series; } }

        protected string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(Name));

                _name = value;
                if (_series != null)
                    _series.Name = _name;
            }
        }

        protected Color? _customColor;
        public Color? CustomColor
        {
            get { return _customColor; }
            set
            {
                _customColor = value;

                if (_series != null)
                    _series.Color = _customColor.Value;
            }
        }

        #endregion

        protected virtual void initialize()
        {
            Series existingSeries = Chart.Series.FindByName(_name);
            if (existingSeries != null)
            {
                _series = existingSeries;
                _series.Points.Clear();
            }
            else
                _series = Chart.Series.Add(_name);

            if (_customColor != null)
                _series.Color = _customColor.Value;

            _series.ChartType = SeriesChartType.FastLine;
            _series.XValueType = ChartValueType.DateTime;
        }

        public virtual void AddPoint(double yValue)
        {
            DateTime currentTime = _parent.CurrentTime;
            DataPoint newDataPoint = new DataPoint(currentTime.ToOADate(), yValue);
            bool shift = _parent.ShouldShiftSeries();

            int pointsCount = _series.Points.Count;

            if (!shift || pointsCount == 0)
                _series.Points.Add(newDataPoint);
            else
            {
                for (int i = 0; i < pointsCount - 1; i++)
                    _series.Points[i] = _series.Points[i + 1].Clone();

                _series.Points[pointsCount - 1] = newDataPoint;
            }
        }

        public void Clear()
        {
            _series?.Points.Clear();
        }

    }


}
