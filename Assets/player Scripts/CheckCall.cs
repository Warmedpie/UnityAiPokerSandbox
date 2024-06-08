using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Check call is an example AI
//This AI will always check or call.

public class CheckCall : PlayerAction {

    //Let the Dealer know this is an AI
    public override PlayerTypes type { get { return PlayerTypes.AI; } }

    public override Action PlayBehavior(List<Card> river, int amountToCall, int pot) {

        //This number is the amount the AI will bet, since it is a check call bot, we will always bet either all our money, or match the "to call" amount
        int bet = Mathf.Min(amountToCall - playerInfo.amountBet, playerInfo.money);

        //The check call bot example will simply make its action 1 (for bet) and the bet amount above
        return new Action(ActionTypes.BET, bet);
    }
}
