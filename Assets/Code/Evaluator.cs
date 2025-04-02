using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SocialPlatforms.Impl;

public static class Evaluator
{
    // Evaluate board from White's perspective: positive is good for White, negative is good for Black.

    public static float EvaluateBoard(Position board, MoveGenerator moveGenerator)
    {

        float score = 0;
        float[] pieceEval = { 0, 100, 330, 320, 500, 900, 20000 };
        //BitBoard[] piecePositions = board.whitePositions.positions.Concat(board.blackPositions.positions).ToArray(); 

        for (int i = 0; i < 64; i++)
        {
            //BitBoard[] attackDefendMap = moveGenerator.getAttackDefendMap(board);
            int piece = board.PieceAt(i);
            int pieceIndex = piece & 7 - 1;
            int pieceColor = piece & 24;
            switch (piece & 7)
            {

                case 1:
                    score += pawnValue(board, i, piece & 24);
                    break;
                case 2:
                    score += bishopValue(board, i, piece & 24);
                    break;
                case 3:
                    score += knightValue(board, i, piece & 24);
                    break;
                case 4:
                    score += rookValue(board, i, piece & 24);
                    break;
                case 5:
                    score += queenValue(board, i, piece & 24);
                    break;
                case 6:
                    score += kingValue(board, i, piece & 24, moveGenerator);
                    break;

            }

            score += pieceColor == 8 ? pieceEval[pieceIndex] : -pieceEval[pieceIndex];
            score += PieceSquareTables.getPSQTScore(pieceColor, piece, board.state.Peek().FullMoveCount, i);
            if (piece != 0) score += mobilityValue(board, i, piece, moveGenerator);

        }

        // for (int i = 0; i < 12; i++)
        // {

        //     BitBoard copy = piecePositions[i];
        //     while(copy > 0){

        //         switch (i & 5)
        //         {

        //             case 0:
        //             score += pawnValue(board, BitBoard.bitscan(copy), i > 5 ? 16 : 8);
        //             break;
        //             case 1:
        //             score +=bishopValue(board, BitBoard.bitscan(copy),i > 5 ? 16 : 8);
        //             break;
        //             case 2:
        //             score +=knightValue(board, BitBoard.bitscan(copy),i > 5 ? 16 : 8);
        //             break;
        //             case 3:
        //             score +=rookValue(board, BitBoard.bitscan(copy),i > 5 ? 16 : 8);
        //             break;
        //             case 4:
        //             score +=queenValue(board, BitBoard.bitscan(copy),i > 5 ? 16 : 8);
        //             break;
        //             case 5:
        //             score +=kingValue(board, BitBoard.bitscan(copy),i > 5 ? 16 : 8);
        //             break;

        //         }

        //         copy ^= copy & -copy;

        //     }

        // }

        return score;

    }

    public static float pieceValue(int piece){
        float[] pieceEval = { 0, 100, 330, 320, 500, 900, 20000 };
        return pieceEval[piece & 7];
    }

    private static float mobilityValue(Position board, int square, int piece, MoveGenerator moveGenerator)
    {

        float score = 0;
        BitBoard attack = moveGenerator.createPieceAttackBitBoard(board, square, piece, board.state.Peek().NextToMove);
        BitBoard centeral = new(0x3c3c3c3c3c3c00);
        int sideMulti = (piece & 24) == 8 ? 1 : -1;

        while (attack > 0)
        {

            if ((centeral & (BitBoard)BitBoard.bitscan(attack)) > 0)
                score += 5 * sideMulti;
            score += sideMulti * 5;
            attack ^= attack & -attack;

        }

        return score;

    }

    private static float xRayAttack(Position board, int square, bool isRook, MoveGenerator moveGenerator)
    {

        float score = 0;

        if (isRook)
        {
            PiecePositions.rookAttack(square);
        }

        return score;

    }

    // private static float connectivity(Position board, int square, int sideToMove){

