using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Animations;

public static class Evaluator {
    // Evaluate board from White's perspective: positive is good for White, negative is good for Black.
    public static float EvaluateBoard(Position board) {
        float score = 0;
        int[] pieceEval = {0,100,300,300,500,900,20000};
        for (int i = 0; i < 64; i++)
        {
            score += (board.whitePositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
            score -= (board.blackPositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
        }
        
        return score;
    }
}