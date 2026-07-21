using AYellowpaper.SerializedCollections;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    public SerializedDictionary<string, Stats> Quotes;
    public TextMeshProUGUI a, b, c, d, quote;

    public int pa, pb, pc, pd;

    public Stats currentStats;
    public string currentQuote;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        string randomKey = Quotes.Keys.ElementAt(
                UnityEngine.Random.Range(0, Quotes.Count)
            );
        currentQuote = randomKey;
        currentStats = new Stats();
        currentStats.a = Quotes[randomKey].a;
        currentStats.b = Quotes[randomKey].b;
        currentStats.c = Quotes[randomKey].c;
        currentStats.d = Quotes[randomKey].d;

        quote.text = currentQuote;

    }

    public void AddNewValuesPositive()
    {
        a.text = (int.Parse(a.text) + currentStats.a).ToString();
        b.text = (int.Parse(b.text) + currentStats.b).ToString();
        c.text = (int.Parse(c.text) + currentStats.c).ToString();
        d.text = (int.Parse(d.text) + currentStats.d).ToString();

        string randomKey = Quotes.Keys.ElementAt(
                UnityEngine.Random.Range(0, Quotes.Count)
            );
        currentQuote = randomKey;
        currentStats = new Stats();
        currentStats.a = Quotes[randomKey].a;
        currentStats.b = Quotes[randomKey].b;
        currentStats.c = Quotes[randomKey].c;
        currentStats.d = Quotes[randomKey].d;

        quote.text = currentQuote;
    }

    public void AddNewValuesNegative()
    {
        a.text = (int.Parse(a.text) - currentStats.a).ToString();
        b.text = (int.Parse(b.text) - currentStats.b).ToString();
        c.text = (int.Parse(c.text) - currentStats.c).ToString();
        d.text = (int.Parse(d.text) - currentStats.d).ToString();

        string randomKey = Quotes.Keys.ElementAt(
                UnityEngine.Random.Range(0, Quotes.Count)
            );
        currentQuote = randomKey;
        currentStats = new Stats();
        currentStats.a = Quotes[randomKey].a;
        currentStats.b = Quotes[randomKey].b;
        currentStats.c = Quotes[randomKey].c;
        currentStats.d = Quotes[randomKey].d;

        quote.text = currentQuote;
    }

}
[Serializable]
public class Stats
{
    public int a,b,c,d;
}