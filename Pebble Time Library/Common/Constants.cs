using System;
using System.Collections.Generic;
using System.Text;

namespace Pebble_Time_Manager.Common
{
    public enum BCState
    {
        OK = 0x00,
        ConnectionFailed = 0x01,
        ExceptionOccurred = 0x02
    }

    public static class Constants
    {
        public const string PebbleGuid = "00000000-deca-fade-deca-deafdecacaff";

        public const String BackgroundCommunicationTaskName = "PBC";
        public const String BackgroundCommunicationTaskEntry = "BackgroundTasks.BackgroundCommunication";
        public const String BackgroundCommunicatieIsRunning = "BCIR";
        public const String BackgroundCommunicatieError = "BCIE";
        public const String BackgroundCommunicatieContinue = "BCC";
        public const String BackgroundCommunicatieLog = "BCL";
        public const String BackgroundCommunicatieSelectItem = "BCSI";
        public const String BackgroundCommunicatieLaunchItem = "BCLI";
        public const String BackgroundCommunicatieDownloadedItem = "BCDI";

        public const String BackgroundPace = "BackgroundPace";
        public const String PaceDuration = "PDu";
        public const String PaceDistance = "PDi";
        public const String PacePace = "PacePace";
        public const String PacePaused = "PacePaused";
        public const String PaceSwitchPaused = "SwitchPaused";
        public const String PaceGPX = "PaceGPX";
        public const String Miles = "Miles";
        public const String PaceGPXFile = "activity.gpx";

        public const String BackgroundTennis = "BackgroundTennis";
        public const String TennisAppGuid = "506ba551-7df8-41ce-a0ff-30d03a88fa8d";
        public const String TennisState = "TennisState";
        public const String TennisCommand = "TennisCommand";

        public const String PebbleWatchItem = "PWI";
        public const String PebbleShowConfiguration = "PSC";
        public const String PebbleWebViewClosed = "PWVC";
    }
}
