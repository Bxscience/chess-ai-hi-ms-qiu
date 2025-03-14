using System;
using System.Collections.Generic;
using System.Linq;
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
    private BitBoard whiteKingsideCastleBitboard = new(0x60);
    private BitBoard whiteQueensideCastleBitboard = new(0xe);
    private BitBoard blackKingsideCastleBitboard = new(0x6000000000000000);
    private BitBoard blackQueensideCastleBitboard = new(0xe00000000000000);
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
        attack &= ~(board.state.Peek().NextToMove == 8 ? board.whitePositions.allPositions : board.blackPositions.allPositions);

        if (board.state.Peek().NextToMove == 8 &&
        (board.state.Peek().WhiteCastlingRights == CastlingFlags.Both || board.state.Peek().WhiteCastlingRights == CastlingFlags.KingSide) &&
        (board.whitePositions.allPositions | board.blackPositions.allPositions & whiteKingsideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x40);
        }

        if (board.state.Peek().NextToMove == 8 &&
        (board.state.Peek().WhiteCastlingRights == CastlingFlags.Both || board.state.Peek().WhiteCastlingRights == CastlingFlags.QueenSide) &&
        (board.whitePositions.allPositions | board.blackPositions.allPositions & whiteQueensideCastleBitboard) == 0)
        {
            attack |= new BitBoard(2);
        }

        if (board.state.Peek().NextToMove == 16 &&
        (board.state.Peek().BlackCastlingRights == CastlingFlags.Both || board.state.Peek().BlackCastlingRights == CastlingFlags.KingSide) &&
        (board.whitePositions.allPositions | board.blackPositions.allPositions & blackKingsideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x4000000000000000);
        }

        if (board.state.Peek().NextToMove == 16 &&
        (board.state.Peek().BlackCastlingRights == CastlingFlags.Both || board.state.Peek().BlackCastlingRights == CastlingFlags.QueenSide) &&
        (board.whitePositions.allPositions | board.blackPositions.allPositions & blackQueensideCastleBitboard) == 0)
        {
            attack |= new BitBoard(0x200000000000000);
        }

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
    public List<Move> generateMoves(Position board)
    {

        List<Move> moves = new List<Move>();
        BitBoard[] allPieces = board.whitePositions.positions.Concat(board.blackPositions.positions).ToArray();

        for (int i = 0; i < 12; i++)
        {

            BitBoard pieces = allPieces[i];
            while (pieces > 0)
            {

                int piecePosition = BitBoard.bitscan(pieces);

                switch (i)
                {

                    case 0: moves.AddRange(createJumpingMove(board, piecePosition)); break;
                    case 1: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 2: moves.AddRange(createJumpingMove(board, piecePosition)); break;
                    case 3: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 4: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 5: moves.AddRange(createKingMove(board, piecePosition)); break;

                    case 6: moves.AddRange(createJumpingMove(board, piecePosition)); break;
                    case 7: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 8: moves.AddRange(createJumpingMove(board, piecePosition)); break;
                    case 9: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 10: moves.AddRange(createSlidingMove(board, piecePosition)); break;
                    case 11: moves.AddRange(createKingMove(board, piecePosition)); break;

                }

                pieces ^= pieces & -pieces;

            }

        }

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

            if ((attackMap & (BitBoard)i) > 0) moves.Add(new Move(startSquare, i, startSquare));

        }

        return moves;

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

