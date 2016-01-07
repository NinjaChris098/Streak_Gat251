﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

enum Enum_IconTypes
{
    FirstBlood,
    Dragon,
    Tower,
    Inhib,
    Baron
}
enum Enum_IconOwner { None, Left, Right }

public class Icon
{
    public GameObject go_Icon;
    // public string s_Name;
    public bool b_IsActive;
    public int i_UIPos;
    public bool b_IsTeamLeft;
}

public class Cs_SystemManager : MonoBehaviour
{
    public string TournamentName;
    public bool SmallText;
    public string TeamName_Left;
    public string TeamName_Right;
    public Sprite TournamentLogo;
    public bool NoIcon;

    GameObject Nametag_Left;
    GameObject Nametag_Right;

    Icon Icon_FirstBlood = new Icon();
    Icon Icon_Dragon = new Icon();
    Icon Icon_Tower = new Icon();
    Icon Icon_Baron = new Icon();
    Icon Icon_Inhib = new Icon();
    Enum_IconOwner IO_FirstBlood = Enum_IconOwner.None;
    Enum_IconOwner IO_Dragon = Enum_IconOwner.None;
    Enum_IconOwner IO_Tower = Enum_IconOwner.None;
    Enum_IconOwner IO_Inhib = Enum_IconOwner.None;
    Enum_IconOwner IO_Baron = Enum_IconOwner.None;

    int i_TeamIcons_Left = 0;
    int i_TeamIcons_Right = 0;

    // Use this for initialization
    void Start ()
    {
        InitializeIcons();
        SetTournamentLogo(TournamentLogo, TournamentName);

        // Nametag_Left = GameObject.Find("Nametag_Left");
        // Nametag_Right = GameObject.Find("Nametag_Right");

        // Nametag_Left.GetComponent<Text>().text = "HH";
    }

    void InitializeIcons()
    {
        // Set Icons
        Icon_FirstBlood.go_Icon = GameObject.Find("Icon_FirstBlood");
        Icon_Dragon.go_Icon = GameObject.Find("Icon_Dragon");
        Icon_Tower.go_Icon = GameObject.Find("Icon_Tower");
        Icon_Baron.go_Icon = GameObject.Find("Icon_Baron");
        Icon_Inhib.go_Icon = GameObject.Find("Icon_Inhib");


        // Turn off Icons
        Icon_FirstBlood.go_Icon.GetComponent<SpriteRenderer>().enabled = false;
        Icon_FirstBlood.b_IsActive = false;

        Icon_Dragon.go_Icon.GetComponent<SpriteRenderer>().enabled = false;
        Icon_Dragon.b_IsActive = false;

        Icon_Tower.go_Icon.GetComponent<SpriteRenderer>().enabled = false;
        Icon_Dragon.b_IsActive = false;

        Icon_Baron.go_Icon.GetComponent<SpriteRenderer>().enabled = false;
        Icon_Dragon.b_IsActive = false;

        Icon_Inhib.go_Icon.GetComponent<SpriteRenderer>().enabled = false;
        Icon_Dragon.b_IsActive = false;
    }

    void SetTournamentLogo(Sprite TournamentLogo_, string TournamentText_)
    {
        GameObject tournamentLogo = GameObject.Find("Overlay_Logo");
        GameObject tournamentText = GameObject.Find("Overlay_Text");

        // Reposition text based on bools
        if (NoIcon)
        {
            tournamentText.transform.localPosition = new Vector3(0, 0, 0);
            tournamentLogo.SetActive(false);
        }

        if(SmallText)
        {
            tournamentText.transform.localScale = new Vector3(0.001f, 0.001f, 1);
        }

        tournamentLogo.GetComponent<SpriteRenderer>().sprite = TournamentLogo_;
        
        if(TournamentText_.Contains("/"))
        {
            TournamentText_ = TournamentText_.Replace("/", "\n");
        }

        tournamentText.GetComponent<TextMesh>().text = TournamentText_;
    }

