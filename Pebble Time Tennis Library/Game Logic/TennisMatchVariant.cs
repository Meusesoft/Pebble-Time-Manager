using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    [DataContract]
    public class TennisMatchVariant
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int NumberGamesPerSet { get; set; }

        [DataMember]
        public int TieBreakAtSameScoreOf { get; set; }

        [DataMember]
        public int TieBreakLength { get; set; }

        [DataMember]
        public bool DeuceSuddenDeath { get; set; }

        [DataMember]
        public bool TieBreakFinalSet { get; set; }

        [DataMember]
        public bool FinalSetIsTiebreak { get; set; }

        [DataMember]
        public int FinalSetTieBreakLength { get; set; }
    }
}
