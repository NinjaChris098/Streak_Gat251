﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum MouseState
{
    Off, // Default position. Set to Off the 1st frame after 'Released'
    Pressed, // Mouse is pressed
    Released, // Mouse is Released
    Held // If Mouse is still held after 0.3f, becomes Held
}

public class Cs_CameraLogic : MonoBehaviour
{
    MouseState mouseState;
    MouseState prevState;
    float f_MouseTimer;
    const float MAX_MOUSE_HELD = 1.0f;
    bool b_Left;
    bool b_Right;
    bool b_Forward;
    bool b_Backward;

    bool b_GameRunning;
    public GameObject go_Canvas;

    bool b_Camera_AttachedToMain;
    public GameObject Cam_Regular;
    public GameObject Cam_TopDown;

    // Use this for initialization
    void Start ()
    {
        mouseState = MouseState.Off;
        f_MouseTimer = 0f;
        b_GameRunning = true;
        SetPauseMenu(false);
        b_Camera_AttachedToMain = true;
    }

    void SetMouseState()
    {
        // Search for an excuse for the mouse to be considered off
        if(Input.GetMouseButtonUp(0) || mouseState == MouseState.Released)
        {
            // If the mouse was pressed last frame, release it now. Otherwise, shut it off.
            if (mouseState == MouseState.Held || mouseState == MouseState.Pressed) { mouseState = MouseState.Released; f_MouseTimer = 0f; return; }
            else { mouseState = MouseState.Off; return; }
        }

        if(mouseState == MouseState.Pressed || mouseState == MouseState.Held)
        {
            f_MouseTimer += Time.deltaTime;

            // If the button has been held less than 0.3f, then it's pressed. Otherwise, it's held down.
            if (f_MouseTimer < MAX_MOUSE_HELD) { mouseState = MouseState.Pressed; return; }
            else { mouseState = MouseState.Held; return; }
        }

        // Mouse pressed
        if (Input.GetMouseButtonDown(0))
        {
            mouseState = MouseState.Pressed;
        }
    }

    void MoveCamera()
    {
        if(b_Camera_AttachedToMain)
        {
            if (Input.GetKeyDown(KeyCode.A)) b_Left = true;
            if (Input.GetKeyDown(KeyCode.W)) b_Forward = true;
            if (Input.GetKeyDown(KeyCode.D)) b_Right = true;
            if (Input.GetKeyDown(KeyCode.S)) b_Backward = true;

            if (Input.GetKeyUp(KeyCode.A)) b_Left = false;
            if (Input.GetKeyUp(KeyCode.W)) b_Forward = false;
            if (Input.GetKeyUp(KeyCode.D)) b_Right = false;
            if (Input.GetKeyUp(KeyCode.S)) b_Backward = false;

            var newPos = Cam_Regular.transform.position;

            if (b_Left)
            {
                // > -3
                if (Cam_Regular.transform.position.x > -3f)
                {
                    newPos.x -= Time.deltaTime * 2;
                }
            }
            if(b_Right)
            {
                if (Cam_Regular.transform.position.x < 3f)
                {
                    newPos.x += Time.deltaTime * 2;
                }
            }

            if (b_Backward)
            {
                // > -3
                if (Cam_Regular.transform.position.z > -6f)
                {
                    newPos.z -= Time.deltaTime * 2;
                }
            }
            if (b_Forward)
            {
                if (Cam_Regular.transform.position.z < 0f)
                {
                    newPos.z += Time.deltaTime * 2;
                }
            }

            Cam_Regular.transform.position = newPos;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Update mouse information
        SetMouseState();
        MoveCamera();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            b_GameRunning = false;
            SetPauseMenu(!b_GameRunning);
        }

        if (Input.GetKeyDown(KeyCode.Space)) b_Camera_AttachedToMain = !b_Camera_AttachedToMain;

        Vector3 newPos;
        Quaternion newRot;

        if (b_Camera_AttachedToMain)
        {
            // Lerp to the cam reference's position
            newPos = Vector3.Lerp(gameObject.transform.position, Cam_Regular.transform.position, 0.1f);

            // Slerp to the cam reference's rotation
            newRot = Quaternion.Slerp(gameObject.transform.rotation, Cam_Regular.transform.rotation, 0.1f);
            
            // Set new information
            gameObject.transform.position = newPos;
            gameObject.transform.rotation = newRot;
        }
        else
        {
            // Lerp to the cam reference's position
            newPos = Vector3.Lerp(gameObject.transform.position, Cam_TopDown.transform.position, 0.1f);

            // Slerp to the cam reference's rotation
            newRot = Quaternion.Slerp(gameObject.transform.rotation, Cam_TopDown.transform.rotation, 0.1f);

            // Set new information
            gameObject.transform.position = newPos;
            gameObject.transform.rotation = newRot;
        }
        
        if(b_GameRunning)
        {
            if(mouseState == MouseState.Pressed && prevState != MouseState.Pressed)
            {
                RaycastHit hit;
                Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;

                    // Check to see if the object, when clicked, has an appropriate collider. If it does, attempt to create a wall.
                    if(objectHit.GetComponent<Cs_GridObjectLogic>())
                    {
                        objectHit.GetComponent<Cs_GridObjectLogic>().ToggleGameObjects();
                    }
                }
            }

            // Store the previous state
            prevState = mouseState;
        }
    }

    public void SetPauseMenu(bool b_IsPaused_)
    {
        if (b_IsPaused_) Time.timeScale = 0; else Time.timeScale = 1;

        go_Canvas.SetActive(b_IsPaused_);

        b_GameRunning = !b_IsPaused_;
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void HowToPlay()
    {
        SceneManager.LoadScene("Menu_1");
        SceneManager.UnloadScene("Level");
    }
}
