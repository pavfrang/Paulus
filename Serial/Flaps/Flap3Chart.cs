using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

using System.Windows.Forms;

namespace Paulus.Serial.Flaps
{
    public class Flap3Chart : LiveChart
    {
        public Flap3Chart(Chart chart, ChartArea chartArea, int pollingInterval, Flap3Commander agent)
            : base(chart, chartArea, pollingInterval)
        {
            this.agent = agent;
        }

        Flap3Commander agent;

        protected override void initializeChart()
        {
            base.initializeChart();

            ChartArea c = _chartArea;
            c.AxisY.MajorGrid.Enabled = true;
            c.AxisY.Maximum = 100;
            c.AxisY.Interval = 10;
            c.AxisY.MajorGrid.Interval = 10;
            c.AxisY.Minimum = 0;
            c.AxisY.Title = "Flap Position [%]";
        }

        public LiveSeries SeriesFlapAActual { get; private set; }
        public LiveSeries SeriesFlapBActual { get; private set; }
        public LiveSeries SeriesFlapCActual { get; private set; }
        public LiveSeries SeriesFlapATarget { get; private set; }

        public LiveSeries SeriesFlapBTarget { get; private set; }
        public LiveSeries SeriesFlapCTarget { get; private set; }


        protected override void initializeSeries()
        {
            base.initializeSeries(); //clean series

            SeriesFlapAActual = Add("Flap A Actual", Color.Blue);
            SeriesFlapATarget = Add("Flap A Target", Color.LightBlue);

            SeriesFlapBActual = Add("Flap B Actual", Color.Green);
            SeriesFlapBTarget = Add("Flap B Target", Color.LightGreen);

            SeriesFlapCActual = Add("Flap C Actual", Color.Red);
            SeriesFlapCTarget = Add("Flap C Target", Color.Orange);
        }

        protected override void updateValues()
        {
            base.updateValues();

            SeriesFlapAActual?.AddPoint(agent.FlapA.Position.ActualValue);
            SeriesFlapATarget?.AddPoint(agent.FlapA.Position.TargetValue);

            SeriesFlapBActual?.AddPoint(agent.FlapB.Position.ActualValue);
            SeriesFlapBTarget?.AddPoint(agent.FlapB.Position.TargetValue);

            SeriesFlapCActual?.AddPoint(agent.FlapC.Position.ActualValue);
            SeriesFlapCTarget?.AddPoint(agent.FlapC.Position.TargetValue);
        }

    }
}
