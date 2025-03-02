public struct  Move {
    public byte SourceSquare { get; }
    public byte TargetSquare { get; }
    public PieceType MovedPiece { get; }
    public PieceType? PromotedToPiece { get; }
    public Move(byte startSquare, byte targetSquare, PieceType piece, PieceType? promote){
        SourceSquare = startSquare;
        TargetSquare = targetSquare;
        MovedPiece = piece;
        PromotedToPiece = promote;
    }
}
