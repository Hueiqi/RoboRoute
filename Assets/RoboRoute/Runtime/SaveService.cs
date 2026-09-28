using System;
using System.IO;
using UnityEngine;

namespace RoboRoute
{
    public static class SaveService
    {
        public static string PathName => Path.Combine(Application.persistentDataPath, Array.IndexOf(Environment.GetCommandLineArgs(), "-roboroute-demo") >= 0 ? "roboroute-smoke.json" : "roboroute-shift.json");

        public static bool Save(ShiftState state, out string message)
        {
            try
            {
                if (!state.IsValid()) throw new InvalidDataException("Invalid shift state.");
                string temp = PathName + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(state, true));
                if (File.Exists(PathName)) File.Replace(temp, PathName, PathName + ".bak");
                else File.Move(temp, PathName);
                message = "Shift saved.";
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("RoboRoute save: " + ex.Message);
                message = "Could not save. Check Console and folder permissions.";
                return false;
            }
        }

        public static bool Load(out ShiftState state, out string message)
        {
            state = null;
            try
            {
                if (!File.Exists(PathName)) { message = "No saved shift yet."; return false; }
                state = JsonUtility.FromJson<ShiftState>(File.ReadAllText(PathName));
                if (state == null || !state.IsValid()) throw new InvalidDataException("Unsupported or damaged save.");
                message = "Shift loaded. Robot returned to charging dock.";
                return true;
            }
            catch (Exception ex)
            {
                state = null;
                Debug.LogWarning("RoboRoute load: " + ex.Message);
                message = "Save could not be loaded; current shift was kept.";
                return false;
            }
        }
    }
}

