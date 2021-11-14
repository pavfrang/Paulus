using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using static Paulus.Serial.GasMixer.UnitConversions;

namespace Paulus.Serial.GasMixer
{
    /// <summary>
    /// Data Tables contain the device data in tabular format (ready to be used by DataGrids).
    /// </summary>
    public class GasMixerSettingsDataTables
    {
        public GasMixerSettingsDataTables(GasMixerSettings settings)
        {
            Settings = settings;
        }

        GasMixerSettings Settings { get; }

        Dictionary<int, Port> Ports { get { return Settings.Ports; } }
        Dictionary<int, MFC> MFCs { get { return Settings.MFCs; } }


        #region Ports table

        public DataTable PortsTable { get; private set; }

        public DataTable UpdatePortsTableForDataGrid()
        {
            DataTable portsTable = initializePortsTable();
            foreach (Port port in Ports.Values)
            {
                DataRow row = portsTable.NewRow();
                row["Port"] = port.ID;
                row["MFC"] = MFCs.First(m => m.Value.Ports.Contains(port)).Key;
                row["Cylinder"] = port.Cylinder;
                row["Code"] = port.Cylinder.CylinderCode;
                row["ID"] = port.Cylinder.ID;
                row["Volume [l]"] = port.Cylinder.CurrentSizeInLiters;
                row["K-Factor"] = port.Cylinder.GasMixture.KFactor;
                portsTable.Rows.Add(row);
            }
            return PortsTable = portsTable;
        }

        private DataTable initializePortsTable()
        {
            DataTable portsTable = new DataTable();
            portsTable.Columns.Add("Port", typeof(int));
            portsTable.Columns.Add("MFC", typeof(int));
            portsTable.Columns.Add("Cylinder", typeof(Cylinder));
            portsTable.Columns.Add("Code", typeof(string));
            portsTable.Columns.Add("ID", typeof(string));
            portsTable.Columns.Add("Volume [l]", typeof(float));
            portsTable.Columns.Add("K-Factor", typeof(float));
            return portsTable;
        }
        #endregion

        #region MFCs table

        public DataTable MFCsTable { get; private set; }


        public DataTable UpdateMfcsTableForDataGrid()
        {
            DataTable mfcsTable = initializeMfcsTable();
            foreach (MFC mfc in MFCs.Values)
            {
                DataRow row = mfcsTable.NewRow();
                row["MFC"] = mfc.ID;
                row["Maximum Flow [l/min]"] = mfc.SizeInCcm / 1000.0f;
                row["Port"] = mfc.CurrentPort.ID;
                row["Cylinder"] = mfc.CurrentPort.Cylinder;
                mfcsTable.Rows.Add(row);
            }
            return MFCsTable = mfcsTable;
        }

        private DataTable initializeMfcsTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MFC", typeof(int));
            table.Columns.Add("Maximum Flow [l/min]", typeof(float));
            table.Columns.Add("Port", typeof(int));

            table.Columns.Add("Cylinder", typeof(Cylinder));

            return table;
        }
        #endregion


        #region Concentration table

        public DataTable ConcentrationModeTable { get; private set; }

