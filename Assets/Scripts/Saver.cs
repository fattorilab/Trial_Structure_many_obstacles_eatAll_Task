using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System;
using PupilLabs;

public class Saver : MonoBehaviour
{

    // Recording and saving
    public long starttime = 0;
    private long time = 0;
    public bool RECORD = false;
    public int MovieWidth = 1280;
    public int MovieHeight = 720;
    public string movieFilePath;
    public bool Want2Save = false;
    public string csvFilePath;


    // Experiment objects
    Forest forest;
    Ardu controller;
    //EyeTrackingSaver eyeTrackingSaver;
    GameObject player;
    public Camera mainCam;
    public Camera leftCam;
    public Camera rightCam;
    GameObject DB;
    public int lastIDFromDB;

    public string hit_info = "None"; //For Debugging

    public GameObject PupilData;
    PupilDataStream pupilDataStream;

    RecorderControllerSettings video_settings;
    MovieRecorderSettings m_SettingsCamera1;
    private RecorderController mainRecorderController;

    // TEST
    private RecorderController recorderController;


    void Start()
    {

        //if (Application.systemLanguage == SystemLanguage.German)
        //{
        //    filePath = "C:/Users/g_brem02/sciebo/Promotion/2xMonkey/data/";
        //}
        //else
        //{   //------------------------------------------------------------------ mod path marrti 29/11/23
        //    //PC ALIEN 
        //    //filePath = "/Users/fatto/Documents/Registrazioni VR/MEF27/DATI/"; 
        //    //PC_TELECAMERE -----------------------------------------------------------------------------
        //    // filePath = "C:/Users/admin/Desktop/Registrazioni_VR/MEF27/DATI/"; 
        //    filePath = "C:/Users/edoar/Desktop/Fattori_lab/test";
        //}

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

        if (RECORD)
        {
            Debug.Log("Movie files will be saved in " + movieFilePath);
        }

        if (Want2Save)
        {
            Debug.Log("CSV files will be saved in " + csvFilePath);
        }

        // Fecth experiment 
        forest = GetComponent<Forest>();
        controller = GetComponent<Ardu>();
        pupilDataStream = PupilData.GetComponent<PupilDataStream>();

        // Fetch database
        DB = GameObject.Find("DB");
        lastIDFromDB = DB.GetComponent<InteractWithDB>().GetLastIDfromDB();

        // Fetch player
        //target = GameObject.Find("Target");
        player = GameObject.Find("Player");

        // Manage time
        //starttime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        starttime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000000;
        addObject("Seed", forest.seed, forest.seed, forest.seed, "Seed");

        // Start the recording
        #if UNITY_EDITOR
        
                if (RECORD)
                {
                    StartRecording_all();
                }

        #endif
    }

    bool got_start = false;
    void LateUpdate()
    {
        time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - starttime;
        writeData();

        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }

