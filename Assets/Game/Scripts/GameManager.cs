﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum GameState
{
    Init,
    RedPlayerRolls,
    RedPlayerMoves,
    WhitePlayerRolls,
    WhitePlayerMoves,

    // 
    Last
}

public class GameManager : MonoBehaviour
{
    public delegate void SwitchGameStateDelegate(GameState InOldState, GameState InNewState);
    public event SwitchGameStateDelegate OnStateChanged;

    public GameObject PawnRedPrefab;
    public GameObject PawnWhitePrefab;    
    public List<FieldController> FieldsOrder = new List<FieldController>(new FieldController[26]);
    public List<DiceController> Dices = new List<DiceController>(new DiceController[2]);
    public BandController Band;
    protected GameState State = GameState.Init;
    protected PlayerColor CurrentPlayer = PlayerColor.Red;
    protected FieldController FromField = null;    
    protected PossibleMoves PossibleMoves = null;

    public FieldController GetField(int InIndex)
    {
        return GetPawnsContainer(InIndex) as FieldController;
    }

    public PawnsContainer GetPawnsContainer(int InIndex)
    {
        if (IsValidFieldIndex(InIndex))
        {
            return FieldsOrder[InIndex];
        }

        return null;
    }
    
    void Start ()
    {
        if (FieldsOrder.Count == 24)
        {
            for(int iField = 0; iField < 24; ++iField)
            {
                if (FieldsOrder[iField])
                {
                    FieldsOrder[iField].OnClicked += Field_OnClicked;
                }
                else
                {
                    Debug.LogError(string.Format("Field {0} isn't set properly (it's empty).", iField));
                }
            }
        }
        else
        {
            Debug.LogError("FieldsOrder list has to contain 24 elements as there are 24 fields in this game.");
        }
        
        if (Dices.Count == 2)
        {
            for (int iDice = 0; iDice < 2; ++iDice)
            {
                if (Dices[iDice] != null)
                {
                    Dices[iDice].OnRolled += Dice_OnRolled;
                    Dices[iDice].OnUsed += Dice_OnUsed;
                }
                else
                {
                    Debug.LogError(string.Format("Dice {0} isn't set properly (it's empty).", iDice));
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("Dices list has to contain 2 elements as 2 dices are needed for this game. Amount of elements= {0}", Dices.Count));
        }

        StartCoroutine(StartGame(3f));
	}

    private void Dice_OnUsed(DiceController InDice, DiceState InState)
    {
        bool bEveryFullyUsed = true;
        foreach(DiceController dice in Dices)
        {
            if(dice.GetUsageState() != DiceState.FullyUsed)
            {
                bEveryFullyUsed = false;
                break;
            }
        }

        // TODO: checking if there are any available moves
        bool bAvailableMovesExist = true;

        if(bEveryFullyUsed || !bAvailableMovesExist)
        {
            SwitchGameState();
        }
    }

    private void Dice_OnRolled(DiceController InDice, int InDots)
    {
        bool bFinished = true;
        foreach (DiceController dice in Dices)
        {
            if (!dice.HasFinishedRolling())
            {
                bFinished = false;
                break;
            }
        }

        if (bFinished)
        {
            MovePawnsFromBand();

            SwitchGameState();
        }
    }

    protected IEnumerator StartGame(float InDelay)
    {
        yield return new WaitForSeconds(InDelay);

        SwitchGameState();
    }

    public GameState GetGameState()
    {
        return State;
    }

    protected virtual void Field_OnClicked(FieldController InField)
    {
        if (State == GameState.RedPlayerMoves || State == GameState.WhitePlayerMoves)
        {
            // If the field on which we clicked is one of possible target fields
            if(PossibleMoves != null && PossibleMoves.IsMovePossible(InField))
            {
                PossibleMoves.MoveTo(InField);
            }
        }
    }

    protected virtual void SwitchGameState()
    {
        GameState OldState = State;
        State = FindNextGameState();

        CurrentPlayer = (State == GameState.RedPlayerMoves || State == GameState.RedPlayerRolls) ? PlayerColor.Red : PlayerColor.White;

        Logger.Log(this, "Switching game state from {0} to {1}. It's {2} player turn.", OldState, State, CurrentPlayer);

        if (OnStateChanged != null)
        {
            OnStateChanged(OldState, State);
        }
    }

    protected virtual GameState FindNextGameState()
    {
        switch (State)
        {
            case GameState.Init:
                return GameState.RedPlayerRolls;
                
            case GameState.RedPlayerRolls:
                return GameState.RedPlayerMoves;

            case GameState.RedPlayerMoves:
                return GameState.WhitePlayerRolls;

            case GameState.WhitePlayerRolls:
                return GameState.WhitePlayerMoves;

            case GameState.WhitePlayerMoves:
                return GameState.RedPlayerRolls;
        }

        Logger.Error(this, "This should never happen.");

        return GameState.RedPlayerRolls;
    }

    [MenuItem("Custom/Setup field order")]
    private static void SetupFieldsOrder()
    {
        var temp = Selection.activeGameObject.GetComponentsInChildren<FieldController>();
        for(int iField = 0; iField < temp.Length; ++iField)
        {
            temp[iField].transform.name = string.Format("Field_{0}", iField + 1);            
        }
    }

    [MenuItem("Custom/Setup field order", true)]
    private static bool SetupFieldsOrderValidation()
    {
        if (Selection.activeGameObject != null)
        {
            return Selection.activeGameObject.GetComponentsInChildren<FieldController>().Length > 0;
        }

        return false;
    }

    public static GameManager Find()
    {
        var GO = GameObject.FindGameObjectWithTag("GameManager");
        if (GO != null)
        {
            var GM = GO.GetComponent<GameManager>();
            if(GM == null)
            {
                Logger.Error("GameManager", "GameManager couldn't be find on the scene. It means that it probably wasn't placed on the level in first place.");
            }

            return GM;
        }

        return null;
    }

    public bool CanPlayerMove(PawnController InPawn)
    {
        if (InPawn != null && CurrentPlayer == InPawn.GetColor())
        {
            return InPawn.GetColor() == PlayerColor.Red ? State == GameState.RedPlayerMoves : State == GameState.WhitePlayerMoves;
        }

        return false;
    }

    public void ShowPossibleMoves(PawnController InPawn)
    {
        Logger.Log(this, "Showing possible moves for pawn. Player: {0}, Pawn: {1}", CurrentPlayer, InPawn.GetColor());
        if (InPawn && CanPlayerMove(InPawn) && InPawn.GetField())
        {
            // Clearing possible moves if needed
            if (PossibleMoves != null)
            {
                PossibleMoves.Clear();
            }

            PossibleMoves = new PossibleMoves(this, InPawn.GetField(), Dices);
        }
    }    

    protected void MovePawnsFromBand()
    {
        if(Band && Band.HasPawns(CurrentPlayer))
        {
            // Clearing possible moves if needed
            if (PossibleMoves != null)
            {
                PossibleMoves.Clear();
            }

            PossibleMoves = new PossibleMoves(this, Band, Dices, true);
            PossibleMoves.DoFirstMove();
        }
    }

    protected bool IsValidMove(PlayerColor InPlayer, int InStartIndex, int InDots, ref int OutEndIndex)
    {
        OutEndIndex = InStartIndex + (InPlayer == PlayerColor.Red ? InDots : -InDots);

        return (IsValidFieldIndex(OutEndIndex) && FieldsOrder[OutEndIndex].IsRoomForPawn(InPlayer));
    }    

    protected bool IsValidFieldIndex(int InIndex)
    {
        return InIndex >= 0 && InIndex < FieldsOrder.Count;
    }

    public GameObject GetPawnTemplateObject(PlayerColor InColor)
    {
        return InColor == PlayerColor.Red ? PawnRedPrefab : PawnWhitePrefab;
    }

    public PlayerColor GetCurrentPlayerColor()
    {
        return CurrentPlayer;
    }

    public PlayerColor GetPlayerTurn()
    {
        return CurrentPlayer;
    }

    public List<DiceController> GetDices()
    {
        return Dices;
    }

    /// <summary>
    /// Returns an index of given PawnContainer.
    /// When a BandController is passed, then the index will be -1 or 24 depending on which player's turn it is.
    /// It's made so, it'll be easier to calculate move based on starting index + amount of steps for pawn to make.
    /// </summary>
    /// <param name="InContainer">The container for which we want to know the index.</param>
    /// <returns>Index of a given container.</returns>
    public int GetIndexOfContainer(PawnsContainer InContainer)
    {
        if (InContainer is BandController)
        {
            return CurrentPlayer == PlayerColor.Red ? -1 : 24;
        }
        else
        {
            return FieldsOrder.IndexOf(InContainer as FieldController);
        }
    }
}