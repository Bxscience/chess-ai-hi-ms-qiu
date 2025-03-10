using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MoveGenerator
{
    private BitBoard[,] rookLookup;
    private BitBoard[,] bishopLookup;
    public void initializeLookup(){
        rookLookup = Position.generateLookupTable(true);
        bishopLookup = Position.generateLookupTable(false);
    }
    public List<Move> createMovesAtSquare(Position board, int square){
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
        List<Move> moves = new List<Move>();
        BitBoard allPieces = board.whitePositions.allPositions | board.blackPositions.allPositions;
        int piece = board.PieceAt(square);
        int playerToMove =board.state.Peek().NextToMove;
        BitBoard attack = PiecePositions.pieceAttack(square, piece & 7, playerToMove == 8);
        attack &= ~endPos[square];
        attack &= allPieces;
        attack = (piece & 7) == 2? bishopLookup[square,((ulong)attack * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]]: attack;
        attack = (piece & 7) == 4? rookLookup[square, ((ulong)attack * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]]: attack;
        attack = (piece & 7) == 5? bishopLookup[square,((ulong)attack * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]]|rookLookup[square, ((ulong)attack * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]] : attack;
        attack &= ~(playerToMove == 8? board.whitePositions.allPositions: board.blackPositions.allPositions);
        for(int i = 0; i < 64; i++)
        {
            if((attack & (BitBoard)i) > 0){
                moves.Add(new Move(square,i,piece,null));
            }
        }
        return moves;
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

