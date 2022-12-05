using System;
using GoogleSheetsForUnity.Scripts.Enums;

namespace GoogleSheetsForUnity.Scripts.Structs
{
    [Serializable]
    public struct ContainerDataStruct
    {
        public string query;
        public string result;
        public string msg;
        public string payload;

        public string objType;
        public string column;
        public string row;
        public string searchField;
        public string searchValue;
        public string value;

        public DriveQueryType QueryType { get { return (DriveQueryType)Enum.Parse(typeof(DriveQueryType), query); } }
    }
}
