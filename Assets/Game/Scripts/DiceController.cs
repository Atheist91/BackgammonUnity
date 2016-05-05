using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public delegate void RolledDelegate(DiceController InDice, int InDots);
    public event RolledDelegate OnRolled;

    public SpriteRenderer MySpriteRenderer;    
    public List<Sprite> Sprites = new List<Sprite>( new Sprite[6] );
    /// <summary>
    /// Reference to GameManager component that exists on the scene.
    /// </summary>
    public GameManager GameManager;

    public int MinFlips = 5;
    public int MaxFlips = 15;
    public bool bConstantFlipTime = true;
    public float MinFlipTime = 0.1f;
    public float MaxFlipTime = 0.4f;
    protected int SpriteIndex = 0;
    protected bool bCanRoll = false;
    protected bool bFinishedRolling = true;

    void Start()
    {
        if (Sprites.Count == 6)
        {
            for (int iSprite = 0; iSprite < 6; ++iSprite)
            {
                if (Sprites[iSprite] == null)
                {
                    Debug.LogError(string.Format("Sprite {0} isn't set properly.", iSprite));
                }
            }
        }
        else
        {
            Debug.LogError("Amount of sprites in dice must equal 6.");
        }

        if (GameManager == null)
        {
            Logger.Warning(this, "Component didn't have a reference to GameManager set up, so it had to find the object on it's own. It's better to setup the reference manually.");
            GameManager = GameManager.Find();
        }

        if (GameManager != null)
        {
            GameManager.OnStateChanged += GameManager_OnStateChanged;
        }
        else
        {
            Logger.Error(this, "Component couldn't be initialized properly because reference to GameManager was missing.");
        }        
    }

    private void GameManager_OnStateChanged(GameState InOldState, GameState InNewState)
    {        
        if (InNewState == GameState.RedPlayerRolls || InNewState == GameState.WhitePlayerRolls)
        {
            Reset();
        }

        if(InNewState != GameState.Init)
        {
            gameObject.SetActive(true);
        }
    }

    public void Roll()
    {
        if(bCanRoll)
        {
            bCanRoll = false;
            bFinishedRolling = false;
            StartCoroutine(RollCoroutine());
        }
        else
        {
            
        }
    }

    public void Use(bool bInWhole = true)
    {
        
    }

    public IEnumerator RollCoroutine()
    {
        // initial delay, so dices won't start rolling at the same time
        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));

        // generating amount of flips this single dice will make
        int flips = Random.Range(MinFlips, MaxFlips + 1);

        // 
        int tempIndex = 0;
        for (; flips >= 0; --flips)
        {
            tempIndex = Random.Range(0, 6);
            MySpriteRenderer.sprite = Sprites[tempIndex];

            yield return new WaitForSeconds(bConstantFlipTime ? MinFlipTime : Random.Range(MinFlipTime, MaxFlipTime));
        }

        SpriteIndex = tempIndex;
        bFinishedRolling = true;

        if (OnRolled != null)
        {
            OnRolled(this, GetDots());
        }
    }

    public int GetDots()
    {
        return SpriteIndex + 1;
    }

    public void Reset()
    {
        bCanRoll = true;
    }

    public bool HasFinishedRolling()
    {
        return bFinishedRolling;
    }
}