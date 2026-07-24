using System;
using System.Collections.Generic;

public sealed class ActionValueTimeline
{
    private const float MinimumSpeed = 0.0001f;

    private readonly List<Entry> entries = new();
    private float actionValueConstant;
    private int nextRegistrationOrder;

    public void Initialize(
        IReadOnlyList<BattleUnit> participants,
        float valueConstant)
    {
        if (participants == null)
            throw new ArgumentNullException(nameof(participants));

        if (valueConstant <= 0f)
            throw new ArgumentOutOfRangeException(nameof(valueConstant));

        actionValueConstant = valueConstant;
        entries.Clear();
        nextRegistrationOrder = 0;

        for (int i = 0; i < participants.Count; i++)
        {
            BattleUnit unit = participants[i];

            if (unit == null)
                continue;

            AddUnit(unit);
        }
    }

    public void AddUnit(
        BattleUnit unit,
        float initialActionRatio = 1f)
    {
        if (unit == null)
            throw new ArgumentNullException(nameof(unit));

        if (initialActionRatio < 0f)
            throw new ArgumentOutOfRangeException(
                nameof(initialActionRatio));

        if (entries.Exists(entry => entry.Unit == unit))
            throw new InvalidOperationException(
                "이미 타임라인에 등록된 유닛입니다.");

        float speed = GetValidSpeed(unit);
        float initialActionValue =
            CalculateBaseActionValue(speed) * initialActionRatio;

        entries.Add(new Entry(
            unit,
            nextRegistrationOrder++,
            speed,
            initialActionValue));
    }

    public bool TryGetNext(
        out BattleUnit unit,
        out float remainingActionValue)
    {
        RefreshSpeeds();

        Entry next = null;

        foreach (Entry entry in entries)
        {
            if (!entry.Unit.CanAct)
                continue;

            if (next == null || IsEarlier(entry, next))
                next = entry;
        }

        if (next == null)
        {
            unit = null;
            remainingActionValue = 0f;
            return false;
        }

        unit = next.Unit;
        remainingActionValue = next.RemainingActionValue;
        return true;
    }

    public void Advance(float actionValue)
    {
        if (actionValue < 0f)
            throw new ArgumentOutOfRangeException(nameof(actionValue));

        foreach (Entry entry in entries)
        {
            if (!entry.Unit.CanAct)
                continue;

            entry.RemainingActionValue = Math.Max(
                0f,
                entry.RemainingActionValue - actionValue);
        }
    }

    public void CompleteTurn(BattleUnit unit)
    {
        Entry entry = FindEntry(unit);
        float speed = GetValidSpeed(unit);

        entry.LastSpeed = speed;
        entry.RemainingActionValue += CalculateBaseActionValue(speed);
    }

    public void AdvanceAction(BattleUnit unit, float ratio)
    {
        if (ratio < 0f)
            throw new ArgumentOutOfRangeException(nameof(ratio));

        Entry entry = FindEntry(unit);
        RefreshSpeed(entry);
        entry.RemainingActionValue = Math.Max(
            0f,
            entry.RemainingActionValue -
            CalculateBaseActionValue(entry.LastSpeed) * ratio);
    }

    public void DelayAction(BattleUnit unit, float ratio)
    {
        if (ratio < 0f)
            throw new ArgumentOutOfRangeException(nameof(ratio));

        Entry entry = FindEntry(unit);
        RefreshSpeed(entry);
        entry.RemainingActionValue +=
            CalculateBaseActionValue(entry.LastSpeed) * ratio;
    }

    public float GetRemainingActionValue(BattleUnit unit)
    {
        Entry entry = FindEntry(unit);
        RefreshSpeed(entry);
        return entry.RemainingActionValue;
    }

    private float CalculateBaseActionValue(float speed)
    {
        return actionValueConstant / speed;
    }

    private void RefreshSpeeds()
    {
        foreach (Entry entry in entries)
        {
            if (entry.Unit.CanAct)
                RefreshSpeed(entry);
        }
    }

    private void RefreshSpeed(Entry entry)
    {
        float currentSpeed = GetValidSpeed(entry.Unit);

        if (Math.Abs(currentSpeed - entry.LastSpeed) < 0.0001f)
            return;

        entry.RemainingActionValue *=
            entry.LastSpeed / currentSpeed;
        entry.LastSpeed = currentSpeed;
    }

    private Entry FindEntry(BattleUnit unit)
    {
        if (unit == null)
            throw new ArgumentNullException(nameof(unit));

        Entry entry = entries.Find(candidate => candidate.Unit == unit);

        if (entry == null)
            throw new InvalidOperationException("타임라인에 등록되지 않은 유닛입니다.");

        return entry;
    }

    private static float GetValidSpeed(BattleUnit unit)
    {
        return Math.Max(MinimumSpeed, unit.Speed);
    }

    private static bool IsEarlier(Entry candidate, Entry current)
    {
        int actionValueComparison = candidate.RemainingActionValue
            .CompareTo(current.RemainingActionValue);

        return actionValueComparison < 0 ||
               actionValueComparison == 0 &&
               candidate.RegistrationOrder < current.RegistrationOrder;
    }

    private sealed class Entry
    {
        public BattleUnit Unit { get; }
        public int RegistrationOrder { get; }
        public float LastSpeed { get; set; }
        public float RemainingActionValue { get; set; }

        public Entry(
            BattleUnit unit,
            int registrationOrder,
            float lastSpeed,
            float remainingActionValue)
        {
            Unit = unit;
            RegistrationOrder = registrationOrder;
            LastSpeed = lastSpeed;
            RemainingActionValue = remainingActionValue;
        }
    }
}
