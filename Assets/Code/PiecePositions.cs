using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
public class PiecePositions
{
    public BitBoard[] positions;
    static BitBoard D = (BitBoard)0xff818181818181ff;
    static BitBoard N = (BitBoard)0x81818181818181ff;
    static BitBoard E = (BitBoard)0xff010101010101ff;
    static BitBoard S =(BitBoard)0xff81818181818181;
    static BitBoard W = (BitBoard)0xff808080808080ff;
    static BitBoard NW = (BitBoard)0x80808080808080ff;
    static BitBoard NE = (BitBoard)0x1010101010101ff;
    static BitBoard SE = (BitBoard)0xff01010101010101;
    static BitBoard SW= (BitBoard)0xff80808080808080;
    static BitBoard[] endPos = {
        NW,N,N,N,N,N,N,NE,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
        SW,S,S,S,S,S,S,SE
        };
    static readonly ulong[] _bishopTable = {};
    static readonly ulong[] _rookTable = {0b100000001000000010000000100000001000000010000000111111110,0b1000000010000000100000001000000010000000100000001011111101,0b10000000100000001000000010000000100000001000000010011111011,0b100000001000000010000000100000001000000010000000100011110111,0b1000000010000000100000001000000010000000100000001000011101111,0b111001011010101110011101111110010011110011011011111,0b100000001000000010000000100000001000000010000000100000010111111,0b1000000010000000100000001000000010000000100000001000000001111111,0b100000001000000010000000100000001000000011111111000000001,0b1000000010000000100000001000000010000000101111110100000010,0b10000000100000001000000010000000100000001001111101100000100,0b100000001000000010000000100000001000000010001111011100001000,0b1000000010000000100000001000000010000000100001110111100010000,0b111001011010101110011101111110010011101111100100100,0b100000001000000010000000100000001000000010000001011111101000000,0b1000000010000000100000001000000010000000100000000111111110000000,0b100000001000000010000000100000001111111100000000100000001,0b1000000010000000100000001000000010111111010000001000000010,0b10000000100000001000000010000000100111110110000010000000100,0b100000001000000010000000100000001000111101110000100000001000,0b1000000010000000100000001000000010000111011110001000000010000,0b111001011010101110011101111111111111110011000100100,0b100000001000000010000000100000001000000101111110100000001000000,0b1000000010000000100000001000000010000000011111111000000010000000,0b100000001000000010000000111111110000000010000000100000001,0b1000000010000000100000001011111101000000100000001000000010,0b10000000100000001000000010011111011000001000000010000000100,0b100000001000000010000000100011110111000010000000100000001000,0b1000000010000000100000001000011101111000100000001000000010000,0b111001011010101110011011111110010011110011000100100,0b100000001000000010000000100000010111111010000000100000001000000,0b1000000010000000100000001000000001111111100000001000000010000000,0b100000001000000011111111000000001000000010000000100000001,0b1000000010000000101111110100000010000000100000001000000010,0b10000000100000001001111101100000100000001000000010000000100,0b100000001000000010001111011100001000000010000000100000001000,0b1000000010000000100001110111100010000000100000001000000010000,0b111001011011111111111101111110010011110011000100100,0b100000001000000010000001011111101000000010000000100000001000000,0b1000000010000000100000000111111110000000100000001000000010000000,0b100000001111111100000000100000001000000010000000100000001,0b1000000010111111010000001000000010000000100000001000000010,0b10000000100111110110000010000000100000001000000010000000100,0b100000001000111101110000100000001000000010000000100000001000,0b1000000010000111011110001000000010000000100000001000000010000,0b111110111110101110011101111110010011110011000100100,0b100000001000000101111110100000001000000010000000100000001000000,0b1000000010000000011111111000000010000000100000001000000010000000,0b111111110000000010000000100000001000000010000000100000001,0b1011111101000000100000001000000010000000100000001000000010,0b10011111011000001000000010000000100000001000000010000000100,0b100011110111000010000000100000001000000010000000100000001000,0b1000011101111000100000001000000010000000100000001000000010000,0b11111111001011010101110011101111110010011110011000100100,0b100000010111111010000000100000001000000010000000100000001000000,0b1000000001111111100000001000000010000000100000001000000010000000,0b1111111000000001000000010000000100000001000000010000000100000001,0b1111110100000010000000100000001000000010000000100000001000000010,0b1111101100000100000001000000010000000100000001000000010000000100,0b1111011100001000000010000000100000001000000010000000100000001000,0b1110111100010000000100000001000000010000000100000001000000010000,0b1111111100000111001011010101110011101111110010011110011000100100,0b1011111101000000010000000100000001000000010000000100000001000000};
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
    public BitBoard allPositions => pawn | knight | bishop | rook | queen | king;
    public static BitBoard pawnAttack(BitBoard piece, bool isWhite)
    {
        return isWhite ?piece.NEshift() | piece.NWshift() : piece.SEshift() | piece.SWshift();
    }
    public static BitBoard bishopAttack(int pos)
    {
        int[] dir = {9,7,-9,-7};
        BitBoard returno = new BitBoard();

        for (int i = 0; i < 4; i++)
        {
            BitBoard wing = (BitBoard)(1ul << pos);
            while(((wing & endPos[pos]) == 0 )|| wing == 0){
                wing <<= dir[i];
                returno |= wing;
            }
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
        return new BitBoard(_rookTable[BitBoard.bitscan(piece)]);
    }
   
    public static BitBoard kingAttack(BitBoard piece)
    {
        return piece.Nshift() | piece.Sshift() | piece.Eshift() | piece.Wshift() | piece.NEshift() | piece.NWshift() | piece.SEshift() | piece.SWshift();
    }
}

public class Position
{
    public Stack<PositionState> state;
    public PiecePositions whitePositions;
    public PiecePositions blackPositions;
    //utility functions
    public Position(){
        state = new Stack<PositionState>();
        state.Push(new PositionState());

    }
    public PiecePositions getPlayerPieces(int side)
    {
        if (side == 8)
        {
            return blackPositions;
        }
        return whitePositions;
    }
    public void updateBoardWithMove(Move move)
    {
        if (state.Peek().NextToMove == 8)
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
    public int PieceAt(int square){
        int returno = 0;
        BitBoard[] allposition = whitePositions.positions.Union(blackPositions.positions).ToArray();
        for (int i = 0; i < 12; i++)
        {
            if((allposition[i] & (BitBoard)i  & (BitBoard)1 )== 1) returno = i > 5? i+1 | 8: (i % 6)+1|16;
        }
        return returno;
    }
    
    //Generation
    // public Span<BitBoard> generateLookupTable(bool isRook){
    //     Span<BitBoard> returno =  new BitBoard[1 << 13];
    //     for (int i = 0; i < 64; i++)
    //     {
    //         BitBoard mask = isRook?PiecePositions.rookAttack((BitBoard)i) : PiecePositions.bishopAttack((BitBoard)i);
    //         BitBoard[] blockingMasks = createBlockers(mask);
    //         foreach (BitBoard blocker in blockingMasks)
    //         {
    //             ulong key =  isRook?(ulong)blocker * PrecomputedMagics.RookMagics[i]  >> PrecomputedMagics.RookShifts[i] : 
    //             (ulong)blocker * PrecomputedMagics.BishopMagics[i]  >> PrecomputedMagics.BishopShifts[i];
    //             BitBoard legalMoves = generateLegalMoves(i,blocker, isRook);
    //             returno[(int)key] = blocker;
    //         }
    //     }
    //     return returno;
    // }
    static BitBoard D = (BitBoard)0xff818181818181ff;
    static BitBoard N = (BitBoard)0x81818181818181ff;
    static BitBoard E = (BitBoard)0xff010101010101ff;
    static BitBoard S =(BitBoard)0xff81818181818181;
    static BitBoard W = (BitBoard)0xff808080808080ff;
    static BitBoard NW = (BitBoard)0x80808080808080ff;
    static BitBoard NE = (BitBoard)0x1010101010101ff;
    static BitBoard SE = (BitBoard)0xff01010101010101;
    static BitBoard SW= (BitBoard)0xff80808080808080;
    static BitBoard[] endPos = {
        NW,N,N,N,N,N,N,NE,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
         W,D,D,D,D,D,D,E,
        SW,S,S,S,S,S,S,SE
        };
    BitBoard generateLegalMoves(int index, BitBoard blocker, bool isRook){
        BitBoard returno = new BitBoard();
        int[] rookDir = {1,8,-1,-8};
        int[] bishopDir = {7,9,-7,-9};
        int[] directions =  isRook ? rookDir:bishopDir;

        for (int i = 0; i < 4; i++)
        {
            BitBoard piece = (BitBoard)index;
            // while((piece & blocker) <= 1 || (D & piece) <= 1 && piece > 0){
            //     Debug.Log(looping);
            //     piece <<= directions[i];
            //     returno |= piece;
            // }
        }
        return returno;
    }

    
}

public class PositionState
{
    public Move AppliedMove { get; }
    public CastlingFlags WhiteCastlingRights { get; }
    public CastlingFlags BlackCastlingRights { get; }
    public int NextToMove { get; }
    public short EnPassantTarget { get; }
    public ushort HalfMoveClock { get; }
    public ushort FullMoveCount { get; }
    public int? CapturedPieceType { get; }
    public int? CaptureSquare { get; }
    public PositionState(){
        AppliedMove = new Move();
        WhiteCastlingRights = CastlingFlags.Both;
        BlackCastlingRights = CastlingFlags.Both;
        NextToMove = 8;
        EnPassantTarget = 0;
        HalfMoveClock = 1;
        FullMoveCount = 0;
    }
    public PositionState(Move move, CastlingFlags whiteflag, CastlingFlags blackflag, int nextToMove, short enPassantTarget, ushort halfMoveClock, ushort fullMoveCount, int? capturedPiece, int? captureSquare)
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
// Curtasy of Sebastion Lague
public static class PrecomputedMagics
{
    public static readonly int[] RookShifts = { 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 52, 54, 53, 53, 53, 53, 54, 53, 52, 53, 54, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 52, 53, 53, 53, 53, 53, 53, 52 };
    public static readonly int[] BishopShifts = { 58, 60, 59, 59, 59, 59, 60, 58, 60, 59, 59, 59, 59, 59, 59, 60, 59, 59, 57, 57, 57, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 57, 57, 57, 59, 59, 60, 60, 59, 59, 59, 59, 60, 60, 58, 60, 59, 59, 59, 59, 59, 58 };
    public static readonly ulong[] RookMagics = { 468374916371625120, 18428729537625841661, 2531023729696186408, 6093370314119450896, 13830552789156493815, 16134110446239088507, 12677615322350354425, 5404321144167858432, 2111097758984580, 18428720740584907710, 17293734603602787839, 4938760079889530922, 7699325603589095390, 9078693890218258431, 578149610753690728, 9496543503900033792, 1155209038552629657, 9224076274589515780, 1835781998207181184, 509120063316431138, 16634043024132535807, 18446673631917146111, 9623686630121410312, 4648737361302392899, 738591182849868645, 1732936432546219272, 2400543327507449856, 5188164365601475096, 10414575345181196316, 1162492212166789136, 9396848738060210946, 622413200109881612, 7998357718131801918, 7719627227008073923, 16181433497662382080, 18441958655457754079, 1267153596645440, 18446726464209379263, 1214021438038606600, 4650128814733526084, 9656144899867951104, 18444421868610287615, 3695311799139303489, 10597006226145476632, 18436046904206950398, 18446726472933277663, 3458977943764860944, 39125045590687766, 9227453435446560384, 6476955465732358656, 1270314852531077632, 2882448553461416064, 11547238928203796481, 1856618300822323264, 2573991788166144, 4936544992551831040, 13690941749405253631, 15852669863439351807, 18302628748190527413, 12682135449552027479, 13830554446930287982, 18302628782487371519, 7924083509981736956, 4734295326018586370 }; 
    public static readonly ulong[] BishopMagics = { 16509839532542417919, 14391803910955204223, 1848771770702627364, 347925068195328958, 5189277761285652493, 3750937732777063343, 18429848470517967340, 17870072066711748607, 16715520087474960373, 2459353627279607168, 7061705824611107232, 8089129053103260512, 7414579821471224013, 9520647030890121554, 17142940634164625405, 9187037984654475102, 4933695867036173873, 3035992416931960321, 15052160563071165696, 5876081268917084809, 1153484746652717320, 6365855841584713735, 2463646859659644933, 1453259901463176960, 9808859429721908488, 2829141021535244552, 576619101540319252, 5804014844877275314, 4774660099383771136, 328785038479458864, 2360590652863023124, 569550314443282, 17563974527758635567, 11698101887533589556, 5764964460729992192, 6953579832080335136, 1318441160687747328, 8090717009753444376, 16751172641200572929, 5558033503209157252, 17100156536247493656, 7899286223048400564, 4845135427956654145, 2368485888099072, 2399033289953272320, 6976678428284034058, 3134241565013966284, 8661609558376259840, 17275805361393991679, 15391050065516657151, 11529206229534274423, 9876416274250600448, 16432792402597134585, 11975705497012863580, 11457135419348969979, 9763749252098620046, 16960553411078512574, 15563877356819111679, 14994736884583272463, 9441297368950544394, 14537646123432199168, 9888547162215157388, 18140215579194907366, 18374682062228545019 };
    
}
