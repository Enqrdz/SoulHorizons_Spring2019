﻿//Colin
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Contains the deck and discard piles. Handles shuffling and drawing.
/// </summary>
public class scr_Deck : MonoBehaviour {

	public int deckSize = 30;
    public int handSize = 4;
    public scr_NameToCard cardMapping; //maps card name to the scriptable object for that card
    public TextAsset deckList;
    [HideInInspector] public List<scr_Card> hand = new List<scr_Card>();
    [HideInInspector] public List<scr_Card> backupHand = new List<scr_Card>();
    List<scr_Card> deck = new List<scr_Card>();
    List<scr_Card> discard = new List<scr_Card>();
    public List<KeyValuePair<string, int>> cardList = new List<KeyValuePair<string, int>>();

    public void Awake()
    {
        AllocateHandSlots();
        IntializeDeck();
    }

    private void AllocateHandSlots()
    {
        for (int i = 0; i < handSize; i++)
        {
            hand.Add(null);
            backupHand.Add(null);
        }
    }

    private void IntializeDeck()
    {
        if (scr_Inventory.numDecks == 0)
        {
            LoadNewDeck();
            SaveManager.Save();
        }
        else
        {
            LoadDeck();
        }
    }

    /// <summary>
    /// Load the deck list from the file, shuffle, then draw a starting hand.
    /// </summary>
    public void LoadNewDeck()
    {
        StringReader reader = new StringReader(deckList.text);
        string strLine;

        while ((strLine = reader.ReadLine()) != null)
        {
            string[] parsedLine = strLine.Split( ':');
            //check that there was only one colon in the line
            if (parsedLine.Length != 2)
            {
                continue;
            }
            
            //check that the second element is a number
            parsedLine[1] = parsedLine[1].Trim();
            if(!Regex.IsMatch(parsedLine[1], @"^\d+$"))
            {
                continue;
            }
            int quantity = int.Parse(parsedLine[1]);

            //attempt to retrieve the object reference from cardMapping
            scr_Card nextCard = cardMapping.ConvertNameToCard(parsedLine[0]);
            if (nextCard == null)
            {
                continue;
            }

            //add that card to the list a number of times equal to the quantity
            for(int i = 0; i < quantity; i++)
            {
                deck.Add(nextCard);
            }

            //add the card and quantity to an inventory card list
            scr_Inventory.addCard(nextCard, quantity);
            cardList.Add(new KeyValuePair<string, int>(nextCard.cardName, quantity));
        }

        if (deck.Count != deckSize)
        {
            Debug.Log("DeckSize is " + deckSize + ", but " + deck.Count + " cards were added to the deck");
        }

        ShuffleHelper<scr_Card>(deck);
        CheckHandSizeAndDraw();

        scr_Inventory.addDeck(cardList);
    

    }

    /// <summary>
    /// Load the deck list from the existing inventory 
    /// </summary>
    void LoadDeckList(scr_Deck loadDeck)
    {
        deckSize = loadDeck.deckSize;
        handSize = loadDeck.handSize;
        cardMapping = loadDeck.cardMapping;
        hand = loadDeck.hand;
        deck = loadDeck.deck;
        discard = loadDeck.discard;
        cardList = loadDeck.cardList;

        ShuffleHelper<scr_Card>(deck);
        CheckHandSizeAndDraw();
    }

    /// <summary>
    /// Load the deck list from the inventory deck
    /// </summary>
    public void LoadDeck()
    {
        List<KeyValuePair<string, int>> cards = scr_Inventory.deckList[scr_Inventory.deckIndex];
        foreach (KeyValuePair<string, int> pair in cards)
        {
            scr_Card nextCard = cardMapping.ConvertNameToCard(pair.Key);
            if (nextCard == null)
            {
                continue;
            }
            //add that card to the list a number of times equal to the quantity
            for (int i = 0; i < pair.Value; i++)
            {
                deck.Add(nextCard);
            }
            cardList.Add(new KeyValuePair<string, int>(nextCard.cardName, pair.Value));
        }

        if (deck.Count != deckSize)
        {
            Debug.Log("DeckSize is " + deckSize + ", but " + deck.Count + " cards were added to the deck");
        }

        ShuffleHelper<scr_Card>(deck);
        CheckHandSizeAndDraw();


    }

