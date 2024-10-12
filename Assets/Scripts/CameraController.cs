using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Transform player; // Player's transform reference

    private bool followPlayer = false;
    public Vector3 followOffset = new Vector3(0, 2.5f, -3);
    public float followPitchOffset = 15f; // Tilt the camera down slightly by default (in degrees)
    public float followSpeed = 5f;
    public float followRotationSpeed = 20f;

    private bool isFreeLook = false;     // Indicates if free look is active
    public float freeLookSpeed = 0.1f; // Speed for rotating during free look
    public float freeLookSnapBackSpeed = 5f; // Speed for snapping back to default rotation
    private float freeLookYaw = 0f;              // To store yaw rotation for free look
    private float freeLookPitch = 0f;            // Store pitch value for free look rotation

    private bool isCombatFocused = false;

    private bool isTransitioning = false;           // Is the camera in transit to a new position?
    private Vector3 transitionTargetPosition;       // Where the camera is transitioning to
    private Quaternion transitionTargetRotation;    // The rotation the camera should have at the target position
    float transitionSpeed = 0.02f;                   // Adjust scaling for desired speed
    float transitionRotationSpeed = 20f;           // Adjust scaling for desired speed
    private int transitionStage = 0;


    private void Update() {

        if (isTransitioning && player != null) {
            SmoothTransitionToPosition();
        } else if (followPlayer && player != null) {
            // Always update the position behind the player with the given offset
            Vector3 targetPosition = player.position + player.rotation * followOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followRotationSpeed * Time.deltaTime);

            // Right-click and drag for free look
            if (Input.GetMouseButton(1)) {

                isFreeLook = true;

                float horizontal = Input.GetAxis("Mouse X") * freeLookSpeed * Time.deltaTime;
                float vertical = -Input.GetAxis("Mouse Y") * freeLookSpeed * Time.deltaTime;

                freeLookYaw += horizontal;
                freeLookPitch = Mathf.Clamp(freeLookPitch + vertical, -30f, 30f); // Limit pitch to prevent extreme angles

            } else if (Input.GetMouseButtonUp(1)) {

                // When right mouse button is released
                isFreeLook = false;
            }
        }
    }

    private void LateUpdate() {

        if (followPlayer && player != null) {
            if (isCombatFocused) {
                // In combat focus mode, the camera always looks at the player
                transform.LookAt(player.position + Vector3.up * 1.5f);
            } else if (isFreeLook) {
                // Orbit around the player based on yaw
                Quaternion rotation = Quaternion.Euler(0f, freeLookYaw, 0f);
                Vector3 desiredPosition = player.position + rotation * followOffset;

                transform.position = desiredPosition;
                transform.LookAt(player.position + Vector3.up * 1.5f);
                transform.rotation = Quaternion.Euler(freeLookPitch, transform.eulerAngles.y, 0f);
            } else {
                // Smoothly snap back to align with the player's rotation and add a slight downward tilt
                Quaternion targetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, freeLookSnapBackSpeed * Time.deltaTime);

                // Reset yaw and pitch to ensure smooth snapping back
                freeLookYaw = 0f;
                freeLookPitch = followPitchOffset; // Reset pitch to default tilt value
            }
        }
    }


    public void AttachToPlayer(Transform playerTransform) {
        player = playerTransform;   // Assign the player reference
        isTransitioning = true;     // Start transitioning to the player
        followPlayer = false;       // Can't follow until after we have transitioned
        transitionStage = 0;        // Reset the transition stages
        // Calculate the transition points
        transitionTargetPosition = player.position + followOffset;
        transitionTargetRotation = Quaternion.Euler(followPitchOffset, player.eulerAngles.y, 0f);

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

        public void EnterCombatFocus() {
        isCombatFocused = true;
    }

    public void ExitCombatFocus() {
        isCombatFocused = false;
    }
}
