using System.Linq;


namespace Paulus.Forms.Charting
{
    public class Results
    {

        public Results(string name) { Name = name; }

        public Results(double[] x, double[] y, string name)
        {
            X = x; Y = y; Name = name;
        }
        
        #region X
        protected double[] _x;
        public double[] X
        {
            get { return _x; }
            set
            {
                _x = value;
                bool empty = IsXEmpty;
                _minX = !empty ? _x.Min() : double.NaN;
                _maxX = !empty ? _x.Max() : double.NaN;
            }
        }

        public bool IsXEmpty { get { return _x == null || _x.Length == 0; } }

        protected double _minX;
        public double MinX { get { return _minX; } }

        protected double _maxX;
        public double MaxX { get { return _maxX; } }
        #endregion

        #region Y
        protected double[] _y;
        public double[] Y
        {
            get { return _y; }
            set
            {
                _y = value;
                bool empty = IsYEmpty;
                _minY = !empty ? _y.Min() : double.NaN;
                _maxY = !empty ? _y.Max() : double.NaN;
            }
        }

        public bool IsYEmpty { get { return _y == null || _y.Length == 0; } }

        protected double _minY;
        public double MinY { get { return _minY; } }

        protected double _maxY;
        public double MaxY { get { return _maxY; } }
        #endregion

        public bool IsEmpty {get {return IsXEmpty || IsYEmpty;}}

        public string Name;

        public override string ToString()
        {
            return Name;
        }
    }
}