using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator
{
    public static List<Move> generateMoves(Position board)
    {
        List<Move> moves = new List<Move>();
        BitBoard[] bishops = new BitBoard[64];

        for (int i = 0; i < 64; i++)
        {
            bishops[i] = PiecePositions.bishopAttack(i);
        }
        Debug.Log(bishops);

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

