namespace DefluoLib;

using Godot;
using FMOD;

/// <summary>
/// Static class used to make calls to raw FMOD API functions.
/// Automatically prints possible errors to console
/// </summary>
public static class FMODCaller
{
    /// <summary>
    /// Checks the result of an FMOD API function
    /// </summary>
    /// <param name="result">The <see href="https://www.fmod.com/docs/2.03/api/core-api-common.html#fmod_result">FMOD_RESULT</see> returned from an API call</param>
    /// <returns>Whether or not result was OK</returns>
    public static bool CheckResult(RESULT result)
    {
        if (result == RESULT.OK)
            return true;

        GD.PushError(FMOD.Error.String(result));
        return false;
    }
}
