using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateAllHoleCards : MonoBehaviour {
    // Start is called before the first frame update
    List<List<Card>> holeCards = new List<List<Card>>();
    void init() {

        //Pocket Pairs
        for (int suit1 = 0; suit1 <= 3; suit1++) {
            for (int suit2 = suit1 + 1; suit2 <= 3; suit2++) {
                for (int i = 1; i <= 13; i++) {
                    List<Card> hand = new List<Card>();
                    Card c0 = new Card(suit1, i);
                    Card c1 = new Card(suit2, i);
                    hand.Add(c0);
                    hand.Add(c1);

                    holeCards.Add(hand);
                }
            }
        }

        //non pairs
        for (int suit1 = 0; suit1 <= 3; suit1++) {
            for (int suit2 = 0; suit2 <= 3; suit2++) {
                for (int i = 1; i <= 13; i++) {
                    for (int j = i + 1; j <= 13; j++) {
                        List<Card> hand = new List<Card>();
                        Card c0 = new Card(suit1, i);
                        Card c1 = new Card(suit2, j);
                        hand.Add(c0);
                        hand.Add(c1);

                        holeCards.Add(hand);
                    }
                }
            }
        }
    }

    public List<List<Card>> GetHoleCards() {
        if (holeCards.Count == 0) this.init();

        return holeCards;
    }

}
