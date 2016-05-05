using UnityEngine;

public class FieldController : PawnsContainer
{
    public delegate void ClickFieldDelegate(FieldController InField);
    public event ClickFieldDelegate OnClicked;

    public SpriteRenderer HighlightSprite;

    void OnMouseDown()
    {
        if (OnClicked != null)
        {
            OnClicked(this);
        }
    }

    public void Reset()
    {
        if(HighlightSprite != null)
        {
            HighlightSprite.gameObject.SetActive(false);
        }
    }

    public void Highlight()
    {
        if(HighlightSprite != null)
        {
            HighlightSprite.gameObject.SetActive(true);
        }
    }

    public override bool IsRoomForPawn(PlayerColor InPlayer)
    {
        return base.IsRoomForPawn(InPlayer) && IsEmpty() || (GetPawn().GetColor() == InPlayer) || Pawns.Count == 1;
    }
}