using UnityEditor.ShaderKeywordFilter;

public enum PieceType { 
None, Pawn, Knight, Bishop, Rook, Queen, King 
}

public enum PieceColor { None, White, Black }
public enum CastlingFlags {KingSide, QueenSide, Both,None}

public struct ChessPiece {
    public PieceType type;
    public PieceColor color;

    public ChessPiece(PieceType type, PieceColor color) {
        this.type = type;
        this.color = color;
    }
    public override bool Equals(object obj)
    {  
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return ((ChessPiece)obj).color == color && ((ChessPiece)obj).type == type;
    }
}
