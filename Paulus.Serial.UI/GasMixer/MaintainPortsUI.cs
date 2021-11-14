using DevExpress.XtraGrid.Views.Grid;
using Paulus.Serial.GasMixer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.UI.GasMixer
{
    public class MaintainPortsUI : DeviceUIBase<GasMixerCommander, GasMixerSettings>
    {
        public MaintainPortsUI(
            GridView gridViewPorts, GridView gridViewMfcPorts, GasMixerUI parent) : base(parent)
        {
            PortsUI = new GridViewPortsUI(gridViewPorts, this);
            MFCsUI = new GridViewMfcsUI(gridViewMfcPorts, this);
        }

        public GridViewPortsUI PortsUI { get; }

        public GridViewMfcsUI MFCsUI { get; }


        internal async Task<bool> GotoPortMaintenanceMode(bool uploadSettingsToGasMixer)
        {

            var reply = await DeviceCommander.Home();
            if (reply.IsError)
            {
                reply = await DeviceCommander.Home(); //send once more
                if (reply.IsError)
                    OnExceptionThrown(reply.Exception); return false;
            }

            if (uploadSettingsToGasMixer)
            {
                bool success = await PortsUI.AssignCylindersAfterConnection();
                if (!success) return false;

                success = await MFCsUI.AssignMfcPortsAfterConnection();
                if (!success) return false;
            }

            return true;
        }




    }
}
