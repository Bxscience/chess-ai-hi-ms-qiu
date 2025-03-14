using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SocialPlatforms.Impl;

public static class Evaluator {
    // Evaluate board from White's perspective: positive is good for White, negative is good for Black.
    public static float EvaluateBoard(Position board) {

        float score = 0;
        int[] pieceEval = {0,100,330,300,500,900,20000};

        for (int i = 0; i < 64; i++)
        {

            score += (board.whitePositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
            score -= (board.blackPositions.allPositions &(BitBoard)i )> 0 ? pieceEval[board.PieceAt(i) & 7]: 0;
            int piece = board.PieceAt(i);

            switch(piece & 7){

                case 1:
                score +=pawnValue(board,i, piece & 24);
                break;
                case 2:
                score +=bishopValue(board,i, piece & 24);
                break;
                case 3:
                score +=knightValue(board, i, piece & 24);
                break;
                case 4:
                score +=rookValue(board,i, piece & 24);
                break;
                case 5:
                score +=queenValue(board,i, piece & 24);
                break;
                case 6:
                score +=kingValue(board, i, piece & 24);
                break;

            }

        }

        return score;

    }
    private static float pawnValue(Position board, int square, int sideToMove){
        float score = 0;

        //stacked pawns

        if(sideToMove == 8 && board.PieceAt(square + 8) == (1 | sideToMove))
            score -= 50;
        else if(sideToMove == 16 && board.PieceAt(square - 8) == (1 | sideToMove))
            score += 50;

        //blockage of Stop



        //passed pawn

        BitBoard file = (BitBoard)0x101010101010101;
        BitBoard alliedPawn = sideToMove == 16 ? board.blackPositions.pawn: board.whitePositions.pawn;
        BitBoard scan = file << square;

        if(!((alliedPawn & scan) > 0)){

            BitBoard enemyPawn = sideToMove == 8 ? board.blackPositions.pawn: board.whitePositions.pawn;
            int filePos = square % 8;
            scan |= scan << Math.Max(0, filePos-1) | scan << Math.Min(7, filePos+1); 
            score += (enemyPawn & scan) > 0? 20:0;

        }

        return score;
    }
    private static float bishopValue(Position board, int square, int sideToMove){
        float score = 0;

        //bishopDrought

        BitBoard enemyBishopBoard = sideToMove == 8 ? board.blackPositions.bishop:board.whitePositions.bishop;
        bool isDrought = true;
        BitBoard checker = new(0xaa55aa55aa55aa55);

        while(enemyBishopBoard > 0){

            if( (((BitBoard)square & checker)> 0 && (enemyBishopBoard & -enemyBishopBoard & checker)> 0) ||
            ((BitBoard)square & checker) == (enemyBishopBoard & -enemyBishopBoard & checker)) 
                isDrought = false;

            enemyBishopBoard ^= enemyBishopBoard & -enemyBishopBoard;

        }

        if(isDrought) score += sideToMove == 8 ? 50:-50;

        //badBishop

        return score;
    }
    private static float knightValue(Position board, int square, int sideToMove){
        
        float score = 0;

        //value loss as pawns dissappear

        BitBoard allPawns = board.whitePositions.pawn | board.blackPositions.pawn;
        int count = 0;
        while(allPawns > 0){
            count++;
            allPawns ^= allPawns & -allPawns;
        }
        score += sideToMove == 8 ? 3*(count-16):-3*(count-16);

        //outpost

        return score;

    }
    private static float rookValue(Position board, int square, int sideToMove){

        float score = 0;

        //value gained as pawns dissappear

        BitBoard allPawns = board.whitePositions.pawn | board.blackPositions.pawn;
        int count = 0;
        while(allPawns > 0){
            count++;
            allPawns ^= allPawns & -allPawns;
        }
        score += sideToMove == 8 ? -2*(count-16):2*(count-16);

        //king blocking pentalty

        return score;

    }
    private static float queenValue(Position board, int square, int sideToMove){

        float score = 0;

        //early movement penalty
        if(board.state.Peek().FullMoveCount < 10) score -= 20;

        //maybe don't evaluate mobility?

        return score;

    }
    private static float kingValue(Position board, int square, int sideToMove){

        float score = 0;

        //king trophism

        //pawn shield LOOPING PROBLEM

        int negative = sideToMove == 8 ? 1:-1;
        BitBoard pawns = sideToMove == 8 ? board.whitePositions.pawn:board.blackPositions.pawn;
        
        if( ((((BitBoard)square << (negative * 8) & pawns) > 0) || ((BitBoard)square << (negative * 16) & pawns) > 0) && 
        (((BitBoard)square << (negative * 7) & pawns)> 0) && 
        (((BitBoard)square << (negative * 9) & pawns)> 0) )
            score += 40;

        //pawn storm
        
        //phantom mobility

        BitBoard attack = PiecePositions.bishopAttack(square) | PiecePositions.rookAttack(square);
        BitBoard enemy = sideToMove == 8 ? board.blackPositions.allPositions:board.whitePositions.allPositions;
        BitBoard danger = attack & enemy;

        while(danger > 0){

            if(board.PieceAt(BitBoard.bitscan(danger)) == (2 | (sideToMove ^ 24)) ||
            board.PieceAt(BitBoard.bitscan(danger)) == (4 | (sideToMove ^ 24)) ||
            board.PieceAt(BitBoard.bitscan(danger)) == (5 | (sideToMove ^ 24))) 
                score += sideToMove == 8 ? -10:10;
            danger ^= danger & -danger; 

        }

        return score;
    }
}