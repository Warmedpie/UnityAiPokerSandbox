using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerTypes : int {
    HUMAN = 0, AI
};
public abstract class PlayerAction : MonoBehaviour {
    //This is a Player class holding info about cards, amount bet, and amount of money
    public Player playerInfo;
    public abstract PlayerTypes type { get; }

    //Function to set the Player_info
    public void SetPlayerInfo(Player p) {
        playerInfo = p;
    }

    //This is behavior for setting bet amount or folding.
    public abstract Action PlayBehavior(List<Card> river, int amountToCall, int pot);

}
