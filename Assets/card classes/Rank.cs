using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Rank {

    //This is the main useage function
    //Given 7 cards, returns the best score (from 5 card rankings)
    public static float Rank7Hands(List<Card> cards) {
        float max_score = 0;
        List<List<Card>> premutations = GeneratePremutations(cards);

        for (int i = 0; i < premutations.Count; i++) {
            float score = RankHand(premutations[i]);
            if (score > max_score) {
                max_score = score;
            }
        }

        return max_score;
    }
    //This is the main driver function
    public static float RankHand(List<Card> cards) {
        //Sort the ranks of our hands (Ace high)
        List<int> ranks = SortCards(cards, true);

        //Reduce tie breaks by high cards
        float score = 0;
        for (int i = 0; i < ranks.Count; i++) {
            score += ranks[i] / Mathf.Pow(15,i+1);
        }

        //Score Straight Flush
        if (FindStraight(cards, true) && FindFlush(cards)) score += ScoreStraightFlush(cards, true);
        //Score Straight (ace low)
        else if (FindStraight(cards, false) && FindFlush(cards)) score += ScoreStraightFlush(cards, false);
        //Score Quads
        else if (CountQuads(cards) == 1) score += ScoreQuads(cards);
        //Score FullHouse
        else if (CountPairs(cards) == 1 && CountTrips(cards) == 1) score += ScoreFullHouse(cards);
        //Score Flush
        else if (FindFlush(cards)) score += ScoreFlush(cards);
        //Score Straight (ace high)
        else if (FindStraight(cards,true)) score += ScoreStraight(cards,true);
        //Score Straight (ace low)
        else if (FindStraight(cards, false)) score += ScoreStraight(cards, false);
        //Score Tripples
        else if (CountTrips(cards) == 1) score += ScoreTripples(cards);
        //Score two pairs
        else if (CountPairs(cards) == 2) score += ScoreTwoPair(cards);
        //Score pair
        else if (CountPairs(cards) == 1) score += ScorePair(cards);

        return score;
    }

    // -------- Helper Functions -------- //

    //Sort the cards, used for kicker value computation, as well as straight calculations
    public static List<int> SortCards(List<Card> cards, bool ace_high = true) {
        List<int> ranks = new List<int>();

        for (int i = 0; i < cards.Count; i++) {
            if (cards[i].rank == 1 && ace_high) ranks.Add(14);
            else ranks.Add(cards[i].rank);
        }

        ranks.Sort();
        ranks.Reverse();

        return ranks;
    }

    //Given 7 cards, return all (unique) premutations of 5 cards
    public static List<List<Card>> GeneratePremutations(List<Card> cards) {
        List<List<Card>> premutations = new List<List<Card>>();

        //First we create a list of indexs to ignore
        List<List<int>> ignore = new List<List<int>>();
        for (int i = 0; i < 7; i++) {
            for (int j = i + 1; j < 7; j++) {
                ignore.Add(new List<int>{i, j});
            }
        }

        //Then, we loop through them creating new lists with the indexs removed
        foreach (List<int> i in ignore) {
            List<Card> clonedList = new List<Card>(cards);
            clonedList.RemoveAt(i[1]);
            clonedList.RemoveAt(i[0]);

            if (clonedList.Count == 5) premutations.Add(clonedList);
        }

        return premutations;

    }

    //Find all the pairs, tripples, and quads
    public static Dictionary<int, int> FindPairInfo(List<Card> cards) {
        //Store the rank of the card in the dictionary
        Dictionary<int, int> pairs = new Dictionary<int, int>();

        //Add cards into dictionary
        for (int i = 0; i < cards.Count; i++) {
            if (!pairs.ContainsKey(cards[i].rank)) {
                pairs.Add(cards[i].rank, 1);
            }
            else {
                pairs[cards[i].rank]++;
            }
        }

        //Return dictionary
        return pairs;
    }

    public static int CountPairs(List<Card> cards) {
        Dictionary<int, int> pairs = FindPairInfo(cards);

        int num_pairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 2) num_pairs++;
        }

        return num_pairs;
    }
    public static int CountTrips(List<Card> cards) {
        Dictionary<int, int> pairs = FindPairInfo(cards);

        int num_pairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 3) num_pairs++;
        }

        return num_pairs;
    }
    public static bool FindStraight(List<Card> cards, bool ace_high) {

        List<int> ranks = SortCards(cards, ace_high);

        return (ranks[0] - 1 == ranks[1]) && (ranks[0] - 2 == ranks[2]) && (ranks[0] - 3 == ranks[3]) && (ranks[0] - 4 == ranks[4]);

    }
    public static bool FindFlush(List<Card> cards) {

        int suit = cards[0].suit;

        for (int i = 0; i < cards.Count; i++) {
            if (cards[i].suit != suit) return false;
        }

        return true;
    }
    public static int CountQuads(List<Card> cards) {
        Dictionary<int, int> pairs = FindPairInfo(cards);

        int num_pairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 4) num_pairs++;
        }

        return num_pairs;
    }

    // -------- Scoring Functions -------- //
    //RankHand calls these to give point values
    public static int ScorePair(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        int rank = 0;

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 2) {
                if (entry.Key == 1) rank = 14;
                else rank = entry.Key;
            }
        }

        return 25 + rank;
    }
    public static int ScoreTwoPair(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        List<int> ranks = new List<int>();

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 2) {
                if (entry.Key == 1) ranks.Add(14);
                else ranks.Add(entry.Key);
            }
        }

        ranks.Sort();

        return 40 + ranks[0] + ranks[1] * 2;
    }
    public static int ScoreTripples(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        int score = 85;

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 3) score += entry.Key;
        }


        return score;
    }

    public static int ScoreStraight(List<Card> cards, bool ace_high) {

        //Get Straight info
        List<int> ranks = SortCards(cards, ace_high);
        int score = 120 + ranks[0];

        return score;
    }
    public static int ScoreFlush(List<Card> cards) {
        return 135;
    }

    public static int ScoreFullHouse(List<Card> cards) {
        return 150 + ScoreTripples(cards) + ScorePair(cards);
    }

    public static int ScoreQuads(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        int score = 500;

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 4) score += entry.Key;
        }


        return score;
    }
    public static int ScoreStraightFlush(List<Card> cards, bool ace_high) {
        return 700 + ScoreStraight(cards, ace_high);
    }

}
