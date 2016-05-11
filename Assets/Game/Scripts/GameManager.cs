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
    protected PossibleMoves PossibleMoves = null;

    void Start()
    {
        StartCoroutine(StartGame(3f));
    }

    /// <summary>
    /// Shows possible moves that can be done from given Field.
    /// </summary>
    /// <param name="InField">The Field for which we want to show possible moves.</param>
    public void ShowPossibleMoves(FieldController InField)
    {
        Logger.Log(this, "Trying to show possible moves from field {0}.", GetIndexOf(InField));
        if (InField)
        {
            PawnController topPawn = InField.GetPawn();
            if (topPawn && CanPawnBeMoved(topPawn))
            {
                // Clearing possible moves if needed
                if (PossibleMoves != null)
                {
                    PossibleMoves.Clear();
                }

                PossibleMoves = new PossibleMoves(this, InField, Dices);
            }
        }        
    }

    /// <summary>
    /// Tries to find GameManager in scene and returns reference to it.
    /// </summary>
    /// <returns>Reference to GameManager on the scene.</returns>
    public static GameManager Find()
    {
        GameObject GO = GameObject.FindGameObjectWithTag("GameManager");
        if (GO != null)
        {
            GameManager GM = GO.GetComponent<GameManager>();
            if (GM == null)
            {
                Logger.Error("GameManager", "GameManager couldn't be find on the scene. It means that it probably wasn't placed on the level in first place.");
            }

            return GM;
        }

        return null;
    }

    /// <summary>
    /// Returns template of a pawn for given color. Used when spawning new pawns.
    /// </summary>
    /// <param name="InColor">The color for which we want to get a pawn template.</param>
    /// <returns>Reference to pawn template (Prefab in GameAssets).</returns>
    public GameObject GetPawnTemplate(PlayerColor InColor)
    {
        return InColor == PlayerColor.Red ? PawnRedPrefab : PawnWhitePrefab;
    }

    /// <summary>
    /// Returns a FieldController that is placed at given index.
    /// </summary>
    /// <param name="InIndex">The index of wanted FieldController.</param>
    /// <returns>FieldController that is placed at given index.</returns>
    public FieldController GetField(int InIndex)
    {
        if (IsValidFieldIndex(InIndex))
        {
            return FieldsOrder[InIndex];
        }

        return null;
    }

    /// <summary>
    /// Returns a color of a player whos turn is now.
    /// </summary>
    /// <returns>A color of a player whos turn is now.</returns>
    public PlayerColor GetPlayer()
    {
        return CurrentPlayer;
    }

    /// <summary>
    /// Returns state in which the game is. The state determines which player's turn it is, what kind of moves the player can make (rolling dices/moving pawns) and stuff.
    /// </summary>
    /// <returns>The state in which the game is.</returns>
    public GameState GetState()
    {
        return State;
    }

    /// <summary>
    /// Returns list of Dices used in game.
    /// </summary>
    /// <returns>List of Dices used in game.</returns>
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
    public int GetIndexOf(PawnsContainer InContainer)
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

    /// <summary>
    /// Checks if given pawn can be moved by current player in current state of the game.
    /// </summary>
    /// <param name="InPawn">The pawn that we want to know if can be moved or not.</param>
    /// <returns>Whether given pawn can be moved or not.</returns>
    public bool CanPawnBeMoved(PawnController InPawn)
    {
        if (InPawn != null && CurrentPlayer == InPawn.GetColor())
        {
            return InPawn.GetColor() == PlayerColor.Red ? State == GameState.RedPlayerMoves : State == GameState.WhitePlayerMoves;
        }

        return false;
    }

    /// <summary>
    /// Coroutine for starting the game. It basically will switch GameState from Init to RedPlayerRolls.
    /// </summary>
    /// <param name="InDelay">The delay (in seconds) after which the game will be started.</param>
    /// <returns>Enumerator needed for Coroutine to work.</returns>
    protected IEnumerator StartGame(float InDelay)
    {
        yield return new WaitForSeconds(InDelay);

        Logger.Log(this, "The game has started.");
        SwitchGameState();
    }

    /// <summary>
    /// Switches game from current state to next one. Also, fires OnStateChanged event.
    /// </summary>
    protected void SwitchGameState()
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

    /// <summary>
    /// Tries to move Pawn that belongs to CurrentPlayer from Band (if any).
    /// </summary>
    /// <returns>Whether move has been made (or Band didn't have any Pawns that belong to CurrentPlayer) or not.</returns>
    protected bool MovePawnFromBand()
    {
        if (Band && Band.HasPawns(CurrentPlayer))
        {
            // Clearing possible moves if needed
            if (PossibleMoves != null)
            {
                PossibleMoves.Clear();
            }

            PossibleMoves = new PossibleMoves(this, Band, Dices, true);
            
            // TODO:
            // Checking if there are any moves
            // If so, do the first move and return true
            // else return false as it means that we didn't make a move

            PossibleMoves.DoFirstMove();
            return true;
        }
        else
        {
            Logger.Log(this, "Band has no {0} pawns left. Player can move now.", CurrentPlayer);
            return true;
        }
    }

    /// <summary>
    /// Finds what GameState should be next after the current one.
    /// </summary>
    /// <returns>State that should successor to current one.</returns>
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

    /// <summary>
    /// Checks if given index is a valid Field index.
    /// </summary>
    /// <param name="InIndex">Index that we want to check if is valid.</param>
    /// <returns>Whether given index is valid Field index or not.</returns>
    protected bool IsValidFieldIndex(int InIndex)
    {
        return InIndex >= 0 && InIndex < FieldsOrder.Count;
    }

    protected virtual void Field_OnClicked(FieldController InField)
    {
        if (State == GameState.RedPlayerMoves || State == GameState.WhitePlayerMoves)
        {
            // If the field on which we clicked is one of possible target fields
            if (PossibleMoves != null && PossibleMoves.IsMovePossible(InField))
            {
                PossibleMoves.MoveTo(InField);
            }
        }
    }

    protected void Dice_OnUsed(DiceController InDice, DiceState InState)
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

    protected void Dice_OnRolled(DiceController InDice, int InDots)
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
            MovePawnFromBand();

            SwitchGameState();
        }
    }

