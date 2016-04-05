﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GridQuadrant
{
    Center,
    Top,
    Bottom,
    Left,
    Right
}

public enum Colors
{
    Default,
    Blue,
    Red,
    Green,
    Purple,
    SemiTransparent
}

public class Cs_DefaultBase : MonoBehaviour
{
    int i_Health;
    int i_Health_Max;
    bool b_IsDead;
    Material mat_Color; // Color is the object color (Red, Blue, Green, etc...)
    Material mat_Color_Base; // Base is the black backdrop
    float f_FireTimer;
    float f_FireTimer_Max;
    BoxCollider col_BaseCollider;
    CapsuleCollider col_RadiusCollider;
    List<GameObject> EnemyList;

    public Material testMat;

    int element_Color = -1; // Color is the object color (Red, Blue, Green, etc...)
    int element_Base = -1; // Base is the black backdrop

    virtual public void Initialize(int i_Health_Max_, float f_FireTimer_Max_, BoxCollider col_BaseCollider_, CapsuleCollider col_RadiusCollider_)
    {
        i_Health = i_Health_Max_;
        i_Health_Max = i_Health_Max_;

        b_IsDead = false;

        // Material newMat = Resources.Load("DEV_Orange", typeof(Material)) as Material;
        SetMaterialElements();
        SetNewMaterialColor(Colors.Default);

        f_FireTimer = 0;
        f_FireTimer_Max = f_FireTimer_Max_;

        col_BaseCollider = col_BaseCollider_;
        col_RadiusCollider = col_RadiusCollider_;

        EnemyList = new List<GameObject>();
    }

    // Gathers the default element positions for the Material backdrops
    void SetMaterialElements()
    {
        Material matColor = Resources.Load("Color_Base", typeof(Material)) as Material; // (Red, Blue, etc...)
        Material matColorBase = Resources.Load("Mat_BASE", typeof(Material)) as Material; // Black Backdrop
        var tempMatList = gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterials;
        
        for (int i = 0; i < tempMatList.Length; ++i)
        {
            // renderer.sharedMaterial
            if      (tempMatList[i] == matColor)        element_Color = i;
            else if (tempMatList[i] == matColorBase)    element_Base = i;
        }
    }

    virtual public void SetHealth(int i_Health_, int i_Health_Max_ = -1)
    {
        i_Health = i_Health_;

        if (i_Health_Max_ != -1) i_Health_Max = i_Health_Max_;
    }

    virtual public void ApplyDamage(int i_Damage)
    {
        i_Health -= i_Damage;

        if(i_Damage <= 0)
        {
            // Destroy GameObject
        }
    }

    public void SetNewMaterialColor(Colors newColor_)
    {
        if (newColor_ == Colors.Default) mat_Color = Resources.Load("Mat_BASE", typeof(Material)) as Material;
        else if (newColor_ == Colors.Blue) mat_Color = Resources.Load("Mat_BLUE", typeof(Material)) as Material;
        else if (newColor_ == Colors.Green) mat_Color = Resources.Load("Mat_GREEN", typeof(Material)) as Material;
        else if (newColor_ == Colors.Purple) mat_Color = Resources.Load("Mat_PURPLE", typeof(Material)) as Material;
        else if (newColor_ == Colors.Red) mat_Color = Resources.Load("Mat_RED", typeof(Material)) as Material;
        else if (newColor_ == Colors.SemiTransparent) mat_Color = Resources.Load("Mat_TRANSPARENT", typeof(Material)) as Material;

        if(newColor_ != Colors.SemiTransparent)
        {
            mat_Color_Base = Resources.Load("Mat_BASE", typeof(Material)) as Material;

            var tempMatList = gameObject.GetComponentInChildren<MeshRenderer>().materials;
            tempMatList[element_Base] = mat_Color_Base;
            tempMatList[element_Color] = mat_Color;
            gameObject.GetComponentInChildren<MeshRenderer>().materials = tempMatList;

            print(gameObject.GetComponentInChildren<MeshRenderer>().name);
        }
        else // Turn semi-transparent for a short while
        {
            var tempTransList = gameObject.GetComponentInChildren<MeshRenderer>().materials;
            for(int i = 0; i < tempTransList.Length; ++i)
            {
                tempTransList[i] = mat_Color;
            }
            gameObject.GetComponentInChildren<MeshRenderer>().materials = tempTransList;
        }
        Debug.Log(gameObject.GetComponentInChildren<MeshRenderer>().materials[element_Color].ToString());
    }
}
