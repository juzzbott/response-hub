using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
    public enum JobCodeType
    {

        [Description("Unknown")]
        UKN = 0,

        [Description("Incident")]
        INCI = 1,

        [Description("Structure Fire")]
        STRU = 2,

        [Description("Grass & Scrub")]
        G_AND_S = 3,

        [Description("Non-Structure Fire")]
        NOST = 4,

        [Description("Alarm")]
        ALAR = 5,

        [Description("Rescue")]
        RESC = 6,

        [Description("Assist Police")]
        ASSIST_POLICE = 7,

        [Description("Assist Ambulance")]
        ASSIST_AV = 8,

        [Description("Flood")]
        FLOOD = 9,

        [Description("Tree Down / Traffic Hazard")]
        TREEDWN_TRFHZD = 10,

        [Description("Building Damage")]
        BLD_DMG = 11,

        [Description("Tree Down")]
        TREE_DOWN = 12,

        [Description("Animal Incident")]
        ANIMAL_INCIDENT = 13,

        [Description("Structure Collapse")]
        STCO = 14

    }
}
