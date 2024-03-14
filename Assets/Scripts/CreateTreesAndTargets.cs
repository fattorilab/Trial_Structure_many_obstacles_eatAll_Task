using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTreesAndTargets : MonoBehaviour
{
    GameObject exp;

    public GameObject tree1;
    public GameObject tree2;
    public GameObject tree3;
    public GameObject lowlevel;
    public GameObject midlevel;
    public GameObject highlevel;
    public GameObject doubleobject;

    void Start()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        exp = GameObject.Find("Experiment");
        createGreenery();
    }

    public void createGreenery()
    {
        for (int i = 0; i < 2; i++) //48 + 8 obj /before 45 + 9
        {
            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor
            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor

            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor
            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor

            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor
            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor

            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor
            createOneObject(tree1, "Obstacle"); //Distraktor
            createOneObject(tree2, "Obstacle"); //Distraktor
            createOneObject(tree3, "Obstacle"); //Distraktor

            /*createOneObject(lowlevel, "LowTarget");
            createOneObject(midlevel, "MidTarget");
            createOneObject(highlevel, "HighTarget");
            createOneObject(doubleobject, "DoubleTarget");*/
        }

        // saveObjects();
        saveObjects_debug();
    }


    void createOneObject(GameObject Prefab, string type)
    {
        Vector3 pos = new Vector3(Random.Range(-25f, 25f), 0, Random.Range(-25f, 25f));
        GameObject newObject = Instantiate(Prefab, transform.position + pos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), transform);
        newObject.tag = type;
    }

    void saveObjects()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "Ground" && child != null)
            {

                //if (child == null)
                //{
                //    Debug.LogError($"Error: {child.GetInstanceID().ToString()} is null in ExampleScript!");
                //    return; // Exit the Start method to prevent further errors
                //}

                //exp.GetComponent<Saver>().addObject(child.GetInstanceID().ToString(), child.tag,
                //    child.transform.position.x, child.transform.position.y, child.transform.position.z,
                //    child.transform.eulerAngles.x, child.transform.eulerAngles.y, child.transform.eulerAngles.z,
                //    child.transform.localScale.x, child.transform.localScale.y, child.transform.localScale.z); // transform.eulerAngles.y

                exp.GetComponent<Saver>().addObject(child.GetInstanceID().ToString(), child.tag,
                                    0f, 0f, 0f,
                                    0f, 0f, 0f,
                                    0f, 0f, 0f);

            }
        }
    }

    void saveObjects_debug()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "Ground")
            {
                if (child != null)
                {
                    // Check if child's tag is not null
                    string tag = child.tag ?? "NoTag";

                    // Check if child's transform is not null
                    Vector3 position = child.transform.position;
                    Vector3 rotation = child.transform.eulerAngles;
                    Vector3 scale = child.transform.localScale;

                    // Log error if any required component is null
                    if (tag == null)
                    {
                        Debug.LogError($"Error: Tag is null for child object '{child.name}'");
                    }
                    else if (position == null)
                    {
                        Debug.LogError($"Error: Position is null for child object '{child.name}'");
                    }
                    else if (rotation == null)
                    {
                        Debug.LogError($"Error: Rotation is null for child object '{child.name}'");
                    }
                    else if (scale == null)
                    {
                        Debug.LogError($"Error: Scale is null for child object '{child.name}'");
                    }
                    else
                    {
                        // If all required components are not null, proceed with saving
                        exp.GetComponent<Saver>().addObject(child.GetInstanceID().ToString(), tag,
                                            position.x, position.y, position.z,
                                            rotation.x, rotation.y, rotation.z,
                                            scale.x, scale.y, scale.z);
                    }
                }
                else
                {
                    Debug.LogWarning("Warning: Child object is null");
                }
            }
        }
    }

    public void deleteGreenery()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "Ground")
            {
                exp.GetComponent<Saver>().addObjectEnd(child.GetInstanceID().ToString());
                Destroy(child.gameObject);
            }
        }
    }
}
