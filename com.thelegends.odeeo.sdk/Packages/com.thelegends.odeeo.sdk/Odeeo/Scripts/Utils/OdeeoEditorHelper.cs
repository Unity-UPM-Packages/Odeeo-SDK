using System;
using System.Collections.Generic;
using System.IO;
using Odeeo.Logging;
using UnityEditor;
using UnityEngine;

namespace Odeeo.Utils
{
    public static class OdeeoEditorHelper
    {
        private static readonly IOdeeoLogging _logging = new OdeeoLogging();
        private const string EDITOR_DATA_LOCAL_PATH = "Odeeo/Editor/Data";
        
        public static string GetAssetBasedPath(string localPath)
        {
            string path = "";

#if UNITY_EDITOR
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                if (allAssetPaths[i].EndsWith(localPath))
                    path = allAssetPaths[i];
            }
#endif

            return path;
        }
        
        internal static int GetScreenDPI(int width, int height)
        {
            // Find and read json file with real devices screens data
            string dataFolderPath = GetAssetBasedPath(EDITOR_DATA_LOCAL_PATH);
            string jsonPath = Path.Combine(dataFolderPath, "screens.json");

            if (!File.Exists(jsonPath))
            {
                _logging.Warning("Screen DPI: Screens data file not exist");
                return -1;
            }

            string dataString = File.ReadAllText(jsonPath);

            Screens screensList = JsonUtility.FromJson<Screens>(dataString);
            
            // Select data that match the width and height
            List<ScreenData> suitableScreens = new List<ScreenData>();
            
            for (int i = 0; i < screensList.screens.Length; i++)
            {
                if (!screensList.screens[i].IsDataValid())
                    continue;

                int w = screensList.screens[i].w;
                int h = screensList.screens[i].h;
                
                if(w == width && h == height || h == width && w == height)
                    suitableScreens.Add(screensList.screens[i]);
            }

            // If nothing found
            if (suitableScreens.Count == 0)
                return -1;

            // Return first matched device DPI
            return suitableScreens[0].GetDPI();
        }
    }

    [Serializable]
    internal struct Screens
    {
        public ScreenData[] screens;
    }
    
    [Serializable]
    internal struct ScreenData
    {
        public string name; // Device Name
        public int w; // Screen Width
        public int h; // Screen Height
        public float d; // Physical screen diagonal in inches
        public float ppi; // Pixels Per Inch (PPI or DPI)
        public float dppx; // Device scale factor

        public bool IsDataValid()
        {
            if (w <= 0 || h <= 0)
                return false;

            if (ppi <= 0 && d <= 0)
                return false;

            return true;
        }

        public int GetDPI()
        {
            // If real device DPI is present in the data
            if (ppi > 0)
                return Mathf.RoundToInt(ppi);

            // Calculate DPI based on physical diagonal
            float diagonalPx = Mathf.Sqrt(w * w + h * h);
            return Mathf.RoundToInt(diagonalPx / d);
        }
    }
}