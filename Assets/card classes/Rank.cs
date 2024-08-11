using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Rank {

    //This is the main useage function
    //Given 7 cards, returns the best score (from 5 card rankings)
    public static float Rank7Hands(List<Card> cards) {
        float maxScore = 0;
        List<List<Card>> premutations = Generate7Premutations(cards);

        for (int i = 0; i < premutations.Count; i++) {
            float score = RankHand(premutations[i]);
            if (score > maxScore) {
                maxScore = score;
            }
        }

        return maxScore;
    }

    //This is a helper function for Bots who want to calculate their score with only 6 cards in their hand
    public static float Rank6Hands(List<Card> cards) {
        float maxScore = 0;
        List<List<Card>> premutations = Generate6Premutations(cards);

        for (int i = 0; i < premutations.Count; i++) {
            float score = RankHand(premutations[i]);
            if (score > maxScore) {
                maxScore = score;
            }
        }

        return maxScore;
    }

    //This is the main driver function
    public static float RankHand(List<Card> cards) {
        //Sort the ranks of our hands (Ace high)
        List<int> ranks = SortCards(cards, true);

        //Reduce tie breaks by high cards
        float score = 0;
        for (int i = 0; i < ranks.Count; i++) {
            score += ranks[i] / Mathf.Pow(15,i);
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
    public static List<int> SortCards(List<Card> cards, bool aceHigh = true) {
        List<int> ranks = new List<int>();

        for (int i = 0; i < cards.Count; i++) {
            if (cards[i].rank == 1 && aceHigh) ranks.Add(14);
            else ranks.Add(cards[i].rank);
        }

        ranks.Sort();
        ranks.Reverse();

        return ranks;
    }

    //Given 6 cards, return all (unique) premutations of 5 cards
    public static List<List<Card>> Generate6Premutations(List<Card> cards) {
        List<List<Card>> premutations = new List<List<Card>>();

        //First we create a list of indexs to ignore
        List<List<int>> ignore = new List<List<int>>();
        for (int i = 0; i < 6; i++) {
           ignore.Add(new List<int> { i });
        }

        //Then, we loop through them creating new lists with the indexs removed
        foreach (List<int> i in ignore) {
            List<Card> clonedList = new List<Card>(cards);
            clonedList.RemoveAt(i[0]);

            if (clonedList.Count == 5) premutations.Add(clonedList);
        }

        return premutations;

    }

    //Given 7 cards, return all (unique) premutations of 5 cards
    public static List<List<Card>> Generate7Premutations(List<Card> cards) {
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

        int numPairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 2) numPairs++;
        }

        return numPairs;
    }
    public static int CountTrips(List<Card> cards) {
        Dictionary<int, int> pairs = FindPairInfo(cards);

        int numPairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 3) numPairs++;
        }

        return numPairs;
    }
    public static bool FindStraight(List<Card> cards, bool aceHigh) {

        List<int> ranks = SortCards(cards, aceHigh);

        if (cards.Count < 5) return false;

        return (ranks[0] - 1 == ranks[1]) && (ranks[0] - 2 == ranks[2]) && (ranks[0] - 3 == ranks[3]) && (ranks[0] - 4 == ranks[4]);

    }
    public static bool FindFlush(List<Card> cards) {

        int suit = cards[0].suit;

        if (cards.Count < 5) return false;

        for (int i = 0; i < cards.Count; i++) {
            if (cards[i].suit != suit) return false;
        }

        return true;
    }
    public static int CountQuads(List<Card> cards) {
        Dictionary<int, int> pairs = FindPairInfo(cards);

        int numPairs = 0;
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 4) numPairs++;
        }

        return numPairs;
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

        return 250 + rank;
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

        return 400 + ranks[0] + ranks[1] * 2;
    }
    public static int ScoreTripples(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        int score = 850;

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 3) score += entry.Key;
        }


        return score;
    }

    public static int ScoreStraight(List<Card> cards, bool aceHigh) {

        //Get Straight info
        List<int> ranks = SortCards(cards, aceHigh);
        int score = 1200 + ranks[0];

        return score;
    }
    public static int ScoreFlush(List<Card> cards) {
        return 1350;
    }

    public static int ScoreFullHouse(List<Card> cards) {
        return 1500 + ScoreTripples(cards) + ScorePair(cards);
    }

    public static int ScoreQuads(List<Card> cards) {

        //Get pair info
        Dictionary<int, int> pairs = FindPairInfo(cards);
        int score = 5000;

        //Get pairs
        foreach (KeyValuePair<int, int> entry in pairs) {
            if (entry.Value == 4) score += entry.Key;
        }


        return score;
    }
    public static int ScoreStraightFlush(List<Card> cards, bool aceHigh) {
        return 7000 + ScoreStraight(cards, aceHigh);
    }

}
