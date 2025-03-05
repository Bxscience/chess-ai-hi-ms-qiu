using UnityEditor.ShaderKeywordFilter;

public static class PieceType{
    public const int none = 0;
    public const int pawn = 1;
    public const int bishop = 2;
    public const int knight = 3;
    public const int rook = 4;
    public const int queen = 5;
    public const int king = 6;
    public const int white = 8;
    public const int black = 16;
}

public enum PieceColor { None, White, Black }
public enum CastlingFlags {KingSide, QueenSide, Both,None}