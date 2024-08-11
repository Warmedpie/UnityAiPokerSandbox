using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Check call is an example AI
//This AI will always check or call.

public class CheckCallFoldNonImproveEV : PlayerAction {

    [SerializeField] GenerateAllHoleCards generateAllHoleCards;

    //Let the Dealer know this is an AI
    public override PlayerTypes type { get { return PlayerTypes.AI; } }

    public override Action PlayBehavior(List<Card> river, int amountToCall, int pot) {

        //Get list of all possible hands we might play against
        List<List<Card>> holeCards = generateAllHoleCards.GetHoleCards();

        //Calculate what % of these hands are ahead (we are not accounting for OUTS to improve into EV for this bot).

        float ourScore = 0;

        if (river.Count == 0) {
            List<Card> ourHand = new List<Card>() { playerInfo.GetCard(0), playerInfo.GetCard(1) };
            ourScore = Rank.RankHand(ourHand);

            Debug.Log("we have " + ourHand.Count);
        }
        if (river.Count == 3) {
            List<Card> ourHand = new List<Card>(river) { playerInfo.GetCard(0), playerInfo.GetCard(1) };
            ourScore = Rank.RankHand(ourHand);

            Debug.Log("we have " + ourHand.Count);
        }
        if (river.Count == 4) {
            List<Card> ourHand = new List<Card>(river) { playerInfo.GetCard(0), playerInfo.GetCard(1) };
            ourScore = Rank.Rank6Hands(ourHand);

            Debug.Log("we have " + ourHand.Count);
        }
        if (river.Count == 5) {
            List<Card> ourHand = new List<Card>(river) { playerInfo.GetCard(0), playerInfo.GetCard(1) };
            ourScore = Rank.Rank7Hands(ourHand);
        }
        float wins = 0;
        float chops = 0;
        float loss = 0;

        for (int i = 0; i < holeCards.Count; i++) {
            List<Card> cards = new List<Card>(river);
            cards.Add(holeCards[i][0]);
            cards.Add(holeCards[i][1]);

            if (river.Count == 0) {
                float s = Rank.RankHand(cards);
                if (s > ourScore) loss++;
                if (s == ourScore) chops++;
                if (s < ourScore) wins++;
            }
            if (river.Count == 3) {
                float s = Rank.RankHand(cards);
                if (s > ourScore) loss++;
                if (s == ourScore) chops++;
                if (s < ourScore) wins++;
            }
            if (river.Count == 4) {
                float s = Rank.Rank6Hands(cards);
                if (s > ourScore) loss++;
                if (s == ourScore) chops++;
                if (s < ourScore) wins++;
            }
            if (river.Count == 5) {
                float s = Rank.Rank7Hands(cards);
                if (s > ourScore) loss++;
                if (s == ourScore) chops++;
                if (s < ourScore) wins++;
            }
        }

        //Generate EV of calling the amount
        float foldValue = (float)pot * -1;
        float winValue = (float)pot + (float)amountToCall;
        float chopValue = ((float)pot + (float)amountToCall) / 2;

        float EV = ((loss / holeCards.Count) * foldValue) + ((wins / holeCards.Count) * winValue) + ((chops / holeCards.Count) * chopValue);

        Debug.Log("EV: " + EV + "(W/D/L): " + "(" + wins + "/" + chops + "/" + loss + ")");
        //If we have positive EV, we call, if we have negative EV, we fold.
        if (EV >= 0) {
            //This number is the amount the AI will bet, since it is a check call bot, we will always bet either all our money, or match the "to call" amount
            int bet = Mathf.Min(amountToCall - playerInfo.amountBet, playerInfo.money);

            //The check call bot example will simply make its action 1 (for bet) and the bet amount above
            return new Action(ActionTypes.BET, bet);
        }
        else if (amountToCall == 0) {
            return new Action(ActionTypes.BET, 0);
        }
        else {
            return new Action(ActionTypes.FOLD, 0);
        }
    }
}
