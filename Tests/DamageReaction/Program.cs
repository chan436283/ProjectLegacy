using System;

static class Program
{
    static void Check(string name, float amount, BattleDamageSource source, bool defending, bool dead, bool expected)
    {
        var damage = new BattleDamageInfo(amount, source, defending, dead);
        if (damage.ShouldPlayHitReaction != expected) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }

    static void Main()
    {
        Check("surviving direct hit", 10, BattleDamageSource.DirectAttack, false, false, true);
        Check("defending", 5, BattleDamageSource.DirectAttack, true, false, false);
        Check("lethal hit", 10, BattleDamageSource.DirectAttack, false, true, false);
        Check("lethal hit while defending", 10, BattleDamageSource.DirectAttack, true, true, false);
        Check("zero applied damage", 0, BattleDamageSource.DirectAttack, false, false, false);
        Check("status damage", 10, BattleDamageSource.StatusEffect, false, false, false);
        Check("other HP loss", 10, BattleDamageSource.Other, false, false, false);
    }
}