#if UNITY_EDITOR
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Editor stuff

    /// <summary>
    /// Sets given Field under given index. Works only in editor.
    /// </summary>
    /// <param name="InField">The Field we want to set under given index.</param>
    /// <param name="InIndex">The index under which we want to place given Field.</param>
    protected void SetField(FieldController InField, int InIndex)
    {
        if(Application.isEditor)
        {
            if (InField && IsValidFieldIndex(InIndex))
            {
                FieldsOrder[InIndex] = InField;
            }
        }
    }

    /// <summary>
    /// Just a simple tool in editor to rename and set order of fields in existing GameManager.
    /// In order to work, user has to select a GameObject that is a parent for all Fields.
    /// Fields will be ordered the same as those are placed in the editor (hierarchy-wise).
    /// </summary>
    [MenuItem("BackgammonHelpers/Setup fields order")]
    private static void SetupFieldsOrder()
    {
        var temp = Selection.activeGameObject.GetComponentsInChildren<FieldController>();
        var GM = Find();

        if (GM)
        {
            for (int iField = 0; iField < temp.Length; ++iField)
            {
                temp[iField].transform.name = string.Format("Field_{0}", iField + 1);
                GM.SetField(temp[iField], iField);
            }

            EditorUtility.SetDirty(GM);
        }
    }
    
    /// <summary>
    /// A validator for above menu option. Causes that the option won't be able to be used if incorrect GameObject is selected.
    /// </summary>
    /// <returns>Whether user selected proper GameObject or not.</returns>
    [MenuItem("BackgammonHelpers/Setup fields order", true)]
    private static bool SetupFieldsOrderValidation()
    {
        if (Selection.activeGameObject != null)
        {
            return Selection.activeGameObject.GetComponentsInChildren<FieldController>().Length > 0;
        }

        return false;
    }
#endif
}