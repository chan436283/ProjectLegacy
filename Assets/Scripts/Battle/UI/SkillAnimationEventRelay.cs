using System.Collections.Generic;
using UnityEngine;

// Must live on the Animator's GameObject, which may be a child of BattleUnitView.
[DisallowMultipleComponent]
public sealed class SkillAnimationEventRelay : MonoBehaviour
{
    private readonly Dictionary<string, int> signals = new();

    public void SkillSignal(string signal)
    {
        if (string.IsNullOrWhiteSpace(signal)) return;
        signals.TryGetValue(signal, out int count);
        signals[signal] = count + 1;
    }

    public void ClearSignals() => signals.Clear();

    public bool Consume(string signal)
    {
        if (string.IsNullOrWhiteSpace(signal) || !signals.TryGetValue(signal, out int count) || count == 0)
            return false;
        signals[signal] = count - 1;
        return true;
    }
}
