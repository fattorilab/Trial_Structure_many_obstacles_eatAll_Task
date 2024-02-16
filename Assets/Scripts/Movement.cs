using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public bool reverse_Xaxis;
    public bool reverse_Yaxis;

    //public bool centerRotationLaterally;
    //public float maximumAngleToTarget;
    public float restrict_horizontal;
    public float restrict_backwards;
    public float restrict_forwards = 1;
    public bool keypressed = false;
    
    public float presstime = 0;
    public GameObject Exp;

    public bool is_eating = false;

    Vector3 CamPosition;
    Vector3 CamRotation;
    float arduX = 0;
    float arduY = 0;

    float lastTimeStatic;
    int x_inversion = 1;
    int y_inversion = 1;

    Rigidbody rb;

    float collision_modifier = 1;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        lastTimeStatic = Time.time;
        if (reverse_Xaxis) { x_inversion = -1; }
        if (reverse_Yaxis) { y_inversion = -1; }
        rb = GetComponent<Rigidbody>();
        //rb.mass = 2;
        target = GameObject.Find("Target");
        //experiment = GameObject.Find("Experiment");
    }

    
    void OnCollisionEnter(Collision collision)
    {
        collision_modifier = 0.5f;
        //if (collision.gameObject.name == "YourWallName") { rigidbody.velocity = Vector3.zero;}
    }

    void OnCollisionExit(Collision other)
    {
        collision_modifier = 1;
        //print("No longer in contact with " + other.transform.name);
    }

    public void fix_collision_mod()
    {
        collision_modifier = 1;
    }

    void FixedUpdate()
    {
        arduX = x_inversion * Exp.GetComponent<Ardu>().ax1;
        arduY = y_inversion * Exp.GetComponent<Ardu>().ax2;
 
        if (Input.anyKey || arduX != 0 || arduY != 0)
        {
            keypressed = true;

            //Movement
            CamRotation = transform.localEulerAngles;
            CamRotation.y += Input.GetAxis("Horizontal") * Time.deltaTime * restrict_horizontal * 40 * speed;

            CamRotation.y += (arduX / 512f) * Time.deltaTime * restrict_horizontal * 40 * speed;
            //CamRotation.y += (arduX / 512f) * restrict_horizontal * speed * 3;


            transform.localEulerAngles = CamRotation;



            if (Input.GetAxis("Vertical") > 0 || arduY > 0) //
            {
                Vector3 moveVector = ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * Input.GetAxis("Vertical") * 4) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * (arduY / 512f) * 4);
                rb.MovePosition(transform.position + Vector3.Normalize(moveVector) * speed * restrict_forwards * collision_modifier * Time.deltaTime);
            }
            else //if (Input.GetAxis("Vertical") < 0 || arduY < 0)
            {
                Vector3 moveVector = ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * Input.GetAxis("Vertical") * 4) + ((transform.rotation * Camera.main.transform.localRotation) * Vector3.forward * (arduY / 512f) * 4);
                rb.MovePosition(transform.position + Vector3.Normalize(moveVector) * speed * restrict_backwards * collision_modifier * Time.deltaTime); // if backwards is restricted (0),everything is set to 0
            }


            presstime = (Time.time - lastTimeStatic) * 1000;
        }
        else
        {
            keypressed = false;
            lastTimeStatic = Time.time;
            presstime = 0;
        }

    }
}
