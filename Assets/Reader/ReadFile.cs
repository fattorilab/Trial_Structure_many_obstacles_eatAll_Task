using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using UnityEditor;

public class ReadFile : MonoBehaviour
{
    int data_coulmns = 2;
    static int num_columns;
    int iterativ = 1;

    public string filename;

    public Vector3[] balls_from_csv;
    // Start is called before the first frame update
    void Awake()
    {
        CultureInfo culture = new CultureInfo("en-US");
        CultureInfo.CurrentCulture = culture;

        num_columns = data_coulmns;
        TextAsset t_csv = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/" + filename + ".csv", typeof(TextAsset));
        Load(t_csv);

        balls_from_csv = new Vector3[NumRows() - 1];

        for (int i = 1; i < NumRows(); i++)
        {
            balls_from_csv[i - 1] = new Vector3(float.Parse(GetAt(i).wert[0]), 0.5f, float.Parse(GetAt(i).wert[1]));
        }
    }



    Vector3 Get_Triple(int nummer1, int nummer2, int nummer3)
    {
        return new Vector3(float.Parse(GetAt(iterativ).wert[nummer1]),
                           float.Parse(GetAt(iterativ).wert[nummer2]),
                           float.Parse(GetAt(iterativ).wert[nummer3]));
    }

    Vector3 Get_Triple(float nummer1, float nummer2, float nummer3)
    {
        return new Vector3(float.Parse(gibmir(nummer1)),
                           float.Parse(gibmir(nummer2)),
                           float.Parse(gibmir(nummer3)));
    }

    string gibmir(float infloat)
    {
        return GetAt(iterativ).wert[Mathf.RoundToInt(infloat)];
    }

    // Der ganze CSV-Stuff
    public class Row
    {
        public string nr;
        public string[] wert = new string[num_columns];

    }

    List<Row> rowList = new List<Row>();
    bool isLoaded = false;

    public bool IsLoaded()
    {
        return isLoaded;
    }

    public List<Row> GetRowList()
    {
        return rowList;
    }

    public void Load(TextAsset csv)
    {
        rowList.Clear();
        string[][] grid = CsvParser2.Parse(csv.text);
        for (int i = 0; i < grid.Length; i++)
        {
            Row row = new Row();
            row.nr = grid[i][0];

            for (int j = 0; j < num_columns; j++)
            {
                //Debug.Log(j);
                row.wert[j] = grid[i][(j)];
            }

            rowList.Add(row);
        }
        isLoaded = true;
    }

    public int NumRows()
    {
        return rowList.Count;
    }

    public Row GetAt(int i)
    {
        if (rowList.Count <= i)
            return null;
        return rowList[i];
    }

    public Row Find_nr(string find)
    {
        return rowList.Find(x => x.nr == find);
    }
}
