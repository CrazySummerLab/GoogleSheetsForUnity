#if UNITY_EDITOR
using GoogleSheetsForUnity.Scripts.Scriptables;
using UnityEditor;
using UnityEngine.Networking;

namespace GoogleSheetsForUnity.Scripts.Connections
{
    public class DriveConnectionEditor : Editor
    {
        private UnityWebRequest _www;
        public ConnectionData _connectionData;
        private double _elapsedTime = 0.0f;
        private double _startTime = 0.0f;

        public void ExecuteRequest(UnityWebRequest www, ConnectionData connectionData)
        {
            EditorApplication.update += EditorUpdate;
            _www = www;
            _connectionData = connectionData;
            _startTime = EditorApplication.timeSinceStartup;
            
            _www.SendWebRequest();
        }

        private void EditorUpdate()
        {
            while (!_www.isDone)
            {
                _elapsedTime = EditorApplication.timeSinceStartup - _startTime;
                if (_elapsedTime >= _connectionData.timeOutLimit)
                {
                    DriveSpreadsheet.ProcessResponse("TIME_OUT", (float)_elapsedTime);
                    EditorApplication.update -= EditorUpdate;
                }
                return;
            }

            if (_www.isNetworkError)
            {
                DriveSpreadsheet.ProcessResponse("Connection error after " + _elapsedTime.ToString() + " seconds: " + _www.error, (float)_elapsedTime);
                return;
            }

            DriveSpreadsheet.ProcessResponse(_www.downloadHandler.text, (float)_elapsedTime);

            EditorApplication.update -= EditorUpdate;
        }
    }
}
#endif
