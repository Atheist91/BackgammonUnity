using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class GameManagerTests
{
    [Test]
    public void A_IsPlaced()
    {
        GameManager GM = GameManager.Find();
        Assert.NotNull(GM, "GameManager has to be placed on scene.");
    }

    [Test]
    public void B_IsRedPawnTemplateValid()
    {
        GameManager GM = GameManager.Find();

        Assert.NotNull(GM.PawnRedPrefab, "GameManager has to have reference to RedPawnPrefab.");
        Assert.NotNull(GM.PawnRedPrefab.GetComponent<PawnController>(), "RedPawnPrefab has to have reference to an object that has PawnController component.");
        Assert.IsTrue(GM.PawnRedPrefab.GetComponent<PawnController>().GetColor() == PlayerColor.Red, "RedPawnPrefab has to be a red pawn.");
        Assert.IsTrue(PrefabUtility.GetPrefabParent(GM.PawnRedPrefab) == null && PrefabUtility.GetPrefabObject(GM.PawnRedPrefab) != null, "Given object has to be a prefab located in assets browser.");
    }

    [Test]
    public void C_IsWhitePawnTemplateValid()
    {
        GameManager GM = GameManager.Find();

        Assert.NotNull(GM.PawnWhitePrefab, "GameManager has to have reference to PawnWhitePrefab.");
        Assert.NotNull(GM.PawnWhitePrefab.GetComponent<PawnController>(), "PawnWhitePrefab has to have reference to an object that has PawnController component.");
        Assert.IsTrue(GM.PawnWhitePrefab.GetComponent<PawnController>().GetColor() == PlayerColor.White, "PawnWhitePrefab has to be a white pawn.");
        Assert.IsTrue(PrefabUtility.GetPrefabParent(GM.PawnWhitePrefab) == null && PrefabUtility.GetPrefabObject(GM.PawnWhitePrefab) != null, "Given object has to be a prefab located in assets browser.");
    }

    [Test]
    public void D_AreFieldValid()
    {
        GameManager GM = GameManager.Find();

        Assert.NotNull(GM.FieldsOrder, "FieldsOrder list has to be initialized.");
        Assert.IsTrue(GM.FieldsOrder.Count == 24, "FieldsOrder list has to have exactly 24 elements");
        if (GM.FieldsOrder != null)
        {
            for(int iField = 0; iField < GM.FieldsOrder.Count; ++iField)
            {
                Assert.NotNull(GM.FieldsOrder[iField], "Field {0} is null, however it has to be set.", iField);
            }
        }
    }

    [Test]
    public void E_AreDicesValid()
    {
        GameManager GM = GameManager.Find();

        Assert.NotNull(GM.Dices, "Dices list has to be initialized.");
        Assert.IsTrue(GM.Dices.Count == 2, "Dices list has to have exactly 2 elements");
        if (GM.Dices != null)
        {
            for (int iDice = 0; iDice < GM.Dices.Count; ++iDice)
            {
                Assert.NotNull(GM.Dices[iDice], "Dice {0} is null, however it has to be set.", iDice);
            }
        }
    }

    [Test]
    public void F_IsBandValid()
    {
        GameManager GM = GameManager.Find();

        Assert.NotNull(GM.Band);
    }
}