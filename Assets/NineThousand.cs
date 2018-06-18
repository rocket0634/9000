using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NineThousand : MonoBehaviour
{
    private static int _moduleIdCounter = 1;
    private int _moduleId, t;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMAudio KMAudio;
    public SpriteRenderer[] screens;
    public Sprite[] sprites;
    public KMSelectable Ka, Me, Ha;
    private int[] selection = new[] { 0, 0, 0, 0 };
    private List<int> expectedSelection = new List<int>(), chosenSelection = new List<int>();
    private int selectionIndex;
    private bool isActive, running;

    private void Start()
    {
        foreach (int i in selection)
        {
            selection[Array.IndexOf(selection, i)] = UnityEngine.Random.Range(0, 5);
        }
        foreach (SpriteRenderer a in screens)
        {
            a.gameObject.SetActive(false);
        }
        _moduleId += _moduleIdCounter;
        Rules();
        Module.OnActivate += delegate () { Activate(); };
    }

    void Activate()
    {
        foreach (SpriteRenderer a in screens)
        {
            a.gameObject.SetActive(true);
            a.sprite = sprites[selection[Array.IndexOf(screens, a)]];
        }

        Ka.OnInteract += delegate () { ButtonPress(0); return false; };
        Me.OnInteract += delegate () { ButtonPress(1); return false; };
        Ha.OnInteract += delegate () { ButtonPress(2); return false; };

        var sequenceReplace = String.Join(", ", screens.Select(x => x.sprite.name).ToArray());
        Debug.LogFormat("[Over 9000 #{0}]: Chosen images: {1}", _moduleId, sequenceReplace);

        sequenceReplace = String.Join(" ", expectedSelection.Select(x => x.ToString()).ToArray()).Replace("0", "Ka").Replace("1", "Me").Replace("2","Ha");

        if (expectedSelection[0] != 3) Debug.LogFormat("[Over 9000 #{0}]: Expected sequence: {1}", _moduleId, sequenceReplace);
        else Debug.LogFormat("[Over 9000 #{0}]: Expected Sequence: Any.", _moduleId);

        isActive = true;
    }

    void Rules()
    {
        var t = selection.Where(x => x == 0).ToList();
        var s = selection.Where(x => x == 1).ToList();
        var l = selection.Where(x => x == 2).ToList();
        var r = selection.Where(x => x == 3).ToList();
        var u = Array.IndexOf(new[] { selection[0] == 0, selection[1] == 0, selection[2] == 1, selection[3] == 2 }, true);

        while (t.Count > 0)
        {
            if (selection.Contains(4))
            {
                switch (u)
                {
                    case 1:
                        expectedSelection.AddRange(new[] { 0, 0 });
                        break;
                    case 2:
                        expectedSelection.AddRange(new[] { 1, 2 });
                        break;
                    default:
                        expectedSelection.AddRange(new[] { 0, 1 });
                        break;
                }
            }
            else expectedSelection.AddRange(new[] { 0, 1 });
            t.Remove(0);
            if (s.Count > 0)
            {
                if (selection.Contains(4))
                {
                    switch (u)
                    {
                        case 0:
                            expectedSelection.AddRange(new[] { 1, 0 });
                            break;
                        case 1:
                            expectedSelection.AddRange(new[] { 2, 2 });
                            break;
                        default:
                            expectedSelection.AddRange(new[] { 2, 1 });
                            break;
                    }
                }
                else expectedSelection.AddRange(new[] { 2, 1 });
                s.Remove(1);
            }
        }

        while (s.Count > 0)
        {
            if (selection.Contains(4))
            {
                switch (u)
                {
                    case 0:
                        expectedSelection.AddRange(new[] { 1, 0 });
                        break;
                    case 1:
                        expectedSelection.AddRange(new[] { 2, 2 });
                        break;
                    default:
                        expectedSelection.AddRange(new[] { 2, 1 });
                        break;
                }
            }
            else expectedSelection.AddRange(new[] { 2, 1 });
            s.Remove(1);
        }

        while (l.Count > 0)
        {
            expectedSelection.Add(2);
            l.Remove(2);
        }

        var h = expectedSelection;
        foreach (int i in r)
        {
            expectedSelection = expectedSelection.Concat(h.Concat(h)).ToList();
        }

        if (selection.Contains(4) && u == 3)
        {
            expectedSelection.Reverse();
        }

        if (selection.All(x => x == selection[0]))
        {
            switch (selection[0])
            {
                case 0:
                case 1:
                    expectedSelection = new[] { 0, 1 }.ToList();
                    break;
                case 2:
                    expectedSelection = new[] { 0, 1, 2 }.ToList();
                    break;
                case 3:
                    expectedSelection = new[] { 0, 1, 2, 1, 2 }.ToList();
                    break;
                case 4:
                    expectedSelection = new[] { 3 }.ToList();
                    break;
            }
        }

        if (expectedSelection.Count < 1) expectedSelection = new List<int> { 2, 1, 2, 1, 0 };
    }

    void ButtonPress(int i)
    {
        if (!isActive) return;
        var k = new[] { Ka, Me, Ha };
        k[i].AddInteractionPunch(.3f);
        if (expectedSelection[0] == 3)
        {
            if (!running) StartCoroutine(Timer());
            t++;
        }
        else
        {
            chosenSelection.Add(i);
            if (selectionIndex < expectedSelection.Count)
            {
                var string1 = String.Join(", ", chosenSelection.Select(x => x.ToString()).ToArray()).Replace("0", "Ka").Replace("1", "Me").Replace("2", "Ha");
                var string2 = String.Join(", ", expectedSelection.Select(x => x.ToString()).ToArray()).Replace("0", "Ka").Replace("1", "Me").Replace("2", "Ha");
                if (chosenSelection[selectionIndex] != expectedSelection[selectionIndex])
                {
                    Debug.LogFormat("[Over 9000 #{0}]: Sequence [{1}] entered, sequence [{2}] expected", _moduleId, string1, string2);
                    Module.HandleStrike();
                    selectionIndex = 0;
                    chosenSelection.Clear();
                }
                else if (selectionIndex == expectedSelection.Count - 1)
                {
                    Module.HandlePass();
                    isActive = false;
                }
                else
                {
                    selectionIndex++;
                }
            } 
        }
    }

    IEnumerator Timer()
    {
        running = true;
        var h = 0;
        var i = t;
        while (h < 1)
        {
            yield return new WaitForSeconds(0.5f);
            if (i == t) h++;
            else
            {
                t = i;
                h--;
            }
        }
        if (i != t) yield break;
        Module.HandlePass();
        isActive = false;
        Debug.LogFormat("[Over 9000 #{0}]: Module solved!", _moduleId);
        yield break;
    }

    private string TwitchHelpMessage = "Submit sequence using !{0} submit ka me ha me ha";

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var split = command.ToLowerInvariant().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (split[0] != "submit") yield break;
        var tapList = split.ToList();
        tapList.RemoveAt(0);
        split = tapList.ToArray();

        foreach (string tap in split)
        {
            yield return null;
            if (tap == "ka")
            {
                yield return new KMSelectable[] { Ka };
            }
            else if (tap == "me")
            {
                yield return new KMSelectable[] { Me };
            }
            else if (tap == "ha")
            {
                yield return new KMSelectable[] { Ha };
            }
            else
            {
                yield break;
            }
        }
    }
}