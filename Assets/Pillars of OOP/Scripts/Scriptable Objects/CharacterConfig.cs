using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configurations/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    // Any custom values for character configuration
    public float walkPlaybackSpeed = 1.0f;
    public float runPlaybackSpeed = 1.0f;
    public string characterName = "Unnamed";
    public string description = "No description";
}