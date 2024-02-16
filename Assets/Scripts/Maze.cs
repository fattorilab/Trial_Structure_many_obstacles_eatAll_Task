using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public int seed;

    public int reward_length;
    public int minimumRewardTime;
    public float minimumDistance;
    //public int maximumJuicyTime;
    public int nullpositionTime;
    public int maximumTimeIdle;
    public int eaten_targets;  

    public bool immediateReward;
    public float targetShuffleFactor;
    public bool switcheroo;

    int state = 0;
    //GameObject target;
    GameObject[] targets;
    GameObject[] obstacles;
    //GameObject greenery;

    public GameObject Player;
    public GameObject Lines;
    public GameObject Obstacle;

    //float target_distance;

    float timeIdle = 0;
    float startTimeIdle;

    float[] pos_x;
    float[] pos_z;

    void Start()
    {
        //All_targets = GameObject.Find("Target");
        targets = GameObject.FindGameObjectsWithTag("Target"); 
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle"); 
        eaten_targets =0;
        //greenery = GameObject.Find("SomeGreenery");

        Random.InitState(seed);
        //target_distance = target.transform.localPosition.z;

        pos_x = new float[targets.Length + obstacles.Length];
        pos_z = new float[targets.Length + obstacles.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            pos_x[i] = targets[i].transform.localPosition.x;
            pos_z[i] = targets[i].transform.localPosition.z;  
        }

        for (int i = 0; i < obstacles.Length; i++)
        {
            pos_x[i + targets.Length] = obstacles[i].transform.localPosition.x;
            pos_z[i + targets.Length] = obstacles[i].transform.localPosition.z;
        }


        randomize_targets();

        Player.transform.hasChanged = false;
        startTimeIdle = Time.time;
        
    }


    void Update()
    {
        //task
        if (Input.GetKey(KeyCode.Return))
        {
            GetComponent<Ardu>().SendReward(reward_length);
        }


        //idleTime
        if (!Player.transform.hasChanged && Player.transform.localPosition != Vector3.zero)
        {
            timeIdle = (Time.time - startTimeIdle) * 1000;
            if (timeIdle > maximumTimeIdle && state != 4)
            {
                state = 3;
            }
        } else
        {
            startTimeIdle = Time.time;
            Player.transform.hasChanged = false;
        }

        //too long juicy
        /*if (target.GetComponent<Fruit>().timeJuicy > maximumJuicyTime && state != 4)
        {
            state = 3;
        }*/

        
        //check target states (By Fra)
        
       
        
        if (eaten_targets == GameObject.FindGameObjectsWithTag("Target").Length)
            {
            Debug.Log("end of trial");    
            reset();
            }
     
               

        switch (state)
        {
            case 0: //red 
                break;
            case 3: //abort

                foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.transform.localPosition += new Vector3(0, 1000, 0);
                    cam.backgroundColor = new Color(255, 255, 255);
                }
                state = 4;
                break;
            case 4: //whitescreen

                foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
                {
                    cam.transform.eulerAngles = new Vector3(0, 0, 0);
                    cam.transform.localEulerAngles = new Vector3(-90, 0, 0);
                }

                if (!Player.GetComponent<Movement_classic>().keypressed)
                {
                    foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
                    {
                        cam.clearFlags = CameraClearFlags.Skybox;
                        cam.transform.eulerAngles = new Vector3(0, 0, 0);
                        cam.transform.localEulerAngles = new Vector3(0, 0, 0);
                        cam.transform.localPosition -= new Vector3(0, 1000, 0);
                        cam.backgroundColor = new Color(49, 77, 121);
                    }
                    reset();
                    
                }


                break;
        }


    }


   
    public void reset()
    {
        //if (!immediateReward && state != 4) { GetComponent<Ardu>().SendReward(reward_length); }
        //target.transform.localPosition = new Vector3(Random.Range(-target_distance * targetLateralShuffleFactor, target_distance * targetLateralShuffleFactor), 1, target_distance + Random.Range(-target_distance* targetShuffleFactor, target_distance * targetShuffleFactor));

        Player.transform.localPosition = Vector3.zero;
        Player.transform.localEulerAngles = Vector3.zero;
        state = 0;

        foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
        {
            cam.transform.eulerAngles = new Vector3(0, 0, 0);
            cam.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        /*
        foreach (GameObject current_tg in targets)
        {   
            current_tg.GetComponent<Fruit>().reset_flag  = true;
        }*/
        randomize_targets();

        eaten_targets = 0;

        timeIdle = 0;
        Player.transform.hasChanged = false;

    }

    private void randomize_targets()
    { 
        if (switcheroo)
        {
            Shuffle();
            for (int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i].transform.localPosition = new Vector3(pos_x[i + targets.Length], obstacles[i].transform.localPosition.y, pos_z[i + targets.Length] );
                //randomize yaw of the objects here??
            }
        }

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].transform.localPosition = new Vector3(pos_x[i] + Random.Range(-1 * targetShuffleFactor, targetShuffleFactor), // + targetLateralOffset * (Random.Range(0, 2) * 2 - 1)
                                                            1,
                                                            pos_z[i] + Random.Range(-1 * targetShuffleFactor, targetShuffleFactor));
            targets[i].GetComponent<Fruit_classic>().reset_flag = true;
        }
    }

    void Shuffle()
    {
        for (int i = 0; i < pos_x.Length - 1; i++)
        {
            int rnd = Random.Range(i, pos_x.Length);
            float tempX = pos_x[rnd];
            float tempZ = pos_z[rnd];

            pos_x[rnd] = pos_x[i];
            pos_z[rnd] = pos_z[i];

            pos_x[i] = tempX;
            pos_z[i] = tempZ;
        }
    }


}
