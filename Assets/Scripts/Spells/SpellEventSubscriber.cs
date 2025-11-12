using System.Collections;
using UnityEngine;

/// <summary>
/// Utility class that centralises the responsibility of subscribing to spell events,
/// but only once the spell cache has been populated.
/// </summary>
public class SpellEventSubscriber : MonoBehaviour
{

    private static SpellEventSubscriber _instance;

    public static SpellEventSubscriber Instance()
    {
        if(_instance == null)
        {
            var obj = new GameObject("SpellEventSubscriber");
            _instance = obj.AddComponent<SpellEventSubscriber>();
            DontDestroyOnLoad(obj);
        }

        return _instance;
    }

    /// <summary>
    /// Called from other classes that want to subscribe to spell events.
    /// </summary>
    /// <param name="spellWord"> The spell we want to listen to. </param>
    /// <param name="action"> The callback we want to subscribe with. </param>
    public void SubscribeToSpell(SpellWords spellWord, System.Action<SpellArgs> action)
    {
        StartCoroutine(SubscribeToSpellBehaviour(spellWord, action));
    }

    /// <summary>
    /// Unsubscribes from a spell event. Useful in case the active spells change during the session.
    /// </summary>
    /// <param name="spellWord"> The spell we want to stop listening to. </param>
    /// <param name="action"> The callback that's been listening to the event. </param>
    public void UnsubscribeFromSpell(SpellWords spellWord, System.Action<SpellArgs> action)
    {
        Spell spell = SpellSessionCache.GetSpell(spellWord);

        if(spell == null)
        {
            Debug.LogWarning($"Spell {spellWord} not found in cache. Cannot unsubscribe.");
            return;
        }

        spell.cast -= action;
        Debug.Log($"Unsubscribed from {spellWord} spell event.");
    }

    /// <summary>
    /// The main functionality of this utility class. It makes sure that the spell cache is ready before subscribing to the spell event.
    /// </summary>
    private IEnumerator SubscribeToSpellBehaviour(SpellWords spellWord, System.Action<SpellArgs> action)
    {
        yield return new WaitUntil(() => SpellSessionCache.IsSpellCacheReady());
        Spell spell = SpellSessionCache.GetSpell(spellWord);
        
        if(spell == null)
        {
            Debug.LogWarning($"Spell {spellWord} not found in cache. Cannot subscribe.");
            yield break;
        }

        spell.cast += action;
        Debug.Log($"Subscribed to {spellWord} spell event.");
    }

}
