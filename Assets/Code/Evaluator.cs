using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Animations;

public static class Evaluator {
    // Evaluate board from White's perspective: positive is good for White, negative is good for Black.
    public static float EvaluateBoard(Position board) {
        float score = 0;
        int[] pieceEval = {0,100,330,300,500,900,20000};
        for (int i = 0; i < 64; i++)
        {
            score += (board.whitePositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
            score -= (board.blackPositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
        }
        return score;
    }
    private static float pawnValue(Position board, int square, int pawn){
        float score = 0;
        int pawnColor = pawn | 24;
        //stacked pawns
        if(pawnColor == 8 && board.PieceAt(square + 8) == pawn){
            score -= 50;
        }
        else if(pawnColor == 16 && board.PieceAt(square - 8) == pawn){
            score += 50;
        }
        //pawn structure

        //blockage of Stop
        return score;
    }
    private static float bishopValue(Position board, int square){
        float score = 0;
        //bishopDrought
        
        //badBishop
        return score;
    }
    private static float knightValue(Position board, int square){
        float score = 0;
        //value loss as pawns dissappear
        for (int i = 0; i < 64; i++)
        {
            if((board.PieceAt(i) & 7)==1) score -= 12;
        }
        //outpost
        return score;
    }
    private static float rookValue(Position board, int square){
        float score = 0;
        //value gained as pawns dissappear
        int count = 0;
        for (int i = 0; i < 64; i++)
        {
            if((board.PieceAt(i) & 7)==1) count++;
        }
        score 5g*(16 - count);
        //king blocking pentalty
        return score;
    }
    private static float queenValue(Position board, int square){
        float score = 0;
        //early movement penalty
        //maybe don't evaluate mobility?
        return score;
    }
    private static float kingValue(Position board, int square){
        float score = 0;
        //king trophism
        //pawn shield
        //pawn storm
        //phantom mobility

        return score;
    }
}