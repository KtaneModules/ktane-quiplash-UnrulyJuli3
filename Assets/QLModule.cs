using KModkit;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QLModule : MonoBehaviour {

    public KMBombModule BombModule;
    public KMBombInfo BombInfo;
    public KMAudio BombAudio;
    public KMSelectable AnswerBubbleLeft;
    public KMSelectable AnswerBubbleRight;
    public TextAsset QL3Prompts;
    public TextMesh[] PromptTexts;
    public KMSelectable JinxObj;
    public KMSelectable Selectable;

    private QLConfig ChosenConfig;
    private bool IsSolved;
    private int CorrectAnswer;

    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;

    class QL3PromptFile
    {
        public List<QLConfig> content;
    }

    class QLConfig
    {
        public string id;
        public bool includesPlayerName;
        public string prompt;
        public List<string> safetyQuips;
        public bool us;
        public bool x;
    }

    int VowelCount(string str)
    {
        return str.ToLower().Count(c => "aeiou".Contains(c));
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        List<QLConfig> PossibleConfigs = JsonConvert.DeserializeObject<QL3PromptFile>(QL3Prompts.text).content;

        //Debug.LogFormat("[Quiplash #{0}] ", _moduleId);
        Debug.LogFormat("[Quiplash #{0}] Loaded {1} prompts", _moduleId, PossibleConfigs.Count);
        PossibleConfigs = PossibleConfigs.Where(c => !c.x && c.safetyQuips.Count == 3).ToList();
        Debug.LogFormat("[Quiplash #{0}] Removed non-family-friendly prompts and applied filters, resulting in a new count of {1}", _moduleId, PossibleConfigs.Count);

        ChosenConfig = PossibleConfigs.PickRandom();
        Debug.LogFormat("[Quiplash #{0}] Chosen prompt: {1}", _moduleId, JsonConvert.SerializeObject(ChosenConfig, Formatting.None));
        foreach (TextMesh t in PromptTexts) t.text = ChosenConfig.id;
        if (ChosenConfig.safetyQuips.Count > 2)
        {
            JinxObj.gameObject.SetActive(true);
            List<KMSelectable> Children = Selectable.Children.ToList();
            Children.Add(JinxObj);
            Children.Add(JinxObj);
            Selectable.Children = Children.ToArray();
            Selectable.UpdateChildren();
        } else JinxObj.gameObject.SetActive(false);

        CorrectAnswer = Mathf.CeilToInt(Mathf.FloorToInt(ChosenConfig.safetyQuips.Sum(sq => VowelCount(sq)) / (float)ChosenConfig.safetyQuips.Count) / 15f * 3f) % 3;
        Debug.LogFormat("[Quiplash #{0}] Final answer: {1}", _moduleId, CorrectAnswer);

        AnswerBubbleLeft.OnInteract += delegate
        {
            ChooseAnswer(0, AnswerBubbleLeft);
            return false;
        };
        AnswerBubbleRight.OnInteract += delegate
        {
            ChooseAnswer(1, AnswerBubbleRight);
            return false;
        };
        JinxObj.OnInteract += delegate
        {
            ChooseAnswer(2, JinxObj);
            return false;
        };
    }

    void ChooseAnswer(int i, KMSelectable bubble)
    {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, bubble.transform);
        bubble.AddInteractionPunch();
        if (IsSolved) return;
        string[] pos = new string[3] { "first", "second", "third" };
        Debug.LogFormat("[Quiplash #{0}] Selected {1} answer, expected {2} answer", _moduleId, pos[i], pos[CorrectAnswer]);
        bool IsCorrect = CorrectAnswer == i;
        if (IsCorrect)
        {
            Debug.LogFormat("[Quiplash #{0}] Answered correctly and module is solved", _moduleId);
            BombModule.HandlePass();
            IsSolved = true;
        } else
        {
            Debug.LogFormat("[Quiplash #{0}] Answered incorrectly, invoking strike", _moduleId);
            BombModule.HandleStrike();
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} [answer] | Answer can be numerical or in word form (like \"left\")";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        switch (command.ToLowerInvariant())
        {
            case "1":
            case "left":
            case "l":
            case "first":
                return new KMSelectable[] { AnswerBubbleLeft };
            case "2":
            case "right":
            case "r":
            case "second":
                return new KMSelectable[] { AnswerBubbleRight };
            case "3":
            case "down":
            case "jinx":
            case "d":
            case "j":
            case "third":
                return new KMSelectable[] { JinxObj };
        }
        return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        ChooseAnswer(CorrectAnswer, new KMSelectable[] { AnswerBubbleLeft, AnswerBubbleRight, JinxObj }[CorrectAnswer]);
        yield break;
    }
}
