using System;
using System.Collections.Generic;

namespace SupportPackage.Csv
{
    public class DataStorage<T> where T : IData
    {
        private static DataStorage<T> instance;
        public static DataStorage<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataStorage<T>();
                }
                return instance;
            }
        }

        private SortedList<int, T> datas;

        public void ChangeDatas(SortedList<int, T> datas)
        {
            if (this.datas == null)
            {
                this.datas = new SortedList<int, T>();
            }

            this.datas = datas;
        }

        public T GetDataByInfoID(int infoID)
        {
            if (this.datas == null)
            {
                ReadAndStoreDatas();
            }

            if (this.datas.ContainsKey(infoID) == false)
            {
                Debug.Log("There is no value matching the infoId in the datas.");
                return default(T);
            }

            return this.datas[infoID];
        }

        public T GetDataByIndex(int index)
        {
            if (this.datas == null)
            {
                ReadAndStoreDatas();
            }

            if (this.datas.Count <= index)
            {
                Debug.LogError("The index is larger than the number of data.");
                return default(T);
            }

            return this.datas.Values[index];
        }

        private void ReadAndStoreDatas()
        {
            // TODO : Csv Class 네임스페이스/저장위치 고민
            string className = "SupportPackage.Csv.Class." + typeof(T).Name;
            Type[] typeArgs = { Type.GetType(className) };
            Type genericType = typeof(DataReader<>);
            Type readerType = genericType.MakeGenericType(typeArgs);
            DataReader<T> reader = (DataReader<T>)Activator.CreateInstance(readerType);

            reader.Read();
            reader.StoreDatasInStorage();
        }
    }
}
