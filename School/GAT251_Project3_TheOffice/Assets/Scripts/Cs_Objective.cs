﻿using UnityEngine;
using System.Collections;

enum Enum_TaskList
{
    BossKickMeSign,
    StaplePapers,
    WriteEmail,
    SendFax,
    UseNerfGun,
    MakeABasket,
    DrinkCoffee,
    EatBagles,
    FirePeople,
    CallMotherRussia,
    GiveCommunistManifesto,
    ChangeRadioStation
}

public enum Enum_ObjectiveState
{
    Disabled,
    InProgress,
    Completed
}

public class Cs_Objective : MonoBehaviour
{
    [SerializeField] Enum_TaskList ObjectiveType;
    [SerializeField] Material mat_CompletedMaterial;

    Material mat_UseThis;

    Enum_ObjectiveState e_ObjectiveState = Enum_ObjectiveState.Disabled;

    Cs_ObjectiveManager go_ObjectiveManager;

	// Use this for initialization
	void Start ()
    {
        mat_UseThis = Resources.Load("mat_UseThis") as Material;

        Set_ObjectiveState(Enum_ObjectiveState.Disabled);

        go_ObjectiveManager = GameObject.Find("LevelManager").GetComponent<Cs_ObjectiveManager>();
    }

    void Set_ObjectiveState( Enum_ObjectiveState e_State_ )
    {
        if(e_State_ == Enum_ObjectiveState.Disabled)
        {
            if(ObjectiveType == Enum_TaskList.BossKickMeSign)
            {
                gameObject.GetComponent<MeshRenderer>().material = mat_UseThis;

                Set_Transparent();

                gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else if(e_State_ == Enum_ObjectiveState.InProgress)
        {
            if (ObjectiveType == Enum_TaskList.BossKickMeSign)
            {
                gameObject.GetComponent<MeshRenderer>().material = mat_UseThis;

                Set_Transparent();

                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            
        }
        else if(e_State_ == Enum_ObjectiveState.Completed)
        {
            if( mat_CompletedMaterial )
            {
                gameObject.GetComponent<MeshRenderer>().material = mat_CompletedMaterial;

                Set_Opaque();

                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            
            if(ObjectiveType == Enum_TaskList.BossKickMeSign)
            {
                go_ObjectiveManager.Complete_BossKickMe();
            }

            if(ObjectiveType == Enum_TaskList.ChangeRadioStation)
            {
                go_ObjectiveManager.Complete_ChangeRadioStation();
            }
        }
    }

    void Set_Opaque()
    {
        Material[] mat_CurrColor = gameObject.GetComponent<MeshRenderer>().materials;

        for (int i_ = 0; i_ < mat_CurrColor.Length; ++i_)
        {
            Color currColor = mat_CurrColor[i_].color;
            Material mat = GetComponentInChildren<MeshRenderer>().materials[i_];

            // Got guide: http://sassybot.com/blog/swapping-rendering-mode-in-unity-5-0/
            mat.SetFloat("_Mode", 0); // Sets the material to Opaque
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
            
            currColor.a = 1f;
            mat_CurrColor[i_].color = currColor;
        }
    }

    void Set_Transparent(float f_Transparency_ = 0.25f)
    {
        Material[] mat_CurrColor = gameObject.GetComponent<MeshRenderer>().materials;

        for (int i_ = 0; i_ < mat_CurrColor.Length; ++i_)
        {
            Color currColor = mat_CurrColor[i_].color;
            Material mat = GetComponentInChildren<MeshRenderer>().materials[i_];

            // Got guide: http://sassybot.com/blog/swapping-rendering-mode-in-unity-5-0/
            mat.SetFloat("_Mode", 3); // Sets the material to Transparent
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            currColor.a = f_Transparency_;
            mat_CurrColor[i_].color = currColor;
        }
    }

    public Enum_ObjectiveState Set_State
    {
        set
        {
            e_ObjectiveState = value;

            Set_ObjectiveState(value);
        }
        get { return e_ObjectiveState; }
    }

    public void Use()
    {
        if(ObjectiveType == Enum_TaskList.BossKickMeSign)
        {
            Set_ObjectiveState(Enum_ObjectiveState.Completed);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
