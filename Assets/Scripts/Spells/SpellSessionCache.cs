using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// A bridge between the SpellWords enum, the Addressables system, and the Spell class. It loads, unloads, manages the spell cache
/// and provides a centralised way to cast them. 
/// </summary>
public static class SpellSessionCache
{

    private static Dictionary<string, Spell> _spells = new();
    private static Dictionary<string, AsyncOperationHandle<Spell>> _handles = new();
    private static bool _isCacheReady = false;

    /// <summary>
    /// Takes a list of spells, loads them into the cache, and sets the cache to ready. Should be used upon session initialisation.
    /// </summary>
    /// <param name="activeSpells"> A list of all spells that could be used in the session. </param>
    public static async void LoadSessionSpells(List<SpellWords> activeSpells)
    {
        foreach(SpellWords spellWord in activeSpells)
        {
            await LoadSpell(spellWord);
        }

        _isCacheReady = true;
    }

    /// <summary>
    /// Takes an individual spell word, looks it up in the Addressables system, and if found, loads it into the cache. 
    /// </summary>
    /// <param name="spellWord"> The SpellWord/Spell which we want to make available. </param>
    /// <returns> A task such that loading progress can be monitored. </returns>
    private static async Task<bool> LoadSpell(SpellWords spellWord)
    {
        string address = spellWord.ToString();

        if(_spells.ContainsKey(address))
        {
            return true;
        }

        var handle = Addressables.LoadAssetAsync<Spell>(address);
        await handle.Task;

        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            _spells[address] = handle.Result;
            _handles[address] = handle;
            Debug.Log($"Loaded spell: {address}");
            return true;
        }
        else
        {
            Debug.LogError($"Failed to load spell: {address}");
            return false;
        }
    }

    /// <summary>
    /// Finds a spell in the cache based on the SpellArgs and returns the corresponding SpellArgs in the required type.
    /// Used by the spell behaviour classes to get their own, specific type of SpellArgs.
    /// </summary>
    /// <typeparam name="T"> The specified args type that inherits from SpellArgs. </typeparam>
    /// <param name="args"> The generic SpellArgs object of the Spell in question. </param>
    /// <returns> The args object if the Spell in the request specific type. </returns>
    public static T GetSpellArgs<T>(SpellArgs args) where T : SpellArgs
    {
        Spell spell = GetSpell(args.spellWord);
        return spell?.GetMyArgs<T>(args);
    }

    /// <summary>
    /// Takes a SpellWord, finds the corresponding spell in the cache, and returns it. 
    /// </summary>
    /// <param name="spellWord"> The SpellWord that signifies the Spell we want to retreive. </param>
    /// <returns> The Spell corresponding to the SpellWord. </returns>
    public static Spell GetSpell(SpellWords spellWord)
    {
        string address = spellWord.ToString();

        if(_spells.TryGetValue(address, out var spell))
        {
            return spell;
        }

        Debug.LogError($"Spell not found: {address}");
        return null;
    }

    /// <summary>
    /// Clears the spell cache and the Addressable handles. This should be called when a session ends.
    /// </summary>
    public static void UnloadAll()
    {
        foreach(var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }

        _spells.Clear();
        _handles.Clear();
    }

    /// <summary>
    /// Returns a list of all the cached spells.
    /// </summary>
    private static List<Spell> GetAllLoadedSpells()
    {
        return new List<Spell>(_spells.Values);
    }

    /// <summary>
    /// Returns the status of the cache loading process.
    /// </summary>
    public static bool IsSpellCacheReady()
    {
        return _isCacheReady;
    }
    
}
