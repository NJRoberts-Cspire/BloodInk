namespace BloodInk.Interaction;

/// <summary>
/// Shared helpers for puzzle / interaction UI text.
/// </summary>
public static class PuzzleUtils
{
    /// <summary>
    /// Convert a snake_case ID to a human-readable name.
    /// E.g. "servant_key" → "Servant Key", "master_key" → "Master Key".
    /// </summary>
    public static string HumanizeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;
        var parts = id.Split('_');
        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Length > 0)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i][1..];
        return string.Join(' ', parts);
    }
}
