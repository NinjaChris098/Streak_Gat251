﻿using UnityEngine;
using System.Collections;

public class Cs_EnergyBoxLogic : MonoBehaviour
{
    public GameObject[] ConnectedObjects = new GameObject[5];

    GameObject childModel;
    float f_FlashModelTimer = 0.1f;
    Color startColor;

    public int i_Health = 5;
    bool b_IsAlive = true;

	// Use this for initialization
	void Start ()
    {
        childModel = transform.FindChild("Mod_EnergyBox").gameObject;
        startColor = childModel.GetComponent<MeshRenderer>().material.color;

        f_FlashModelTimer = 1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            TurnBoxOn();
        }

        FlashModel();
    }

    void FlashModel()
    {
        if(b_IsAlive)
        {
            if(i_Health > 0)
            {
                // Keep counting upward to compare against
                if(f_FlashModelTimer <= 1) f_FlashModelTimer += Time.deltaTime;

                if (f_FlashModelTimer < 0.2f)
                {
                    Color currColor = childModel.GetComponent<MeshRenderer>().material.color;
                    currColor.g = 0;
                    currColor.b = 0;
                    childModel.GetComponent<MeshRenderer>().material.color = currColor;
                }
                else
                {
                    childModel.GetComponent<MeshRenderer>().material.color = startColor;
                }
            }
            else // Turns off the button
            {
                TurnBoxOff();
            }
        }
    }

    void TurnBoxOff()
    {
        Color currColor = childModel.GetComponent<MeshRenderer>().material.color;
        currColor.g = 0;
        currColor.b = 0;
        currColor.r = 0;
        childModel.GetComponent<MeshRenderer>().material.color = currColor;

        b_IsAlive = false;

        DisableAllChildObjects();

        gameObject.GetComponent<HealthSystem>().SetObjectiveStatus(false);
    }

    public void TurnBoxOn()
    {
        childModel.GetComponent<MeshRenderer>().material.color = startColor;

        b_IsAlive = true;

        EnableAllChildObjects();

        gameObject.GetComponent<HealthSystem>().SetObjectiveStatus(true);
    }

    void DisableAllChildObjects()
    {
        for(uint i = 0; i < ConnectedObjects.Length; ++i)
        {
            if(ConnectedObjects[i] != null)
            {
                if (ConnectedObjects[i].GetComponent<HealthSystem>())
                {
                    ConnectedObjects[i].GetComponent<HealthSystem>().SetObjectiveStatus(false);
                }
            }
        }
    }

    void EnableAllChildObjects()
    {
        for (uint i = 0; i < ConnectedObjects.Length; ++i)
        {
            if (ConnectedObjects[i] != null)
            {
                if (ConnectedObjects[i].GetComponent<HealthSystem>())
                {
                    ConnectedObjects[i].GetComponent<HealthSystem>().SetObjectiveStatus(true);
                }
            }
        }

        i_Health = 5;
    }

    void OnTriggerEnter(Collider collider_)
    {
        if (collider_.tag == "Laser")
        {
            i_Health -= 1;
            f_FlashModelTimer = 0;
        }

        // GameObject.Destroy(gameObject);
    }
}
