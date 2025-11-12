using UnityEngine;

/// <summary>
/// This is an example Spell on how we can create and utilise spells that require unique input arguments.
/// </summary>
[CreateAssetMenu(fileName = "ExampleComplexSpell", menuName = "Spells/ExampleComplexSpell")]
public class ExampleComplexSpell : Spell
{

    public int someProperty;
    public float anotherProperty;
    public bool someOtherProperty;

    protected override SpellArgs CreateArgs()
    {
        ExampleComplexArgs myArgs = new ExampleComplexArgs
        {
            someProperty = this.someProperty
        };

        return CopyTo(myArgs);
    }
    
    ///NOTE: For complex spells, here we can also include calculations, logic, or even other services, if needed.

}

/// <summary>
/// The args classes, used for passing around spell specific input parameters to MonoBehaviours, are included in the spell class 
/// for convenience and simplicity. This addition is only necessary, if this spell is used in a MonoBehaviour context and it 
/// needs to pass spell specific properties to the behaviour.
/// </summary>
public class ExampleComplexArgs : SpellArgs
{

    public int someProperty;
    public bool someOtherProperty;

}
