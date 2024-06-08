using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : PlayerAction {

    [SerializeField] TMPro.TextMeshProUGUI infobox;
    [SerializeField] Button fold;
    [SerializeField] Button check;
    [SerializeField] Button raise;
    [SerializeField] Scrollbar raise_amount_slider;

    //Let the Dealer know this is an AI
    public override PlayerTypes type { get { return PlayerTypes.HUMAN; } }

    //Variables for display
    int pot = 0;
    int call_amount = 0;
    int raise_amount = 0;

    //Init listeners
    private void Start() {
        fold.onClick.AddListener(FoldFunc);
        check.onClick.AddListener(CallFunc);
        raise.onClick.AddListener(RaiseFunc);
        raise_amount_slider.onValueChanged.AddListener(delegate { RaiseVal(); });
    }

    //This function is only used by the AI players, it is required to be here to satisfy the abstract requirment
    public override Action PlayBehavior(List<Card> river, int amount_toCall, int pot) {

        return new Action(0,0);
    }
    
    //On a humans turn, the dealer sends information regarding the betting and pot
    public void SetValues(int amount_toCall, int p) {
        pot = p;
        call_amount = amount_toCall - PlayerInfo.amount_bet;

        SetInfo();
    }

    //Update display info
    private void SetInfo() {
        string s = "Pot: " + pot + "\nAmount to call: " + call_amount + "\nRaise by: " + raise_amount + "(" + (call_amount + raise_amount) + ")";

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
        Dealer.instance.DoPlayerAction(new Action(ActionTypes.BET, call_amount));
    }

    //Raise
    private void RaiseFunc() {
        Debug.Log("RAISE");
        Dealer.instance.DoPlayerAction(new Action(ActionTypes.BET, call_amount + raise_amount));
    }

    //Update raise amount
    private void RaiseVal() {
        raise_amount = (int)(PlayerInfo.money * raise_amount_slider.value);

        SetInfo();
    }
}