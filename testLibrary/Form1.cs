using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Paulus.Forms;

namespace testLibrary
{
    public partial class Form1 : ExtendedForm
    {
        public Form1()
        {
            InitializeComponent();

            //sample comment to test sync

            customCursor = CursorExtensions.CreateSampleCursor();
        }

        Cursor customCursor;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            customCursor.Dispose();

            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Cursor = customCursor;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
