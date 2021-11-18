using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.AgentModel
{

    public enum MessageType
    {
        FromAgent,
        FromSystem
    }

    public class Message
    {

        public Message(string from, string to, string text, string[] args)
        {
            From = from;
            To = to;
            Text = text;
            Args = args;
        }

        public Message(string from, string to, string text)
        {
            From = from;
            To = to;
            Text = text;
            Args = new List<string>().ToArray();
        }

        //public MessageType MessageType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public string[] Args { get; set; }

    }

    public static class MessagesTexts
    {

        

        public static readonly string changeNormativeSagValue = "Change normative sag value";
        public static readonly string changeActualSagValue = "Change actual sag value";

        //Environment
        public static readonly string strongWind = "Strong wind";
        public static readonly string normalEnvironmentConditions = "Normal conditions";
        public static readonly string strongWindAndIceFormation = "Ice formation, strong wind";
        public static readonly string iceFormation = "Ice formation";

        public static readonly string breakRodInClamp = "Break rod in the clamp";
        public static readonly string breakRodInClampEliminated = "Break rods in the clamp eliminated";
        public static readonly string breakRodOutClamp = "Break rod outside the clamp";
        public static readonly string breakRodOutClampEliminated = "Break rods outside the clamp eliminated";

        public static readonly string cableCorrosion = "Cable corrosion";
        public static readonly string cableCorrosionEliminated = "Cable corrosion eliminated";

        public static readonly string updateStresses = "Update mechanical stresses";
        public static readonly string updateSags = "Update normative and actual sags";

        public static readonly string vibrationDamperOffset = "Vibration damper offset";
        public static readonly string vibrationDamperOffsetEliminated = "Vibration damper offset eliminated";
        public static readonly string vibrationDamperAbsence = "Vibration damper absence";
        public static readonly string vibrationDamperAbsenceEliminated = "Vibration damper absence eliminated";
        public static readonly string vibrationDamperWeightsAbsence = "Vibration damper weights absence";
        public static readonly string vibrationDamperWeightsAbsenceEliminated = "Vibration damper weights absence eliminated";


        public static readonly string updateTSV = "Update TSV";
        public static readonly string updateTSVByPointCloud = "Update TSV by point cloud";
        public static readonly string updateGladeWidth = "Update glade width";
        public static readonly string updateCableDimensions = "Update cables dimenstions in span";

        public static readonly string updateEnvironment = "Update environment params";


        public static readonly List<string> consoleMessages = new List<string>()
        {
            updateEnvironment,breakRodInClamp,breakRodOutClamp,cableCorrosion,vibrationDamperOffset,vibrationDamperAbsence,vibrationDamperWeightsAbsence,updateTSV,
            updateTSVByPointCloud,updateGladeWidth,breakRodInClampEliminated,breakRodOutClampEliminated,cableCorrosionEliminated,vibrationDamperOffsetEliminated,
            vibrationDamperAbsenceEliminated,vibrationDamperWeightsAbsenceEliminated
        };

        public static readonly Dictionary<string, List<string>> messagesArgsDict = new Dictionary<string, List<string>>()
        {
             {updateEnvironment, new List<string>(){ "IsTDifferenceToLessZeroCondition [0 - no, 1 - yes]",
                              "Temperature [Celsium]", "Wind speed [m/s]", "H [%]", "P [mm Hq]", "Wind direction [Perpendicular = 0, Along = 1]" } },
                  {breakRodInClamp, new List<string>(){"Phase","Distance from tower 1 [m.]", "Na", "Ns"} },

                    {breakRodOutClamp, new List<string>(){ "Phase", "Distance from tower 1 [m.]", "Na", "Ns" } },

                      {cableCorrosion, new List<string>(){ "Phase" } },

                          {vibrationDamperOffset, new List<string>(){ "Phase","Number" } },

                          {vibrationDamperAbsence, new List<string>(){ "Phase", "Number" } },

                          {vibrationDamperWeightsAbsence, new List<string>(){ "Phase", "Number" } },

                          {updateTSV, new List<string>(){ "MaxTreeHeightParam\n[1 ~ 0-1 m., 4 ~ 1-4 m., 5 ~ >4 m.]", "IsUnsafeTrees [0 - no, 1 - yes]" } },
                          {updateTSVByPointCloud, new List<string>(){ } },
                          {updateGladeWidth, new List<string>(){ "ActualValue" } },
                          {breakRodInClampEliminated, new List<string>(){ "Phase", "Distance from tower 1 [m.]"} },
                           {breakRodOutClampEliminated, new List<string>(){ "Phase","Distance from tower 1 [m.]"} },
                            {cableCorrosionEliminated, new List<string>(){ "Phase" } },
                             {vibrationDamperOffsetEliminated, new List<string>(){ "Phase", "Number" } },
                              {vibrationDamperAbsenceEliminated, new List<string>(){ "Phase", "Number" } },
                               {vibrationDamperWeightsAbsenceEliminated, new List<string>(){ "Phase", "Number" } }




        };
    }
}
