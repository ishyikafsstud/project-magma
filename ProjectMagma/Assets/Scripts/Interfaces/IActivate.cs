using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Interface for objects that are activated by triggers, like by buttons that are
/// pressed by player attacks and activate the activatable objects connected to it.
/// </summary>
public interface IActivate
{
    void Activate();
}
