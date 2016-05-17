using UnityEngine;

public enum PlayerColor
{
    Red,
    White
}

public class PawnController : MonoBehaviour
{
    public SpriteRenderer MySpriteRenderer;
    public PlayerColor Color = PlayerColor.Red;
    public float AlphaWhenFaded = 0.35f;

    protected GameManager GameManager;
    protected PawnsContainer Container;
    protected FieldController Field;

    public virtual void SetGameManager(GameManager InGameManager)
    {
        GameManager = InGameManager;
        if(GameManager)
        {
            GameManager.OnStateChanged += GameManager_OnStateChanged;
        }
    }

    private void GameManager_OnStateChanged(GameState InOldState, GameState InNewState)
    {
        bool bFade = false;
        if(InNewState == GameState.RedPlayerMoves)
        {
            bFade = Color == PlayerColor.White;
        }
        else if (InNewState == GameState.WhitePlayerMoves)
        {
            bFade = Color == PlayerColor.Red;
        }

        if(MySpriteRenderer)
        {
            MySpriteRenderer.color = new Color(1f, 1f, 1f, bFade ? AlphaWhenFaded : 1f);
        }       
    }

    public virtual void SetOrder(int InOrder)
    {
        if(MySpriteRenderer != null)
        {
            MySpriteRenderer.sortingOrder = InOrder;
        }
        else
        {
            // error
        }
    }

    public virtual void SetParent(PawnsContainer InContainer)
    {
        if(InContainer != null)
        {
            transform.parent = InContainer.transform;
            Container = InContainer;
            Field = Container as FieldController;
        }
        else
        {
            // error
        }
    }

    void OnMouseDown()
    {
        // If pawn is placed on any Field
        if(Field != null)
        {
            // If we got proper reference to GameManager
            if(GameManager != null)
            {
                // If the game is in state that allows to make a move with Pawn
                if (GameManager.CanPawnBeMoved(this))
                {
                    // Showing possible moves that can be done by any Pawn lying on the field at which this Pawn is.
                    // It's because we won't be moving the Pawn that was clicked, but instead we'll move the one from the top of the Field.
                    GameManager.ShowPossibleMoves(Field);
                }
            }
            else
            {
                Logger.Error(this, "Can't show possible moves, Pawn has no reference to GameManager.");
            }
        }
    }

    public FieldController GetField()
    {
        return Field;
    }

    public PlayerColor GetColor()
    {
        return Color;
    }
}