    /// <summary>
    /// Pass a string telling the method what list to shuffle. Options are "deck", "discard", "discard into deck", and "all".
    /// </summary>
    /// <param name="list"></param>
    public void Shuffle(string list)
    {
        if (list.Equals("deck"))
        {
            ShuffleHelper<scr_Card>(deck);
        }
        else if(list.Equals("discard"))
        {
            ShuffleHelper<scr_Card>(discard);
            return;
        }
        else if(list.Equals("discard into deck"))
        {
            //move discard into deck
            foreach (scr_Card card in discard)
            {
                deck.Add(card);
                discard.Remove(card);
            }
            //shuffle
            ShuffleHelper<scr_Card>(deck);
        }
        else if(list.Equals("all"))
        {
            //move everything into deck
            foreach (scr_Card card in discard)
            {
                deck.Add(card);
                discard.Remove(card);
            }
            foreach (scr_Card card in hand)
            {
                deck.Add(card);
                hand.Remove(card);
            }

            //shuffle
            ShuffleHelper<scr_Card>(deck);
            //draw a new hand
            CheckHandSizeAndDraw();
        }
    }

    /// <summary>
    /// Shuffles the list provided.
    /// </summary>
    /// <param name="list"></param>
    void ShuffleHelper<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Make sure that the hand size is correct. Draw cards as necessary.
    /// </summary>
    void CheckHandSizeAndDraw()
    {
        int i = 0;
        foreach (scr_Card item in hand)
        {
            if(item == null)
            {
                Draw(i);
            }
            i++;
        }
    }

    /// <summary>
    /// Remove the top card from the deck and add it to the hand.
    /// </summary>
    /// <param name="index">The index to put the new card at</param>
    public void Draw(int index)
    {
        if (deck.Count > 0)
        {
            hand[index] = deck[0];
            deck.RemoveAt(0);
        }
        else
        {
            //TODO: What do we do when the deck runs out?
            Shuffle("discard into deck");
        }
    }

    /// <summary>
    /// Play the selected card, then move it to the discard pile.
    /// </summary>
    /// <param name="index"></param>
    public void Activate(int index)
    {
        if (hand[index] == null)
        {
            return;
        }

        StartCoroutine(ActivateHelper(index));
    }

    private IEnumerator ActivateHelper(int index)
    {
        scr_Card cardToPlay = hand[index];
        //wait however long is required
        if (cardToPlay.castingTime != 0)
        {
            //start initial effects and stop player input
            cardToPlay.StartCastingEffects();
            scr_InputManager.cannotInput = true;
            yield return new WaitForSeconds(cardToPlay.castingTime);
            scr_InputManager.cannotInput = false;
        }

        hand[index].Activate();
        discard.Add(cardToPlay);
        hand[index] = null;
        //make sure hand size is correct
        CheckHandSizeAndDraw();
    }

    /// <summary>
    /// Play the selected card, then move it to the discard pile.
    /// </summary>
    /// <param name="index"></param>
    public void ActivateBackup(int index)
    {
        if (backupHand[index] == null)
        {
            return;
        }
        StartCoroutine(ActivateBackupHelper(index));
    }

    private IEnumerator ActivateBackupHelper(int index)
    {
        scr_Card cardToPlay = backupHand[index];
        //wait however long is required
        if (cardToPlay.castingTime != 0)
        {
            //start initial effects and stop player input
            cardToPlay.StartCastingEffects();
            scr_InputManager.cannotInput = true;
            yield return new WaitForSeconds(cardToPlay.castingTime);
        }
        scr_InputManager.cannotInput = false;
        backupHand[index].Activate();
        discard.Add(cardToPlay);
        //hand.Remove(cardToPlay);
        backupHand[index] = null;
    }

    /// <summary>
    /// Swaps the card at the given index in the hand with the backup slot for that card. If the backup slot is empty,
    /// the card is moved to the backup slot, then a new card is drawn to replace it.
    /// </summary>
    /// <param name="index"></param>
    public void Swap(int index)
    {
        //swap the card at this index with the card in its backup slot
        scr_Card temp = backupHand[index];
        backupHand[index] = hand[index];
        
        if (temp  == null)
        {
            //there was no card in the backup slot; need to draw a new card at this index in the hand
            Draw(index);
        }
        else
        {
            hand[index] = temp;
        }
    }
}
