﻿using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public enum Enum_CameraState
{
    Lerp_ToPlayer,
    Lerp_FromPlayer,
    OnPlayer,
    OnTempPoint
}

public class Cs_PlayerController : MonoBehaviour
{
    // PLAYER STATS & INFORMATION
    public float MAX_PLAYER_SPEED;
    public float ACCELERATION;
    [Range( 0, 1 )]
    public float f_Magnitude_Sneak;
    [Range(0, 1)]
    public float f_Magnitude_Brisk;
    [Range(0, 2)]
    public float f_Magnitude_Sprint;

    [SerializeField]
    public float currSpeedReadOnly;
    
    // Player variables
    Vector3 v3_CurrentVelocity;
    Vector3 v3_PriorVector;
    bool b_IsSprinting = false;

    // Controller vs. Keyboard - Last Used
    bool b_KeyboardUsedLast;
    float f_DoubleTapForSprintTimer;

    // Controller Input
    GamePadState state;
    GamePadState prevState;
    public PlayerIndex playerOne = PlayerIndex.One;

    // Raycast Objects
    GameObject go_SlopeRaycast;

    // Camera information
    GameObject go_Camera;
    GameObject go_Camera_DefaultPos;
    GameObject go_Camera_TempPos;
    Enum_CameraState cameraState = Enum_CameraState.Lerp_ToPlayer;
    float cameraLerpTime = 0.75f;
    float cameraLerpTime_Curr = 0.75f;

    // Abilities/Projectile
    public GameObject go_FireLocation;
    public GameObject prefab_Rock;
    public GameObject go_TargetObject;
    Vector3 v3_TargetLocation;
    public float f_FiringAngle = 45.0f;
    public float f_Gravity = 9.8f;

    // Use this for initialization
    void Start ()
    {
        go_SlopeRaycast = transform.FindChild("SlopeRaycast").gameObject;

        // Define Camera Information
        go_Camera = GameObject.Find("Main Camera");
        go_Camera_DefaultPos = transform.Find("Camera_Player").gameObject;

        // Abilities/Projectile
        v3_TargetLocation = go_TargetObject.transform.position;

        SetCameraPosition(go_Camera_DefaultPos);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        b_KeyboardUsedLast = KeyboardCheck(b_KeyboardUsedLast);

        if (b_KeyboardUsedLast)
        {
            // Input_Keyboard();
        }
        else Input_Controller();

        currSpeedReadOnly = gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        // Update Camera position/rotation
        UpdateCameraPosition();
	}

