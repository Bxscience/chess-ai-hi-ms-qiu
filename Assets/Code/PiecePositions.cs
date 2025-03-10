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
    static readonly ulong[] bishopTable = { 0x8040201008040200, 0x80402010080500, 0x804020110a00, 0x8041221400, 0x182442800, 0x10204885000, 0x102040810a000, 0x102040810204000, 0x4020100804020002, 0x8040201008050005, 0x804020110a000a, 0x804122140014, 0x18244280028, 0x1020488500050, 0x102040810a000a0, 0x204081020400040, 0x2010080402000204, 0x4020100805000508, 0x804020110a000a11, 0x80412214001422, 0x1824428002844, 0x102048850005088, 0x2040810a000a010, 0x408102040004020, 0x1008040200020408, 0x2010080500050810, 0x4020110a000a1120, 0x8041221400142241, 0x182442800284482, 0x204885000508804, 0x40810a000a01008, 0x810204000402010, 0x804020002040810, 0x1008050005081020, 0x20110a000a112040, 0x4122140014224180, 0x8244280028448201, 0x488500050880402, 0x810a000a0100804, 0x1020400040201008, 0x402000204081020, 0x805000508102040, 0x110a000a11204080, 0x2214001422418000, 0x4428002844820100, 0x8850005088040201, 0x10a000a010080402, 0x2040004020100804, 0x200020408102040, 0x500050810204080, 0xa000a1120408000, 0x1400142241800000, 0x2800284482010000, 0x5000508804020100, 0xa000a01008040201, 0x4000402010080402, 0x2040810204080, 0x5081020408000, 0xa112040800000, 0x14224180000000, 0x28448201000000, 0x50880402010000, 0xa0100804020100, 0x40201008040201 };
    static readonly ulong[] rookTable = { 0x1010101010101fe, 0x2020202020202fd, 0x4040404040404fb, 0x8080808080808f7, 0x10101010101010ef, 0x20202020202020df, 0x40404040404040bf, 0x808080808080807f, 0x10101010101fe01, 0x20202020202fd02, 0x40404040404fb04, 0x80808080808f708, 0x101010101010ef10, 0x202020202020df20, 0x404040404040bf40, 0x8080808080807f80, 0x101010101fe0101, 0x202020202fd0202, 0x404040404fb0404, 0x808080808f70808, 0x1010101010ef1010, 0x2020202020df2020, 0x4040404040bf4040, 0x80808080807f8080, 0x1010101fe010101, 0x2020202fd020202, 0x4040404fb040404, 0x8080808f7080808, 0x10101010ef101010, 0x20202020df202020, 0x40404040bf404040, 0x808080807f808080, 0x10101fe01010101, 0x20202fd02020202, 0x40404fb04040404, 0x80808f708080808, 0x101010ef10101010, 0x202020df20202020, 0x404040bf40404040, 0x8080807f80808080, 0x101fe0101010101, 0x202fd0202020202, 0x404fb0404040404, 0x808f70808080808, 0x1010ef1010101010, 0x2020df2020202020, 0x4040bf4040404040, 0x80807f8080808080, 0x1fe010101010101, 0x2fd020202020202, 0x4fb040404040404, 0x8f7080808080808, 0x10ef101010101010, 0x20df202020202020, 0x40bf404040404040, 0x807f808080808080, 0xfe01010101010101, 0xfd02020202020202, 0xfb04040404040404, 0xf708080808080808, 0xef10101010101010, 0xdf20202020202020, 0xbf40404040404040, 0x7f80808080808080 };
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
    public static BitBoard pieceAttack(int position, int piece, bool isWhite)
    {
        switch (piece & 7)
        {
            case 1:
                return pawnAttack(position, isWhite);
            case 2:
                return bishopAttack(position);
            case 3:
                return knightAttack(position);
            case 4:
                return rookAttack(position);
            case 5:
                return rookAttack(position) | bishopAttack(position);
            case 6:
                return kingAttack(position);
        }
        throw new ArgumentOutOfRangeException();
    }
    public static BitBoard pawnAttack(int piece, bool isWhite)
    {
        BitBoard bitboardPiece = (BitBoard)piece;
        return isWhite ? bitboardPiece.NEshift() | bitboardPiece.NWshift() : bitboardPiece.SEshift() | bitboardPiece.SWshift();
    }
    public static BitBoard bishopAttack(int pos)
    {
        return new BitBoard(bishopTable[pos]);
    }
    public static BitBoard knightAttack(int piece)
    {
        BitBoard bitboardPiece = (BitBoard)piece;
        BitBoard returno = new BitBoard();
        BitBoard north = bitboardPiece.Nshift().Nshift();
        BitBoard south = bitboardPiece.Sshift().Sshift();
        BitBoard east = bitboardPiece.Eshift().Eshift();
        BitBoard west = bitboardPiece.Wshift().Wshift();
        return returno | north.Eshift() | north.Wshift() | south.Eshift() | south.Wshift() | east.Nshift() | east.Sshift() | west.Nshift() | west.Sshift();
    }
    public static BitBoard rookAttack(int piece)
    {
        return new BitBoard(rookTable[piece]);
    }

    public static BitBoard kingAttack(int piece)
    {
        BitBoard bitboardPiece = (BitBoard)piece;
        return bitboardPiece.Nshift() | bitboardPiece.Sshift() | bitboardPiece.Eshift() | bitboardPiece.Wshift() | bitboardPiece.NEshift() | bitboardPiece.NWshift() | bitboardPiece.SEshift() | bitboardPiece.SWshift();
    }
}

