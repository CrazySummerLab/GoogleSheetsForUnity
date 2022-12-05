using System.Collections;
using GoogleSheetsForUnity.Scripts.Scriptables;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleSheetsForUnity.Scripts.Connections
{
    public class DriveConnection : MonoBehaviour
    {
        public void ExecuteRequest(UnityWebRequest www, ConnectionData connectionData)
        {
            StartCoroutine(CoExecuteRequest(www, connectionData));
        }

        private IEnumerator CoExecuteRequest(UnityWebRequest www, ConnectionData connectionData)
        {
            www.SendWebRequest();

            float elapsedTime = 0.0f;

            while (!www.isDone)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= connectionData.timeOutLimit)
                {
                    DriveSpreadsheet.HandleError("Operation timed out, connection aborted. Check your internet connection and try again.", elapsedTime);
                    yield break;
                }

                yield return null;
            }

            if (www.isNetworkError)
            {
                DriveSpreadsheet.HandleError("Connection error after " + elapsedTime.ToString() + " seconds: " + www.error, elapsedTime);
                yield break;
            }
            DriveSpreadsheet.ProcessResponse(www.downloadHandler.text, elapsedTime);
        }

    }
}
