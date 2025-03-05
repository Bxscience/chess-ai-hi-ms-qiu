using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Windows.Speech;
public class PiecePositions
{
    public BitBoard[] positions;
    public PiecePositions(
    BitBoard pawns,
    BitBoard bishops,
    BitBoard knights,
    BitBoard rooks,
    BitBoard queens,
    BitBoard kings
    )
    {
        positions = new BitBoard[6];
        positions[0] = pawns;
        positions[1] = bishops;
        positions[2] = knights;
        positions[3] = rooks;
        positions[4] = queens;
        positions[5] = kings;
    }
    public BitBoard pawn => positions[0];
    public BitBoard bishop => positions[1];
    public BitBoard knight => positions[2];
    public BitBoard rook => positions[3];
    public BitBoard queen => positions[4];
    public BitBoard king => positions[5];
    public BitBoard pawnAttack(BitBoard piece)
    {
        return piece.NEshift() | piece.NWshift();
    }
    public static BitBoard bishopAttack(BitBoard piece)
    {
        BitBoard returno = new BitBoard();
        BitBoard NEwing = piece;
        BitBoard NWwing = piece;
        BitBoard SEwing = piece;
        BitBoard SWwing = piece;
        for (int i = 0; i < 8; i++)
        {
            NEwing = NEwing.NEshift();
            NWwing = NWwing.NWshift();
            SEwing = SEwing.SEshift();
            SWwing = SWwing.SWshift();
            returno = returno | NEwing | NWwing | SEwing | SWwing;
        }
        return returno;
    }
    public static BitBoard knightAttack(BitBoard piece)
    {
        BitBoard returno = new BitBoard();
        BitBoard north = piece.Nshift().Nshift();
        BitBoard south = piece.Sshift().Sshift();
        BitBoard east = piece.Eshift().Eshift();
        BitBoard west = piece.Wshift().Wshift();
        return returno | north.Eshift() | north.Wshift() | south.Eshift() | south.Wshift() |east.Nshift() | east.Sshift() |west.Nshift() | west.Sshift();
    }
    public static BitBoard rookAttack(BitBoard piece)
    {
        BitBoard returno = new BitBoard();
        BitBoard Nwing = piece;
        BitBoard Ewing = piece;
        BitBoard Swing = piece;
        BitBoard Wwing = piece;
        for (int i = 0; i < 8; i++)
        {
            Nwing = Nwing.Nshift();
            Ewing = Ewing.Eshift();
            Swing = Swing.Sshift();
            Wwing = Wwing.Wshift();
            returno = returno | Nwing | Wwing | Ewing | Swing;
        }
        return returno;
    }
   
    public static BitBoard kingAttack(BitBoard piece)
    {
        return piece.Nshift() | piece.Sshift() | piece.Eshift() | piece.Wshift() | piece.NEshift() | piece.NWshift() | piece.SEshift() | piece.SWshift();
    }
    public BitBoard allPieces(){
        return king | queen | rook | knight | bishop | pawn;
    }
}

public class Position
{
    public Stack<PositionState> state;
    public PiecePositions whitePositions;
    public PiecePositions blackPositions;
    public Position(string FENPosition)
    {
        
    }
    //utility functions
    public PiecePositions getPlayerPieces(PieceColor side)
    {
        if (side == PieceColor.White)
        {
            return blackPositions;
        }
        return whitePositions;
    }

    public BitBoard[] getAttackDefendMap()
    {
        throw new NotImplementedException();
        BitBoard[] map = new BitBoard[64];
        for (int i = 0; i < 64; i++)
        {
            map[i] = (BitBoard)i;
        }
        return map;
    }
    public void updateBoardWithMove(Move move)
    {
        if (state.Peek().NextToMove == PieceColor.White)
            whitePositions.positions[move.MovedPiece] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
        else
            blackPositions.positions[move.MovedPiece] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
    }
    BitBoard[] createBlockers(BitBoard mask)
    {
        List<int> attackbits = new List<int>();
        for (int i = 0; i < 64; i++)
        {
            if (((mask >> i) & (BitBoard)1) == 1)
            {
                attackbits.Add(i);
            }
        }
        int totalPaths = 1 << attackbits.Count;
        BitBoard[] blockers = new BitBoard[totalPaths];
        for (int pattern = 0; pattern < totalPaths; pattern++)
        {
            for (int bitIndex = 0; bitIndex < attackbits.Count; bitIndex++)
            {
                int bit = (pattern >> bitIndex) & 1;
                blockers[pattern] |= new BitBoard((ulong)bit) << attackbits[bitIndex];
            }
        }
        return blockers;
    }
    
    //Generation
    Dictionary<Tuple<int, BitBoard>, BitBoard> generateLookupTable(bool isRook){
        Dictionary<Tuple<int, BitBoard>, BitBoard> returno = new Dictionary<Tuple<int, BitBoard>, BitBoard>();
        for (int i = 0; i < 64; i++)
        {
            BitBoard mask = PiecePositions.rookAttack((BitBoard)i);
            BitBoard[] blockingMasks = createBlockers(mask);
            foreach (BitBoard blocker in blockingMasks)
            {
                BitBoard legalMoves = generateLegalMoves(i,blocker, isRook);
                returno.Add(new Tuple<int, BitBoard>(i, blocker),legalMoves);
            }
        }
        return returno;
    }
    BitBoard generateLegalMoves(int index, BitBoard blocker, bool isRook){
        BitBoard returno = new BitBoard();
        int[] rookDir = {1,8,-1,-8};
        int[] bishopDir = {7,9,-7,-9};
        int[] directions =  isRook ? rookDir:bishopDir;

        for (int i = 0; i < 4; i++)
        {
            BitBoard piece = (BitBoard)index;
            while((piece & blocker) <= 1 | ((BitBoard)0xff818181818181ff & piece) <= 1){
                piece <<= directions[i];
                returno |= piece;
            }
        }
        return returno;
    }

    
}

public class PositionState
{
    public Move AppliedMove { get; }
    public CastlingFlags WhiteCastlingRights { get; }
    public CastlingFlags BlackCastlingRights { get; }
    public PieceColor NextToMove { get; }
    public short EnPassantTarget { get; }
    public ushort HalfMoveClock { get; }
    public ushort FullMoveCount { get; }
    public int? CapturedPieceType { get; }
    public int? CaptureSquare { get; }
    public PositionState(Move move, CastlingFlags whiteflag, CastlingFlags blackflag, PieceColor nextToMove, short enPassantTarget, ushort halfMoveClock, ushort fullMoveCount, int? capturedPiece, int? captureSquare)
    {
        AppliedMove = move;
        WhiteCastlingRights = whiteflag;
        BlackCastlingRights = blackflag;
        NextToMove = nextToMove;
        EnPassantTarget = enPassantTarget;
        HalfMoveClock = halfMoveClock;
        FullMoveCount = fullMoveCount;
        CapturedPieceType = capturedPiece;
        CaptureSquare = captureSquare;
    }
}