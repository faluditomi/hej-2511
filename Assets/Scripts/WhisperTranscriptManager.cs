using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

//NOTE: for now, this is a singleton, but that might have to change once the game becomes multiplayer
/// <summary>
/// Manages the transcription of the microphone input using Whisper.
/// </summary>
public class WhisperTranscriptManager : MonoBehaviour
{

    #region Fields
    public static WhisperTranscriptManager _instance { get; private set; }

    private WhisperManager whisperManager;
    private MicrophoneRecord microphoneRecord;
    private WhisperStream whisperStream;
    
    [SerializeField, Tooltip("GameObject with a Text component that will hold the transcription.")]
    private Text transcriptionTextUI;
    [SerializeField, Tooltip("GameObject with ScrollRect component that will be scrolled down when the transcription is updated.")]
    private ScrollRect TranscriptionWindowScrollUI;
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

        whisperManager = FindFirstObjectByType<WhisperManager>();
        microphoneRecord = FindFirstObjectByType<MicrophoneRecord>();
    }

    /// <summary>
    /// Subscribing to events and initialising the transcription process
    /// </summary>
    private async void Start()
    {
        whisperStream = await whisperManager.CreateStream(microphoneRecord);

        whisperStream.OnResultUpdated += OnResult;
        whisperStream.OnSegmentUpdated += OnSegmentUpdated;
        whisperStream.OnSegmentFinished += OnSegmentFinished;
        // whisperStream.OnStreamFinished += OnFinished;

        microphoneRecord.StartRecord();
        whisperStream.StartStream();

        Debug.Log("Whisper initialised successfully");
    }

    /// <summary>
    /// Unsubscribing from events and cleaning caches when we no longer need transcription
    /// </summary>
    private void OnDestroy()
    {
        if(whisperStream == null)
        {
            return;
        }

        whisperStream.OnResultUpdated -= OnResult;
        whisperStream.OnSegmentUpdated -= OnSegmentUpdated;
        whisperStream.OnSegmentFinished -= OnSegmentFinished;
        // whisperStream.OnStreamFinished -= OnFinished;

        microphoneRecord.StopRecord();
        whisperStream.StopStream();
        whisperStream = null;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Called when Whisper updates the full transcription.
    /// </summary>
    /// <param name="result"> The complete, updated transcription string from the beginning. </param>
    private void OnResult(string result)
    {
        if(transcriptionTextUI && TranscriptionWindowScrollUI)
        {
            transcriptionTextUI.text = result;
            UiUtils.ScrollDown(TranscriptionWindowScrollUI);
        }
    }
    
    /// <summary>
    /// Called when Whisper updates the current piece of text that it's in the middle of transcribing.
    /// </summary>
    /// <param name="segment"> The current transcription that is being formed. </param>
    private void OnSegmentUpdated(WhisperResult segment)
    {
        // Debug.Log($"Segment updated: {segment.Result}");
        //NOTE: instead of relying only on the segment updates, we could prep the spell (visuals, particals, etc.) when
        //      it is recognised in the update, and cast it for real when it appears in the finished segment
        SpellRecognitionManager._instance.ScanSegment(segment.Result);
        
    }

    /// <summary>
    /// Called when Whisper is finished transcribing the current piece of text.
    /// </summary>
    /// <param name="segment"> The final form of the current segment that will be added to the result. </param>
    private void OnSegmentFinished(WhisperResult segment)
    {
        // Debug.Log($"Segment finished: {segment.Result}");
        SpellRecognitionManager._instance.ResetSegmentation();
    }
    #endregion

}
