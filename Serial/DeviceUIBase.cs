using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial
{

    public abstract class DeviceUIBase<TCommander, TSettings>
        where TCommander : DeviceCommander where TSettings : DeviceSettingsBase
    {
        public DeviceUIBase(DeviceUIBase<TCommander, TSettings> parent) :
            this(parent.deviceCommander)
        {
            Parent = parent;
            //cascade the error to its parent
            parent.children.Add(this);
        }

        public DeviceUIBase(TCommander deviceCommander)
        {
            DeviceCommander = deviceCommander;
        }

        protected List<DeviceUIBase<TCommander, TSettings>> children = new List<DeviceUIBase<TCommander, TSettings>>();

        /// <summary>
        /// Parent is set at the constructor level.
        /// </summary>
        public DeviceUIBase<TCommander, TSettings> Parent { get; }

        #region Exception management
        public event EventHandler<ExceptionEventArgs> ExceptionThrown;
        public Exception LastExceptionThrown { get; protected set; }

        /// <summary>
        /// If CascadeException is true then the exception is raised from the first ascendant which has CascadeException is false.
        /// If all ascendants have this property to true, then the top ascendant raises the event.
        /// </summary>
        public bool CascadeException { get; set; } = true;

        protected virtual void OnExceptionThrown(ExceptionEventArgs e)
        {
            LastExceptionThrown = e.Exception;

            if (Parent == null || !CascadeException)
                ExceptionThrown?.Invoke(this, e);
            else if (Parent != null && CascadeException)
                Parent.OnExceptionThrown(e);
        }
        protected void OnExceptionThrown(System.Exception exception) =>
          OnExceptionThrown(new ExceptionEventArgs(exception));
        #endregion

        #region Agent
        protected TCommander deviceCommander;
        /// <summary>
        /// This is the "command sender" and the container of the device "gasmixer" settings.
        /// </summary>
        public TCommander DeviceCommander
        {
            get { return deviceCommander; }
            set
            {
                if (deviceCommander == value) return;
                if (value == null) throw new ArgumentNullException(nameof(DeviceCommander));

                //if agent is already assigned then remove handlers
                if (deviceCommander != null)
                {
                    deviceCommander.Connected -= Agent_Connected;
                    deviceCommander.Disconnecting -= Agent_Disconnecting;
                    deviceCommander.Disconnected -= Agent_Disconnected;
                }

                deviceCommander = value;
                foreach (var child in children)
                    child.DeviceCommander = value;

                deviceCommander.Connected += Agent_Connected;
                deviceCommander.Disconnecting += Agent_Disconnecting;
                deviceCommander.Disconnected += Agent_Disconnected;

                OnAgentChanged();
            }
        }

        protected virtual void Agent_Disconnecting(object sender, EventArgs e) { DisableControls(); }
        protected virtual void Agent_Disconnected(object sender, EventArgs e) { }
        protected virtual void Agent_Connected(object sender, EventArgs e) { }

        public event EventHandler AgentChanged;
        protected virtual void OnAgentChanged() =>
            AgentChanged?.Invoke(this, EventArgs.Empty);

        public bool CascadeDisableControls { get; set; } = true;
        public bool CascadeEnableControls { get; set; } = true;

        public virtual void DisableControls()
        {
            if (CascadeDisableControls)
                foreach (var child in children)
                    child.DisableControls();
        }

        public virtual void EnableControls()
        {
            if (CascadeEnableControls)
                foreach (var child in children)
                    child.DisableControls();
        }

        #endregion

        #region Settings

        /// <summary>
        /// Represent the gasmixer settings after any edit but before any commit. After successful commits then they
        /// coincide with the Agent.GasMixer settings.
        /// </summary>
        public TSettings EditSettings
        {
            get
            {
                return (TSettings)deviceCommander.EditSettings;
            }
        }

        public TSettings RuntimeSettings
        {
            get
            {
                return (TSettings)deviceCommander.RuntimeSettings;
            }
        }
        #endregion
    }
}
