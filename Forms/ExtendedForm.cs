using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Configuration;
using Paulus.Drawing;

namespace Paulus.Forms
{
    public enum FormRegionType
    {
        None, RoundedRectangle, BitmapMask
    }

    public class ExtendedForm : Form
    {
        // 1) DRAGGABLE FORM
        //       1a) Set Draggable property to true at any time in the program.
        // 2) USE COMMON PICTURE-BUTTON LIKE OPERATIONS
        //       2a) override the LoadPictureBoxToImageListDictionary function
        //           -) add a line for each pair eg. pictureBoxToImageList.Add(picSearch, img32);
        //       2b) the imagelist must contain pair of images corresponding to Leave and Enter events as explained below:
        //   eg. "picSearch" and "picSearchEnter" correspond to the MouseLeave and MouseEnter events of the picSearch picturebox
        // 3) STORE THE CURRENT POSITION OF THE FORM
        //      3a) Set the StoreLocation/StoreSize at any time preferably at design/time or at the constructor
        //      *** StoreSize is ignored if the FormBorderStyle is other than Sizable, SizableToolWindow 
        // 4) SET NON-CUSTOM FORM REGION
        //      4a) Set the RegionType property to set the desired region type at design time or constructor.
        //      4b) If the BitmapMask is used set the TransparencyKey for use at the constructor.
        //      4c) Optionally Set at design time or inside the constructor the:
        //          Radius, CropRectangle, CornerType if RegionType is RoundedRectangle
        //          or the TransparencyTolerance if RegionType is BitmapMask

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) { base.OnLoad(e); return; }

            #region Load picturebox to imagelist dictionary and set handlers
            pictureBoxToImageList = new Dictionary<PictureBox, ImageList>();
            LoadPictureBoxToImageListDictionary();
            SetPictureBoxEventHandlers();
            #endregion

            #region Set form region
            //Hide(); //the form has to be hidden during this operation
            if (_regionType != FormRegionType.None)
            {
                switch (_regionType)
                {
                    case FormRegionType.BitmapMask:
                        Region = BitmapMaskRegion.GetRegion((Bitmap)BackgroundImage,TransparencyKey);//BitmapMaskRegion.CreateFast((Bitmap)BackgroundImage, TransparencyKey, TransparencyTolerance); 
                        
                        break;
                    case FormRegionType.RoundedRectangle:
                        Region = GetRoundedRectanglePath(); break;
                    default: break;
                }
            }
            #endregion

            //load the local settings file that contains FormLocation and FormSize parameters
            this.Settings = ExtendedFormSettings.Default;

            //works and its great!
            openHand = new Cursor(FormResources.open_hand.GetHicon());
            closedHand = new Cursor(FormResources.closed_hand.GetHicon());

            if (shouldUseOpenHand) Cursor = openHand;

