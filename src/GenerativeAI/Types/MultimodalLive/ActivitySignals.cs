namespace GenerativeAI.Types;

/// <summary>
/// Marks the start of user activity.
/// This can only be sent if automatic (i.e. server-side) activity detection is disabled.
/// </summary>
public class ActivityStart
{
}

/// <summary>
/// Marks the end of user activity.
/// This can only be sent if automatic (i.e. server-side) activity detection is disabled.
/// </summary>
public class ActivityEnd
{
}
