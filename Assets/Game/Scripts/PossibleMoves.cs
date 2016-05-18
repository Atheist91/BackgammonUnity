using System.Collections.Generic;
using System.Linq;

public class Move
{
    protected int StartIndex = -1;
    protected int Steps = -1;
    protected int DestinationIndex = -1;
    protected PawnsContainer Start = null;
    protected FieldController DestinationField = null;
    protected GameManager GameManager = null;
    protected List<DiceController> Dices = new List<DiceController>();
    protected bool bAreDicesTheSame = false;

    public Move(GameManager InGameManager, PawnsContainer InContainer, DiceController[] InDices, bool bInAreDicesTheSame)
    {
        GameManager = InGameManager;
        if (GameManager)
        {
            bAreDicesTheSame = bInAreDicesTheSame;

            Dices = InDices.ToList();
            Steps = Dices.Select(d => d.GetDots()).Sum();

            Start = InContainer;
            StartIndex = GameManager.GetIndexOf(Start);

            DestinationIndex = StartIndex + (GameManager.GetPlayer() == PlayerColor.Red ? Steps : -Steps);
            DestinationField = GameManager.GetField(DestinationIndex);
        }

        Logger.Log(this, "Move created. Start band: {0}, destination: {1}({2}), steps: {3}, dices: {4}({5}), valid: {6}",
            Start ? Start.name : "NULL", DestinationIndex, DestinationField ? DestinationField.name : "NULL", Steps, Dices.Count, string.Join(", ", Dices.Select(d => d.name).ToArray()), IsValid());
    }

    public bool IsValid()
    {
        bool bAllUsable = true;
        foreach(DiceController dice in Dices)
        {
            if(dice.GetUsageState() == DiceState.FullyUsed)
            {
                bAllUsable = false;                
            }
        }

        return bAllUsable && DestinationField != null && Start != null && DestinationField.IsRoomForPawn(GameManager.GetPlayer());
    }

    public void Show()
    {
        if (DestinationField)
        {
            DestinationField.Highlight();
        }
    }

    public void Hide()
    {
        if (DestinationField)
        {
            DestinationField.Reset();
        }
    }

    public int GetSteps()
    {
        return Steps;
    }

    public PawnsContainer GetDestination()
    {
        return DestinationField;
    }

    public PawnsContainer GetStart()
    {
        return Start;
    }

    public override string ToString()
    {
        return string.Format("[{0}->{1}({2} steps)]", StartIndex, DestinationIndex, Steps);
    }

    public void DoMove()
    {
        Logger.Log(this, "Making move {0}. Dices required by this move {1}. Rolled dices were the same [{2}]", ToString(), Dices.Count, bAreDicesTheSame);
        if (IsValid())
        {            
            Start.MovePawn(DestinationField);
            
            foreach (DiceController dice in Dices)
            {
                dice.Use(!bAreDicesTheSame);
            }
        }
        else
        {
            // Should never happen
            Logger.Warning(this, "Couldn't make a move {0} because it's invalid.", ToString());
        }
    }
}

public class PossibleMoves
{
    public List<Move> Moves = new List<Move>();
    protected List<DiceController> Dices = new List<DiceController>();

    public PossibleMoves(GameManager InGameManager, PawnsContainer InContainer, List<DiceController> InDices, bool bSingleDiceMoves = false)
    {
        if(InGameManager)
        {
            Dices = InDices;

            // Doubling amount of moves if amount of dots on both dices are the same.
            bool bDicesAreTheSame = Dices[0].GetDots() == Dices[1].GetDots();
            if (bDicesAreTheSame && !bSingleDiceMoves)
            {
                List<DiceController> temp = new List<DiceController>();
                foreach (DiceController dice in Dices)
                {
                    if (dice.GetUsageState() != DiceState.FullyUsed)
                    {
                        temp.Add(dice);
                        if (dice.GetUsageState() == DiceState.NotUsed)
                        {
                            temp.Add(dice);
                        }
                    }
                }

                if (temp.Count > 1)
                {
                    for (int iCombination = 1; iCombination <= temp.Count; ++iCombination)
                    {
                        Moves.Add(new Move(InGameManager, InContainer, temp.Take(iCombination).ToArray(), bDicesAreTheSame));
                    }
                }
                else
                {
                    if (temp.Count == 1)
                    {
                        Moves.Add(new Move(InGameManager, InContainer, temp.ToArray(), bDicesAreTheSame));
                    }
                    else
                    {
                        // error
                    }
                }
            }
            else
            {
                Moves.Add(new Move(InGameManager, InContainer, new DiceController[] { Dices[0] }, bDicesAreTheSame));
                Moves.Add(new Move(InGameManager, InContainer, new DiceController[] { Dices[1] }, bDicesAreTheSame));
                if (!bSingleDiceMoves)
                {
                    Moves.Add(new Move(InGameManager, InContainer, new DiceController[] { Dices[0], Dices[1] }, bDicesAreTheSame));
                }
            }

            Logger.Log(this, "Possible moves based on dices roll [{0}-{1}] are: {2}", Dices[0].GetDots(), Dices[1].GetDots(), string.Join(", ", Moves.Select(m => m.GetSteps().ToString()).ToArray()));

            // Removing invalid moves
            for (int iMove = Moves.Count - 1; iMove >= 0; --iMove)
            {
                if (Moves[iMove].IsValid())
                {
                    Moves[iMove].Show();
                }
                else
                {
                    Moves.RemoveAt(iMove);
                }
            }

            Logger.Log(this, "Possible, validated moves are: {0}", string.Join(", ", Moves.Select(m => m.ToString()).ToArray()));
        }
    }

    public void Clear()
    {
        foreach (Move move in Moves)
        {
            move.Hide();
        }

        Moves.Clear();
    }

    public bool HasAnyMoves()
    {
        return Moves.Count > 0;
    }

    public bool IsMovePossible(FieldController InField)
    {
        foreach (Move move in Moves)
        {
            if(move.GetDestination() == InField)
            {
                return true;
            }
        }

        return false;
    }

    public void DoFirstMove()
    {
        if(Moves.Count > 0)
        {
            Moves.First().DoMove();
        }

        Clear();
    }

    public void MoveTo(FieldController InDestination)
    {
        // Getting the move that we'll be doing
        int moveIndex = -1;
        Move move = FindMove(InDestination, ref moveIndex);
        if(move != null && move.IsValid())
        {
            move.DoMove();
        }

        Clear();
    }

    protected Move FindMove(FieldController InDestination, ref int OutMoveIndex)
    {
        for(int iMove = 0; iMove < Moves.Count; ++iMove)
        {
            if (Moves[iMove].GetDestination() == InDestination)
            {
                OutMoveIndex = iMove;
                return Moves[iMove];
            }
        }

        OutMoveIndex = -1;
        return null;
    }

    protected void RemoveMove(int InMoveIndex)
    {
        if(IsValidMoveIndex(InMoveIndex))
        {
            Moves[InMoveIndex].Hide();
            Moves.RemoveAt(InMoveIndex);
        }
    }

    protected bool IsValidMoveIndex(int InIndex)
    {
        return InIndex >= 0 && InIndex < Moves.Count;
    }    
}