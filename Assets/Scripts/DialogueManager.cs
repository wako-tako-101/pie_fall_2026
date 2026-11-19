using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Reads dialogue from a text file and plays it through a dialogue UI
/// (a name box, a speech box and a "next" button).
///
/// Text file format (one line of dialogue per line):
///     Speaker Name: What they say.
///
/// - A line with no "Name:" prefix continues with the previous speaker.
/// - Blank lines are ignored.
/// - Lines starting with // or # are comments and are ignored.
/// - Write \n inside a line to force a line break in the speech box.
///
/// Start a conversation by calling StartDialogue(TextAsset), for example from the
/// 'onPlayerArrived' event of a point of interest on the guide.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    private struct DialogueLine
    {
        public string speaker;
        public string text;
    }

    // Anything before the first colon longer than this is treated as dialogue, not a speaker name.
    private const int MaxSpeakerNameLength = 30;

    [Header("UI References")]
    [Tooltip("The root of the dialogue UI that gets shown/hidden. Should be a child if this script lives on an object that stays active.")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text speechText;
    [SerializeField] private Button nextButton;

    [Header("Options")]
    [Tooltip("Hide the panel when the dialogue finishes.")]
    [SerializeField] private bool hidePanelOnEnd = true;
    [Tooltip("Optional: a dialogue to play if StartDialogue() is called without a file (e.g. from a UnityEvent with no argument).")]
    [SerializeField] private TextAsset defaultDialogue;

    [Header("Events")]
    public UnityEvent onDialogueStarted;
    [Tooltip("Fires when the last line has been dismissed. Hook up VRGuideCompanion.ResumeGuiding here.")]
    public UnityEvent onDialogueEnded;

    private readonly List<DialogueLine> lines = new List<DialogueLine>();
    private int currentIndex = -1;
    private bool listenerAdded;

    public bool IsActive { get; private set; }

    private void Start()
    {
        EnsureInitialised();

        if (dialoguePanel != null && !IsActive)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null && listenerAdded)
        {
            nextButton.onClick.RemoveListener(NextLine);
        }
    }

    // Done lazily so it still works if this object starts inactive (Awake/Start never ran).
    private void EnsureInitialised()
    {
        if (listenerAdded || nextButton == null) return;

        nextButton.onClick.AddListener(NextLine);
        listenerAdded = true;
    }

    /// <summary>Starts the dialogue assigned to 'Default Dialogue'.</summary>
    public void StartDialogue()
    {
        StartDialogue(defaultDialogue);
    }

    /// <summary>Starts the dialogue contained in the given text file.</summary>
    public void StartDialogue(TextAsset dialogueFile)
    {
        if (dialogueFile == null)
        {
            Debug.LogWarning("DialogueManager: no dialogue file supplied.", this);
            return;
        }

        if (IsActive)
        {
            Debug.LogWarning("DialogueManager: a dialogue is already playing, ignoring new request.", this);
            return;
        }

        EnsureInitialised();

        ParseDialogue(dialogueFile.text);

        if (lines.Count == 0)
        {
            Debug.LogWarning($"DialogueManager: '{dialogueFile.name}' contains no dialogue lines.", this);
            return;
        }

        IsActive = true;
        currentIndex = -1;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        onDialogueStarted?.Invoke();
        NextLine();
    }

    /// <summary>
    /// Shows the next line, or ends the dialogue if there are no more.
    /// Wired to the button automatically; also callable from a controller input if you like.
    /// </summary>
    public void NextLine()
    {
        if (!IsActive) return;

        currentIndex++;

        if (currentIndex >= lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines[currentIndex];

        if (nameText != null) nameText.text = line.speaker;
        if (speechText != null) speechText.text = line.text;
    }

    private void EndDialogue()
    {
        IsActive = false;
        lines.Clear();
        currentIndex = -1;

        if (hidePanelOnEnd && dialoguePanel != null) dialoguePanel.SetActive(false);

        onDialogueEnded?.Invoke();
    }

    private void ParseDialogue(string raw)
    {
        lines.Clear();

        raw = raw.TrimStart('\uFEFF');   // strip a byte-order mark if the file has one
        string lastSpeaker = string.Empty;

        foreach (string rawLine in raw.Split('\n'))
        {
            string line = rawLine.Trim();   // also removes any trailing \r

            if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;

            string speaker = lastSpeaker;
            string text = line;

            int colon = line.IndexOf(':');
            if (colon > 0 && colon <= MaxSpeakerNameLength)
            {
                speaker = line.Substring(0, colon).Trim();
                text = line.Substring(colon + 1).Trim();
                lastSpeaker = speaker;
            }

            text = text.Replace("\\n", "\n");

            if (text.Length == 0) continue;

            lines.Add(new DialogueLine { speaker = speaker, text = text });
        }
    }
}
