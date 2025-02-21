#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace Odeeo.Editor
{
    public class OdeeoEdmInstaller
    {
        private const string DIALOG_TEXT_TITLE = "Required External Dependency Manager";
        private const string DIALOG_TEXT_MESSAGE = "Odeeo SDK requires External Dependency Manager to resolve native dependencies.\nWould you like to import EDM package?";

        private const string DIALOG_ERROR_TITLE = "Error";
        private const string MESSAGE_DOWNLOAD_COMPLETE = "Download Complete";
        private const string MESSAGE_DOWNLOAD_ERROR = "Failed to download EDM package.";
        private const string MESSAGE_INSTALL_ERROR = "Can't install EDM package. File not exists";
        
        private const string BUTTON_TEXT_OK = "Ok";
        private const string BUTTON_TEXT_IMPORT = "Import";
        private const string BUTTON_TEXT_CANCEL = "Cancel";
        private const string BUTTON_TEXT_SKIP = "Skip - Do not ask again during this session";

        private const string DOWNLOAD_URL = "https://raw.githubusercontent.com/googlesamples/unity-jar-resolver/master/external-dependency-manager-latest.unitypackage";
        private const string DOWNLOAD_LOCATION = "Temp/ExternalDependencyManager.unitypackage";

        private const string KEY_DO_NOT_ASK = "Odeeo.EdmInstaller.DoNotAsk";

        private static bool _isQuittingCallbackAdded = false;
        
        [InitializeOnLoadMethod]
        private static void CheckAndInstallEdm()
        {
            if(IsEdmInstalled())
                return;

            if (!_isQuittingCallbackAdded)
            {
                EditorApplication.quitting += EditorApplicationOnQuitting;
                _isQuittingCallbackAdded = true;
            }

            if ((Application.isBatchMode && Environment.GetEnvironmentVariable("UNITY_THISISABUILDMACHINE") != null) || ShowInstallDialog())
                DownloadPackage();
        }
        
        private static bool ShowInstallDialog()
        {
            if (EditorPrefs.GetBool(KEY_DO_NOT_ASK))
                return false;

            int result = EditorUtility.DisplayDialogComplex(DIALOG_TEXT_TITLE, DIALOG_TEXT_MESSAGE,
                BUTTON_TEXT_IMPORT, BUTTON_TEXT_CANCEL, BUTTON_TEXT_SKIP);

            switch (result)
            {
                case 0:
                    return true;
                case 1:
                    return false;
                case 2:
                    EditorPrefs.SetBool(KEY_DO_NOT_ASK, true);
                    return false;
                default:
                    return false;
            }
        }
        
        static void EditorApplicationOnQuitting()
        {
            EditorPrefs.DeleteKey(KEY_DO_NOT_ASK);
        }

        private static void DownloadPackage()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(DOWNLOAD_URL, DOWNLOAD_LOCATION);
                    Log(MESSAGE_DOWNLOAD_COMPLETE);
                }
                catch (Exception e)
                {
                    if (!Application.isBatchMode)
                        EditorUtility.DisplayDialog(DIALOG_ERROR_TITLE, MESSAGE_DOWNLOAD_ERROR + "\n" + e.Message, BUTTON_TEXT_OK);
                    
                    Log(MESSAGE_DOWNLOAD_ERROR + ": " + e.Message);
                }
            }
            
            InstallPackage();
        }

        private static void InstallPackage()
        {
            string path = Path.GetFullPath(DOWNLOAD_LOCATION);

            if (!File.Exists(path))
            {
                if (!Application.isBatchMode)
                    EditorUtility.DisplayDialog(DIALOG_ERROR_TITLE, MESSAGE_INSTALL_ERROR, BUTTON_TEXT_OK);
                
                Log(MESSAGE_INSTALL_ERROR);
                return;
            }

            AssetDatabase.ImportPackage(path, false);
            File.Delete(path);
        }
        
        private static bool IsEdmInstalled()
        {
            try
            {
                return Type.GetType("Google.VersionHandler, Google.VersionHandler") != null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static void Log(string message)
        {
            Debug.Log("Odeeo EDM Installer: " + message);
        }
    }
}
#endif