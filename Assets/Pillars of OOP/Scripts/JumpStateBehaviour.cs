using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStateBehaviour : StateMachineBehaviour {
    private CharacterController characterController;
    private float groundDelay = 0.1f; // Delay of 0.1 seconds to ensure proper landing
    private float groundedTimer = 0.0f;

    // Called when the state is first entered
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        characterController = animator.GetComponent<CharacterController>();
        groundedTimer = 0.0f; // Reset the timer when entering the jump state
    }

    // Called on each Update frame while the character is in this state
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        if (characterController != null && characterController.isGrounded) {
            groundedTimer += Time.deltaTime; // Increment the timer if grounded

            // If grounded for more than the specified delay, set isGrounded to true
            if (groundedTimer >= groundDelay) {
                animator.SetBool("isGrounded", true);
            }
        } else {
            // Reset the timer if not grounded
            groundedTimer = 0.0f;
        }
    }

    // Optional: Called when exiting the state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Reset jump trigger or any other parameters as needed
        animator.ResetTrigger("jumpTrigger");
    }
}