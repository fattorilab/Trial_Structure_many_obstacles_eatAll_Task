using System.Data;
using UnityEngine;
using IDbCommand = System.Data.IDbCommand;
using IDbConnection = System.Data.IDbConnection;
using Mono.Data.Sqlite;
using System;

public class InteractWithDB: MonoBehaviour
{
    // -------------------------------------------------------------------------------------- mod marrti  29/11/23
    //public string DB_filepath = "C:/Users/Francesco/Desktop/esperimentiVR.db";
    public string DB_filepath = "C:/Users/admin/Desktop/Registrazioni_VR/esperimentiVR.db"; //--------------------
    string GiannisPath = "C:/Users/g_brem02/sciebo/Promotion/2xMonkey/esperimentiVR.db";
    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader dbreader;
    //string new_Date = DateTime.Now.ToString("yyyy/MM/dd");
    //string new_Task = "";
    //string new_Param = "";
    //int lastID;
    //int new_ID;

    string DBfilepath_applied;
    void Start()
    {
        if (Application.systemLanguage == SystemLanguage.German)
        {
            Debug.Log("Gianni here");
            DBfilepath_applied = GiannisPath;
        } else
        {
            DBfilepath_applied = DB_filepath;
        }
    }

    public int GetLastIDfromDB()
    {
        int lastID = -1; // Initialize lastID with a default value in case no records are found

        Debug.Log("Connecting to DB " + $"DB_filepath={DBfilepath_applied} for reading last ID");
        conn = "URI=file:" + DBfilepath_applied;

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();

            dbcmd = dbconn.CreateCommand();
            sqlQuery = "SELECT * FROM Recordings ORDER BY ID DESC LIMIT 1;";
            dbcmd.CommandText = sqlQuery;

            dbreader = dbcmd.ExecuteReader();

            // Check if there's a record in the result
            if (dbreader.Read())
            {
                // Get the value of "ID" from the current record
                lastID = dbreader.GetInt32(dbreader.GetOrdinal("ID"));
            }

            dbreader.Close();
            dbconn.Close();

            // Return the lastID value
            Debug.Log("Last ID from DB " + lastID);
            return lastID;
        }
    }


    public void AddRecording(int new_ID, string new_Date, string new_Task, string new_Param)
    {
        Debug.Log("Connecting to DB " + $"DB_filepath={DBfilepath_applied} to add new recording");
        conn = "URI=file:" + DBfilepath_applied;

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();

            dbcmd = dbconn.CreateCommand();
            sqlQuery = "INSERT INTO Recordings (ID, Date, Task, Param) VALUES ('" + new_ID + "','" +  new_Date + "','" + new_Task + "','" + new_Param + "')";
            dbcmd.CommandText = sqlQuery;

            dbcmd.ExecuteNonQuery();

            dbconn.Close();

            Debug.Log("New rec. added to DB with ID " + new_ID);
        }
    }
}