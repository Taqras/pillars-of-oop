using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Transform player; // Player's transform reference

    private bool followPlayer = false;  // Is the camera ready to follow the player?
    bool playerLock = false;            // Does the player want the camera to follow?
    // public Vector3 followOffset = new Vector3(0, 2.5f, -3);
    public Vector3 followOffset = new Vector3(0, 1.5f, -7f);
    public Vector3 combatFollowOffset = new Vector3(0, 1.5f, -7f);
    public float followPitchOffset = 15f; // Tilt the camera down slightly by default (in degrees)
    public float followSpeed = 10f;
    public float followRotationSpeed = 10f;
    private Quaternion combatStartRotation;     // Store the starting rotation for combat mode
    private Vector3 lastPlayerPosition; // Track player's position to calculate movement
    private bool isFreeLook = false;             // Indicates if free look is active
    public float freeLookSpeed = 0.001f;           // Speed for rotating during free look
    private float freeLookYaw = 0f;              // To store yaw rotation for free look
    private float freeLookPitch = 0f;            // Store pitch value for free look rotation
    private Vector3 freeLookOffset = Vector3.zero;
    private bool isTransitioningFromFreeLook = false; // Flag to indicate transition from free-look
    private Vector3 transitionStartPosition;          // Where the camera starts transitioning from
    private Quaternion transitionStartRotation;       // The initial rotation when transitioning
    private bool isTransitioning = false;           // Is the camera in transit to a new position?
    private Vector3 transitionTargetPosition;       // Where the camera is transitioning to
    private Quaternion transitionTargetRotation;    // The rotation the camera should have at the target position
    float transitionSpeed = 2f;                     // Adjust scaling for desired speed
    float transitionRotationSpeed = 360f;            // Adjust scaling for desired speed
    private int transitionStage = 0;


    private void Start() {
        isFreeLook = false;
        freeLookOffset = Vector3.zero;
    }

    private void Update() {

        if (followPlayer && player != null) {

            playerLock = player.GetComponent<Player>().CameraLock;

            if (!isFreeLook) {
                Vector3 currentEulerAngles = transform.eulerAngles;
                freeLookYaw = currentEulerAngles.y;  // Set initial yaw to the current Y rotation
                freeLookPitch = currentEulerAngles.x;  // Set initial pitch to the current X rotation
            }

            // Right-click and drag for free look
            if (Input.GetMouseButton(1) && !Input.GetMouseButton(0)) {

                isFreeLook = true;

                float horizontal = Input.GetAxis("Mouse X") * freeLookSpeed * Time.deltaTime;
                float vertical = -Input.GetAxis("Mouse Y") * freeLookSpeed * Time.deltaTime;

                freeLookYaw += horizontal;
                freeLookPitch = Mathf.Clamp(freeLookPitch + vertical, -30f, 30f); // Limit pitch to prevent extreme angles

                // Calculate rotation using yaw and pitch
                Quaternion rotation = Quaternion.Euler(freeLookPitch, freeLookYaw, 0f);
                freeLookOffset = rotation * followOffset;

            } else if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
                // Both buttons pressed: Freeze free-look and continue turning
                // isFreeLook = false;  // Disable free-look when both buttons are pressed
            } else if (Input.GetMouseButtonUp(1)) {
                // When right mouse button is released
                isFreeLook = false;
                // Start a smooth transition back to the follow state
                freeLookYaw = 0f;  // Reset yaw for smoothness
                freeLookPitch = followPitchOffset;  // Reset pitch to the default tilt
                // Initiate transition
                isTransitioningFromFreeLook = true;
                transitionStartPosition = transform.position;  // Capture current position
                transitionStartRotation = transform.rotation;  // Capture current rotation

            }
        }
    }

    private void LateUpdate() {

        if (isTransitioning && player != null) {
            SmoothEnterPosition();
        } else if (followPlayer && player != null) {

            bool hasCombatFocus = player.GetComponent<Player>().IsCombatReady;

            if (isFreeLook) {
                
                // Set the camera position based on the calculated offset
                Vector3 desiredPosition = player.position + freeLookOffset;
                
                // Use Lerp for smooth position updates and Slerp for smooth rotation
                transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
                transform.LookAt(player.position + Vector3.up * 1.5f);

            } else if (isTransitioningFromFreeLook) {
                // Smooth transition from free-look to follow

                Vector3 targetPosition = player.position + player.rotation * followOffset;
                Quaternion targetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);

                // Lerp position and Slerp rotation with a fixed interpolation factor for smoothness
                float interpolationFactor = 0.1f;  // Increase or decrease this to control transition smoothness
                transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationFactor);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolationFactor);

                // Check if the transition is close to completion
                if (Vector3.Distance(transform.position, targetPosition) < 0.05f && 
                    Quaternion.Angle(transform.rotation, targetRotation) < 0.05f) {
                    isTransitioningFromFreeLook = false; // End the transition
                    // Reset yaw and pitch after transition completes
                    freeLookYaw = 0f;
                    freeLookPitch = followPitchOffset; // Reset pitch to default tilt value
                    freeLookOffset = Vector3.zero;
                }
            } else {

                if (playerLock) {

                    // Player lock mode: follow player’s position and direction
                    Vector3 targetPosition = player.position + player.rotation * followOffset;
                    Quaternion targetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);

                    // Smoothly update position and rotation
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);

                    // Save current rotation for combat mode starting view
                    combatStartRotation = transform.rotation;
                    lastPlayerPosition = player.position; // Track player position for movement calculation
                }


                if (hasCombatFocus) {

                    // Step 1: Rotation adjustments to keep the player in the same spot within the view
                    Vector3 playerMovementOffset = player.position - lastPlayerPosition;

                    Vector3 localOffset = transform.InverseTransformDirection(playerMovementOffset);
                    float horizontalOffset = localOffset.x;
                    float forwardOffset = localOffset.z;

                    // Step 1: Adjust rotation only if there’s significant lateral (horizontal) movement
                    if (Mathf.Abs(horizontalOffset) > 0.01f) { // Adjust threshold as needed
                        float horizontalRotationAdjustment = horizontalOffset * followSpeed * Time.deltaTime;
                        combatStartRotation *= Quaternion.Euler(0, horizontalRotationAdjustment, 0);
                    }

                    // Step 2: Directly update the position based on combatFollowOffset
                    // Vector3 targetPosition = player.position + combatStartRotation * combatFollowOffset;
                    if (Mathf.Abs(forwardOffset) > 0.01f) { // Adjust threshold as needed
                        Vector3 targetPosition = player.position + combatStartRotation * combatFollowOffset;
                        Debug.Log($"Combat Target Position: {targetPosition} | Player Position: {player.position} | Combat Offset: {combatFollowOffset} | Normal Offset: {followOffset}");

                        transform.position = targetPosition; // Directly set the position for combat mode
                    }

                    // Apply the fixed rotation to maintain orientation
                    transform.rotation = combatStartRotation;

                    // Update the last known player position
                    lastPlayerPosition = player.position;

                    // // Step 2: Adjust position if there’s a forward (distance) offset
                    // if (Mathf.Abs(forwardOffset) > 0.01f) { // Adjust threshold as needed
                    //     Vector3 targetPosition = player.position + combatStartRotation * combatFollowOffset;
                    //     transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                    // }

                    // // Apply the fixed rotation to maintain orientation
                    // transform.rotation = combatStartRotation;

                    // // Update the last known player position
                    // lastPlayerPosition = player.position;

/* 
                    float horizontalRotationAdjustment = localOffset.x * followSpeed * Time.deltaTime;
                    float verticalRotationAdjustment = -localOffset.z * followSpeed * Time.deltaTime;

                    // Adjust rotation to maintain player position in the camera's frame
                    combatStartRotation *= Quaternion.Euler(verticalRotationAdjustment, horizontalRotationAdjustment, 0);

                    transform.rotation = combatStartRotation;

                    // Step 2: Maintain the constant distance from the player
                    Vector3 targetPosition = player.position + combatStartRotation * followOffset;
                    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

                    lastPlayerPosition = player.position;
 */

/* 
                    // In combat focus mode, the camera always looks at the player
                    // Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);

                    // Rotate smoothly to keep player centered in view without moving position
                    // not at all working --> transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);

                    // Calculate player movement vector in local space
                    Vector3 playerMovement = transform.InverseTransformDirection(player.position - lastPlayerPosition);

                    // Apply small incremental adjustments to combatStartRotation based on player movement
                    if (playerMovement.x != 0 || playerMovement.z != 0) {
                        // Horizontal and vertical adjustments based on movement vector
                        float horizontalAdjustment = playerMovement.x * followSpeed * Time.deltaTime;
                        float verticalAdjustment = playerMovement.z * followSpeed * Time.deltaTime;

                        // Modify combatStartRotation without recalculating direction
                        combatStartRotation *= Quaternion.Euler(-verticalAdjustment, horizontalAdjustment, 0);
                        transform.rotation = combatStartRotation;
                    }

                    // Update lastPlayerPosition for the next frame
                    lastPlayerPosition = player.position;
 */
                }

            }
        }
    }


    public void AttachToPlayer(Transform playerTransform) {
        player = playerTransform;   // Assign the player reference
        isTransitioning = true;     // Start transitioning to the player
        followPlayer = false;       // Can't follow until after we have transitioned
        transitionStage = 0;        // Reset the transition stages
        // Calculate the transition points
        // transitionTargetPosition = player.position + followOffset;
        transitionTargetPosition = player.position + player.rotation * followOffset;
        transitionTargetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);

    }

    private void SmoothEnterPosition() {

        // Smoothly rotate to align with the player's rotation and add a slight downward tilt
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, transitionTargetRotation, transitionRotationSpeed * Time.deltaTime * 2);
        // transform.position = Vector3.Lerp(transform.position, transitionTargetPosition, transitionSpeed);
        transform.position = Vector3.Lerp(transform.position, transitionTargetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, transitionTargetRotation, transitionSpeed * Time.deltaTime);

        // if (Quaternion.Angle(transform.rotation, transitionTargetRotation) < 1f && Vector3.Distance(transform.position, transitionTargetPosition) < 0.2) {
        if (Quaternion.Angle(transform.rotation, transitionTargetRotation) < 0.1f && Vector3.Distance(transform.position, transitionTargetPosition) < 0.1f) {
            isTransitioning = false;
            followPlayer = true; // Start following the player after transition is complete
            Debug.Log($"End position before follow: {transform.position}");
        }
    }

    private void SmoothTransitionToPosition() {
        
        Quaternion lookRotation;

        switch (transitionStage) {
        // switch (transitionStage) {
            case 0:
                lookRotation = Quaternion.LookRotation((transitionTargetPosition - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, transitionRotationSpeed * Time.deltaTime);
               
                if (Quaternion.Angle(transform.rotation, lookRotation) < 20) {
                    transitionStage++;
                }

                break;

            case 1:
                lookRotation = Quaternion.LookRotation((transitionTargetPosition - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, transitionRotationSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, transitionTargetPosition, transitionSpeed);

                if (Vector3.Distance(transform.position, transitionTargetPosition) < 5f) {
                    transitionStage++;
                }

                break;

            case 2:
                // Smoothly rotate to align with the player's rotation and add a slight downward tilt
                transform.rotation = Quaternion.RotateTowards(transform.rotation, transitionTargetRotation, transitionRotationSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, transitionTargetPosition, transitionSpeed);

                if (Quaternion.Angle(transform.rotation, transitionTargetRotation) < 1f && Vector3.Distance(transform.position, transitionTargetPosition) < 0.3) {
                    isTransitioning = false;
                    followPlayer = true; // Start following the player after transition is complete
                }
                break;
        }
    }

    public void DisableFollowing() {
        followPlayer = false; // Disable following
    }

}
