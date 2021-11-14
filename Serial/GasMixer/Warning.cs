using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    /// <summary>
    /// Warnings may be retrieved when in concentration of flow mode only.
    /// </summary>
    public enum Warning
    {
        [Description("No warning")]
        NoWarning = 0,
        [Description("Under 10%")]
        Under10 = 1,
        [Description("Over 90%")]
        Over90 = 2,
        [Description("Over max")]
        OverMax = 3,
        [Description("Under 0%")]
        Under0 = 4
    }
}
