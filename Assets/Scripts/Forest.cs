using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor; test
//using UnityEditor.Recorder;
using System.Linq;
using PupilLabs;

public class Forest : MonoBehaviour
{
    Vector3[] fixed_positions = { new Vector3(-2f, 0.5f, 9f), new Vector3(0f, 0.5f, 9.5f), new Vector3(2f, 0.5f, 9f),  //L_close, C_close, R_close
                                new Vector3(-4f, 0.5f, 12f), new Vector3(0f, 0.5f, 13f),  new Vector3(4f, 0.5f, 12f),  //L_mid,   C_mid,   R_mid
                                new Vector3(-6f, 0.5f, 15f), new Vector3(0f, 0.5f, 16.5f), new Vector3(6f, 0.5f, 15f)};//L_far,   C_far,   R_far
    string[] fixed_names = {"Fruit_left_close", "Fruit_center_close", "Fruit_right_close",
                            "Fruit_left_middle", "Fruit_center_middle", "Fruit_right_middle",
                            "Fruit_left_far", "Fruit_center_far", "Fruit_right_far"};
    float[] fixed_orientations = {160, 0, -160, 0, 0, 0, 0, 0, 0};

    GameObject experiment;
    GameObject environment;

    public int seed;

    public myEnum2 Trial_type = new myEnum2();
    public enum myEnum2
    {
        GivenTarget,
        FreeChoice,
        ManyObstacles,
        ManyObstacles_EatThemAll
    };
    public int TargetsInObstacleTask;

    public myEnum Target_setting = new myEnum();
    public enum myEnum
    {
        RandomThree,
        MiddleThree,
        Six,
        All,
        Custom
    };

    public float REACTION_TIME;
    public bool abortTrial = true; //mod by marrti
    public bool RESET_NOT_JUST_BLOCKED;
    public bool AVOID_SAME_TARGET_TWICE;
    //used by fruit??
    public int reward_length;
    public float whitescreenseconds;
    [HideInInspector] public int minimumRewardTime; //delay before juicy ??unnecessary??
    public float minimumDistance; //to get juicy

    public int nullpositionTime; //for releasing when juicy
                         
    public GameObject Player;
    // public bool RECORD;
    public bool RECORD_EYES;
    public int maximum_trial_time;
    public bool RESET_IF_FULL_TURN;
    public bool RESET_AT_TARGET_NOT_GIVEN;

    GameObject[] targets;
    GameObject correctTarget;

    Camera camL;
    Camera camR;
    Camera camM;

    public GameObject Fruit_Abstract;

    float timer = 0f;
    float time_to_end = 0f;

    [HideInInspector] public int trial = 0;
    [HideInInspector] public int correct_trials = 0;
    [HideInInspector] public int phase = 0;
    [HideInInspector] public float interval = 0;
    [HideInInspector] public bool row_close_active = false;
    [HideInInspector] public bool row_middle_active = false;
    [HideInInspector] public bool row_far_active = false;
    [HideInInspector] public string correct_target_name = "Unknown";
    bool movement_prohibition;
    bool dont_change_correct_target = false;

    [HideInInspector] public long start_ms; // zero della registrazione in ms
    [HideInInspector] private int frame_number = 0; // counter del numero di frame
    [HideInInspector] public bool exp_has_started = false;

    //For Many Obstacles
    float[] mo_x_pos = {-15, -10, -5, 0, 5, 10, 15};
    float[] mo_z_pos = { 5, 10, 15, 20, 25, 30, 35};
    private int fruits_eaten = 0;
    private int fruits_to_eat = 0;
    bool manyObstacleCondition = false;

//#if UNITY_EDITOR --------------------------------------------------------------------------> Moved to SAVER
//    private RecorderWindow GetRecorderWindow()
//    {
//        return (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
//    }
//#endif

//RecorderWindow recorderWindow;
//RecorderControllerSettings video_settings;
//RecorderSettings rc_setting;

    GameObject DB;
    public GameObject PupilDataManagment;

