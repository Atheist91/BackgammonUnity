using UnityEngine;
using System.Collections.Generic;

public class PawnsContainer : MonoBehaviour
{
    [Header("Pawns container config")]

    /// <summary>
    /// Maximum amount of pawns placed in a row before the spacing between them starts decreasing.
    /// </summary>
    [Range(0, 30)]
    public int MaxPawnsWithMaxSpacing = 10;
    [Range(0, 30)]
    public int MaxPawns = 15;    
    public float PawnStartingY = -1.30f;
    public float MaxPawnSpacing = 0.6f;
    public float MinPawnSpacing = 0.1f;
    public Vector3 PawnScale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Initial pawns config")]
    [Range(0, 30)]
    public int InitialPawns = 0;
    public PlayerColor InitialPawnsColor = PlayerColor.Red;

    [Header("Basic")]
    public GameManager GameManager;

    protected List<PawnController> Pawns;

    void Start()
    {
        Pawns = new List<PawnController>();

        GameManager GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (GameManager)
        {
            GameObject pawnTemplate = GameManager.GetPawnTemplateObject(InitialPawnsColor);
            GameObject tempPawnObj;

            for (int iPawn = 0; iPawn < InitialPawns; ++iPawn)
            {
                tempPawnObj = GameObject.Instantiate<GameObject>(pawnTemplate);
                if (tempPawnObj != null)
                {
                    AddPawn(tempPawnObj);
                }
                else
                {
                    // error
                }
            }

            Rearrange();
        }
        else
        {
            // error
        }
    }

    public virtual void AddPawn(PawnController InPawn, bool bInDoRearrange = false)
    {
        if (InPawn != null)
        {
            if(Pawns.Count < MaxPawns)
            {
                int firstEmpty = Pawns.IndexOf(null);
                if (firstEmpty == -1)
                {
                    Pawns.Add(null);
                    firstEmpty = Pawns.Count - 1;
                }

                Debug.Log(string.Format("Adding pawn to the triangle [{0}].", firstEmpty));

                InPawn.SetParent(this);
                InPawn.SetOrder(firstEmpty + 10);
                InPawn.SetGameManager(GameManager);

                Pawns[firstEmpty] = InPawn;

                if (bInDoRearrange)
                {
                    Rearrange();
                }
            }
            else
            {
                // warning
            }
        }
        else
        {
            // error
        }
    }

    public virtual void AddPawn(GameObject InPawnObject, bool bInDoRearrange = false)
    {
        if (InPawnObject)
        {
            AddPawn(InPawnObject.GetComponent<PawnController>(), bInDoRearrange);
        }
        else
        {
            // error
        }
    }

    public virtual bool IsRoomForPawn(PlayerColor InPlayer)
    {
        return Pawns.Count < MaxPawns;
    }

    public virtual PawnController GetPawn()
    {
        if(!IsEmpty())
        {
            return Pawns[Pawns.Count - 1];
        }

        return null;
    }

    public virtual PawnController GetPawn(PlayerColor InColor)
    {
        if (!IsEmpty())
        {
            for (int iPawn = Pawns.Count - 1; iPawn >= 0; --iPawn)
            {
                if (Pawns[iPawn].GetColor() == InColor)
                {
                    return Pawns[iPawn];
                }
            }
        }

        return null;
    }

    public virtual void RemovePawn(PawnController InPawn)
    {
        if(InPawn != null)
        {
            int index = Pawns.IndexOf(InPawn);
            if (IsValidPawnIndex(index))
            {
                Pawns.RemoveAt(index);
            }
        }        
    }

    protected bool IsValidPawnIndex(int InIndex)
    {
        return (InIndex >= 0 && InIndex < Pawns.Count);
    }

    protected void Rearrange()
    {
        Transform pawnTransform = null;
        for (int iPawn = 0; iPawn < Pawns.Count; ++iPawn)
        {
            pawnTransform = Pawns[iPawn].transform;
            pawnTransform.localScale = PawnScale;
            pawnTransform.localPosition = new Vector3(0f, PawnStartingY + (iPawn * GetSpacing()));            
        }
    }

    protected float GetSpacing()
    {
        float alpha = Pawns.Count <= MaxPawnsWithMaxSpacing ? 0f : (float)Pawns.Count / 15f;
        return Mathf.Lerp(MaxPawnSpacing, MinPawnSpacing, alpha);
    }

    public bool IsEmpty()
    {
        return Pawns.Count == 0;
    }

    public virtual bool HasRoomForPawn(PlayerColor InPawnColor)
    {
        return true;
    }

    public void MovePawn(PawnsContainer InDestination)
    {
        MovePawn(InDestination, GetPawn());
    }

    public void MovePawn(PawnsContainer InDestination, PlayerColor InPawnColor)
    {
        MovePawn(InDestination, GetPawn(InPawnColor));
    }

    protected void MovePawn(PawnsContainer InDestination, PawnController InPawn)
    {
        RemovePawn(InPawn);
        InDestination.AddPawn(InPawn, true);
    }
}