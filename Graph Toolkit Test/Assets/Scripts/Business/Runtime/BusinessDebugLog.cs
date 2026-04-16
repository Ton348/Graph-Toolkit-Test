using UnityEngine;

public static class BusinessDebugLog
{
	public static bool enabled;

	public static void Log(string message)
	{
		if (!enabled)
		{
			return;
		}

		Debug.Log(message);
	}

	public static void Warn(string message)
	{
		Debug.LogWarning(message);
	}

	public static void Error(string message)
	{
		Debug.LogError(message);
	}
}