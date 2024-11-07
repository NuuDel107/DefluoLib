using Godot;
using FMOD;
using System;
using FMOD.Studio;

namespace DefluoLib;

/// <summary>
/// Static class used to make calls to raw FMOD API functions.
/// Automatically prints possible errors to console
/// </summary>
public static class FMODCaller
{
    public static bool CheckResult(RESULT result)
    {
        if (result == RESULT.OK)
            return true;

        GD.PushError(FMOD.Error.String(result));
        return false;
    }
}
