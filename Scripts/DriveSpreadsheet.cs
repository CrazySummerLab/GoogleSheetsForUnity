using System;
using System.Collections.Generic;
using GoogleSheetsForUnity.Scripts.Connections;
using GoogleSheetsForUnity.Scripts.Enums;
using GoogleSheetsForUnity.Scripts.Scriptables;
using GoogleSheetsForUnity.Scripts.Structs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GoogleSheetsForUnity.Scripts
{
    public class DriveSpreadsheet
    {
        /// <summary>
        ///  Subscribe to this event to receive the response data from Google Drive.
        /// </summary>
        public static UnityAction<ContainerDataStruct> responseCallback;
        /// <summary>
        /// Subscribe to this event if you want to receive error callbacks.
        /// </summary>
        public static UnityAction<string> errorResponseCallback;

        /// <summary>
        /// Retrieves from the spreadsheet an array of all the objects found in the specified table. 
        /// Expects the table name. 
        /// </summary>
        /// <param name="tableTypeName">The name of the table to be retrieved.</param>
        /// <param name="runtime">Bool value indicating if the request was sent from Unity Editor or running game.</param>
        public static void GetSpreadsheet(string tableTypeName)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("action", DriveQueryType.GetSpreadsheet.ToString());
            form.Add("type", tableTypeName);

            CreateRequest(form);
        }

        #region Connection Handling
        private static void CreateRequest(Dictionary<string, string> dataForm)
        {
            var connectionData = Resources.Load<ConnectionData>(typeof(ConnectionData).Name);
            if (connectionData == null)
                return;

            var form = CompleteForm(connectionData, dataForm);
            if (form == null)
                return;

            UnityWebRequest www;

            if (connectionData.usePOST)
            {
                UpdateStatus("Establishing Connection at URL ", connectionData.webServiceUrl);
                www = UnityWebRequest.Post(connectionData.webServiceUrl, form);
            }
            else
            {
                string urlParams = "?";
                foreach (KeyValuePair<string, string> item in form)
                {
                    urlParams += item.Key + "=" + item.Value + "&";
                }
                UpdateStatus("Establishing Connection at URL ", connectionData.webServiceUrl, urlParams);
                www = UnityWebRequest.Get(connectionData.webServiceUrl + urlParams);
            }

#if UNITY_EDITOR
            var driveConnectorRuntimeEditor = UnityEditor.Editor.CreateInstance<DriveConnectionEditor>();
            if (driveConnectorRuntimeEditor == null)
                return;
            driveConnectorRuntimeEditor.ExecuteRequest(www, connectionData);
#else
            var driveConnectorRuntime = GameObject.FindObjectOfType<DriveConnection>();
            if (driveConnectorRuntime == null)
                return;
            driveConnectorRuntime.ExecuteRequest(www, connectionData);
#endif

        }

        private static Dictionary<string, string> CompleteForm(ConnectionData connectionData, Dictionary<string, string> form)
        {
            form.Add("ssid", connectionData.spreadsheetId);
            form.Add("password", connectionData.servicePassword);
            return form;
        }

#endregion

#region Response Handling
        // This method is called from the connection handlers (DriveConnection or DriveConnectionEditor).
        public static void ProcessResponse(string response, float time)
        {
            ContainerDataStruct dataContainer;
            try
            {
                dataContainer = JsonUtility.FromJson<ContainerDataStruct>(response);
            }
            catch (Exception)
            {
                HandleError("Undefined server response: \n" + response, time);
                return;
            }

            if (dataContainer.result == "ERROR")
            {
                HandleError(dataContainer.msg, time);
                return;
            }

            if (string.IsNullOrEmpty(dataContainer.result) || dataContainer.result != "ERROR" && dataContainer.result != "OK")
            {
                HandleError("Undefined server response: \n" + response, time);
                return;
            }

            if (responseCallback != null)
                responseCallback(dataContainer);
        }

        public static void HandleError(string response, float time)
        {
            UpdateStatus(response);

            if (errorResponseCallback != null)
                errorResponseCallback(response);
        }
#endregion

        private static void UpdateStatus(params string[] statusDetails)
        {
            Debug.Log(string.Concat(statusDetails));
        }
    }

    // Helper class: because UnityEngine.JsonUtility does not support deserializing an array...
    // http://forum.unity3d.com/threads/how-to-load-an-array-with-jsonutility.375735/
    public class JsonHelper
    {
        public static T[] ArrayFromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        public static string ToJson<T>(T[] array, bool prettyPrint = false)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.array = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
