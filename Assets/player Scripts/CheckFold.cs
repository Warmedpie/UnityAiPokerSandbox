using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Check call is an example AI
//This AI will always check or fold.

public class CheckFold : PlayerAction {

    //Let the Dealer know this is an AI
    public override PlayerTypes type { get { return PlayerTypes.AI; } }
       

    public override Action PlayBehavior(List<Card> river, int amount_toCall, int pot) {

        //If there is a bet, the bot will fold (flag 0)
        if (amount_toCall != 0) return new Action(ActionTypes.FOLD, 0);

        //Otherwise the bot will check (flag for bet, with amount 0)
        return new Action(ActionTypes.BET, 0);
    }
}
