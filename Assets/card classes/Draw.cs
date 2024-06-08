using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Draw : MonoBehaviour {
    [SerializeField] Material[] ranks;
    [SerializeField] Material[] suits;


    MeshRenderer rank;
    MeshRenderer suit;

    public void Init(Card c) {

        rank = transform.GetChild(0).GetComponent<MeshRenderer>();
        suit = transform.GetChild(1).GetComponent<MeshRenderer>();

        /*
        ----- Ranks -----
            0: blank card
            1: Ace
            2-10: Two-Ten
            11: Jack
            12: Queen
            13: King
        */
        rank.material = ranks[c.rank];


        /*
        ----- Suits -----
            0: Hearts 
            1: Diamonds
            2: Clubs
            3: Spades
        */
        suit.material = suits[c.suit];
    }
}
