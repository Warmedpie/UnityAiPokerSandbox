using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : PlayerAction {

    [SerializeField] TMPro.TextMeshProUGUI infobox;
    [SerializeField] Button fold;
    [SerializeField] Button check;
    [SerializeField] Button raise;
    [SerializeField] Scrollbar raiseSlider;

    //Let the Dealer know this is an AI
    public override PlayerTypes type { get { return PlayerTypes.HUMAN; } }

    //Variables for display
    int pot = 0;
    int callAmount = 0;
    int raiseAmount = 0;

    //Init listeners
    private void Start() {
        fold.onClick.AddListener(FoldFunc);
        check.onClick.AddListener(CallFunc);
        raise.onClick.AddListener(RaiseFunc);
        raiseSlider.onValueChanged.AddListener(delegate { RaiseVal(); });
    }

    //This function is only used by the AI players, it is required to be here to satisfy the abstract requirment
    public override Action PlayBehavior(List<Card> river, int amountToCall, int pot) {

        return new Action(0,0);
    }
    
    //On a humans turn, the dealer sends information regarding the betting and pot
    public void SetValues(int amountToCall, int p) {
        pot = p;
        callAmount = amountToCall - playerInfo.amountBet;

        SetInfo();
    }

    //Update display info
    private void SetInfo() {
        string s = "Pot: " + pot + "\nAmount to call: " + callAmount + "\nRaise by: " + raiseAmount + "(" + (callAmount + raiseAmount) + ")";

        infobox.text = s;
    }

    //Fold
    private void FoldFunc() {
        Debug.Log("FOLD");
        Dealer.instance.DoPlayerAction(new Action(ActionTypes.FOLD, 0));
    }

    //Call
    private void CallFunc() {
        Debug.Log("CALL");
        Dealer.instance.DoPlayerAction(new Action(ActionTypes.BET, callAmount));
    }

    //Raise
    private void RaiseFunc() {
        Debug.Log("RAISE");
        Dealer.instance.DoPlayerAction(new Action(ActionTypes.BET, Mathf.Min(playerInfo.money, callAmount + raiseAmount)));
    }

    //Update raise amount
    private void RaiseVal() {
        raiseAmount = (int)(playerInfo.money * raiseSlider.value);

        SetInfo();
    }
}