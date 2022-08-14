using System.Collections.Generic;

namespace SupportPackage.Csv
{
    public class TextStorage
    {
        private static TextStorage instance;
        public static TextStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextStorage();
                }
                return instance;
            }
        }

        /// <summary>
        /// Dictionary <csv name, Dictionary<info id, List<country index>> texts;
        /// </summary>
        private Dictionary<string, Dictionary<int, List<string>>> texts;

        public void ChangeDatas(string csvName, Dictionary<int, List<string>> textDictionaries)
        {
            if (this.texts == null)
            {
                this.texts = new Dictionary<string, Dictionary<int, List<string>>>();
            }

            this.texts[csvName] = textDictionaries;
        }

        public string GetText(string csvName, int infoID, int countryIndex)
        {
            if (this.texts == null)
            {
                ReadAndStoreDatas(csvName);
            }

            if (!this.texts.ContainsKey(csvName) || !this.texts[csvName].ContainsKey(infoID) || this.texts[csvName][infoID].Count <= countryIndex)
            {
                Debug.Log("The text does not exist.");
                return "";
            }

            return this.texts[csvName][infoID][countryIndex];
        }

        private void ReadAndStoreDatas(string csvName)
        {
            TextReader reader = new TextReader(csvName);

            reader.Read();
            reader.StoreDatasInStorage();
        }
    }

}