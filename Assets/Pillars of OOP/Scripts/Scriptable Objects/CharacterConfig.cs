using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configurations/Character Config")]
public class CharacterConfig : ScriptableObject
{
    // Any custom values for character configuration

    // I didn't get the animation playback speed adustments to work, but they're here as a reminder to try maybe again later
    public float walkPlaybackSpeed = 1.0f;
    public float runPlaybackSpeed = 1.0f;

    // Character in-game identification
    public string characterName = "Unnamed";
    public string description = "No description";
    public float attackDistance = 2f;
    public GameObject attackEffect; // Optional attack effect (spell for mage, none for sword fighter)
    public float effectDuration = 2f; // Duration of the effect (used if autoDestroy is false)
    public bool autoDestroy = true; // If true, automatically destroy effect at the end
    public int armour = 100;

}