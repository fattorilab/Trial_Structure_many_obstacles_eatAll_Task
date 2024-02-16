using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using UnityEditor;

public class ReadMovement : MonoBehaviour
{
    public TextAsset File;
    private long startt = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
    
    int Daten_Spaltenanzahl = 20;
    static int spaltenanzahl;

    public int game_time = 0;
    public bool keypressed = false;
    /*
    public Vector2 Augen_Position = new Vector2(12, 22);
    public Vector2 Augen_Ziel = new Vector2(9, 19);
    public Vector2 Augen_ZielDistanz = new Vector2(18, 28);
    */

    public Material farbe;

    // Start is called before the first frame update
    void Start()
    {
        CultureInfo culture = new CultureInfo("en-US");
        CultureInfo.CurrentCulture = culture;

        spaltenanzahl = Daten_Spaltenanzahl;
        Load(File);
        //Debug.Log(GetAt(0).nr);
        //Debug.Log(GetAt(2).wert[7]);
        Camera.main.transform.parent = transform;
        //Camera.main.transform.localPosition = new Vector3(0, 0.4f, -0.5f);
        //Camera.main.transform.localEulerAngles = new Vector3(20, 0, 0);

    }

    int iterativ = 1;

    void Update()
    {
        long time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - startt;

        while (time > System.Int32.Parse(GetAt(iterativ).nr) && rowList.Count > iterativ + 1) {
            iterativ += 1;
        }

        game_time = int.Parse(GetAt(iterativ).wert[0]);
        //TODOTODO
        //keypressed

        transform.localPosition = new Vector3(float.Parse(GetAt(iterativ).wert[3]),
                                                1.5f,
                                                float.Parse(GetAt(iterativ).wert[4]));

        
        Camera.main.transform.localEulerAngles = new Vector3(float.Parse(GetAt(iterativ).wert[6]),
                                                    //float.Parse(GetAt(iterativ).wert[7]), 
                                                    float.Parse(GetAt(iterativ).wert[5]),
                                                    float.Parse(GetAt(iterativ).wert[8]));
        
    


    }


    float Distance = 0;
    void EyeLiner()
    {
        Vector3 StartP, EndP;
        GL.Begin(GL.LINES);
        farbe.SetPass(0);
        Color farbeColor = new Color(farbe.color.r, farbe.color.g, farbe.color.b, farbe.color.a);
        GL.Color(farbeColor);

        if (gibmir(9) != "NA")
        {
            try
            {
                Distance = float.Parse(gibmir(19));
            }
            catch // (Exception e)
            {
                Distance = 100f;
            }

            StartP = transform.localPosition + Get_Triple(12, 13, 14) + new Vector3(0f,-0.23f,0f);
            GL.Vertex3(StartP.x, StartP.y, StartP.z);
            EndP = StartP + Camera.main.transform.localRotation * Get_Triple(9, 10, 11) * Distance;
            GL.Vertex3(EndP.x, EndP.y, EndP.z);
            //Debug.DrawLine(StartP, EndP, farbeColor);
            //Handles.DrawBezier(StartP, EndP, StartP, EndP, farbeColor, null, 30);
            Handles.color = farbeColor;
            Handles.DrawSolidDisc(EndP, (EndP-StartP), 0.09f);
            Handles.DrawLine(StartP, EndP, 7f);
        }
        

        GL.End();
    }


    public void MyPostRender(Camera cam)
    {
        EyeLiner(); //Das gilt für alle Kameras die er findet
    }
    

    public void OnEnable()
    {
        // register the callback when enabling object
        Camera.onPostRender += MyPostRender;
    }

    public void OnDisable()
    {
        // remove the callback when disabling object
        Camera.onPostRender -= MyPostRender;
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
       public string[] wert = new string[spaltenanzahl];

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
 
            for (int j = 0; j < spaltenanzahl; j++)
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
