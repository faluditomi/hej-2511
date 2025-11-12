using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This class takes the Whisper transcripts from the WhisperTranscriptManager and takes care of recognising spell words within it.
/// </summary>
public class SpellRecognitionManager : MonoBehaviour
{

    #region Fields
    public static SpellRecognitionManager _instance { get; private set; }
    
    [SerializeField] private List<SpellWords> activeSpells;
    private ConcurrentBag<SpellWords> spellsInCurrentSegment = new();
    private CancellationTokenSource cancellationTokenSource;
    private Dictionary<SpellWords, string> spellWordCache;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        SpellSessionCache.LoadSessionSpells(activeSpells);
        spellWordCache = activeSpells.ToDictionary(spell => spell, spell => SpellEnumToStringUtil(spell));
    }
    
    private void OnDisable()
    {
        SpellSessionCache.UnloadAll();
    }
    #endregion

    #region General Methods
    /// <summary>
    /// Takes a trascript segment and begins the spell recognition process in a parallelised manner. It also creates 
    /// a cancellation token, such that when a new segment comes in, the previous scanning tasks can be cancelled, thus
    /// preventing inconsistencies between the spells already scanned and the contents of the spellsInCurrentSegment bag.
    /// </summary>
    /// <param name="segment"> The text to be scanned for spell words. </param>
    public void ScanSegment(string segment)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        segment = RemoveRegisteredSpells(segment);

        try
        {
            Parallel.ForEach(activeSpells, new ParallelOptions{ CancellationToken = token }, spellWord =>
            {
                CheckForSpell(segment, spellWord, token);
            });
        }
        catch(OperationCanceledException)
        {
            Debug.LogError("ScanSegment operation was canceled before segment got processed.");
        }
    }

    /// <summary>
    /// Takes a specific spell word and checks whether the transcript segment contains it. If it does, it will cast the spell.
    /// It will also add the spell to the spellsInCurrentSegment bag and remove the spell word and the precefing text from
    /// the segment, such that that spell won't be activated again the next time this segment is scanned.
    /// </summary>
    /// <param name="segment"> The transcript to be scanned. </param>
    /// <param name="spellWord"> The spell word we are scanning for. </param>
    /// <param name="token"> The token that can potentially halt the scanning process. </param>
    private void CheckForSpell(string segment, SpellWords spellWord, CancellationToken token)
    {
        while(!token.IsCancellationRequested && ContainsSpellStringUtil(segment, spellWord))
        {
            spellsInCurrentSegment.Add(spellWord);
            SpellEventManager.CastSpell(spellWord);
            segment = RemoveSpellAndBeforeUtil(segment, spellWord);
        }

        //NOTE: if this log ever appears, we have to restructure how spells are checked for, to make sure that all uttered spells are registered
        //      and that the segment is not empty when we check for the next spell
        if(token.IsCancellationRequested)
        {
            Debug.LogError($"CheckForSpell was canceled for segment: {segment}");
        }
    }

    /// <summary>
    /// The same segment is scanned multiple times, since the scanning process is activated OnSegmentUpdated and not 
    /// OnSegmentFinished (WhisperTranscriptManager), thus, when the same segment comes in for the Nth time, we remove
    /// the spell words from it that we have already scanned in a previous round of scans.
    /// </summary>
    /// <param name="segment"> The transcript to be manipulated. </param>
    /// <returns> Returns the segment without the spell words in spellsInCurrentSegment. </returns>
    private string RemoveRegisteredSpells(string segment)
    {
        if(spellsInCurrentSegment.Count > 0)
        {
            foreach(SpellWords spellWord in spellsInCurrentSegment)
            {
                if(ContainsSpellStringUtil(segment, spellWord))
                {
                    segment = RemoveSpellStringUtil(segment, spellWord);
                }
                else
                {
                    Debug.LogWarning($"Potential false positive detected: {spellWord}");
                }
            }
        }

        return segment;
    }

    /// <summary>
    /// This method is called OnSegmentFinished (WhisperTranscriptManager) when we want to clear the spellsInCurrentSegment
    /// and cancel the scanning processes that still may be running.
    /// </summary>
    public void ResetSegmentation()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            // Debug.Log($"ResetSegmentation: Canceled running tasks.");
        }

        spellsInCurrentSegment = new ConcurrentBag<SpellWords>();
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Checks whether the context contains the spell word.
    /// </summary>
    private bool ContainsSpellStringUtil(string context, SpellWords spellToCheck)
    {
        return context.ToLower().Contains(spellWordCache[spellToCheck]);
    }

    /// <summary>
    /// Removes one instance of the spell word from the context.
    /// </summary>
    private string RemoveSpellStringUtil(string context, SpellWords spellToRemove)
    {
        string spellWordToRemove = spellWordCache[spellToRemove];
        int index = context.IndexOf(spellWordToRemove, StringComparison.OrdinalIgnoreCase);
        return index != -1 ? context.Remove(index, spellWordToRemove.Length).Trim() : context;
    }

    /// <summary>
    /// Removes the first instance of the spell word and everything before it from the context.
    /// </summary>
    private string RemoveSpellAndBeforeUtil(string context, SpellWords spellToRemove)
    {
        string spellWordToRemove = spellWordCache[spellToRemove];
        int index = context.IndexOf(spellWordToRemove, StringComparison.OrdinalIgnoreCase);
        return index != -1 ? context.Substring(index + spellWordToRemove.Length).Trim() : context;
    }

    /// <summary>
    /// Takes the SpellWords enum and converts it to a string that looks the same as it would appear in the transcript.
    /// It is used to create the spellWordCache dictionary upon initialisation so this method is only called once per spell.
    /// </summary>
    private string SpellEnumToStringUtil(SpellWords spellWord)
    {
        return spellWord.ToString().ToLower().Replace("_", " ");
    }
    #endregion

}