    void Start()
    {
        Application.runInBackground = true;
        Random.InitState(seed);

        PupilDataManagment.GetComponent<RequestController>().connectOnEnable = RECORD_EYES;
        PupilDataManagment.SetActive(true);

        DB = GameObject.Find("DB");
        int lastIDFromDB = DB.GetComponent<InteractWithDB>().GetLastIDfromDB();

//#if UNITY_EDITOR --------------------------------------------------------------------------> Moved to SAVER
//        recorderWindow = GetRecorderWindow();
//        List<RecorderSettings> recorderSettingsList = RecorderControllerSettings.GetGlobalSettings().RecorderSettings.ToList();
//        rc_setting = recorderSettingsList[0];
//        //Debug.Log(rc_setting.name);

//        rc_setting.name = DateTime.Now.ToString("yyyy_MM_dd") + "_ID" + (lastIDFromDB+1).ToString();

//        video_settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
//        video_settings.AddRecorderSettings(rc_setting);
//        video_settings.SetRecordModeToManual();
//        video_settings.FrameRate = 100.0f;
//        recorderWindow.SetRecorderControllerSettings(video_settings);

//        //rc_settings = RecorderControllerSettings.GetGlobalSettings().RecorderSettings.ToList();


//        /*
//        video_settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
//        m_SettingsCamera1 = ScriptableObject.CreateInstance<MovieRecorderSettings>();
//        m_SettingsCamera1.name = "Camera 1 Recorder";
//        m_SettingsCamera1.Enabled = true;
//        m_SettingsCamera1.AudioInputSettings.PreserveAudio = false;
//        m_SettingsCamera1.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MOV;
//        video_settings.AddRecorderSettings(m_SettingsCamera1);
//        video_settings.SetRecordModeToManual();
//        video_settings.FrameRate = 60.0f;
//        recorderWindow.SetRecorderControllerSettings(video_settings);
//        */
//#endif

        experiment = GameObject.Find("Experiment");
        environment = GameObject.Find("Environment");

        /////////////////ALL//POSSIBLE//SETTINGS///////////////////////////////////////////////////////////////////////////////
        if (Trial_type == myEnum2.ManyObstacles || Trial_type == myEnum2.ManyObstacles_EatThemAll)
        {
            manyObstacleCondition = true;

            /*
            if (Target_setting == myEnum.MiddleThree || Target_setting == myEnum.RandomThree)
            {
                fruits_to_eat = 3;
                GetComponent<Saver>().addObject("Target_Setting: ThreeWithObstacles", 0, 0, 0, "Setting");
            } else if (Target_setting == myEnum.Custom)
            {
                fruits_to_eat = 2;
                GetComponent<Saver>().addObject("Target_Setting: TwoWithObstacles", 0, 0, 0, "Setting");
            } else
            {
                fruits_to_eat = 1;
                GetComponent<Saver>().addObject("Target_Setting: OneWithObstacles", 0, 0, 0, "Setting");
            }*/
            fruits_to_eat = TargetsInObstacleTask;
            
            targets = new GameObject[fruits_to_eat];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, new Vector3(0,0.5f,10), Quaternion.Euler(0, 0, 0), environment.transform);
                targets[i].name = "Fruit" + i.ToString();
                if (Trial_type == myEnum2.ManyObstacles_EatThemAll)
                {
                    targets[i].GetComponent<Fruit>().multiple_fruit_mode = true;
                }
            }

