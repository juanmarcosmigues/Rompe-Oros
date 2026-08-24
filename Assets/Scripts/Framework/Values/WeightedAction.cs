using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WeightedAction<T>
{
    public string Name;
    [Range(0, 100)]
    public int Weight;
    public ActionType Action;

    public WeightedAction (string Name, int Weight, ActionType Action) 
    {
        this.Name = Name;
        this.Weight = Weight;
        this.Action = Action;
    }

    public delegate T ActionType ();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="addWeight">addWeight is used to modify outcome (sum operation on random number)</param>
    /// <returns></returns>
    public static WeightedAction<T> GetAction (WeightedAction<T>[] actions, int addWeight = 0)
    {
        int currentWeight = 0;
        int currentIndex = 0;
        for (currentIndex = 0; currentIndex < actions.Length; currentIndex++)
        {
            currentWeight += actions[currentIndex].Weight;
        }
        Debug.Log("Max Weight: " + currentWeight);
        int randomValue = Random.Range(0, currentWeight + 1);
        Debug.Log("Random Value: " + randomValue);
        randomValue = Mathf.Clamp(randomValue + addWeight, 0, currentWeight);
        Debug.Log("Random Value + Luck: " + randomValue);

        currentIndex = 0;
        currentWeight = 0;
        for (currentIndex = 0; currentIndex < actions.Length; currentIndex++)
        {
            currentWeight += actions[currentIndex].Weight;
            if (randomValue <= currentWeight)
            {
                return actions[currentIndex];
            }
        }

        Debug.Log("Fallback Weighted Action");
        return actions[0];
    }
}
