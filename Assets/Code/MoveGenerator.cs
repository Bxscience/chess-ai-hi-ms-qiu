using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class MoveGenerator
{
    public BitBoard whiteAttackBitboard;
    public BitBoard blackAttackBitboard;
    public BitBoard[] totalWhiteAttackBitboards;
    public BitBoard[] totalBlackAttackBitboards;
    private BitBoard[,] rookLookup;
    private BitBoard[,] bishopLookup;
    private BitBoard[] knightLookup;
    private BitBoard[] kingLookup;
    private BitBoard rank2 = new BitBoard(0xff00);
    private BitBoard rank7 = (BitBoard)0xff000000000000;
    private bool inCheck = false;
    private BitBoard whiteKingsideCastleBitboard = new(0x60);
    private BitBoard whiteQueensideCastleBitboard = new(0xe);
    private BitBoard blackKingsideCastleBitboard = new(0x6000000000000000);
    private BitBoard blackQueensideCastleBitboard = new(0xe00000000000000);

    public void generateMoves(Position board, ref Span<Move> moves, ref int count)
    {

        BitBoard[] allPieces = board.state.Peek().NextToMove == 8 ? board.whitePositions.positions : board.blackPositions.positions;
        inCheck = false;

        for (int i = 0; i < 6; i++)
        {

            BitBoard pieces = allPieces[i];
            while (pieces > 0)
            {
                
                int piecePosition = BitBoard.bitscan(pieces);

                switch (i)
                {

                    case 0: createJumpingMove(board, piecePosition, ref moves, ref count); break;
                    case 1: createSlidingMove(board, piecePosition, ref moves, ref count); break;
                    case 2: createJumpingMove(board, piecePosition, ref moves, ref count); break;
                    case 3: createSlidingMove(board, piecePosition, ref moves, ref count); break;
                    case 4: createSlidingMove(board, piecePosition, ref moves, ref count); break;
                    case 5: createKingMove(board, piecePosition, ref moves, ref count); break;

                }

                pieces ^= pieces & -pieces;

            }

        }

        moves = moves.Slice(0, count);

    }
    public void initializeLookup()
    {

        rookLookup = Position.generateLookupTable(true);
        bishopLookup = Position.generateLookupTable(false);
        knightLookup = new BitBoard[64];
        kingLookup = new BitBoard[64];
        whiteAttackBitboard = new();
        blackAttackBitboard = new();
        totalBlackAttackBitboards = new BitBoard[6];
        totalWhiteAttackBitboards = new BitBoard[6];

        for (int i = 0; i < 64; i++)
        {
            knightLookup[i] = PiecePositions.knightAttack(i);
            kingLookup[i] = PiecePositions.kingAttack(i);
        }

    }
    public void createJumpingMove(Position board, int square, ref Span<Move> moves, ref int count)
    {

        int piece = board.PieceAt(square);
        BitBoard attack = new();
        PositionState currentState = board.state.Peek();

        if ((piece & 7) == 3)
        {
            attack = knightLookup[square];
        }

        else
        {

            bool isWhite = currentState.NextToMove == 8;
            attack = ((isWhite ? board.blackPositions.allPositions : board.whitePositions.allPositions) | (BitBoard)currentState.EnPassantTarget) & PiecePositions.pawnAttack(square, currentState.NextToMove);
            attack |= isWhite ? (BitBoard)square << 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions) : (BitBoard)square >> 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

            if ((rank2 & (BitBoard)square) > 0 && isWhite && attack > 0)
                attack |= (BitBoard)square << 16 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

            else if ((rank7 & (BitBoard)square) > 0 && !isWhite && attack > 0)
                attack |= (BitBoard)square >> 16 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

        }

        attack &= currentState.NextToMove == 8 ? ~board.whitePositions.allPositions : ~board.blackPositions.allPositions;

        createMoves(attack, square, piece, ref moves, ref count);

    }
    public void createKingMove(Position board, int square, ref Span<Move> moves, ref int count)
    {

        BitBoard attack = kingLookup[square];
        PositionState state = board.state.Peek();
        attack &= ~(state.NextToMove == 8 ? board.whitePositions.allPositions : board.blackPositions.allPositions);

        if (state.NextToMove == 8 &&
        (state.WhiteCastlingRights == CastlingFlags.Both || state.WhiteCastlingRights == CastlingFlags.KingSide) &&
        ((board.whitePositions.allPositions | board.blackPositions.allPositions) & whiteKingsideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x40);
        }

        if (state.NextToMove == 8 &&
        (state.WhiteCastlingRights == CastlingFlags.Both || state.WhiteCastlingRights == CastlingFlags.QueenSide) &&
        ((board.whitePositions.allPositions | board.blackPositions.allPositions) & whiteQueensideCastleBitboard) == 0)
        {
            attack |= new BitBoard(2);
        }

        if (state.NextToMove == 16 &&
        (state.BlackCastlingRights == CastlingFlags.Both || state.BlackCastlingRights == CastlingFlags.KingSide) &&
        ((board.whitePositions.allPositions | board.blackPositions.allPositions) & blackKingsideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x4000000000000000);
        }

        if (state.NextToMove == 16 &&
        (state.BlackCastlingRights == CastlingFlags.Both || state.BlackCastlingRights == CastlingFlags.QueenSide) &&
        ((board.whitePositions.allPositions | board.blackPositions.allPositions) & blackQueensideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x200000000000000);
        }

        updateAttackBitboard(board, state.NextToMove ^ 24);
        BitBoard enemyAttackBitboard = state.NextToMove == 8 ? blackAttackBitboard : whiteAttackBitboard;
        attack &= ~enemyAttackBitboard;

        if(((BitBoard)square & enemyAttackBitboard) > 0)
            inCheck = true;

        createMoves(attack, square, 6 | state.NextToMove ,ref moves, ref count);

    }
    public void createSlidingMove(Position board, int square, ref Span<Move> moves, ref int count)
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

        createMoves(attack, square, piece, ref moves, ref count);

    }
    public BitBoard createPieceAttackBitBoard(Position board, int square, int piece, int playerToMove)
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
        PositionState currentState = board.state.Peek();
        BitBoard alliedPieces = ~(playerToMove == 8 ? board.whitePositions.allPositions : board.blackPositions.allPositions);

        switch (piece & 7)
        {

            case 1:
                BitBoard attack = new();
                bool isWhite = playerToMove == 8;
                attack = ((isWhite ? board.blackPositions.allPositions : board.whitePositions.allPositions) | (BitBoard)currentState.EnPassantTarget) & PiecePositions.pawnAttack(square, currentState.NextToMove);
                attack |= isWhite ? (BitBoard)square << 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions) : (BitBoard)square >> 8 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

                if ((rank2 & (BitBoard)square) > 0 && isWhite && attack > 0)
                    attack |= (BitBoard)square << 16 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

                else if ((rank7 & (BitBoard)square) > 0 && !isWhite && attack > 0)
                    attack |= (BitBoard)square >> 16 & ~(board.whitePositions.allPositions | board.blackPositions.allPositions);

                return alliedPieces & attack;

            case 2:
                return alliedPieces & bishopLookup[square, ((ulong)(PiecePositions.bishopAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]];

            case 3:
                return alliedPieces & knightLookup[square];

            case 4:
                return alliedPieces & rookLookup[square, ((ulong)(PiecePositions.rookAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]];

            case 5:
                return alliedPieces & bishopLookup[square, ((ulong)(PiecePositions.bishopAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.BishopMagics[square]) >> PrecomputedMagics.BishopShifts[square]] | rookLookup[square, ((ulong)(PiecePositions.rookAttack(square) & ~endPos[square] & allPieces) * PrecomputedMagics.RookMagics[square]) >> PrecomputedMagics.RookShifts[square]];

            case 6:
                return alliedPieces & kingLookup[square];

        }
        throw new IndexOutOfRangeException();

    }
    public void updateAttackBitboard(Position board, int side){

        bool isWhite = side == 8;
        BitBoard[] bitBoards = isWhite? board.whitePositions.positions:board.blackPositions.positions ;
        BitBoard returno = new();

        for (int i = 0; i < 6; i++)
        {
            BitBoard copy = bitBoards[i];
            while(copy > 0){
                BitBoard x;
                if (i == 0){
                    x = PiecePositions.pawnAttack(BitBoard.bitscan(copy), side);
                    returno |= x;
                    if(isWhite)
                        totalWhiteAttackBitboards[i] |= x;
                    else
                        totalBlackAttackBitboards[i] |= x;
                }
                else{
                    x = createPieceAttackBitBoard(board,BitBoard.bitscan(copy), (i+1) | side, side); 
                    returno |= x;
                    if(isWhite)
                        totalWhiteAttackBitboards[i] |= x;
                    else
                    totalBlackAttackBitboards[i] |= x;
                }

                copy ^= copy & -copy;

            }
        }

        if(isWhite)
            whiteAttackBitboard = returno;
        else
            blackAttackBitboard = returno;
    }
    
    public BitBoard[] getAttackDefendMap(Position board)
    {

        BitBoard[] returno = new BitBoard[128];
        PositionState state = board.state.Peek();
        BitBoard[] piecePositions = board.blackPositions.positions.Concat(board.whitePositions.positions).ToArray();


        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
    
                int index = BitBoard.bitscan(piecePositions[j]);
                BitBoard copy;

                    if (j == 0 || j == 6)
                        copy = PiecePositions.pawnAttack(index, j > 5 ? 16:8);
                    else
                        copy = createPieceAttackBitBoard(board, index,  ((j & 5) + 1) | (j > 5 ? 16:8),j > 5 ? 16:8);
    
                while (copy > 0)
                {
    
                    if((copy & (BitBoard)i) > 0) returno[i + (j > 5 ? 64:0)] |= (BitBoard)BitBoard.bitscan(copy);
    
                    copy ^= copy & -copy;

                }
    
            }
        }

        return returno;

    }
    private void createMoves(BitBoard attackMap, int startSquare, int piece, ref Span<Move> moves, ref int count)
    {

        for (int i = 0; i < 64; i++)
        {

            if ((attackMap & (BitBoard)i) > 0) {
                moves[count] = new Move(startSquare, i, piece);
                count++;
            }

        }

    }

}
public struct Move
{
    public int SourceSquare { get; }
    public int TargetSquare { get; }
    public int MovedPiece { get; }
    public Move(int startSquare, int targetSquare, int piece)
    {
        SourceSquare = startSquare;
        TargetSquare = targetSquare;
        MovedPiece = piece;
    }



}