            row_close_active = false;
            row_middle_active = false;
            row_far_active = false;

            
        }
        else if (Target_setting == myEnum.Custom)
        {
            Vector3[]balls_from_csv = experiment.GetComponent<ReadFile>().balls_from_csv;
            Debug.Log(balls_from_csv[0]);
            targets = new GameObject[balls_from_csv.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, balls_from_csv[i], Quaternion.Euler(0, 0, 0), environment.transform);
                targets[i].name = "Fruit" + i.ToString();
                GetComponent<Saver>().addObject(targets[i].name, balls_from_csv[i].x, balls_from_csv[i].z, 0, "Position");
            }

            GetComponent<Saver>().addObject("Target_Setting: Custom", 0, 0, 0, "Setting");

        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        else if (Target_setting == myEnum.All)
        {
            targets = new GameObject[9];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, fixed_positions[i], Quaternion.Euler(0, fixed_orientations[i], 0), environment.transform);
                targets[i].name = fixed_names[i];
                GetComponent<Saver>().addObject(targets[i].name, fixed_positions[i].x, fixed_positions[i].z, fixed_orientations[i], "Position");
            }

            row_close_active = true;
            row_middle_active = true;
            row_far_active = true;

            GetComponent<Saver>().addObject("Target_Setting: All", 0, 0, 0, "Setting");
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        else if (Target_setting == myEnum.Six)
        {
            targets = new GameObject[6];
            for (int i = 0; i < 3; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, fixed_positions[i], Quaternion.Euler(0, fixed_orientations[i], 0), environment.transform);
                targets[i].name = fixed_names[i];
                GetComponent<Saver>().addObject(targets[i].name, fixed_positions[i].x, fixed_positions[i].z, fixed_orientations[i], "Position");
            }
            for (int i = 3; i < 6; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, fixed_positions[i+3], Quaternion.Euler(0, fixed_orientations[i+3], 0), environment.transform);
                targets[i].name = fixed_names[i+3];
                GetComponent<Saver>().addObject(targets[i].name, fixed_positions[i+3].x, fixed_positions[i+3].z, fixed_orientations[i+3], "Position");
            }

            row_close_active = true;
            row_middle_active = false;
            row_far_active = true;

            GetComponent<Saver>().addObject("Target_Setting: Six", 0, 0, 0, "Setting");
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        else if (Target_setting == myEnum.MiddleThree)
        {
            targets = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, fixed_positions[i + 3], Quaternion.Euler(0, fixed_orientations[i + 3], 0), environment.transform);
                targets[i].name = fixed_names[i + 3];
                GetComponent<Saver>().addObject(targets[i].name, fixed_positions[i + 3].x, fixed_positions[i + 3].z, fixed_orientations[i + 3], "Position");
            }

            row_close_active = false;
            row_middle_active = true;
            row_far_active = false;

            GetComponent<Saver>().addObject("Target_Setting: MiddleThree", 0, 0, 0, "Setting");
        }  
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        else if (Target_setting == myEnum.RandomThree)
        {
            targets = new GameObject[9];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = Instantiate(Fruit_Abstract, fixed_positions[i], Quaternion.Euler(0, fixed_orientations[i], 0), environment.transform);
                targets[i].name = fixed_names[i];
                GetComponent<Saver>().addObject(targets[i].name, fixed_positions[i].x, fixed_positions[i].z, fixed_orientations[i], "Position");
            }

            GetComponent<Saver>().addObject("Target_Setting: RandomThree", 0, 0, 0, "Setting");
        }


        //Save the Settings
        if (Trial_type == myEnum2.GivenTarget)
        {
            GetComponent<Saver>().addObject("Trial_Type: Given Target", 0, 0, 0, "Setting");
        } else if (Trial_type == myEnum2.FreeChoice)
        {
            GetComponent<Saver>().addObject("Trial_Type: Free Choice", 0, 0, 0, "Setting");
        } else if (Trial_type == myEnum2.ManyObstacles)
        {
            GetComponent<Saver>().addObject("Trial_Type: Many Obstacles", 0, 0, 0, "Setting");
        }
        else if (Trial_type == myEnum2.ManyObstacles_EatThemAll)
        {
            GetComponent<Saver>().addObject("Trial_Type: Many Obstacles eat them all", 0, 0, 0, "Setting");
        }


        GetComponent<Saver>().addObject("Reward Length", reward_length, 0, 0, "Setting");
        GetComponent<Saver>().addObject("Whitescreen [s]", whitescreenseconds, 0, 0, "Setting");
        GetComponent<Saver>().addObject("Reaction Time", REACTION_TIME, 0, 0, "Setting");

        GetComponent<Saver>().addObject("Abort Trial", System.Convert.ToSingle(abortTrial), 0, 0, "Setting");
        GetComponent<Saver>().addObject("reset not just block", System.Convert.ToSingle(RESET_NOT_JUST_BLOCKED), 0, 0, "Setting");
        GetComponent<Saver>().addObject("avoid the same target twice", System.Convert.ToSingle(AVOID_SAME_TARGET_TWICE), 0, 0, "Setting");
        
        GetComponent<Saver>().addObject("minimal distance to get juicy", minimumDistance, 0, 0, "Setting");
        GetComponent<Saver>().addObject("nullpositionTime", nullpositionTime, 0, 0, "Setting");

        GetComponent<Saver>().addObject("maximal trial time", maximum_trial_time, 0, 0, "Setting");
        GetComponent<Saver>().addObject("reset if full turn", System.Convert.ToSingle(RESET_IF_FULL_TURN), 0, 0, "Setting");
        GetComponent<Saver>().addObject("reset if uncued ball is reached", System.Convert.ToSingle(RESET_AT_TARGET_NOT_GIVEN), 0, 0, "Setting");


        camL = GameObject.Find("LeftCam").GetComponent<Camera>();
        camR = GameObject.Find("RightCam").GetComponent<Camera>();
        camM = GameObject.Find("Main Camera").GetComponent<Camera>();

        reset();
    }

    
    void Update()
    {
        frame_number++;

        if (frame_number == 10) //Unity needs some frames to start, please keep this at 10, Gianni
        {
            Camera.main.backgroundColor = Color.black;
            Debug.Log("START");


//#if UNITY_EDITOR --------------------------------------------------------------------------> Moved to SAVER
//            if (!recorderWindow.IsRecording() && RECORD)
//                recorderWindow.StartRecording();
//            EditorApplication.ExecuteMenuItem("Window/General/Console");
//#endif

            GetComponent<TriggerBox>().SendToTriggerBox(1); //So it's getting saved in Supplement
            // TRIGGER START REGISTRAZIONE
            experiment.GetComponent<Ardu>().SendStartRecordingOE();
            start_ms = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            exp_has_started = true;
        }

        // Press Q to quit and abort the current trial
        //mod marrti---------------------------------
        if (abortTrial == true)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                reset();
            }
        }
        //--------------------------------------------

        timer += Time.deltaTime;
        if (phase == 103) //immediate reset
        {
            time_to_end = timer;
            dont_change_correct_target = true;
            trial += 1;
            reset();
        }
        if (timer > time_to_end + 0.5f && phase == 102) //wrong fruit end
        {
            trial += 1;
            reset();
        }
        if (timer > time_to_end + 0.5f + whitescreenseconds && phase == 101) //end whitescreen
        {
            environment.transform.position += new Vector3(0, 1000f, 0);
            camL.clearFlags = CameraClearFlags.Skybox;
            camR.clearFlags = CameraClearFlags.Skybox;
            camM.clearFlags = CameraClearFlags.Skybox;

            if (!dont_change_correct_target) { correct_trials += 1; }
            trial += 1;
            reset();
        }
        if (timer > time_to_end + 0.5f && phase == 100)
        {
            environment.transform.position += new Vector3(0, -1000f, 0);
            camL.clearFlags = CameraClearFlags.SolidColor;
            camR.clearFlags = CameraClearFlags.SolidColor;
            camM.clearFlags = CameraClearFlags.SolidColor;

            camL.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            camR.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            camM.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            phase = 101;
        } else if (phase == 99) //corect target
        {
            time_to_end = timer;
            dont_change_correct_target = false;
            phase = 100;
        }
        else if (phase == 98) //wrong target
        {
            time_to_end = timer;
            dont_change_correct_target = true;
            phase = 102;
        }
        else if (timer > (1 + interval + REACTION_TIME) && phase == 3 && Player.transform.position == new Vector3(0, 0.25f, 0)) //REACTION PHASE
        {
            dont_change_correct_target = true;
            reset();
        }
        else if (timer > (1+ interval) && phase == 2) //Fruits turn red
        {
            GetComponent<TriggerBox>().SendToTriggerBox(6);
            if (Trial_type == myEnum2.GivenTarget)
            {
                if (RESET_AT_TARGET_NOT_GIVEN)
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        targets[i].GetComponent<Fruit>().obviously_wrong_but_possible = true;
                    }
                }
                correctTarget.GetComponent<Fruit>().obviously_wrong_but_possible = false;
                correctTarget.GetComponent<Fruit>().chosen = false;
            }
            else if (Trial_type == myEnum2.FreeChoice)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].GetComponent<Fruit>().chosen = false;
                    targets[i].GetComponent<Fruit>().fake_prejuicy = true;
                }
            }

            correctTarget.GetComponent<Fruit>().prejuicy = true;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            movement_prohibition = false;
            phase = 3;
            //experiment.GetComponent<Ardu>().SendStopReferenceMarkerOE();

        } else if (timer > 1 && phase == 1) //Fruits turn green
        {
            GetComponent<TriggerBox>().SendToTriggerBox(5);
            if (Trial_type == myEnum2.GivenTarget)
            {
                correctTarget.GetComponent<Fruit>().chosen = true;
            } else if (Trial_type == myEnum2.FreeChoice)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].GetComponent<Fruit>().chosen = true;
                }
            }


            phase = 2;
            //experiment.GetComponent<Ardu>().SendStartReferenceMarkerOE();

        } else if (timer > 0.5 && phase == 0) //Fruits appear
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GetComponents<MeshRenderer>()[0].enabled = true;
            }

            phase = 1;
        } else if (phase == 0 && manyObstacleCondition)
        {
            GetComponent<TriggerBox>().SendToTriggerBox(6);
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GetComponents<MeshRenderer>()[0].enabled = true;

                if (Trial_type == myEnum2.ManyObstacles)
                {
                    targets[i].GetComponent<Fruit>().classic_look = true;
                    targets[i].GetComponent<Fruit>().prejuicy = true;
                }
            }
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            movement_prohibition = false;
            phase = 10;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Make a grid and randomly place the balls

            if (!dont_change_correct_target)
            {
                environment.GetComponent<Rocks>().deleteRocks();

                int rnd = Random.Range(0, mo_x_pos.Length);
                Shuffle(mo_x_pos);
                Shuffle(mo_z_pos);
                for (int i = 0; i < targets.Length; i++)
                {
                    
                    targets[i].GetComponent<Fruit>().classic_look = true;
                    targets[i].GetComponent<Fruit>().prejuicy = true;

                    if (mo_z_pos[i] > 15) //artificially make it less likely to just be there
                    {
                        targets[i].transform.position = new Vector3(mo_x_pos[i], 0.5f, mo_z_pos[i]);
                        GetComponent<Saver>().addObject(targets[i].name, targets[i].transform.position.x, targets[i].transform.position.z, 0, "Position");
                    } else
                    {
                        targets[i].transform.position = new Vector3(mo_x_pos[i], 0.5f, mo_z_pos[mo_z_pos.Length - 1]);
                        GetComponent<Saver>().addObject(targets[i].name, targets[i].transform.position.x, targets[i].transform.position.z, 0, "Position");
                    }
                    
                }

                for (int i = targets.Length; i < mo_x_pos.Length; i++)
                {
                    environment.GetComponent<Rocks>().MakeRock(mo_x_pos[i], mo_z_pos[i]);
                }

                for (int i = 0; i < mo_x_pos.Length - 1; i++)
                {
                    environment.GetComponent<Rocks>().MakeRock(mo_x_pos[i], mo_z_pos[i + 1]);
                    environment.GetComponent<Rocks>().MakeRock(mo_x_pos[i + 1], mo_z_pos[i]);
                }

                environment.GetComponent<Rocks>().saveObjects();
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        }

        //too early
        if (movement_prohibition && Player.GetComponent<Movement>().keypressed && RESET_NOT_JUST_BLOCKED)
        {
            dont_change_correct_target = true;
            reset();
        }


        if (phase < 50 && !Player.GetComponent<Movement>().is_eating)
        {
            if (RESET_IF_FULL_TURN && ((Player.transform.eulerAngles.y > 175 && Player.transform.eulerAngles.y < 185 )|| (Player.transform.eulerAngles.y < -175 && Player.transform.eulerAngles.y > -185)) && !manyObstacleCondition)
            {
                Debug.Log("Full_Turn");
                dont_change_correct_target = true;
                reset();
            }

            if (timer > maximum_trial_time)
            {
                Debug.Log("Maximum Trial Time was reached");
                dont_change_correct_target = true;
                reset();
            }
        }
        
        
    }

    //public void giveManualReward()
    //{
    //    if (Input.GetKeyDown("space")) //(Input.GetKeyDown(KeyCode.Space))
    //    {
    //        ardu.SendReward(reward_length);
    //        Debug.Log("************** This is an EXTRA REWARD *************");
    //    }
    //}

    void OnApplicationQuit() // se l'app si interrompe (es. pause dell'editor) senza  che venga premuto 'esc'
    {
        experiment.GetComponent<Ardu>().SendStopRecordingOE();
        Debug.Log("END");
    }

    void reset()
    {
        timer = 0f;
        phase = 0;
        Player.transform.position = new Vector3(0,0.25f,0);
        Player.transform.eulerAngles = new Vector3(0, 0, 0);
        Player.GetComponent<Movement>().restrict_backwards = 0;
        Player.GetComponent<Movement>().restrict_forwards = 0;
        Player.GetComponent<Movement>().restrict_horizontal = 0;
        Player.GetComponent<Movement>().is_eating = false;
        Player.GetComponent<Movement>().fix_collision_mod();
        movement_prohibition = true;
        //MeshRenderer[] ml = ObstacleL.GetComponents<MeshRenderer>();
        //ml[0].enabled = false;

        if (!dont_change_correct_target || Trial_type != myEnum2.ManyObstacles_EatThemAll)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GetComponents<MeshRenderer>()[0].enabled = false;
                targets[i].GetComponent<Fruit>().eaten = false;
                targets[i].GetComponent<Fruit>().chosen = false;
                targets[i].GetComponent<Fruit>().prejuicy = false;
                targets[i].GetComponent<Fruit>().fake_prejuicy = false;
            }
        }

        interval = Random.Range(0.5f, 2.5f);

        set_position();
    }


    
    void set_position()
    {
        if (!dont_change_correct_target && !manyObstacleCondition) // || Trial_type == myEnum2.GivenTarget
        {
            //If random three, get new pos
            if (Target_setting == myEnum.RandomThree)
            {
                float placing = Random.Range(0f, 3f);
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].SetActive(false);

                }
                if (placing < 1)
                {
                    row_close_active = true;
                    row_middle_active = false;
                    row_far_active = false;
                    targets[0].SetActive(true);
                    targets[1].SetActive(true);
                    targets[2].SetActive(true);

                }
                else if (placing > 2)
                {
                    row_close_active = false;
                    row_middle_active = false;
                    row_far_active = true;
                    targets[6].SetActive(true);
                    targets[7].SetActive(true);
                    targets[8].SetActive(true);
                }
                else
                {
                    row_close_active = false;
                    row_middle_active = true;
                    row_far_active = false;
                    targets[3].SetActive(true);
                    targets[4].SetActive(true);
                    targets[5].SetActive(true);
                }
            }

            List<GameObject> activeTargets = new List<GameObject>();
            foreach (GameObject target in targets)
            {
                if (target.activeSelf)
                {
                    if (target != correctTarget || !AVOID_SAME_TARGET_TWICE)
                    {
                        activeTargets.Add(target);
                    }
                }
            }

            // Choose a random active target
            int randomIndex = Random.Range(0, activeTargets.Count);
            correctTarget = activeTargets[randomIndex];

            correct_target_name = correctTarget.name;
            Debug.Log("The correct target is: " + correct_target_name);
        }
        


    }

    private System.Random _random = new System.Random();

    void Shuffle(float[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = _random.Next(0, n);
            float t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }

    public void fruit_eaten_notification()
    {
        fruits_eaten += 1;
        if (fruits_eaten == fruits_to_eat)
        {
            fruits_eaten = 0;
            phase = 99;
        }
    }
}