            base.OnLoad(e);
        }

        Cursor openHand, closedHand;

        protected override void OnClosing(global::System.ComponentModel.CancelEventArgs e)
        {
            if (_settings != null &&
                WindowState != FormWindowState.Minimized && WindowState != FormWindowState.Maximized)
            {
                if (_storeSize) _settings["FormSize"] = Size;
                if (_storeLocation || _storeSize) Settings.Save(); //the location binding works
            }

            base.OnClosing(e);
        }

        #region Startup position and size


        #region StoreLocation
        private bool _storeLocation;
        public bool StoreLocation
        {
            get { return _storeLocation; }
            set
            {
                if (_storeLocation != value)
                {
                    _storeLocation = value;
                    OnStoreLocationChanged();
                }
            }
        }

        public event EventHandler StoreLocationChanged;
        protected void OnStoreLocationChanged()
        {
            if (shouldBindLocation) bindLocationToSettings();

            var handler = StoreLocationChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region Location and settings
        private bool shouldBindLocation { get { return _settings != null && _storeLocation && locationBinding == null; } }

        private Binding locationBinding;
        private void bindLocationToSettings()
        {
            locationBinding = new Binding("Location", Settings, "FormLocation", true, DataSourceUpdateMode.OnPropertyChanged);
            this.DataBindings.Add(locationBinding);
        }

        #endregion
        #endregion

        #region StoreSize
        private bool _storeSize;
        public bool StoreSize
        {
            get { return _storeSize; }
            set
            {
                if (_storeSize != value)
                {
                    _storeSize = value;
                    OnStoreSizeChanged();
                }
            }
        }

        public event EventHandler StoreSizeChanged;
        protected void OnStoreSizeChanged()
        {
            if (shouldReadSize) readSizeFromSettings();

            var handler = StoreSizeChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region Size and settings
        private bool sizeIsReadFromSettings;
        private void readSizeFromSettings()
        {
            //size is loaded only here (manually)
            Size settingsSize = (Size)_settings["FormSize"];
            if (!settingsSize.IsEmpty)
                Size = settingsSize; //manual set because the Binding with size is problematic (according to MSDN)
            sizeIsReadFromSettings = true;
        }
        private bool shouldReadSize
        {
            get
            {
                return _settings != null && _storeSize && !sizeIsReadFromSettings &&
                    FormBorderStyle == FormBorderStyle.Sizable ||
                    FormBorderStyle == FormBorderStyle.SizableToolWindow;
            }
        }
        #endregion

        #endregion

        private void readSettings()
        {
            if (shouldBindLocation) bindLocationToSettings();

            if (shouldReadSize) readSizeFromSettings();
        }

        private ApplicationSettingsBase _settings;
        private ApplicationSettingsBase Settings
        {
            get { return _settings; }
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    readSettings();
                }
            }
        }

        #endregion

        #region Mouse click move

        #region Draggable
        protected bool _draggable;
        public bool Draggable
        {
            get { return _draggable; }
            set
            {
                if (_draggable != value)
                {
                    _draggable = value;
                    OnDraggableChanged();
                }
            }
        }

        private bool shouldUseOpenHand { get { return !DesignMode && _draggable && openHand != null && _useDraggableHand; } }

        public event EventHandler DraggableChanged;
        protected void OnDraggableChanged()
        {
            if (shouldUseOpenHand) Cursor = openHand;
            var handler = DraggableChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion


        #region UseDraggableHand
        protected bool _useDraggableHand=true;
        public bool UseDraggableHand
        {
            get { return _useDraggableHand; }
            set
            {
                if (_useDraggableHand != value)
                {
                    _useDraggableHand = value;
                    OnUseDraggableHandChanged();
                }
            }
        }

        public event EventHandler UseDraggableHandChanged;
        protected void OnUseDraggableHandChanged()
        {
            if (shouldUseOpenHand) Cursor = openHand;
            var handler = UseDraggableHandChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion


        private Point lastPoint;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_draggable && e.Button == MouseButtons.Left)
            {
                lastPoint = new Point(e.X, e.Y);
                if (_useDraggableHand) Cursor = closedHand;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_draggable && e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_draggable && e.Button == MouseButtons.Left)
            {
                if (_useDraggableHand) Cursor = openHand;
            }
            base.OnMouseUp(e);
        }

        #endregion

        #region Common picture box "buttons" operations
        protected Dictionary<PictureBox, ImageList> pictureBoxToImageList;
        public virtual void LoadPictureBoxToImageListDictionary()
        {
            //pictureBoxToImageList.Add(picSearch, img32);
            //pictureBoxToImageList.Add(picInfo, img32);
            //pictureBoxToImageList.Add(picX, img32);
            //pictureBoxToImageList.Add(picChange, imgRefresh);
        }

        private void SetPictureBoxEventHandlers()
        {
            foreach (PictureBox pic in pictureBoxToImageList.Keys)
            {
                pic.MouseEnter += pic_MouseEnter;
                pic.MouseLeave += pic_MouseLeave;
                pic.MouseDown += pic_MouseDown;
                pic.MouseUp += pic_MouseUp;
            }
        }

        #region Common events (Enter,Leave,Down,Up)
        private void pic_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            pic.Image = pictureBoxToImageList[pic].Images[pic.Name + "Enter"];
        }

        private void pic_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            pic.Image = pictureBoxToImageList[pic].Images[pic.Name];
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            Point p = pic.Location;
            p.Offset(1, 1); pic.Location = p;
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            Point p = pic.Location;
            p.Offset(-1, -1); pic.Location = p;
        }
        #endregion

        #endregion

        #region Form region
        protected FormRegionType _regionType = FormRegionType.None;
        public FormRegionType RegionType { get { return _regionType; } set { _regionType = value; } }

        //used when the RegionType is RoundedRectangle
        protected int _roundedRectangleRadius = 14;
        public int RoundedRectangleRadius { get { return _roundedRectangleRadius; } set { _roundedRectangleRadius = value; } }

        protected RoundedRectangle.Corners _cornerType = RoundedRectangle.Corners.All;
        public RoundedRectangle.Corners CornerType { get { return _cornerType; } set { _cornerType = value; } }

        protected Rectangle _cropRectangle = new Rectangle(13, 13, 545, 324);
        public Rectangle CropRectangle { get { return _cropRectangle; } set { _cropRectangle = value; } }

        private Region GetRoundedRectanglePath()
        {
            return RoundedRectangle.Create(_cropRectangle, _roundedRectangleRadius, _cornerType);
        }

        //used when the RegionType is BitmapMask
        protected int _transparencyTolerance = 100;
        public int TransparencyTolerance { get { return _transparencyTolerance; } set { _transparencyTolerance = value; } }

        #endregion
    }
}