    // }
    private static float pawnValue(Position board, int square, int sideToMove)
    {
        float score = 0;

        //stacked pawns

        if ((((BitBoard)square & (BitBoard)0xff00000000000000) == 0) && sideToMove == 8 && board.PieceAt(square + 8) == (1 | sideToMove))
            score -= 50;
        else if (((BitBoard)square & new BitBoard(0xff)) > 0 && sideToMove == 16 && board.PieceAt(square - 8) == (1 | sideToMove))
            score += 50;

        //blockage of Stop



        //passed pawn

        BitBoard file = (BitBoard)0x101010101010101;
        BitBoard alliedPawn = sideToMove == 16 ? board.blackPositions.pawn : board.whitePositions.pawn;
        BitBoard scan = file << square;

        if (!((alliedPawn & scan) > 0))
        {

            BitBoard enemyPawn = sideToMove == 8 ? board.blackPositions.pawn : board.whitePositions.pawn;
            int filePos = square & 7;
            scan |= scan << Math.Max(0, filePos - 1) | scan << Math.Min(7, filePos + 1);
            score += (enemyPawn & scan) > 0 ? 20 : 0;

        }

        //ug.Log(square + " score: " + score);

        return score;
    }
    private static float bishopValue(Position board, int square, int sideToMove)
    {
        float score = 0;

        //bishopDrought

        BitBoard enemyBishopBoard = sideToMove == 8 ? board.blackPositions.bishop : board.whitePositions.bishop;
        bool isDrought = true;
        BitBoard checker = new(0xaa55aa55aa55aa55);

        while (enemyBishopBoard > 0)
        {

            if ((((BitBoard)square & checker) > 0 && (enemyBishopBoard & -enemyBishopBoard & checker) > 0) ||
            ((BitBoard)square & checker) == (enemyBishopBoard & -enemyBishopBoard & checker))
                isDrought = false;

            enemyBishopBoard ^= enemyBishopBoard & -enemyBishopBoard;

        }

        if (isDrought) score += sideToMove == 8 ? 50 : -50;

        //badBishop

        //ug.Log(square + " score: " + score);

        return score;
    }
    private static float knightValue(Position board, int square, int sideToMove)
    {

        float score = 0;

        //value loss as pawns dissappear

        BitBoard allPawns = board.whitePositions.pawn | board.blackPositions.pawn;
        int count = 0;
        while (allPawns > 0)
        {
            count++;
            allPawns ^= allPawns & -allPawns;
        }
        score += sideToMove == 8 ? 3 * (count - 16) : -3 * (count - 16);

        //outpost

        //ug.Log(square + " score: " + score);

        return score;

    }
    private static float rookValue(Position board, int square, int sideToMove)
    {

        float score = 0;
        PositionState state = board.state.Peek();

        //value gained as pawns dissappear

        BitBoard allPawns = board.whitePositions.pawn | board.blackPositions.pawn;
        int count = 0;
        while (allPawns > 0)
        {
            count++;
            allPawns ^= allPawns & -allPawns;
        }
        score += sideToMove == 8 ? -2 * (count - 16) : 2 * (count - 16);

        //king blocking pentalty

        if (square == 0 && (state.WhiteCastlingRights == CastlingFlags.Both || state.WhiteCastlingRights == CastlingFlags.QueenSide))
            score -= 20;
        else if (square == 7 && (state.WhiteCastlingRights == CastlingFlags.Both || state.WhiteCastlingRights == CastlingFlags.KingSide))
            score -= 20;
        else if (square == 56 && (state.WhiteCastlingRights == CastlingFlags.Both || state.BlackCastlingRights == CastlingFlags.QueenSide))
            score += 20;
        else if (square == 63 && (state.WhiteCastlingRights == CastlingFlags.Both || state.BlackCastlingRights == CastlingFlags.KingSide))
            score += 20;

        //ug.Log(square + " score: " + score);

        return score;

    }
    private static float queenValue(Position board, int square, int sideToMove)
    {

        float score = 0;

        //early movement penalty
        if (board.state.Peek().FullMoveCount < 10) score -= 20;

        //maybe don't evaluate mobility?

        //ug.Log(square + " score: " + score);

        return score;

    }
    private static float kingValue(Position board, int square, int sideToMove, MoveGenerator moveGenerator)
    {

        float score = 0;

        //king trophism

        //pawn shield LOOPING PROBLEM

        int negative = sideToMove == 8 ? 1 : -1;
        BitBoard pawns = sideToMove == 8 ? board.whitePositions.pawn : board.blackPositions.pawn;

        if (((((BitBoard)square << (negative * 8) & pawns) > 0) || ((BitBoard)square << (negative * 16) & pawns) > 0) &&
        (((BitBoard)square << (negative * 7) & pawns) > 0) &&
        (((BitBoard)square << (negative * 9) & pawns) > 0))
            score += 40;

        //pawn storm

        //phantom mobility

        BitBoard attack = moveGenerator.createPieceAttackBitBoard(board, square, 5 | sideToMove, sideToMove);
        int sideMulti = sideToMove == 8 ? -1 : 1;

        while (attack > 0)
        {

            score += 5 * sideMulti;
            attack ^= attack & -attack;

        }

        //ug.Log(square + " score: " + score);

        return score;
    }
}