        public DataTable UpdateConcentrationModeTableForDataGrid()
        {
            DataTable table = initializeConcentrationModeTable();

            foreach (MFC mfc in MFCs.Values)
            {
                Port currentPort = mfc.CurrentPort;

                DataRow newRow = table.NewRow();
                newRow["MFC"] = mfc.ID;
                newRow["Port"] = currentPort.ID;
                newRow["Cylinder"] = currentPort.Cylinder;
                newRow["Balance"] = Settings.BalanceMfc.ID == mfc.ID;

                ConcentrationUnit unit = currentPort.Cylinder.GetDefaultConcentrationUnit();
                if (!unit.IsWellDefined(ConcentrationUnit.None))
                    throw new InvalidOperationException($"Unit is undefined for cylinder {currentPort.Cylinder.ToString()}.");

                newRow["Unit"] = unit.GetDisplayName();

                //if (mfc == Settings.BalanceMfc) newRow["Target Concentration"] = DBNull.Value;
                //else
                float sizeRatio = mfc.SizeInCcm / Settings.TotalTargetFlowInCcm;
                float maximumConcentration = (sizeRatio > 1 ? 1 : sizeRatio) * currentPort.ConcentrationInPpm * currentPort.KFactor;
                if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                {
                    if (unit == ConcentrationUnit.PPM)
                    {
                        newRow["Target Concentration"] = mfc.TargetConcentrationInPpm;
                        newRow["Maximum Concentration"] = maximumConcentration * 0.9f;
                        newRow["Minimum Concentration"] = maximumConcentration * 0.1f;
                    }
                    else //if (unit == ConcentrationUnit.PerCent) //per cent
                    {
                        newRow["Target Concentration"] = ToPerCent(mfc.TargetConcentrationInPpm);
                        newRow["Maximum Concentration"] = ToPerCent(maximumConcentration * 0.9f);
                        newRow["Minimum Concentration"] = ToPerCent(maximumConcentration * 0.1f);
                    }
                }
                else newRow["Maximum Concentration"] = newRow["Minimum Concentration"] = DBNull.Value;


                if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                {
                    if (unit == ConcentrationUnit.PPM)
                        newRow["Actual Concentration"] = mfc.ActualConcentrationInPpm;
                    else //%
                        newRow["Actual Concentration"] = ToPerCent(mfc.ActualConcentrationInPpm);
                }
                else
                    newRow["Actual Concentration"] = DBNull.Value;

                if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                {
                    newRow["Target Flow"] = Settings.TotalTargetFlowInCcm / 1000.0f * mfc.TargetConcentrationInPpm / currentPort.ConcentrationInPpm / currentPort.KFactor;
                    newRow["Actual Flow"] = Settings.TotalActualFlowInCcm / 1000.0f * mfc.ActualConcentrationInPpm / currentPort.ConcentrationInPpm / currentPort.KFactor;
                }
                else
                    newRow["Target Flow"] = newRow["Actual Flow"] = DBNull.Value;

                if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                {
                    float maximumFlow = (sizeRatio > 1f ? Settings.TotalTargetFlowInCcm : mfc.SizeInCcm) / 1000.0f / mfc.CurrentPort.KFactor;
                    newRow["Minimum Flow"] = maximumFlow * 0.1f;
                    newRow["Maximum Flow"] = maximumFlow * 0.9f;
                    newRow["Absolute Maximum Flow"] = maximumFlow; //in lpm
                }
                else
                    newRow["Minimum Flow"] = newRow["Maximum Flow"] = newRow["Absolute Maximum Flow"] = DBNull.Value;

                newRow["Status"] = mfc.Warning;//Warning.NoWarning;

                table.Rows.Add(newRow);
            }

            return ConcentrationModeTable = table;
        }

        private DataTable initializeConcentrationModeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MFC", typeof(int));
            table.Columns.Add("Port", typeof(int));
            table.Columns.Add("Cylinder", typeof(string));
            table.Columns.Add("Balance", typeof(bool));
            table.Columns.Add("Target Concentration", typeof(float));  //in the case of balance this is set to null
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("Actual Concentration", typeof(float));
            table.Columns.Add("Minimum Concentration", typeof(float));
            table.Columns.Add("Maximum Concentration", typeof(float));
            table.Columns.Add("Target Flow", typeof(float)); //in lpm
            table.Columns.Add("Actual Flow", typeof(float)); //in lpm
            table.Columns.Add("Minimum Flow", typeof(float)); //in lpm
            table.Columns.Add("Maximum Flow", typeof(float)); //in lpm
            table.Columns.Add("Absolute Maximum Flow", typeof(float)); //in lpm

