using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {

    private CharacterController controller;
    private Animator animator;
    // Implement health in the derived class
    public override int Health { get; protected set; }
    private int experience = 0;
    public int Experience {
        get { return experience; }
        private set { experience = value; }
    }

    private void Start() {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        SetHealth(100);  // Initial health for the player

        // Optionally override speed programmatically if needed
        // Speed = 3f;
    }

    private void Update() {
        Move();  // Call the overridden Move method from the base class
    }
    
    public void GainExperience(int amount) {
        Experience += amount;
    }

    public override void Move() {

        // Specific movement for Player
        // Player movement logic using input
        float horizontal = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");      // W/S or Up/Down arrows
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // Calculate movement speed (e.g., walking or running)
        float inputMagnitude = direction.magnitude;

        // Set the Speed parameter in the Animator to trigger animations
        animator.SetFloat("Speed", inputMagnitude);

        if (inputMagnitude >= 0.1f) {

            // Rotate the player towards the movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            // Apply movement to the CharacterController to move the player forward
            controller.Move(direction * Speed * Time.deltaTime);  // Use Move() for full control of movement
        }

    }
    
    public override void Interact() {
        // Interaction logic for Player
    }
}
