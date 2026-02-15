using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TurnBasedSystem))]
public class TurnBasedSystemEditor : Editor
{
    bool showFields = true;

    // Simple, direct descriptions for the serialized fields shown in the TurnBasedSystem inspector.
    // Each entry: field name -> what to assign there and what it's used for.
    readonly (string title, string desc)[] fieldDescriptions = new (string, string)[]
    {
        ("player", "Player prefab (must include a PlayerUnit component). Used to spawn the player at the start of the fight."),
        ("enemy1", "Prefab for Enemy 1 (include PlayerUnit). Used to spawn the first enemy."),
        ("enemy2", "Prefab for Enemy 2 (include PlayerUnit). Used to spawn the second enemy."),
        ("enemy3", "Prefab for Enemy 3 (include PlayerUnit). Used to spawn the third enemy."),
        ("playerPoint", "Transform where the player prefab will be instantiated."),
        ("enemyPoint1", "Transform where Enemy 1 will be instantiated."),
        ("enemyPoint2", "Transform where Enemy 2 will be instantiated."),
        ("enemyPoint3", "Transform where Enemy 3 will be instantiated."),
        ("dialogue", "TextMeshProUGUI used to show messages (dodges, hits, victory/defeat)."),
        ("playerHUD", "Player HUD reference. Used to display and update the player's HP."),
        ("enemy1HUD", "HUD for Enemy 1. Shows that enemy's HP and UI updates."),
        ("enemy2HUD", "HUD for Enemy 2. Shows that enemy's HP and UI updates."),
        ("enemy3HUD", "HUD for Enemy 3. Shows that enemy's HP and UI updates."),
        ("deck", "List of Card objects that make up the player's deck. Populate here or at runtime."),
        ("hand", "Array of hand slots (3). Each slot holds a Card drawn from the deck."),
        ("cardButtons", "Optional UI Buttons for each hand slot. Assign to let players click to play cards."),
        ("cardLabels", "Optional Text labels for each hand slot to show card name / damage."),
        ("maxFocusPoints", "Maximum FreeShot bullets (focus) the player can hold."),
        ("focusPoints", "Current FreeShot bullets available. Consumed when using FreeShot."),
        ("focusLabel", "Optional UI text to display current bullets / focus to the player."),
        ("freeShotDamage", "Fallback damage value applied for FreeShot if playerUnit.damage is not set."),
        ("freeShotToggleKey", "Key to toggle FreeShot mode during the player's turn (default: E)."),
        ("enemyDodgeConfigs", "Per-enemy dodge settings (window seconds and success chance). Index matches enemy slots (1..3).")
    };

    public override void OnInspectorGUI()
    {
        // Draw default inspector so fields remain editable
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inspector Field Descriptions", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Simple guidance: what to assign to each field and what it is used for.", MessageType.Info);
        EditorGUILayout.Space();

        showFields = EditorGUILayout.Foldout(showFields, "Field Descriptions");
        if (showFields)
        {
            foreach (var item in fieldDescriptions)
            {
                DrawFieldDescription(item.title, item.desc);
            }
        }
    }

    void DrawFieldDescription(string fieldName, string description)
    {
        // Show the field name and help text
        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(description, MessageType.None);
    }
}
#endif