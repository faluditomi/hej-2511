using System;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// The base class for all other, more complex spells. Also the default spell to be created in the editor by:
/// Hierarchy Right Click > Create > Spells > Spell. These spell classes hold both the Sciptable object and the 
/// Args class for the sake of convenience and simplicity. 
/// </summary>
[CreateAssetMenu(fileName = "Spell", menuName = "Spells/Spell")]
public class Spell : ScriptableObject
{
    
    [Tooltip("The spell word that triggers the spell when spoken.")]
    public SpellWords spellWord;
    [Min(0f), Tooltip("Time that needs to pass after the spell is cast before it can be cast again.")]
    public float cooldownDuration;
    public event Action<SpellArgs> cast;

    /// <summary>
    /// Called to trigger the spell. It creates the args object and invokes the cast event.
    /// </summary>
    public void Cast()
    {
        SpellArgs args = CreateArgs();
        Debug.Log($"{spellWord} triggered.");
        cast?.Invoke(args);
    }

    /// <summary>
    /// Called when the spell is cast. It creates the spell specific arguments that are passed on to the behaviour
    /// callbacks that are listening to the cast event. Can be overwritten in child classes in case they require 
    /// more specific properties. (See ExampleComplexSpell for usage example)
    /// </summary>
    /// <returns> The spell specific arguments required for behavioural logic, etc. </returns>
    protected virtual SpellArgs CreateArgs()
    {
        return new SpellArgs
        {
            spellWord = this.spellWord,
            cooldownDuration = this.cooldownDuration
        };
    }

    /// <summary>
    /// Copies the properties of the current spell args to the target args. This has to be used in case a child class
    /// overwrites the CreateArgs method and needs to get the properties of the base class in its own args object.
    /// (See ExampleComplexSpell for usage example) 
    /// </summary>
    /// <param name="target"> The SpellArgs inheritor object we want to populate with the properties of the base Args class </param>
    /// <returns> The SpellArgs object that holds the properties of both the target and the base class. </returns>
    protected SpellArgs CopyTo(SpellArgs target)
    {
        target.spellWord = this.spellWord;
        target.cooldownDuration = this.cooldownDuration;
        return target;
    }

    /// <summary>
    /// Used by the behavioral classes of specific spells to get the specialised args object that was passed to the cast event. 
    /// It takes a generic SpellArgs object, a specific type that inherits from SpellArgs, and returns the casted object.
    /// </summary>
    /// <typeparam name="T"> The requested type that inherits from SpellArgs </typeparam>
    /// <param name="args"> The generic SpellArgs object </param>
    /// <returns> The casted specific args object that inherits SpellArgs </returns>
    public T GetMyArgs<T>(SpellArgs args) where T : SpellArgs
    {
        if(args is T castedArgs)
        {
            return castedArgs;
        }
        else
        {
            Debug.LogError($"{spellWord} was cast with the wrong args type. Expected {typeof(T)}, but got {args.GetType()}.");
            return null;
        }
    }

}

public class SpellArgs
{

    public SpellWords spellWord;
    //TODO: implement consequence for cooldownDuration
    public float cooldownDuration;

}
