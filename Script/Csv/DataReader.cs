using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using UnityEngine;

namespace SupportPackage.Csv
{
    public class DataReader<T> where T : IData
    {
        private static class Constants
        {
            public const int MinimumLineLengthOfCsv = 2;
        }

        private string csvPath;
        private SortedList<int, T> datas;

        public DataReader()
        {
            this.csvPath = "Csv/Data/" + typeof(T).Name;
        }

        public DataReader(string csvPath)
        {
            this.csvPath = RemoveUnnecessaryCsvPathStrings(csvPath);
        }

        private string RemoveUnnecessaryCsvPathStrings(string path)
        {
            string csvPath = path;

            csvPath = csvPath.Replace("Assets/Resources/", "");
            csvPath = csvPath.Replace(".csv", "");

            return csvPath;
        }

        public void Read()
        {
            string csvPath = this.csvPath;
            TextAsset data = Resources.Load<TextAsset>(csvPath);

            const string LineSplitChars = @"\r\n|\n\r|\n|\r";
            string[] csvRows = Regex.Split(data.text, LineSplitChars);
            if (csvRows.Length <= Constants.MinimumLineLengthOfCsv)
            {
                Debug.LogError("The number of csv rows is less than the minimum number.");
                return;
            }

            ConvertLowsToDatas(csvRows);
        }

        private void ConvertLowsToDatas(string[] lines)
        {
            const string SplitChars = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
            char[] trimChars = { '\"' };

            if (this.datas == null)
            {
                this.datas = new SortedList<int, T>();
            }

            string[] header = Regex.Split(lines[0], SplitChars);
            for (var i = Constants.MinimumLineLengthOfCsv; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SplitChars);

                if (values.Length == 0 || values[0] == "")
                {
                    continue;
                }

                int infoID = Int32.Parse(values[0]);

                object entry = Activator.CreateInstance(typeof(T));
                var propertyValues = entry.GetType().GetProperties();
                for (var j = 0; j < header.Length && j < values.Length && j < propertyValues.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(trimChars).TrimEnd(trimChars).Replace("\\", "");
                    value = value.Replace("<br>", "\n");
                    value = value.Replace("<c>", ",");

                    object convertValue = null;
                    if (propertyValues[j].PropertyType.IsGenericType == true && propertyValues[j].PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = GetCollectionItemType(propertyValues[j].PropertyType);

                        if (itemType == typeof(int))
                        {
                            convertValue = GetListFromCsv<int>(value);
                        }
                        else if (itemType == typeof(float))
                        {
                            convertValue = GetListFromCsv<float>(value);
                        }
                    }
                    else
                    {
                        convertValue = Convert.ChangeType(value, propertyValues[j].PropertyType);
                    }

                    propertyValues[j].SetValue(entry, convertValue);
                }

                this.datas.Add(infoID, (T)Convert.ChangeType(entry, typeof(T)));
            }
        }

        public Type GetCollectionItemType(Type collectionType)
        {
            var intIndexer = collectionType.GetMethod("get_Item", new[] { typeof(int) });

            return intIndexer?.ReturnType ?? null;
        }

        private List<T> GetListFromCsv<T>(string commaSplited)
        {
            return (from e in commaSplited.Trim('[', ']').Split(',') select (T)Convert.ChangeType(e.Trim(), typeof(T))).ToList();
        }

        public void StoreDatasInStorage()
        {
            DataStorage<T>.Instance.ChangeDatas(this.datas);
        }
    }
}