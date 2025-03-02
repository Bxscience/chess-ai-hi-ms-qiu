/*using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Animations;

public static class Evaluator {
    // Assign simple values to each piece type
    public static int GetPieceValue(ChessPiece piece) {
        switch (piece.type) {
            case PieceType.Pawn:   return 100;
            case PieceType.Knight: return 320;
            case PieceType.Bishop: return 330;
            case PieceType.Rook:   return 500;
            case PieceType.Queen:  return 900;
            case PieceType.King:   return 20000;
            default:               return 0;
        }
    }

    // Evaluate board from White's perspective: positive is good for White, negative is good for Black.
    public static float EvaluateBoard(Position board) {
        float score = 0;
        
        return score;
    }
    public static int centerDistaceScore(){
        
    }
     public static int defenceScore(Position board,BitBoard piecePos){
        int positionalScore = 0;
        
        return positionalScore;
    }
    public static int pawnValue(Position board,BitBoard piecePos){
        //pawn bloackage
        int value = 0;
        
        return value;
    }
    public static int bishopValue(Position board,BitBoard piecePos){
        //bad bishop
    }
    public static int knightValue(Position board,BitBoard piecePos){

    }
    public static int rookValue(Position board,BitBoard piecePos){

    }
    public static int queenValue(Position board,BitBoard piecePos){

    }
}
*/