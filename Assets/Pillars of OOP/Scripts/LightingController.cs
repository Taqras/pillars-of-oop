using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{
    public Light directionalLight;        // The main directional light (sunlight)
    public Transform player;              // The player or object whose position will affect the lighting
    
    public Color southernLightColor = Color.white;  // Warm light for the southern region
    public Color northernLightColor = Color.gray;   // Cool light for the northern region
    public float transitionSpeed = 0.1f;  // Speed of transition between light settings

    public Vector2 terrainCenter = new Vector2(250, 500);  // Center north of the terrain (X = 250, Z = 500)
    public float maxZ = 500;                             // Max Z value (northern edge of terrain)
    public float maxX = 500;                             // Max X value (eastern edge of terrain)

    void Update()
    {
        // Get the player's current X and Z positions
        float playerZPosition = player.position.z;
        float playerXPosition = player.position.x;

        // Normalize the player's Z position (closer to north = dimmer, cooler)
        float zFactor = Mathf.Clamp01(playerZPosition / maxZ);  // 0 when Z = 0 (southern edge), 1 when Z = 500 (northern edge)

        // Calculate xFactor: 0 at edges (X = 0 or X = 500), 1 at center (X = 250)
        float xFactor = Mathf.Abs(playerXPosition - maxX / 2) / (maxX / 2);

        // Blend the color and intensity: Coolest in the north-center (X = 250, Z = 500)
        Color targetColor = Color.Lerp(southernLightColor, northernLightColor, (xFactor + zFactor) / 2);
        float targetIntensity = Mathf.Lerp(1.0f, 0.5f, (xFactor + zFactor) / 2);

        // Gradually transition the light to the target color and intensity
        directionalLight.color = Color.Lerp(directionalLight.color, targetColor, transitionSpeed * Time.deltaTime);
        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetIntensity, transitionSpeed * Time.deltaTime);
    }
}