    // Used by Keyboard Input to apply Icon's to the screen
    void ActivateIcon(Enum_IconTypes iconType_, Enum_IconOwner iconOwner_, float f_DT)
    {
        // Pre-loading generic Icon for manipulation
        Icon currentIcon = Icon_FirstBlood;

        // Load icon to check against
        if (iconType_ == Enum_IconTypes.Baron) currentIcon = Icon_Baron;
        if (iconType_ == Enum_IconTypes.Dragon) currentIcon = Icon_Dragon;
        if (iconType_ == Enum_IconTypes.Tower) currentIcon = Icon_Tower;
        if (iconType_ == Enum_IconTypes.Inhib) currentIcon = Icon_Inhib;

        // If the icon is still disabled...
        if (!currentIcon.b_IsActive)
        {
            // Figure out which position the icon will go in
            if(iconOwner_ == Enum_IconOwner.Left)
            {
                // Reposition the icon based on the current i_TeamIcons_Left/Right number
                // 520 - 90 * i_TeamIcons. Right Team *= -1;
                var finalPos = currentIcon.go_Icon.gameObject.transform.position;
                finalPos.x = -520 + (90 * i_TeamIcons_Left);

                var currPos = currentIcon.go_Icon.gameObject.transform.position;
                currPos.x = -520 + (90 * i_TeamIcons_Left) + 45;

                currentIcon.go_Icon.gameObject.transform.position = currPos;

                // Increment the i_TeamIcons
                ++i_TeamIcons_Left;
            }
            else if(iconOwner_ == Enum_IconOwner.Right)
            {
                var finalPos = currentIcon.go_Icon.gameObject.transform.position;
                finalPos.x = 520 - (90 * i_TeamIcons_Right);

                var currPos = currentIcon.go_Icon.gameObject.transform.position;
                currPos.x = 520 - (90 * i_TeamIcons_Right) - 45;

                currentIcon.go_Icon.gameObject.transform.position = currPos;

                // Increment the i_TeamIcons
                ++i_TeamIcons_Right;
            }
            
            // Enable & place the icon
            currentIcon.go_Icon.GetComponent<SpriteRenderer>().enabled = true;
            currentIcon.b_IsActive = true;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // ActivateIcon(Enum_IconTypes.FirstBlood, Enum_IconOwner.Left, Time.deltaTime);

        // Quit Application (For now)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Left Team Input
        if (Input.GetKeyDown(KeyCode.Q)) IO_FirstBlood = Enum_IconOwner.Left;
        if (Input.GetKeyDown(KeyCode.W)) IO_Dragon = Enum_IconOwner.Left;
        if (Input.GetKeyDown(KeyCode.E)) IO_Tower = Enum_IconOwner.Left;
        if (Input.GetKeyDown(KeyCode.R)) IO_Inhib = Enum_IconOwner.Left;
        if (Input.GetKeyDown(KeyCode.T)) IO_Baron = Enum_IconOwner.Left;

        if(IO_FirstBlood != Enum_IconOwner.None)
        {
            if(IO_FirstBlood == Enum_IconOwner.Left) ActivateIcon(Enum_IconTypes.FirstBlood, Enum_IconOwner.Left, Time.deltaTime);
            else ActivateIcon(Enum_IconTypes.FirstBlood, Enum_IconOwner.Right, Time.deltaTime);
        }
        if (IO_Dragon != Enum_IconOwner.None)
        {
            if (IO_Dragon == Enum_IconOwner.Left) ActivateIcon(Enum_IconTypes.Dragon, Enum_IconOwner.Left, Time.deltaTime);
            else ActivateIcon(Enum_IconTypes.Dragon, Enum_IconOwner.Right, Time.deltaTime);
        }

        // Right Team Input
        if (Input.GetKeyDown(KeyCode.P)) IO_FirstBlood = Enum_IconOwner.Right;
        if (Input.GetKeyDown(KeyCode.O)) IO_Dragon = Enum_IconOwner.Right;
        if (Input.GetKeyDown(KeyCode.I)) IO_Tower = Enum_IconOwner.Right;
        if (Input.GetKeyDown(KeyCode.U)) IO_Inhib = Enum_IconOwner.Right;
        if (Input.GetKeyDown(KeyCode.Y)) IO_Baron = Enum_IconOwner.Right;

    }
}