public class Position
{
    public Stack<PositionState> state;
    public PiecePositions whitePositions;
    public PiecePositions blackPositions;
    //utility functions
    public Position(BitBoard[] white, BitBoard[] black)
    {
        state = new Stack<PositionState>();
        state.Push(new PositionState());
        whitePositions = new PiecePositions(white[0],white[1],white[2],white[3],white[4],white[5]);
        blackPositions = new PiecePositions(black[0],black[1],black[2],black[3],black[4],black[5]);
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
        if (state.Peek().NextToMove == 8){
            whitePositions.positions[(move.MovedPiece & 7)-1] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
            for (int i = 0; i < blackPositions.positions.Length; i++)
            {
                blackPositions.positions[i] &= ~(BitBoard)move.TargetSquare;
            }
            }
        else{
            blackPositions.positions[(move.MovedPiece & 7)-1] ^= (BitBoard)move.TargetSquare | (BitBoard)move.SourceSquare;
            for (int i = 0; i < whitePositions.positions.Length; i++)
            {
                whitePositions.positions[i] &= ~(BitBoard)move.TargetSquare;
            }
            }
    }

    public int PieceAt(int square)
    {
        int returno = 0;
        BitBoard[] allposition = whitePositions.positions.Concat(blackPositions.positions).ToArray();
        for (int i = 0; i < allposition.Length; i++)
        {
            if ((allposition[i] & (BitBoard)square ) > 0) 
                returno = i > 6 ? (i % 6) + 1 | 16 : i + 1 | 8;
        }
        return returno;
    }

    //Generation
    public static BitBoard[,] generateLookupTable(bool isRook)
    {
        BitBoard H = (BitBoard)0x101010101010101;
        BitBoard H2 = (BitBoard)0x8080808080808080;
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
        BitBoard[,] returno = new BitBoard[64,1 << 13];
        for (int i = 0; i < 64; i++)
        {
            BitBoard mask = isRook ? PiecePositions.rookAttack(i) : PiecePositions.bishopAttack(i);
            mask &= ~endPos[i];
            BitBoard[] blockingMasks = createBlockers(mask);
            foreach (BitBoard blocker in blockingMasks)
            {
                ulong key = isRook ? ((ulong)blocker * PrecomputedMagics.RookMagics[i]) >> PrecomputedMagics.RookShifts[i] :
                (ulong)blocker * PrecomputedMagics.BishopMagics[i] >> PrecomputedMagics.BishopShifts[i];
                BitBoard legalMoves = generateLegalMoves(i, blocker, isRook);
                returno[i,key] = legalMoves;
            }
        }
        return returno;

        BitBoard[] createBlockers(BitBoard mask)
        {
            List<int> attackbits = new List<int>();
            for (int i = 0; i < 64; i++)
            {
                if (((mask >> i) & BitBoard.one) == 1)
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
        BitBoard generateLegalMoves(int index, BitBoard blocker, bool isRook)
        {
            BitBoard returno = new BitBoard();
            int[] rookDir = { 1, 8, -1, -8 };
            int[] bishopDir = { 7, 9, -7, -9 };
            int[] directions = isRook ? rookDir : bishopDir;

            for (int i = 0; i < 4; i++)
            {
                BitBoard piece = (BitBoard)index;
                while(((piece & blocker) == 0)&&(piece > 0)&&((piece & endPos[index]) == 0) && !(((BitBoard)index & H) > 0 && (directions[i] ==  -1 || directions[i] ==  -9 ||directions[i] ==  7))&& !(((BitBoard)index & H2) > 0 && (directions[i] ==  1 || directions[i] ==  -7 || directions[i] ==  9))){
                    piece <<= directions[i];
                    returno |= piece;
                }
            }
            return returno;
        }
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
    public PositionState()
    {
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
