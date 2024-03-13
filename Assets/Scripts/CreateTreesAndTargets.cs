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

        saveObjects();
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
            if (child.name != "Ground")
            {
                exp.GetComponent<Saver>().addObject(child.GetInstanceID().ToString(), child.tag,
                                            child.transform.position.x, child.transform.position.y, child.transform.position.z,
                                            child.transform.rotation.x, child.transform.rotation.y, child.transform.rotation.z,
                                            child.transform.localScale.x, child.transform.localScale.y, child.transform.localScale.z); // transform.eulerAngles.y
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
