using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public class BetterRand {
    public static System.Random rand;

    public static void Init() {
        //rand = new System.Random(Guid.NewGuid().GetHashCode());
        int state = (int)(System.DateTime.Now.Ticks % int.MaxValue);
        rand = new System.Random(state);
        UnityEngine.Random.InitState(state);

        //for (int i = 0; i < 9999; i++)
        //    UnityEngine.Debug.Log(Range(0, 5));
    }

    public static void SetState(int state) {
        rand = new System.Random(state);
    }

    public static int Range(int min, int maxExclusive) {
        return rand.Next(min, maxExclusive);
    }
    public static float Remap(float CurrentVal, float oldMin, float oldMax, float newMin, float newMax) {
        return (CurrentVal - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }
    public static float Range(float min, float max) {
        return Remap((float)rand.NextDouble(),0, 1, min, max);
    }
}

public class Card {
    /*
    ----- Suits -----
        0: Hearts 
        1: Diamonds
        2: Clubs
        3: Spades
        4: NO SUIT
    */
    public int suit = 4;

    /*
    ----- Ranks -----
        0: blank card
        1: Ace
        2-10: Two-Ten
        11: Jack
        12: Queen
        13: King
    */
    public int rank = 0;

    public Card(int s, int r) {
        suit = s;
        rank = r;
    }

}

public enum ActionTypes : int {
    FOLD = 0, BET
};

public class Action {
    public ActionTypes type;
    public int betAmount;

    public Action(ActionTypes t, int b) {
        type = t;
        betAmount = b;
    }
}

public enum PlayerStatus : int {
    PLAYING = 0, ALLIN, FOLDED
}
public class Player {
    //These are the two cards in the Players hand
    private List<Card> hand = new List<Card>();

    public int money = 2000;
    public int amountBet = 0;
    public PlayerStatus PlayerStatus = PlayerStatus.PLAYING;
    public bool playedTurn = false;

    public Card GetCard(int index) {
        return hand[index];
    }

    //Put a card in the Players hand
    public void AddCard(Card c) {
        hand.Add(c);
    }

    public void ResetCards() {
        hand.Clear();
    }

}

public class Dealer : MonoBehaviour {

    // ----- Serialized Fields ----- //
    //The following sets are for setting card textures on the board

    //Cards that Player 1 is holding
    [SerializeField] GameObject[] playerCardsPrefabs;

    //Cards that the opponent is holding
    [SerializeField] GameObject[] villanCardsPrefabs;

    //The cards within the river
    [SerializeField] GameObject[] riverCardsPrefabs;

    //The control scripts for the game
    [SerializeField] GameObject[] playerScripts;

    //This is text to display the games state
    [SerializeField] TMPro.TextMeshProUGUI stateText;

    //Bot play speed
    [SerializeField] float botSpeed = 0.25f;

    //Post Game Speed
    [SerializeField] float postGameSpeed = 5f;

    //Blind about (This version doesnt have little/big blind, however feel free to add this)
    [SerializeField] int blindAmount = 10;

    //On start
    public static Dealer instance;
    private void Start() {
        BetterRand.Init();

        for (int i = 0; i < playerCount; i++) players.Add(new Player());
  
        NewGame();
        instance = this;
    }

    // ----- Game State ----- //
    private void SetState() {
        String str = "Pot:         $" + this.pot + "\n";
        for (int i = 0; i < playerCount; i++) {
            str += "Player " + i + ": $" + players[i].money;
            if (players[i].PlayerStatus == PlayerStatus.FOLDED) str += " FOLDED\n";
            if (players[i].PlayerStatus == PlayerStatus.ALLIN) str += " ALL IN\n";
            else str += "\n";
        }
        stateText.text = str;
    }
    bool resetting = false;

    // ----- Deck information ----- //

    //This is the deck of cards, and all the associated helper functions for dealing, and making a full deck
    private List<Card> deck = new List<Card>();
    private void GenerateDeck() {
        //Clear the deck from previous game
        deck.Clear();

        //Generate the cards
        for (int suit = 0; suit <= 3; suit++) {
            for (int rank = 1; rank <= 13; rank++) {
                deck.Add(new Card(suit, rank));

            }
        }
    }
    private Card GrabCard() {
        //Get a random card from the deck
        int index = BetterRand.Range(0,deck.Count);
        Card picked = deck[index];

        //Remove the card from the deck
        deck.RemoveAt(index);

        //return the card
        return picked;
    }

    // ----- Player information ----- //
    int playerCount = 4;
    private List<Player> players = new List<Player>();

    // ----- Dealing information ----- //
    private List<Card> river = new List<Card>();

    //Deals out a new hand
    public void NewGame() {
        resetting = false;
        //Hero cards are at q = 0, these are already face up
        for (int q = 1; q < playerCount; q++) {
            villanCardsPrefabs[((q - 1) * 2) + 0].GetComponent<Draw>().Init(new Card(4,0));
            villanCardsPrefabs[((q - 1) * 2) + 1].GetComponent<Draw>().Init(new Card(4,0));
        }

        //Resets and clears river
        river.Clear();

        //Reset pot
        pot = 0;
        amountToCall = blindAmount;

        for (int q = 0; q < 5; q++) {
            riverCardsPrefabs[q].GetComponent<Draw>().Init(new Card(4, 0));
        }

        //Generate a new deck
        GenerateDeck();

        //number of playing Players (Players who have neither folded or all-ined)
        remainingPlayers = players.Count;

        //Add two cards to each Players hand
        for (int i = 0; i < playerCount; i++) {
            players[i].ResetCards();

            if (players[i].money == 0) {
                remainingPlayers -= 1;
                players[i].PlayerStatus = PlayerStatus.FOLDED;
            }

            players[i].AddCard(GrabCard());
            players[i].AddCard(GrabCard());

            playerScripts[i].GetComponent<PlayerAction>().SetPlayerInfo(players[i]);
            players[i].playedTurn = false;
            players[i].PlayerStatus = PlayerStatus.PLAYING;
            players[i].amountBet = 0;
        }

        //Set turn to the first playable player (if player 0 is out of chips)
        turn = -1;
        UpdateTurn();

        playerCardsPrefabs[0].GetComponent<Draw>().Init(players[0].GetCard(0));
        playerCardsPrefabs[1].GetComponent<Draw>().Init(players[0].GetCard(1));

        //Finally after dealing, take a turn
        SetState();
        TakeTurn();

    }
    //Deal will behave differently depending on the stage of the game we are in
    public void Deal() {

        if (resetting) return;

        int gameState = river.Count;

        for (int i = 0; i < playerCount; i++) {
            players[i].playedTurn = false;
        }

        //Set turn to the first playable player
        if (remainingPlayers > 1) {
            turn = -1;
            UpdateTurn();
        }

        //First we need to do the flop
        if (river.Count == 0) {
            river.Add(GrabCard());
            river.Add(GrabCard());
            river.Add(GrabCard());

            riverCardsPrefabs[0].GetComponent<Draw>().Init(river[0]);
            riverCardsPrefabs[1].GetComponent<Draw>().Init(river[1]);
            riverCardsPrefabs[2].GetComponent<Draw>().Init(river[2]);
        }
        //If there aren't 5 cards, deal another
        else if (river.Count != 5) {
            river.Add(GrabCard());
            riverCardsPrefabs[river.Count - 1].GetComponent<Draw>().Init(river[river.Count - 1]);
        }

        //if all 5 cards are added, we calculate the winner
        else {
            //Hero cards are at q = 0, these are already face up
            for (int q = 1; q < playerCount; q++) {
                villanCardsPrefabs[((q-1) * 2) + 0].GetComponent<Draw>().Init(players[q].GetCard(0));
                villanCardsPrefabs[((q - 1) * 2) + 1].GetComponent<Draw>().Init(players[q].GetCard(1));
            }

            int winner = 0;
            float winningScore = 0;
            for (int q = 0; q < playerCount; q++) {
                if (players[q].PlayerStatus != PlayerStatus.FOLDED) {
                    List<Card> cards = new List<Card>(river);
                    cards.Add(players[q].GetCard(0));
                    cards.Add(players[q].GetCard(1));

                    float score = Rank.Rank7Hands(cards);

                    Debug.Log("Player " + q + " Scores: " + score);

                    if (score > winningScore) {
                        winningScore = score;
                        winner = q;
                    }
                }
            }

            Debug.Log("Player " + winner + "wins $" + pot);
            players[winner].money += pot;

            resetting = true;
            StartCoroutine(ResetGame(postGameSpeed));

            return;
        }


        //If every Player (besides 1) is either ALL IN or FOLDED, just keep dealing
        if (remainingPlayers <= 1 && gameState != 5) {;
            Deal();
            return;
        }
        else {
            TakeTurn();
        }

        return;
    }

    // ----- Game Flow information ----- //

    //The Player in slot 0 is to play
    int turn = 0;
    int amountToCall = 0;
    int pot = 0;
    int remainingPlayers;

    void TakeTurn() {
        //Return if Player is either folded or ALL IN
        if (players[turn].PlayerStatus != PlayerStatus.PLAYING) return;

        if (players[turn].amountBet == amountToCall && players[turn].playedTurn) return;

        //AI action simply calls the AI behavior function
        if (playerScripts[turn].GetComponent<PlayerAction>().type == PlayerTypes.AI) {
            playerScripts[turn].GetComponent<PlayerAction>().SetPlayerInfo(players[turn]);
            Action play = playerScripts[turn].GetComponent<PlayerAction>().PlayBehavior(river, amountToCall, pot);
            DoPlayerAction(play, botSpeed);

        }
        //Players turn
        else {
            playerScripts[turn].GetComponent<HumanPlayer>().SetValues(amountToCall, pot);
        }
    }

    void UpdateTurn() {

        //If every Player (besides 1) is either ALL IN or FOLDED, just keep dealing
        if (remainingPlayers < 1) {
            Deal();
            return;
        }

        turn++;
        //All Players have played
        if (turn == playerCount) turn = 0;

        int playersChecked = 0;
        //Loop through Players to see if they can play
        int i = turn;
        while (true) {

            //Reset the loop if we are at the last Player
            if (i == playerCount) i = 0;

            //if they can play, make it their turn
            if (players[i].PlayerStatus == PlayerStatus.PLAYING && !(players[i].amountBet == amountToCall && players[i].playedTurn)) {

                Debug.Log(i);

                turn = i;
                TakeTurn();
                return;
            }

            playersChecked++;
            i++;
            //if we checked every single Player in the board and still didnt find any playable Players, break the loop
            if (playersChecked > playerCount + 1) break;

        }

        //We only reach this code if noone can play, so deal new cards
        Deal();

    }

    public void DoPlayerAction(Action play, float secs = 0) {

        if (play.type == ActionTypes.FOLD) {
            players[turn].PlayerStatus = PlayerStatus.FOLDED;
            remainingPlayers -= 1;

            Debug.Log("player " + turn + " folds");

            SetState();
            StartCoroutine(WaitUpdateTurn(secs));

            return;
        }

        //Take away the money from the Player, and update the total amount he has bet
        players[turn].playedTurn = true;
        players[turn].money -= play.betAmount;
        players[turn].amountBet += play.betAmount;

        //check for all in
        if (players[turn].money == 0) {
            players[turn].PlayerStatus = PlayerStatus.ALLIN;

            Debug.Log("player " + turn + " ALL INS");
            remainingPlayers -= 1;
        }
        else {
            Debug.Log("player " + turn + " bets $" + play.betAmount);
        }
        //Update the pot, and the amount needed to bet
        pot += play.betAmount;

        if (amountToCall < players[turn].amountBet) {
            amountToCall = players[turn].amountBet;
        }

        SetState();
        StartCoroutine(WaitUpdateTurn(secs));

    }

    IEnumerator ResetGame(float secs) {
        yield return new WaitForSeconds(secs);
        NewGame();
    }

    IEnumerator WaitUpdateTurn(float secs) {
        yield return new WaitForSeconds(secs);
        UpdateTurn();
    }
}