        if (!got_start)
        {
            if (forest.exp_has_started)
            {
                starttime = forest.start_ms;
                got_start = true;
            }
        }
    }

    private void OnApplicationQuit()
    {
        saveAllData(";");
    }

    List<List<string>> PerFrameData = new List<List<string>>();
    List<List<string>> OneTimeData = new List<List<string>>();

    float[] EyeInfo = new float[6];

    private void writeData() //add data per frame
    {

        PerFrameData.Add(new List<string>()); //Adds new sub List
                                              //PerFrameData[(PerFrameData.Count - 1)].Add((milliseconds - starttime).ToString());

        PerFrameData[(PerFrameData.Count - 1)].Add((time).ToString());

        PerFrameData[(PerFrameData.Count - 1)].Add((forest.correct_trials).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.trial).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.phase).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.row_close_active).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.row_middle_active).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.row_far_active).ToString());
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.correct_target_name));
        PerFrameData[(PerFrameData.Count - 1)].Add((forest.interval).ToString("F7"));


        PerFrameData[(PerFrameData.Count - 1)].Add((controller.ax1).ToString("F7"));
        PerFrameData[(PerFrameData.Count - 1)].Add((controller.ax2).ToString("F7"));
        PerFrameData[(PerFrameData.Count - 1)].Add((player.transform.position.x).ToString("F7"));
        PerFrameData[(PerFrameData.Count - 1)].Add((player.transform.position.z).ToString("F7"));
        PerFrameData[(PerFrameData.Count - 1)].Add((player.transform.eulerAngles.y).ToString("F7"));


        PerFrameData[(PerFrameData.Count - 1)].Add((controller.reward_counter).ToString("F5"));

        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_L.x).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_L.y).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_L.z).ToString("F5"));

        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_R.x).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_R.y).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Vector_R.z).ToString("F5"));

        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Theta_L).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Theta_R).ToString("F5"));

        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Phi_L).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.Phi_R).ToString("F5"));


        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.DiameterLeft).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.DiameterRight).ToString("F5"));


        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.confidence_L).ToString("F5"));
        PerFrameData[(PerFrameData.Count - 1)].Add((pupilDataStream.confidence_R).ToString("F5"));
    }


    public void addObject(string identifier, float x_pos, float z_pos, float orientation, string type)
    {
        //go.GetInstanceID() int to str
        //long time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - starttime;
        OneTimeData.Add(new List<string>()); //Adds new sub List
        OneTimeData[(OneTimeData.Count - 1)].Add(identifier); //.ToString()
        OneTimeData[(OneTimeData.Count - 1)].Add((x_pos).ToString("F5"));
        OneTimeData[(OneTimeData.Count - 1)].Add((z_pos).ToString("F5"));
        OneTimeData[(OneTimeData.Count - 1)].Add((orientation).ToString("F5"));
        OneTimeData[(OneTimeData.Count - 1)].Add(type);
        OneTimeData[(OneTimeData.Count - 1)].Add((time).ToString());
        OneTimeData[(OneTimeData.Count - 1)].Add("-1"); //time of object end is initialized -1
    }


    public void addObjectEnd(string identifier) //add the time of object end
    {
        //Debug.Log("Trying to remove " + identifier);
        // Pi� veloce se si passa l'elenco di quelli da eliminare e poi si scorre sempre l'elenco e si elimina quando viene trovato.
        bool found = false;
        for (int i = 0; i < OneTimeData.Count; i++)
        {
            if (OneTimeData[i][0] == identifier)
            {
                //Debug.Log("Found Object at " + i);
                OneTimeData[i][6] = (time).ToString();
                found = true;
            }

        }

        if (!found)
        {
            Debug.Log("Couldn't find object with ID " + identifier);
        }

    }

    private void saveAllData(string delimiter)
    {
        string Line = "";
        StringBuilder sb_PerFrame = new StringBuilder();
        StringBuilder sb_OneTime = new StringBuilder();

        //Create Data writer
        sb_PerFrame.AppendLine("Time; trial; trial_with_repeats; phase; close_active; middle_active; far_active; correct_target; interval; " +
            "arduino_x; arduino_y; player_x; player_z; player_orientation; reward_counter" +
            "eye_vec_lx; eye_vec_ly; eye_vec_lz; eye_vec_rx; eye_vec_ry; eye_vec_rz; eye_theta_l; eye_theta_r; eye_phi_l; eye_phi_r;" +
            "eye_diameter_l; eye_diameter_r; eye_confidence_l; eye_confidence_r");

        for (int index = 0; index < PerFrameData.Count; index++)
        {
            Line = "";

            for (int counteri = 0; counteri < PerFrameData[index].Count; counteri++)
            {
                Line += PerFrameData[index][counteri];   //Costruzione delle righe
                if (counteri != (PerFrameData[index].Count - 1)) { Line += delimiter; }
            }
            sb_PerFrame.AppendLine(Line);
        }

        /*
        if (PerFrameData.Count > 100)
        {
            float fps = (PerFrameData.Count - 50) / ((int.Parse(PerFrameData[PerFrameData.Count - 20][0]) - int.Parse(PerFrameData[30][0])) / 1000);
            Debug.Log("Saving Frequency: " + fps);
        }
        */



        //Create Supplement writer
        sb_OneTime.AppendLine("Name; x; y; orientation; info; TimeEntry; TimeExit"); //Id x y type 

        for (int index = 0; index < OneTimeData.Count; index++)
        {
            Line = "";

            for (int counteri = 0; counteri < OneTimeData[index].Count; counteri++)
            {
                Line += OneTimeData[index][counteri];  //Costruzione delle righe
                if (counteri != (OneTimeData[index].Count - 1)) { Line += delimiter; }
            }
            sb_OneTime.AppendLine(Line);
        }

        if (Want2Save)
        {
            string new_Date = DateTime.Now.ToString("yyyy/MM/dd");
            string new_Task = "Trial_structure";
            string new_Param = forest.Trial_type.ToString() + ", " + forest.Target_setting.ToString();

            new_Param = new_Param + "reverse_Xaxis=" + player.GetComponent<Movement>().reverse_Xaxis.ToString(); //add param to DB line
            new_Param = new_Param + "reverse_Yaxis=" + player.GetComponent<Movement>().reverse_Yaxis.ToString();

            int lastIDFromDB = DB.GetComponent<InteractWithDB>().GetLastIDfromDB();

            int new_ID = lastIDFromDB + 1;

            DB.GetComponent<InteractWithDB>().AddRecording(new_ID, new_Date, new_Task, new_Param);

            string filePath_PerFrame = csvFilePath + "/" + DateTime.Now.ToString("yyyy_MM_dd") + "_ID" + new_ID.ToString() + "data.csv";
            string filePath_OneTime = csvFilePath + "/" + DateTime.Now.ToString("yyyy_MM_dd") + "_ID" + new_ID.ToString() + "supplement.csv";
            File.WriteAllText(filePath_PerFrame, sb_PerFrame.ToString());
            File.WriteAllText(filePath_OneTime, sb_OneTime.ToString());
            Debug.Log("Saved all CSV data");
        }
    }


    void StartRecording_all()
    {
        // Create a RecorderControllerSettings
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        if (controllerSettings == null)
        {
            Debug.LogError("Failed to create RecorderControllerSettings");
            return;
        }

        // Add the MovieRecorderSettings related to left and right cameras to the RecorderController
        controllerSettings.AddRecorderSettings(CreateRecorderSettings("MainCamera"));
        controllerSettings.AddRecorderSettings(CreateRecorderSettings("LeftCamera"));
        controllerSettings.AddRecorderSettings(CreateRecorderSettings("RightCamera"));
        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 60.0f;

        // Create a RecorderController with the RecorderControllerSettings
        recorderController = new RecorderController(controllerSettings);

        // Prepare and start
        recorderController.PrepareRecording();
        recorderController.StartRecording();
        Debug.Log("Recording started");

        MovieRecorderSettings CreateRecorderSettings(string camTag)
        {

            string fileName = DateTime.Now.ToString("yyyy_MM_dd") + "_ID" + (lastIDFromDB + 1).ToString() + $"_{camTag}";

            // Create a CameraInputSettings for the camera
            CameraInputSettings camSettings = new CameraInputSettings
            {
                Source = ImageSource.TaggedCamera,
                OutputWidth = MovieWidth,  // Set your desired output width
                OutputHeight = MovieHeight, // Set your desired output height
                CameraTag = camTag,
            };

            // Create a MovieRecorderSettings for the camera
            MovieRecorderSettings movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieRecorderSettings.Enabled = true;
            movieRecorderSettings.OutputFile = movieFilePath + "/" + fileName;
            movieRecorderSettings.ImageInputSettings = camSettings;
            movieRecorderSettings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
            movieRecorderSettings.VideoBitRateMode = VideoBitrateMode.High;

            movieRecorderSettings.AudioInputSettings.PreserveAudio = false;

            return movieRecorderSettings;
        }

    }
}