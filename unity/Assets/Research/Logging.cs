using System;
using UnityEngine;

namespace WorldAsSupport.Research {
    public static class LogType {
        public static string POSITION = "POSITION";
        public static string INTERACTION = "INTERACTION";
        public static string OBSERVATION = "OBSERVATION";
    }

    public static class ActionType {
        public static string GRAB = "GRAB";
        public static string DROP = "DROP";
    }

    public static class OperatingSystem {
        public static string WIN = "WIN";
        public static string OSX = "OSX";
        public static string IOS = "IOS";
        public static string UNKNOWN = "UNKNOWN";
    }

    [Serializable]
    public class UserData {
        public string gender;
        public string age;
    }

    [Serializable]
    public class EnvironmentData {
        public string operatingSystem;
    }

    [Serializable]
    public class LogEventData {
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public string action;
        public string item;
        public int time;
    }
    
    public static class Environment {
        public static EnvironmentData GetEnvironment() {
            EnvironmentData envData = new EnvironmentData();
            envData.operatingSystem = OperatingSystem.UNKNOWN;
            #if UNITY_IOS
                envData.operatingSystem = OperatingSystem.IOS;
            #endif
            #if UNITY_STANDALONE_WIN
                envData.operatingSystem = OperatingSystem.WIN;
            #endif
            #if UNITY_STANDALONE_OSX
                envData.operatingSystem = OperatingSystem.OSX;
            #endif
            return envData;
        }
    }
}
