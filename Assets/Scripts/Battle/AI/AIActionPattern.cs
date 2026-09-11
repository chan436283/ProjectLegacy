using System;

// Each unit owns its cursor; shared assets contain configuration only.
public sealed class AIActionPattern
{
    public int CurrentActionIndex { get; private set; } = -1;
    public AIActionSet ActionSet { get; private set; }

    public void SetActionSet(AIActionSet actionSet)
    {
        ActionSet = actionSet;
        CurrentActionIndex = -1;
    }

    // Selection is read-only. Commit only when execution is accepted.
    public bool TrySelect(Func<AIActionEntry, bool> canExecute, out int index)
    {
        index = -1;
        if (ActionSet == null || ActionSet.Actions.Count == 0) return false;
        int count = ActionSet.Actions.Count;
        for (int offset = 1; offset <= count; offset++)
        {
            int candidate = (CurrentActionIndex + offset) % count;
            AIActionEntry entry = ActionSet.Actions[candidate];
            if (entry == null || !canExecute(entry)) continue;
            index = candidate;
            return true;
        }
        return false;
    }

    public void Commit(int index)
    {
        if (ActionSet == null || index < 0 || index >= ActionSet.Actions.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        CurrentActionIndex = index;
    }
}
