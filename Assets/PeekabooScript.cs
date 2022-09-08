using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeekabooScript : MonoBehaviour {

    public KMAudio Audio;
    public KMNeedyModule module;
    public List<KMSelectable> buttons;
    public Renderer[] brends;
    public Material[] shapes;
    public Material lmat;
    public Light lflash;
    public Transform lpos;

    private readonly Color32[] cols = new Color32[7] { new Color32(255, 0, 0, 255), new Color32(255, 220, 0, 255), new Color32(0, 255, 128, 255), new Color32(0, 128, 255, 255), new Color32(128, 0, 220, 255), new Color32(255, 255, 255, 255), new Color32(0, 0, 0, 255)};
    private readonly string[] clog = new string[] { "red", "yellow", "jade", "azure", "violet", "white", "black"};
    private readonly string[] shlog = new string[] { "circle", "hexagon", "pentagon", "semicircle", "square", "pentagram", "triangle" };
    private readonly string[] sylog = new string[] { "arrow", "bolt", "crescent", "cross", "diamond", "heart", "note" };
    private int[,] binfo = new int[6, 7];
    private bool peek;
    private int ans;

    private static int moduleIDCounter;
    private int moduleID;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        lmat.color = cols[6];
        lflash.enabled = false;
        lflash.range *= module.transform.lossyScale.x;
        for(int i = 0; i < 6; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                int r = Random.Range(0, 7);
                while(j > 2 && binfo[i, j - 1] == r)
                    r = Random.Range(0, 7);
                binfo[i, j] = r;
                Setinfo(i, j, r);
            }
            Debug.LogFormat("[Peekaboo #{0}] The {1}-{2} button is {3} with a {4} border and has a {5} with {6} boundary and {7} interior containing a {8} {9}.", moduleID, i < 3 ? "top" : "bottom", new string[] { "left", "middle", "right"}[i % 3], clog[binfo[i, 3]], clog[binfo[i, 2]], shlog[binfo[i, 1]], clog[binfo[i, 4]], clog[binfo[i, 5]], clog[binfo[i, 6]], sylog[binfo[i, 0]]);
        }
        module.OnNeedyActivation = Activate;
        module.OnTimerExpired = delegate ()
        {
            StopCoroutine("Blink");
            module.HandleStrike();
            lmat.color = cols[6];
            lflash.enabled = false;
            Debug.LogFormat("[Peekaboo #{0}] Time's up. {1}", moduleID, peek ? "Nothing selected." : "It's rude to stare.");
            peek = false;
        };
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                if (peek)
                {
                    peek = false;
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    button.AddInteractionPunch(0.75f);
                    module.HandlePass();
                    Debug.LogFormat("[Peekaboo #{0}] {1}-{2} button selected.", moduleID, b < 3 ? "Top" : "Bottom", new string[] { "left", "middle", "right"}[b % 3]);
                    if (b != ans)
                        module.HandleStrike();
                }
                return false;
            };
        }
    }

    private void Setinfo(int i, int j, int c)
    {
        switch (j)
        {
            case 0:
                brends[(5 * i) + 4].material = shapes[(3 * c) + 2];
                brends[(5 * i) + 4].material.color = cols[binfo[i, 6]];
                break;
            case 1:
                brends[(5 * i) + 2].material = shapes[(3 * c) + 1];
                brends[(5 * i) + 3].material = shapes[3 * c];
                brends[(5 * i) + 2].material.color = cols[binfo[i, 4]];
                brends[(5 * i) + 3].material.color = cols[binfo[i, 5]];
                break;
            default:
                brends[(5 * i) + j - 2].material.color = cols[c];
                break;
        }
    }

    private void Activate()
    {
        lflash.enabled = true;
        StartCoroutine("Blink");
    }

    private IEnumerator Blink()
    {
        int i = 100;
        int s = 200;
        while (s > 0)
        {
            lmat.color = new Color(i / 100f, 0, 0);
            lflash.intensity = i;
            i--;
            if (i < 1)
                i = 100;
            if (lpos.up.z > 0.3f)
                s--;
            else
                s = 200;
            yield return new WaitForSeconds(0.015f);
        }
        lmat.color = cols[6];
        lflash.enabled = false;
        peek = true;
        ans = Random.Range(0, 6);
        int r = Random.Range(0, 7);
        int c = 0;
        if(r > 1)
        {
            List<int> a = new List<int> { 0, 1, 2, 3, 4, 5, 6};
            for (int j = Mathf.Max(2, r - 1); j < Mathf.Min(r + 2, 7); j++)
                a.Remove(binfo[ans, j]);
            c = a.PickRandom();
        }
        else
            c = (binfo[ans, r] + Random.Range(1, 7)) % 7;
        Setinfo(ans, r, c);
        Debug.LogFormat("[Peekaboo #{0}] The {1}-{2} button has switched its {3} from {4} to {5}.", moduleID, ans < 3 ? "top" : "bottom", new string[] { "left", "middle", "right"}[ans % 3], new string[] { "symbol", "shape", "border colour", "colour", "shape boundary colour", "shape interior colour", "symbol colour"}[r], r > 1 ? clog[binfo[ans, r]] : (r == 1 ? shlog[binfo[ans, r]] : sylog[binfo[ans, r]]), r > 1 ? clog[c] : (r == 1 ? shlog[c] : sylog[c]));
        binfo[ans, r] = c;
        Audio.PlaySoundAtTransform("Ding", transform);
    }
}
