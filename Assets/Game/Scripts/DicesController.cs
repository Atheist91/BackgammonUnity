using UnityEngine;
using System.Collections.Generic;

public class DicesController : MonoBehaviour
{
    /// <summary>
    /// List of dices that are controlled by this DicesController.
    /// </summary>
    public List<DiceController> Dices = new List<DiceController>(new DiceController[2]);
    public GameManager GameManager;
    public SpriteRenderer MySpriteRenderer;

    void Start ()
    {
        if (Dices.Count == 2)
        {
            for (int iDice = 0; iDice < 2; ++iDice)
            {
                if (Dices[iDice] == null)
                {
                    Logger.Error(this, "Dice {0} isn't set properly (it's empty).", iDice);
                }
            }
        }
        else
        {
            Logger.Error(this, "Dices list has to contain 2 elements as 2 dices are needed for this game. Amount of elements= {0}", Dices.Count);
        }

        if(GameManager)
        {
            GameManager.OnStateChanged += GameManager_OnStateChanged;
        }
    }

    private void GameManager_OnStateChanged(GameState InOldState, GameState InNewState)
    {
        if(MySpriteRenderer && (InNewState == GameState.RedPlayerRolls || InNewState == GameState.WhitePlayerRolls))
        {
            MySpriteRenderer.enabled = true;
        }
    }

    void OnMouseDown()
    {
        foreach(DiceController dice in Dices)
        {
            if(dice != null)
            {
                dice.Roll();
            }
        }

        if(MySpriteRenderer)
        {
            MySpriteRenderer.enabled = false;
        }
    }
}