using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_classic : MonoBehaviour
{
    public float speed;
    public bool reverse_axis;

    public bool centerRotationLaterally;
    public float maximumAngleToTarget;
    public float restrict_horizontal;
    public float restrict_backwards;
    public float restrict_forwards = 1;
    public bool keypressed = false;
    
    public float presstime = 0;
    public GameObject Exp;

    Vector3 CamPosition;
    Vector3 CamRotation;
    float arduX = 0;
    float arduY = 0;

    float lastTimeStatic;
    int ax_inversion = 1;

    Rigidbody rb;

    float collision_modifier = 1;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        lastTimeStatic = Time.time;
        if (reverse_axis) { ax_inversion = -1; }
        rb = GetComponent<Rigidbody>();
        rb.mass = 2;
        target = GameObject.Find("Target");
        //experiment = GameObject.Find("Experiment");
        //experiment = GameObject.Find("Experiment");
    }

    void OnCollisionEnter(Collision collision)
    {
        collision_modifier = 0.5f;
        /*if (collision.gameObject.name == "YourWallName") 
        {
            rigidbody.velocity = Vector3.zero;
        }*/
    }

    void OnCollisionExit(Collision other)
    {
        collision_modifier = 1;
        //print("No longer in contact with " + other.transform.name);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        arduX = ax_inversion * Exp.GetComponent<Ardu>().ax1;
        arduY = ax_inversion * Exp.GetComponent<Ardu>().ax2;

        if (Input.anyKey || arduX != 0 || arduY != 0)
        {
            keypressed = true;

            //Movement
            if (!centerRotationLaterally)
            {
                CamRotation = transform.localEulerAngles;
                CamRotation.y += Input.GetAxis("Horizontal") * Time.deltaTime * restrict_horizontal * 50 * speed;
                CamRotation.y += (arduX / 512f) * Time.deltaTime * restrict_horizontal * 20 * speed*3;



                if (target.GetComponent<Fruit_classic>().playerAngle > maximumAngleToTarget)
                {
                    CamRotation.y -= maximumAngleToTarget;
                }

                transform.localEulerAngles = CamRotation;

            }

            /*
            if (experiment.GetComponent<Main>().Reward_for_rotation)
            {
                if (abs(Input.GetAxis("Vertical")) < 10 || abs(arduY) < 10)
                {
                    Vector3 moveVector = ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * Input.GetAxis("Horizontal") * 5) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * (arduX / 512f) * 2);
                }
            }

            else
            {
            */
                if (Input.GetAxis("Vertical") > 0 || arduY > 0) //
                {
                    Vector3 moveVector = ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * Input.GetAxis("Vertical") * 5) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * (arduY / 512f) * 4);
                    if (centerRotationLaterally)
                    { moveVector += ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * Input.GetAxis("Horizontal") * 5) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * (arduX / 512f) * 2); }
                    rb.MovePosition(transform.position + moveVector * speed * restrict_forwards * collision_modifier * Time.deltaTime);
                }
                else //if (Input.GetAxis("Vertical") < 0 || arduY < 0)
                {
                    Vector3 moveVector = ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * Input.GetAxis("Vertical") * 5) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * (arduY / 512f) * 4);
                    if (centerRotationLaterally)
                    { moveVector += ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * Input.GetAxis("Horizontal") * 5) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.right * (arduX / 512f) * 2); }
                    rb.MovePosition(transform.position + moveVector * speed * restrict_backwards * collision_modifier * Time.deltaTime); // if backwards is restricted (0),everything is set to 0
                }
            //}
      


            if (centerRotationLaterally || (target.GetComponent<Fruit_classic>().playerAngle > maximumAngleToTarget))
            {
                foreach (Transform child in transform)
                {
                    child.LookAt(target.transform.localPosition, Vector3.up);
                }
            }

           
            presstime = (Time.time - lastTimeStatic)*1000;
        } else {
            keypressed = false;
            lastTimeStatic = Time.time;
            presstime = 0;
        }
        
    }
}
