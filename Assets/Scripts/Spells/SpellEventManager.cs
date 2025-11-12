using Unity.Collections;

/// <summary>
/// A central point for the triggering/casting of spells.
/// </summary>
public struct SpellEventManager
{

    /// <summary>
    /// Finds a spell in the cache, delegates its casting to the main thread.
    /// </summary>
    /// <param name="spellWord"> The SpellWord/Spell we want to cast. </param>
    public static void CastSpell(SpellWords spellWord)
    {
        Spell spell = SpellSessionCache.GetSpell(spellWord);
    }

}
