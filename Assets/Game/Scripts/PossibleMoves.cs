using System.Collections.Generic;
using System.Linq;

public class Move
{
    protected int StartIndex = -1;
    protected int Steps = -1;
    protected int DestinationIndex = -1;
    protected FieldController StartField = null;
    protected FieldController DestinationField = null;
    protected GameManager GameManager = null;
    protected List<DiceController> Dices = new List<DiceController>();
    protected bool bAreDicesTheSame = false;

    public Move(GameManager InGameManager, int InStartIndex, DiceController[] InDices, bool bInAreDicesTheSame)
    {
        GameManager = InGameManager;
        bAreDicesTheSame = bInAreDicesTheSame;

        // Getting values for fields passed as params
        StartIndex = InStartIndex;
        Steps = InDices.Select(d => d.GetDots()).Sum();
        Dices = InDices.ToList();

        // Getting references to start and destination field
        if (GameManager)
        {
            DestinationIndex = StartIndex + (GameManager.GetPlayerTurn() == PlayerColor.Red ? Steps : -Steps);

            StartField = GameManager.GetField(StartIndex);
            DestinationField = GameManager.GetField(DestinationIndex);
        }
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

        return bAllUsable && DestinationField != null && StartField != null && DestinationField.IsRoomForPawn(GameManager.GetPlayerTurn());
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

    public FieldController GetDestination()
    {
        return DestinationField;
    }

    public override string ToString()
    {
        return string.Format("[{0}->{1}({2} steps)]", StartIndex, DestinationIndex, Steps);
    }

    public void UseDices()
    {
        Logger.Log(this, "Using {0} dices for move {1} while dices were the same [{2}]", Dices.Count, ToString(), bAreDicesTheSame);
        foreach(DiceController dice in Dices)
        {
            dice.Use(!bAreDicesTheSame);
        }
    }
}

public class PossibleMoves
{
    public List<Move> Moves = new List<Move>();
    protected List<DiceController> Dices = new List<DiceController>();

    public PossibleMoves(GameManager InGameManager, int InStartIndex, List<DiceController> InDices)
    {
        if(InGameManager && InDices.Count == 2)
        {
            Dices = InDices;

            // Doubling amount of moves if amount of dots on both dices are the same.
            bool bDicesAreTheSame = Dices[0].GetDots() == Dices[1].GetDots();
            int Counter = bDicesAreTheSame ? 2 : 1;
            List<DiceController> temp = new List<DiceController>();
            do
            {
                temp.Clear();

                foreach (DiceController dice in Dices)
                {
                    Moves.Add(new Move(InGameManager, InStartIndex, new DiceController[] { dice }, bDicesAreTheSame));
                    temp.Add(dice);
                }

                Moves.Add(new Move(InGameManager, InStartIndex, temp.ToArray(), bDicesAreTheSame));                

                Counter--;
            }
            while (Counter > 0);

            // Adding last move in case the amount of dots were the same on both dices.
            if (bDicesAreTheSame)
            {
                temp.Concat(temp);

                Moves.Add(new Move(InGameManager, InStartIndex, temp.ToArray(), bDicesAreTheSame));
            }

            Logger.Log(this, "Possible moves based on dices roll [{0}-{1}] are: {2}", Dices[0], Dices[1], string.Join(", ", Moves.Select(m => m.GetSteps().ToString()).ToArray()));

            // Removing invalid moves
            for (int iMove = Moves.Count - 1; iMove >= 0; --iMove)
            {
                if(Moves[iMove].IsValid())
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

    public void MoveTo(FieldController InDestination)
    {
        // Getting the move that we'll be doing
        int moveIndex = -1;
        Move move = FindMove(InDestination, ref moveIndex);
        if(move != null)
        {
            move.UseDices();
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