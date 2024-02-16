using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public myEnum setting = new myEnum();
    public enum myEnum
    {
        Free,
        ForwardMotion,
        ForwardAndFar,
        ForwardAndRelease,
        ForwardAndBackward,
        AssistedLateral,
        FixedLateral,
        UnassistedLateral,
        ObstacleCourse,
        RotationOnly
    };

    //public Item1 item1;

    //public string tasktype = "pigiobevo";
    public int seed;

    public int reward_length;
    public int minimumRewardTime;
    public float minimumDistance;
    public int MaxDistAllowed = 15;
    public int MaxAngleAllowed = 90;
    public int maximumJuicyTime;
    public int nullpositionTime;
    public int maximumTimeIdle;

    public bool immediateReward;
    public bool Reward_for_rotation=false;
    public float targetShuffleFactor;
    public float targetLateralShuffleFactor;
    public float targetLateralOffset;
    public float angleForRotationReward;

    public bool abortIfFar;
    public bool abortIfLookingAway=false;

    int state = 0;
    GameObject[] targets;
    GameObject greenery;

    public GameObject Player;
    public GameObject Lines;
    public GameObject Obstacle;

    float[] target_distance;
    float[] target_lateral;

    float timeIdle = 0;
    float startTimeIdle;

    bool starting = true;

    void Start()
    {
        greenery = GameObject.Find("SomeGreenery");

        Random.InitState(seed);

        targets = GameObject.FindGameObjectsWithTag("Target");
        target_distance = new float[targets.Length];
        target_lateral = new float[targets.Length];

        //get initial positions
        for (int i = 0; i < targets.Length; i++)
        {
            target_distance[i] = targets[i].transform.localPosition.z;
            target_lateral[i] = targets[i].transform.localPosition.x;
        }


        Player.transform.hasChanged = false;
        startTimeIdle = Time.time;

        if (setting == myEnum.ForwardMotion)
        {
            immediateReward = true;
            minimumDistance = target_distance[0] - 0.001f;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 0;
            Player.GetComponent<Movement>().restrict_backwards = 0;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.ForwardAndFar)
        {
            immediateReward = true;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 0;
            Player.GetComponent<Movement>().restrict_backwards = 0;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.ForwardAndRelease)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 0;
            Player.GetComponent<Movement>().restrict_backwards = 0;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.ForwardAndBackward)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 0;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.AssistedLateral)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            //Player.GetComponent<Movement>().maximumAngleToTarget = 50;
            Lines.SetActive(true);
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.FixedLateral)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = true;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.UnassistedLateral)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        if (setting == myEnum.ObstacleCourse)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            Player.GetComponent<Movement>().restrict_backwards = 1;
            Obstacle.SetActive(true);
            Player.GetComponent<Movement>().restrict_forwards = 1;
            Reward_for_rotation = false;
        }

        
        if (setting == myEnum.RotationOnly)
        {
            immediateReward = false;
            minimumDistance = 2;
            Player.GetComponent<Movement>().centerRotationLaterally = false;
            Player.GetComponent<Movement>().restrict_horizontal = 1;
            Player.GetComponent<Movement>().restrict_backwards = 0;
            Player.GetComponent<Movement>().restrict_forwards = 0;
            Reward_for_rotation = true;
            abortIfLookingAway = true;
        }

        reset();
        starting = false;

    }

    float minTargetDist = 1000;
    int thisTargetNumber = 0;
    int iterator = 0;
    //public float angle_player_target = 90;

    void Update()
    {
        //task
        //if (Input.GetKey(KeyCode.Return))
        if (Input.GetButtonDown("Submit"))
        {
            GetComponent<Ardu>().SendReward(reward_length);
        }

        //angle_player_target = targets[0].GetComponent<Fruit>().playerAngle;
        


        //idleTime
        if (!Player.transform.hasChanged && Player.transform.localPosition != Vector3.zero)
        {
            timeIdle = (Time.time - startTimeIdle) * 1000;
            if (timeIdle > maximumTimeIdle && state != 4)
            {
                state = 3;
            }
        } 
        else
        {
            startTimeIdle = Time.time; // set the startTimeIdle to current moment
            Player.transform.hasChanged = false;
        }

        //iterate through targets
        minTargetDist = 1000;
        iterator = 0;
        foreach (GameObject target in targets) //too long juicy
        {
            if (target.GetComponent<Fruit>().timeJuicy > maximumJuicyTime && state != 4)
            {
                state = 3;
            }

            if (target.GetComponent<Fruit>().distance < minTargetDist)
            {
                minTargetDist = target.GetComponent<Fruit>().distance;
                thisTargetNumber = iterator;
            }

            if (Reward_for_rotation)
            {
                target.GetComponent<Fruit>().JuicyByAngle(angleForRotationReward);
            }
            iterator++;
        }

        //completely far of:
        if (abortIfFar && state != 4)
        {

            /*
            if (target.GetComponent<Fruit>().distance > 10 && targets[thisTargetNumber].GetComponent<Fruit>().playerAngle > 50)
            {
                state = 3;
            }*/

            if (targets[thisTargetNumber].GetComponent<Fruit>().distance > MaxDistAllowed)
            {
                state = 3;
            }

            if (minTargetDist > 25)
            {
                state = 3;
            } else if (minTargetDist > 10 && targets[thisTargetNumber].GetComponent<Fruit>().playerAngle > 50)
            {
                state = 3;
            }

            
        }

        if (abortIfLookingAway && state != 4)
        {
            if (targets[thisTargetNumber].GetComponent<Fruit>().playerAngle > MaxAngleAllowed)
            {
                state = 3;
            }

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
                    cam.backgroundColor = new Color(255, 255, 255); //whitescreen
                }
                state = 4;
                break;
            case 4: //whitescreen

                foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
                {
                    cam.transform.eulerAngles = new Vector3(0, 0, 0);
                    cam.transform.localEulerAngles = new Vector3(-90, 0, 0);
                }

                if (!Player.GetComponent<Movement>().keypressed)
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
        if (!immediateReward && state != 4 && !starting) { GetComponent<Ardu>().SendReward(reward_length); }
        
        //target.transform.localPosition = new Vector3(-2, 1, target_distance + Random.Range(-target_distance * targetShuffleFactor, target_distance * targetShuffleFactor)); //fixed value for medio-lateral position of the target (==-2)

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].transform.localPosition = new Vector3(target_lateral[i] + Random.Range(-target_lateral[i] * targetLateralShuffleFactor, target_lateral[i] * targetLateralShuffleFactor) + targetLateralOffset * (Random.Range(0, 2) * 2 - 1), 
                                                            1, 
                                                            target_distance[i] + Random.Range(-target_distance[i] * targetShuffleFactor,  target_distance[i] * targetShuffleFactor));
            targets[i].GetComponent<Fruit>().angleJuice = false;
        }

        Player.transform.localPosition = Vector3.zero; // modify to introduce a bias in the initial position of the player
        Player.transform.localEulerAngles = Vector3.zero;
        state = 0;
        try
        {
            greenery.GetComponent<SomeGreenery>().deleteGreenery();
            greenery.GetComponent<SomeGreenery>().createGreenery();
        }
        catch //(Exception e)
        {
            //Debug.Log("greenery deactivated");
        }

        foreach (Camera cam in Player.GetComponentsInChildren<Camera>())
        {
            cam.transform.eulerAngles = new Vector3(0, 0, 0);
            cam.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        timeIdle = 0;
        Player.transform.hasChanged = false;

    }
}
