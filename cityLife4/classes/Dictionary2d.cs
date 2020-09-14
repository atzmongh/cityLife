using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public class Dictionary2d<KeyRows, KeyColumns, V>
    {
        public Dictionary2d()
        {

        }
        private Dictionary<KeyRows, Dictionary<KeyColumns, V>> dict2d = new Dictionary<KeyRows, Dictionary<KeyColumns, V>>();
        public void Add(KeyRows keyRows, KeyColumns keyColumns, V value)
        {
            if (dict2d.ContainsKey(keyRows))
            {
                Dictionary<KeyColumns, V> aRow = dict2d[keyRows];
                if (aRow.ContainsKey(keyColumns))
                {
                    throw new AppException(125, null, keyRows, keyColumns);
                }
                aRow.Add(keyColumns, value);
            }
            else
            {
                Dictionary<KeyColumns, V> aRow = new Dictionary<KeyColumns, V>();
                aRow.Add(keyColumns, value);
                dict2d.Add(keyRows, aRow);
            }
        }
        public bool containsKeys(KeyRows rowsKey, KeyColumns columnsKey)
        {
            if (dict2d.ContainsKey(rowsKey))
            {
                if (dict2d[rowsKey].ContainsKey(columnsKey))
                {
                    return true;
                }
            }
            return false;
        }
        public V this[KeyRows rowsKey, KeyColumns columnsKey]
        {
            get { return dict2d[rowsKey][columnsKey]; }
        }
        public Dictionary<KeyColumns, V> getByRowsKey(KeyRows rowsKey)
        {
            return dict2d[rowsKey];
        }
        public Dictionary<KeyRows, V> getByColumnsKey(KeyColumns columnsKey)
        {
            Dictionary<KeyRows, V> dictRowsKey = new Dictionary<KeyRows, V>();
            foreach (var dictk2 in dict2d)
            {
                if (dictk2.Value.ContainsKey(columnsKey))
                {
                    dictRowsKey.Add(dictk2.Key, dictk2.Value[columnsKey]);
                }
            }
            return dictRowsKey;
        }
        public List<KeyRows> getRowKeys()
        {
            string.Format("dd", 3);
            return dict2d.Keys.ToList();
        }
    }
}