using UnityEngine;

[System.Serializable]
public sealed class ReferenceStatusStage
{
    [Header("Stage")]
    [Min(0f)]
    public float minStatus;

    [Header("Display")]
    public string statusName = "Бездомный";

    public Color statusColor = Color.white;

    [Header("Appearance")]
    public GameObject appearance;
}