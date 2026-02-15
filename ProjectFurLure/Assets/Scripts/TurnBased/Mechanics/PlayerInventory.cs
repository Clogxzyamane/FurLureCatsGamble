using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInventory : MonoBehaviour
{
    [Header("Deck")]
    [Tooltip("Cards available in the player's draw deck.")]
    public List<Card> deck = new List<Card>();

    [Tooltip("Discard pile, used for reshuffle when deck is empty.")]
    public List<Card> discard = new List<Card>();

    [Tooltip("Default hand size when drawing.")]
    public int handSize = 3;

    /// <summary>
    /// Shuffle deck in-place (Fisher–Yates).
    /// </summary>
    public void ShuffleDeck()
    {
        if (deck == null || deck.Count <= 1) return;
        for (int i = 0; i < deck.Count - 1; i++)
        {
            int j = Random.Range(i, deck.Count);
            var tmp = deck[i];
            deck[i] = deck[j];
            deck[j] = tmp;
        }
    }

    /// <summary>
    /// Draw one card. If deck empty, reshuffle discard into deck.
    /// Returns null if no cards available.
    /// </summary>
    public Card DrawOne()
    {
        if (deck == null) deck = new List<Card>();
        if (deck.Count == 0 && discard != null && discard.Count > 0)
        {
            deck.AddRange(discard);
            discard.Clear();
            ShuffleDeck();
        }

        if (deck.Count == 0) return null;

        Card c = deck[0];
        deck.RemoveAt(0);
        return c;
    }

    /// <summary>
    /// Refill null slots in the provided hand array up to handSize using this inventory.
    /// </summary>
    public void RefillHand(Card[] hand)
    {
        if (hand == null) return;
        for (int i = 0; i < hand.Length && i < handSize; i++)
        {
            if (hand[i] == null)
                hand[i] = DrawOne();
        }
    }

    /// <summary>
    /// Add a card to the deck (top or bottom).
    /// </summary>
    public void AddCardToDeck(Card card, bool top = false)
    {
        if (card == null) return;
        if (deck == null) deck = new List<Card>();
        if (top) deck.Insert(0, card);
        else deck.Add(card);
    }

    /// <summary>
    /// Send a card to discard pile.
    /// </summary>
    public void DiscardCard(Card card)
    {
        if (card == null) return;
        if (discard == null) discard = new List<Card>();
        discard.Add(card);
    }
}