            //table.Columns.Add("Cylinder Volume [l]", typeof(float));
            table.Columns.Add("Status", typeof(Warning));
            return table;
        }
        #endregion

        #region Flow table

        public DataTable FlowModeTable { get; private set; }

        public DataTable UpdateFlowModeTableForDataGrid()
        {
            DataTable table = initializeFlowModeTable();

            foreach (MFC mfc in MFCs.Values)
            {
                DataRow newRow = table.NewRow();
                newRow["MFC"] = mfc.ID;
                newRow["Port"] = mfc.CurrentPort.ID;
                newRow["Cylinder"] = mfc.CurrentPort.Cylinder;

                FlowUnit unit = FlowUnit.CCM; //TODO: Check if the Flow unit should be saved
                //if (!unit.IsWellDefined(FlowUnit.None))
                //    throw new InvalidOperationException($"Unit is undefined for cylinder {mfc.CurrentPort.Cylinder.ToString()}.");
                newRow["Unit"] = unit.GetDisplayName();


                if (unit == FlowUnit.CCM)
                    newRow["Target Flow"] = mfc.TargetFlowInCcm;
                else //if (unit == ConcentrationUnit.PerCent) //per cent
                    newRow["Target Flow"] = ToLpm(mfc.TargetFlowInCcm);

                if (unit == FlowUnit.CCM)
                    newRow["Actual Flow"] = mfc.ActualFlowInCcm;
                else //%
                    newRow["Actual Flow"] = ToLpm(mfc.ActualFlowInCcm);

                newRow["Status"] = Warning.NoWarning.GetDisplayName();

                table.Rows.Add(newRow);
            }

            return FlowModeTable = table;
        }

        private DataTable initializeFlowModeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MFC", typeof(int));
            table.Columns.Add("Port", typeof(int));
            table.Columns.Add("Cylinder", typeof(string));
            table.Columns.Add("Target Flow", typeof(float));  //in the case of balance this is set to null
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("Actual Flow", typeof(float));

            table.Columns.Add("Status", typeof(string));
            return table;
        }

        #endregion

        #region Purge table

        public DataTable PurgeModeTable { get; private set; }

        public DataTable UpdatePurgeModeTableForDataGrid()
        {
            DataTable table = initializePurgeModeTable();

            foreach (MFC mfc in MFCs.Values)
            {
                DataRow newRow = table.NewRow();
                newRow["MFC"] = mfc.ID;

                newRow["On/Off"] = true; //TODO: Check if the balance information should be storable

                //force unit to lpm (there is no need to use ccm unit)
                FlowUnit unit = FlowUnit.LPM; //TODO: Check if the Flow unit should be saved
                //if (!unit.IsWellDefined(FlowUnit.None))
                //    throw new InvalidOperationException($"Unit is undefined for cylinder {mfc.CurrentPort.Cylinder.ToString()}.");
                newRow["Unit"] = unit.GetDisplayName();

                if (unit == FlowUnit.CCM)
                    newRow["Maximum Flow"] = mfc.SizeInCcm;
                else
                    newRow["Maximum Flow"] = ToLpm(mfc.SizeInCcm);


                //  if (unit == FlowUnit.CCM)
                //     newRow["Target Flow"] = mfc.TargetPurgeFlowInCcm;
                // else //if (unit == ConcentrationUnit.PerCent) //per cent
                newRow["Target Flow"] = ToLpm(mfc.TargetPurgeFlowInCcm);

                //if (unit == FlowUnit.CCM)
                //    newRow["Actual Flow"] = mfc.ActualFlowInCcm;
                //else //%
                //    newRow["Actual Flow"] = ToLpm(mfc.ActualFlowInCcm);

                table.Rows.Add(newRow);
            }

            return PurgeModeTable = table;
        }

        private DataTable initializePurgeModeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MFC", typeof(int));
            table.Columns.Add("On/Off", typeof(bool));
            table.Columns.Add("Target Flow", typeof(float));  //in the case of balance this is set to null
            table.Columns.Add("Maximum Flow", typeof(float));
            table.Columns.Add("Unit", typeof(string));
            // table.Columns.Add("Actual Flow", typeof(float));
            return table;
        }


        #endregion


    }
}