    bool KeyboardCheck(bool b_KeyboardPressed_)
    {
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.Space))
        {
            return true;
        }

        return false;
    }

    void PlayerMovement( Vector3 v3_InputVector_, float f_Magnitude_ )
    {
        // Grab previous velocity to compare against
        Vector3 v3_PreviousVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        Vector3 v3_NewVelocity = v3_InputVector_ * MAX_PLAYER_SPEED * f_Magnitude_;

        v3_CurrentVelocity = Vector3.Lerp(v3_PreviousVelocity, v3_NewVelocity, ACCELERATION * Time.deltaTime);

        // Receive the ramp angle below the player
        RaycastHit rayHit = EvaluateGroundVector();

        Vector3 v3_FinalVelocity = Vector3.ProjectOnPlane( v3_CurrentVelocity, rayHit.normal );

        if(rayHit.distance >= 0.265f)
        {
            v3_FinalVelocity.y = v3_PreviousVelocity.y - (Time.deltaTime * 50);
        }

        gameObject.GetComponent<Rigidbody>().velocity = v3_FinalVelocity;
    }

    void Input_Controller()
    {
        // Capture latest input
        prevState = state;
        state = GamePad.GetState(playerOne);

        #region Movement
        // Create new temporary Vector3 to apply Controller input
        Vector3 v3_InputVector = new Vector3();

        // Accept Left Analog Stick input, apply into Vector3
        v3_InputVector.x = state.ThumbSticks.Left.X;
        v3_InputVector.z = state.ThumbSticks.Left.Y;

        #endregion

        #region Sprinting
        float f_Magnitude = 0f;

        if( !b_IsSprinting)
        {
            if (state.Buttons.LeftStick == ButtonState.Pressed && prevState.Buttons.LeftStick == ButtonState.Released) b_IsSprinting = true;
        }
        else
        {
            if (v3_InputVector.magnitude < 0.1f) b_IsSprinting = false;
        }

        // If the player speed isn't 0, apply preset speeds
        if (v3_InputVector.magnitude != float.Epsilon)
        {
            if      (v3_InputVector.magnitude < 0.15f)   f_Magnitude = 0;
            else if (v3_InputVector.magnitude < 0.82f)   f_Magnitude = f_Magnitude_Sneak; // 0.82 is the minimum magnitude reachable when the analog stick is pushed in one direction
            else    f_Magnitude = f_Magnitude_Brisk;

            if (b_IsSprinting)   f_Magnitude = f_Magnitude_Sprint;

        }
        #endregion

        #region Update Aim/Fire
        bool b_AllowedToFire_ReticleMagnitude = false;

        // Get the currect Vector of the right analog stick
        Vector2 v2_RightStickVector;
        v2_RightStickVector.x = state.ThumbSticks.Right.X;
        v2_RightStickVector.y = state.ThumbSticks.Right.Y;
        v2_RightStickVector.Normalize();

        // Get the 'magnitude' of the stick being pressed in
        float f_RightStickMagnitude = Vector2.SqrMagnitude(new Vector2(state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y));

        // Pass information in to the UpdateReticle function
        v3_TargetLocation = go_TargetObject.transform.position;
        UpdateReticle(v2_RightStickVector, f_RightStickMagnitude);

        if (f_RightStickMagnitude >= 0.3f) b_AllowedToFire_ReticleMagnitude = true;
        #endregion

        #region Use Ability
        // Throw Rock
        if( state.Buttons.RightShoulder == ButtonState.Pressed && prevState.Buttons.RightShoulder == ButtonState.Released &&
            b_AllowedToFire_ReticleMagnitude )
        {
            float f_StickMagnitude = Vector2.SqrMagnitude(new Vector2(state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y));
            Vector3 v3_ThrowVector = CalculateThrow(f_StickMagnitude);

            ThrowRockAtLocation(v3_ThrowVector);
        }
        #endregion

        // Normalize
        v3_InputVector.Normalize();
        
        // Pass information into PlayerMovement()
        PlayerMovement(v3_InputVector, f_Magnitude);
    }

    public float f_AimingDistance = 5.0f;
    float f_ReticleFadeTimer;
    void UpdateReticle( Vector2 v2_Vector_, float f_Magnitude )
    {
        // Reposition the Reticle's X/Z in comparison to the player's position
        Vector3 v3_ReticlePosition = gameObject.transform.position;
        Vector3 v3_ConvertedVector = new Vector3(v2_Vector_.x, 0, v2_Vector_.y);
        v3_ReticlePosition += v3_ConvertedVector * f_Magnitude * f_AimingDistance;

        // int layer_mask = LayerMask.GetMask("Player", "Enemy");
        int layer_mask = LayerMask.GetMask("Ground");

        // Raycast down from the Reticle's current X/Z position to find ground to apply on to
        RaycastHit hit;
        Physics.Raycast(new Vector3(v3_ReticlePosition.x, v3_ReticlePosition.y + 5, v3_ReticlePosition.z), -transform.up, out hit, 10.0f, layer_mask);

        Vector3 v3_NewPosition = hit.point;
        v3_NewPosition.y += 0.1f;

        // Rotate the reticle to match the surface it hits
        Vector3 v3_FinalRotation = Vector3.ProjectOnPlane(-transform.up + new Vector3(90, 0, 0), hit.normal);

        // Apply final position
        go_TargetObject.transform.position = v3_NewPosition;
        go_TargetObject.transform.eulerAngles = v3_FinalRotation;

        if (f_Magnitude >= 0.4f)
        {
            if(f_ReticleFadeTimer < 1.0f)
            {
                f_ReticleFadeTimer += Time.deltaTime * 10;

                if (f_ReticleFadeTimer > 1.0f) f_ReticleFadeTimer = 1.0f;
            }
        }
        else
        {
            if (f_ReticleFadeTimer > 0)
            {
                if (f_ReticleFadeTimer > 0.5f) f_ReticleFadeTimer = 0.5f;

                f_ReticleFadeTimer -= Time.deltaTime;

                if (f_ReticleFadeTimer < 0.0f) f_ReticleFadeTimer = 0.0f;
            }
        }

        Color clr_Fade = go_TargetObject.GetComponent<MeshRenderer>().material.color;
        clr_Fade.a = f_ReticleFadeTimer;
        go_TargetObject.GetComponent<MeshRenderer>().material.color = clr_Fade;
    }

    void Input_Keyboard()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Vector3 v3_ThrowVector = CalculateThrow();

            //ThrowRockAtLocation(v3_ThrowVector);
        }

        #region Movement
        // Create new temporary Vector3 to apply keyboard input
        Vector3 v3_InputVector = new Vector3();

        // Accept keyboard input, apply into Vector3
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) v3_InputVector.z = 1;
        else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) v3_InputVector.z = -1;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) v3_InputVector.x = -1;
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) v3_InputVector.x = 1;
        #endregion

        #region Sprinting
        float f_Magnitude = 0f;

        #region Shift Key
        if (!b_IsSprinting)
        {
            if (Input.GetKeyDown( KeyCode.LeftShift )) b_IsSprinting = true;
        }
        else
        {
            if (v3_InputVector.magnitude < 0.1f) b_IsSprinting = false;
        }
        #endregion

        #region Double Tap Direction

        // Reference Variable
        float f_TIME_ALLOWANCE = 0.4f;

        // Decrement the timer if it's currently 'active'
        if( f_DoubleTapForSprintTimer > 0.0f)
        {
            // Decrement
            f_DoubleTapForSprintTimer -= Time.deltaTime;

            // Clamp
            if (f_DoubleTapForSprintTimer < 0.0f) f_DoubleTapForSprintTimer = 0.0f;
        }

        // If the player pressed a button, evaluate if we BEGIN sprinting
        if( Input.GetKeyDown( KeyCode.W ) ||
            Input.GetKeyDown( KeyCode.S ) ||
            Input.GetKeyDown( KeyCode.A ) ||
            Input.GetKeyDown( KeyCode.D ))
        {
            // First press
            if( f_DoubleTapForSprintTimer == 0.0f )
            {
                // Set timer to the Alloted Time to wait
                f_DoubleTapForSprintTimer = f_TIME_ALLOWANCE;

                v3_PriorVector = v3_InputVector;
            }
            else
            {
                // This is the second (or up) time we pressed a movement button within the time limit. Sprint.
                b_IsSprinting = true;
            }
        }

        // If we're already sprinting and are holding a directional key down, keep the sprint timer up. Allows walking into sprinting.
        if(b_IsSprinting)
        {
            if ( Input.GetKey(KeyCode.W) ||
                 Input.GetKey(KeyCode.S) ||
                 Input.GetKey(KeyCode.A) ||
                 Input.GetKey(KeyCode.D))
            {
                f_DoubleTapForSprintTimer = f_TIME_ALLOWANCE;
            }
        }

        // print("Sprinting: " + b_IsSprinting + ", Timer: " + f_DoubleTapForSprintTimer);
        #endregion

        // If the player speed isn't 0, apply preset speeds
        if (v3_InputVector.magnitude != float.Epsilon)
        {
            if (v3_InputVector.magnitude < 0.15f) f_Magnitude = 0;
            else if (v3_InputVector.magnitude < 0.5f) f_Magnitude = f_Magnitude_Sneak;
            else f_Magnitude = f_Magnitude_Brisk;

            if (b_IsSprinting) f_Magnitude = f_Magnitude_Sprint;
        }
        #endregion

        // Normalize
        v3_InputVector.Normalize();

        // Pass information into PlayerMovement()
        PlayerMovement(v3_InputVector, f_Magnitude);

        v3_PriorVector = v3_InputVector;
    }

    void UpdateCameraPosition()
    {
        #region Camera On Player
        if (cameraState == Enum_CameraState.OnPlayer)
        {
            /*
            // Set default parameters
            go_Camera.transform.rotation = go_Camera_DefaultPos.transform.rotation;
            go_Camera.transform.position = go_Camera_DefaultPos.transform.position;
            */
        }
        #endregion

        #region Camera On Temporary Location
        else if (cameraState == Enum_CameraState.OnTempPoint)
        {
            /*
            // Set temporary parameters
            if(go_Camera_TempPos != null)
            {
                go_Camera.transform.rotation = go_Camera_TempPos.transform.rotation;
                go_Camera.transform.position = go_Camera_TempPos.transform.position;
            }
            else
            {
                // Reset to player's position & set camera state
            }
            */
        }
        #endregion

        #region Camera Lerp FROM player TO temp location
        else if (cameraState == Enum_CameraState.Lerp_FromPlayer || cameraState == Enum_CameraState.Lerp_ToPlayer)
        {
            // Camera timer increments as it travels to the temp location
            if(cameraState == Enum_CameraState.Lerp_FromPlayer)
            {
                cameraLerpTime_Curr += Time.deltaTime;

                if (cameraLerpTime_Curr > cameraLerpTime)
                {
                    cameraLerpTime_Curr = cameraLerpTime;
                }
            }
            // Camera timer decrements as it travels back to the player
            else
            {
                cameraLerpTime_Curr -= Time.deltaTime;

                if(cameraLerpTime_Curr <= 0)
                {
                    cameraLerpTime_Curr = 0;
                }
            }

            // Lerp calculations
            float perc = cameraLerpTime_Curr / cameraLerpTime;

            if(go_Camera_TempPos != null && go_Camera_DefaultPos != null)
            {
                Vector3 v3_Vector = go_Camera_TempPos.transform.position - go_Camera_DefaultPos.transform.position;
                Vector3 v3_Rotation = go_Camera_TempPos.transform.eulerAngles - go_Camera_DefaultPos.transform.eulerAngles;

                go_Camera.transform.position = go_Camera_DefaultPos.transform.position + (v3_Vector * perc);
                go_Camera.transform.eulerAngles = go_Camera_DefaultPos.transform.eulerAngles + (v3_Rotation * perc);
            }
            
        }
            #endregion
    }

    public void SetCameraPosition( GameObject go_CameraPos_ = null )
    {
        if(go_CameraPos_ == null)
        {
            cameraState = Enum_CameraState.Lerp_ToPlayer;
        }
        else
        {
            go_Camera_TempPos = go_CameraPos_;
            cameraState = Enum_CameraState.Lerp_FromPlayer;
        }
    }

    RaycastHit EvaluateGroundVector()
    {
        RaycastHit hit;

        Physics.Raycast(go_SlopeRaycast.transform.position, -transform.up, out hit);

        return hit;
    }

    Vector3 CalculateThrow( float f_AnalogStickMagnitude_ )
    {
        f_Gravity = Physics.gravity.magnitude;
        float f_Angle = (f_FiringAngle * f_AnalogStickMagnitude_) * Mathf.Deg2Rad;

        Vector3 v3_HorizontalTarget = new Vector3(v3_TargetLocation.x, 0, v3_TargetLocation.z);
        Vector3 v3_HorizontalPosition = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);

        float f_Distance = Vector3.Distance(v3_HorizontalTarget, v3_HorizontalPosition);
        float f_yOffset = gameObject.transform.position.y - v3_TargetLocation.y;

        float f_InitialVelocity = (1 / Mathf.Cos(f_Angle)) * Mathf.Sqrt((0.5f * f_Gravity * Mathf.Pow(f_Distance, 2)) / (f_Distance * Mathf.Tan(f_Angle) + f_yOffset));

        Vector3 v3_Velocity = new Vector3(0, f_InitialVelocity * Mathf.Sin(f_Angle), f_InitialVelocity * Mathf.Cos(f_Angle));

        float f_AngleBetweenObjects = Vector3.Angle(Vector3.forward, v3_HorizontalTarget - v3_HorizontalPosition);

        Vector3 v3_FinalVelocity = Quaternion.AngleAxis(f_AngleBetweenObjects, Vector3.up) * v3_Velocity;
       

        if(v3_HorizontalTarget.x < v3_HorizontalPosition.x)
        {
            v3_FinalVelocity.x *= -1;
        }

        return v3_FinalVelocity;
    }

    void ThrowRockAtLocation(Vector3 v3_Velocity_)
    {
        GameObject go_Rock = (GameObject)Instantiate(prefab_Rock, go_FireLocation.transform.position, gameObject.transform.rotation);

        if(v3_Velocity_ != null)
        {
            go_Rock.GetComponent<Rigidbody>().velocity = v3_Velocity_;
        }
    }
}
