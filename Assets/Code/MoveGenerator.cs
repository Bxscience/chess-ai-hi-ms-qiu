using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class MoveGenerator
{
    private BitBoard[,] rookLookup;
    private BitBoard[,] bishopLookup;
    private BitBoard[] knightLookup;
    private BitBoard rank2 = (BitBoard)(ulong)0xff00;
    private BitBoard rank7 = (BitBoard)0xff000000000000;
    public void initializeLookup()
    {
        rookLookup = Position.generateLookupTable(true);
        bishopLookup = Position.generateLookupTable(false);
        knightLookup = new BitBoard[64];
        for (int i = 0; i < 64; i++)
        {
            knightLookup[i] = PiecePositions.knightAttack(i);
        }
    }
    public List<Move> createJumpingMove(Position board, int square)
    {
        int piece = board.PieceAt(square);
        BitBoard attack = new();
        if ((piece & 7) == 3)
        {
            attack = knightLookup[square];
        }
        else
        {
            attack = (board.state.Peek().NextToMove == 8 ? board.blackPositions.allPositions : board.whitePositions.allPositions) & PiecePositions.pawnAttack(square, board.state.Peek().NextToMove);
            bool isWhite = board.state.Peek().NextToMove == 8;
            attack |= isWhite ? (BitBoard)square << 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions) : (BitBoard)square >> 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);
            if ((rank2 & (BitBoard)square) > 0 && isWhite && attack > 0)
                attack |= (BitBoard)square << 16;
            else if ((rank7 & (BitBoard)square) > 0 && !isWhite && attack > 0)
                attack |= (BitBoard)square >> 16;
        }
        attack &= board.state.Peek().NextToMove == 8 ? ~board.whitePositions.allPositions : ~board.blackPositions.allPositions;
        return createMoves(attack, square);
    }
    public List<Move> createKingMove(Position board, int square)
    {
        BitBoard attack = PiecePositions.kingAttack(square);
        attack &= ~(board.state.Peek().NextToMove == 8 ? board.whitePositions.allPositions:board.blackPositions.allPositions);
        return createMoves(attack, square);
    }
    public List<Move> createSlidingMove(Position board, int square)
    {
        BitBoard D = (BitBoard)0xff818181818181ff;
        BitBoard N = (BitBoard)0x81818181818181ff;
        BitBoard E = (BitBoard)0xff010101010101ff;
        BitBoard S = (BitBoard)0xff81818181818181;
        BitBoard W = (BitBoard)0xff808080808080ff;
        BitBoard NW = (BitBoard)0x80808080808080ff;
        BitBoard NE = (BitBoard)0x1010101010101ff;
        BitBoard SE = (BitBoard)0xff01010101010101;
        BitBoard SW = (BitBoard)0xff80808080808080;
        BitBoard[] endPos = {
        SW,S,S,S,S,S,S,SE,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
        NW,N,N,N,N,N,N,NE
        };
        BitBoard allPieces = board.whitePositions.allPositions | board.blackPositions.allPositions;
        int piece = board.PieceAt(square);
        int playerToMove = board.state.Peek().NextToMove;
        BitBoard attack = (piece & 7) == 5 ? bishopLookup[square, ((ulong)(PiecePositions.bishopAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]] | rookLookup[square, ((ulong)(PiecePositions.rookAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]] : PiecePositions.pieceAttack(square, piece & 7, playerToMove);
        attack = (piece & 7) == 2 ? bishopLookup[square, ((ulong)(attack & allPieces & ~endPos[square]) * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]] : attack;
        attack = (piece & 7) == 4 ? rookLookup[square, ((ulong)(attack & allPieces & ~endPos[square]) * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]] : attack;
        attack &= ~(playerToMove == 8 ? board.whitePositions.allPositions : board.blackPositions.allPositions);
        return createMoves(attack, square);
    }
    public static List<Move> generateMoves(Position board)
    {
        List<Move> moves = new List<Move>();

        // Span<BitBoard>BishopMagicLookup = board.generateLookupTable(false);
        // BitBoard allMask = board.whitePositions.allPositions | board.blackPositions.allPositions;
        // for(int i = 0; i < 64; i++)
        // {
        //     BitBoard attackMap = BitBoard.zero;
        //     int piece = board.PieceAt(i);
        //     switch (piece & 7)
        //     {
        //         case 1:
        //         attackMap = piece >> 3 == 1? PiecePositions.pawnAttack((BitBoard)i, true):PiecePositions.pawnAttack((BitBoard)i, false);
        //         break;
        //         case 2:
        //         attackMap = BishopMagicLookup[(int)((ulong)(PiecePositions.bishopAttack((BitBoard)i) & allMask )* PrecomputedMagics.BishopMagics[i] >> PrecomputedMagics.BishopShifts[i])];
        //         break;
        //         case 3:
        //         attackMap = PiecePositions.knightAttack((BitBoard)i);
        //         break;
        //         case 4:
        //         attackMap = RookMagicLookUp[(int)((ulong)(PiecePositions.rookAttack((BitBoard)i) & allMask )* PrecomputedMagics.RookMagics[i] >> PrecomputedMagics.RookShifts[i])];
        //         break;
        //         case 5:
        //         attackMap = BishopMagicLookup[(int)((ulong)(PiecePositions.bishopAttack((BitBoard)i) & allMask )* PrecomputedMagics.BishopMagics[i] >> PrecomputedMagics.BishopShifts[i])] |
        //         RookMagicLookUp[(int)((ulong)(PiecePositions.rookAttack((BitBoard)i) & allMask )* PrecomputedMagics.RookMagics[i] >> PrecomputedMagics.RookShifts[i])];
        //         break;
        //         case 6:
        //         attackMap = PiecePositions.kingAttack((BitBoard)i);
        //         break;
        //     }
        //     attackMap &= ~(piece >> 3 == 1? board.whitePositions.allPositions: board.blackPositions.allPositions);
        //     for (int j = 0; j < 64; j++)
        //     {
        //         if((attackMap & (BitBoard)j)> 0){
        //             moves.Add(new Move(i ,j ,piece, null));
        //         }
        //     }
        // }

        return moves;
    }
    BitBoard[] getAttackDefendMap()
    {
        throw new NotImplementedException();
        BitBoard[] map = new BitBoard[64];
        for (int i = 0; i < 64; i++)
        {
            map[i] = (BitBoard)i;
        }
        return map;
    }
    private List<Move> createMoves(BitBoard attackMap, int startSquare)
    {
        List<Move> moves = new();
        for (int i = 0; i < 64; i++)
        {
            if ((attackMap & (BitBoard)i) > 0)
            {
                moves.Add(new Move(startSquare, i, startSquare, null));
            }
        }
        return moves;
    }
}
public struct Move
{
    public int SourceSquare { get; }
    public int TargetSquare { get; }
    public int MovedPiece { get; }
    public int? PromotedToPiece { get; }
    public Move(int startSquare, int targetSquare, int piece, int? promote)
    {
        SourceSquare = startSquare;
        TargetSquare = targetSquare;
        MovedPiece = piece;
        PromotedToPiece = promote;
    }
}

