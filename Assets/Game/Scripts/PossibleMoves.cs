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

    public Move(GameManager InGameManager, int InStartIndex, int InSteps)
    {
        GameManager = InGameManager;

        // Getting values for fields passed as params
        StartIndex = InStartIndex;
        Steps = InSteps;

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
        return DestinationField != null && StartField != null && DestinationField.IsRoomForPawn(GameManager.GetPlayerTurn());
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
}

public class PossibleMoves
{
    public List<Move> Moves = new List<Move>();

    public PossibleMoves(GameManager InGameManager, int InStartIndex, int InFirstDiceValue, int InSecondDiceValue)
    {
        if(InGameManager)
        {
            // Doubling amount of moves if amount of dots on both dices are the same.
            int Counter = InFirstDiceValue == InSecondDiceValue ? 2 : 1;
            do
            {
                Moves.Add(new Move(InGameManager, InStartIndex, InFirstDiceValue));
                Moves.Add(new Move(InGameManager, InStartIndex, InSecondDiceValue));
                Moves.Add(new Move(InGameManager, InStartIndex, InFirstDiceValue + InSecondDiceValue));

                Counter--;
            }
            while (Counter > 0);

            // Adding last move in case the amount of dots were the same on both dices.
            if (InFirstDiceValue == InSecondDiceValue)
            {
                Moves.Add(new Move(InGameManager, InStartIndex, InFirstDiceValue * 4));
            }

            Logger.Log(this, "Possible moves based on dices roll [{0}-{1}] are: {2}", InFirstDiceValue, InSecondDiceValue, string.Join(", ", Moves.Select(m => m.GetSteps().ToString()).ToArray()));

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
}