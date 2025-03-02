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
enum FENPieces { p, b, n, r, q, k }
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
        positions[(int)PieceType.Pawn] = pawns;
        positions[(int)PieceType.Bishop] = bishops;
        positions[(int)PieceType.Knight] = knights;
        positions[(int)PieceType.Rook] = rooks;
        positions[(int)PieceType.Queen] = queens;
        positions[(int)PieceType.King] = kings;
    }
    public BitBoard pawn => positions[(int)PieceType.Pawn];
    public BitBoard bishop => positions[(int)PieceType.Bishop];
    public BitBoard knight => positions[(int)PieceType.Knight];
    public BitBoard rook => positions[(int)PieceType.Rook];
    public BitBoard queen => positions[(int)PieceType.Queen];
    public BitBoard king => positions[(int)PieceType.King];
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
    public static BitBoard queenAttack(BitBoard piece)
    {
        BitBoard returno = new BitBoard();
        BitBoard NEwing = piece;
        BitBoard NWwing = piece;
        BitBoard SEwing = piece;
        BitBoard SWwing = piece;
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
            NEwing = NEwing.NEshift();
            NWwing = NWwing.NWshift();
            SEwing = SEwing.SEshift();
            SWwing = SWwing.SWshift();
            returno = returno | Nwing | Wwing | Ewing | Swing | NEwing | NWwing | SEwing | SWwing;
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
        string[] FEN = splitFEN(FENPosition);
        BitBoard[] piecePlacement = placementParse(FEN[0]);
        PieceColor sideToMove = sideParse(FEN[1]);
        CastlingFlags[] castlingAbility = castlingRightsParse(FEN[2]);
        short enPassantTarget = enPassantTargetParse(FEN[3]);
        ushort halfMoveClock = halfMoveClockParse(FEN[4]);
        ushort fullMoveCount = fullMoveCountParse(FEN[5]);
        whitePositions = new PiecePositions(piecePlacement[0], piecePlacement[1], piecePlacement[2], piecePlacement[3], piecePlacement[4], piecePlacement[5]);
        blackPositions = new PiecePositions(piecePlacement[6], piecePlacement[7], piecePlacement[8], piecePlacement[9], piecePlacement[10], piecePlacement[11]);
        Move currentMove = new Move();

        state.Push(new PositionState(currentMove, castlingAbility[0], castlingAbility[1], sideToMove, enPassantTarget, halfMoveClock, fullMoveCount, null, null));

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
        //NOT IMPLEMENTED
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
        {
            whitePositions.positions[(int)move.MovedPiece] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
        }
        else
        {
            blackPositions.positions[(int)move.MovedPiece] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
        }
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
    Dictionary<Tuple<int, BitBoard>, BitBoard> generateRookLookupTable(){
        Dictionary<Tuple<int, BitBoard>, BitBoard> returno = new Dictionary<Tuple<int, BitBoard>, BitBoard>;
        for (int i = 0; i < 64; i++)
        {
            BitBoard mask = PiecePositions.rookAttack((BitBoard)1 << i);
            BitBoard[] blockingMasks = createBlockers(mask);
            foreach (BitBoard blocker in blockingMasks)
            {
                BitBoard legalMoves = generateRookLegalMoves(i,blocker);
                returno.Add(new Tuple<int, BitBoard>(i, blocker),legalMoves);
            }
        }
        return returno;
    }
    Dictionary<Tuple<int, BitBoard>, BitBoard> generateBishopLookupTable(){
        Dictionary<Tuple<int, BitBoard>, BitBoard> returno = new Dictionary<Tuple<int, BitBoard>, BitBoard>;
        for (int i = 0; i < 64; i++)
        {
            BitBoard mask = PiecePositions.bishopAttack((BitBoard)1 << i);
            BitBoard[] blockingMasks = createBlockers(mask);
            foreach (BitBoard blocker in blockingMasks)
            {
                BitBoard legalMoves = generateBishopLegalMoves(i,blocker);
                returno.Add(new Tuple<int, BitBoard>(i, blocker),legalMoves);
            }
        }
        return returno;
    }
    Dictionary<Tuple<int, BitBoard>, BitBoard> generateQueenLookupTable(){
        Dictionary<Tuple<int, BitBoard>, BitBoard> returno = new Dictionary<Tuple<int, BitBoard>, BitBoard>;
        for (int i = 0; i < 64; i++)
        {
            BitBoard mask = PiecePositions.queenAttack((BitBoard)1 << i);
            BitBoard[] blockingMasks = createBlockers(mask);
            foreach (BitBoard blocker in blockingMasks)
            {
                BitBoard legalMoves = generateQueenLegalMoves(i,blocker);
                returno.Add(new Tuple<int, BitBoard>(i, blocker),legalMoves);
            }
        }
        return returno;
    }
    //Generation
    BitBoard generateRookLegalMoves(int index, BitBoard blocker, bool isRook){
        BitBoard returno = new BitBoard();
        int[] rookDir = {1,8,-1,-8};
        int[] bishopDir = {7,9,-7,-9};
        int[] directions =  isRook ? rookDir:bishopDir;

        for (int i = 0; i < 4; i++)
        {
            BitBoard piece = (BitBoard)index;
            while(){
                piece <<= directions[i];
                returno |= piece;
            }
        }
        return returno;
    }

    //FEN parse
    private readonly Dictionary<string, short> posLookup = new Dictionary<string, short>{
        {"A", 1},
        {"B", 2},
        {"C", 3},
        {"D", 4},
        {"E", 5},
        {"F", 6},
        {"G", 7},
        {"H", 8}
    };
    private string[] splitFEN(string FENPosition)
    {
        try
        {
            string[] FENparts = new string[6];
            for (int i = 0; i < 6; i++)
            {
                FENparts[i] = FENPosition.Substring(0, FENPosition.IndexOf(' '));
                FENPosition = FENPosition.Substring(FENPosition.IndexOf(' ') + 1);
            }
            return FENparts;
        }
        catch (System.Exception)
        {
            Debug.Log("FEN must be 6 parts!");
            throw;
        }
    }
    private BitBoard[] placementParse(string FENPosition)
    {
        BitBoard[] board = new BitBoard[12];
        for (int i = 0; i < FENPosition.Length; i++)
        {
            string piece = FENPosition.Substring(0, 1);
            if (piece != "/")
            {
                Enum.TryParse(piece, out FENPieces FENpiece);
                board[(int)FENpiece] = board[(int)FENpiece] | (BitBoard)i;
            }
        }
        return board;
    }
    private PieceColor sideParse(string FENPosition)
    {
        if (FENPosition == "w")
        {
            return PieceColor.White;
        }
        return PieceColor.Black;
    }
    private CastlingFlags[] castlingRightsParse(string FENPosition)
    {
        CastlingFlags[] flags = new CastlingFlags[2];
        flags[0] = CastlingFlags.None;
        flags[1] = CastlingFlags.None;
        for (int i = 0; i < FENPosition.Length; i++)
        {
            string right = FENPosition.Substring(i, i + 1);
            switch (right)
            {
                case "K":
                    flags[1] = CastlingFlags.KingSide;
                    break;
                case "Q":
                    if (flags[1] == CastlingFlags.KingSide) flags[1] = CastlingFlags.Both;
                    else flags[1] = CastlingFlags.QueenSide;
                    break;
                case "k":
                    flags[0] = CastlingFlags.KingSide;
                    break;
                case "q":
                    if (flags[0] == CastlingFlags.KingSide) flags[0] = CastlingFlags.Both;
                    else flags[0] = CastlingFlags.QueenSide;
                    break;
            }
        }
        return flags;
    }
    private short enPassantTargetParse(string FENPosition)
    {
        if (FENPosition == "-") return -1;
        return (short)(posLookup[FENPosition.Substring(0, 1)] * 8 + short.Parse(FENPosition.Substring(1, 2)));
    }
    private ushort halfMoveClockParse(string FENPosition)
    {
        return ushort.Parse(FENPosition);
    }
    private ushort fullMoveCountParse(string FENPosition)
    {
        return ushort.Parse(FENPosition);
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
    public PieceType? CapturedPieceType { get; }
    public int? CaptureSquare { get; }
    public PositionState(Move move, CastlingFlags whiteflag, CastlingFlags blackflag, PieceColor nextToMove, short enPassantTarget, ushort halfMoveClock, ushort fullMoveCount, PieceType? capturedPiece, int? captureSquare)
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