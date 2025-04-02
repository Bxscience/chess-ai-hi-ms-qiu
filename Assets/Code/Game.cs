//TODO
/*
null pruning maybe
do the memory allocation state thing maybe
killer moves
move ordering
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    public GameObject tile;
    public GameObject[] pieces;
    public int depth;
    private PositionState[] states;
    private Position gameState;
    private const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private GameObject[] gameObjectBoard = new GameObject[64];
    private GameObject[] tiles = new GameObject[64];
    private int[] board = new int[64];
    private Transform selectedTile;
    private bool isMakingMove = false;
    private Color lastTileColor;
    private PlayerInput playerInput;
    private MoveGenerator moveGenerator = new();
    private TranspositionTable transpositionTable = new();
    private BitBoard whitePromoteRow = new(0xff00000000000000);
    private BitBoard blackPromoteRow = new(0xff);
    private readonly Dictionary<string, short> posLookup = new Dictionary<string, short> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 }, { "E", 5 }, { "F", 6 }, { "G", 7 }, { "H", 8 } };
    void Awake()
    {
        playerInput = new PlayerInput();
    }
    private void OnEnable()
    {
        playerInput.PlayerInputs.Click.performed += onClick;
        playerInput.PlayerInputs.Undo.performed += undoLastMove;
        playerInput.PlayerInputs.Test.performed += test;
        playerInput.Enable();
    }

    void OnDisable()
    {
        playerInput.Disable();
        playerInput.PlayerInputs.Click.performed -= onClick;
        playerInput.PlayerInputs.Undo.performed -= undoLastMove;
        playerInput.PlayerInputs.Test.performed -= test;
    }

    private void test(InputAction.CallbackContext context)
    {

        Span<int> ints = stackalloc int[5];
        ints[0] = 3;
        ints[1] = 4;

        //Evaluator.EvaluateBoard(gameState);
        // Debug.Log(Evaluator.EvaluateBoard(gameState, moveGenerator));

        // foreach (Move item in moveGenerator.generateMoves(gameState))
        // {
        //     //if((item.MovedPiece & 7 )== 1 && (item.TargetSquare ==  item.SourceSquare + 9|| item.TargetSquare == item.SourceSquare + 7)) 

        //     //Debug.Log("source: " + item.SourceSquare + " target: " + item.TargetSquare + " piece: " + item.MovedPiece);
        // }

    }

    void Start()
    {

        BitBoard[] white = new BitBoard[6];
        BitBoard[] black = new BitBoard[6];
        PositionState startState = initalizeGame(startFEN);

        for (int j = 0; j < 64; j++)
        {

            if (board[j] != 0)
            {
                if (board[j] >> 3 == 1) white[(board[j] & 7) - 1] |= (BitBoard)j;
                else black[(board[j] & 7) - 1] |= (BitBoard)j;
            }

        }

        gameState = new Position(white, black);
        gameState.state.Push(startState);
        moveGenerator.initializeLookup();
        transpositionTable.init();
        initalizeBoard();

    }
    void Update()
    {

    }
    //For initalizing the internal movement and bitboards
    PositionState initalizeGame(string FENString)
    {

        string[] FEN = splitFEN(FENString);
        placementParse(FEN[0]);
        int side = sideParse(FEN[1]);
        CastlingFlags[] castlings = castlingRightsParse(FEN[2]);
        int enPassantTarget = enPassantTargetParse(FEN[3]);
        int halfMoveClock = halfMoveClockParse(FEN[4]);
        int fullMoveCount = fullMoveCountParse(FEN[5]);
        return new(new(), castlings[0], castlings[1], side, enPassantTarget, halfMoveClock, fullMoveCount, -1, -1, -1);

    }
    //For initalizing game objects
    void initalizeBoard()
    {

        for (int i = 0; i < 8; i++)
        {

            for (int j = 0; j < 8; j++)
            {

                int index = i * 8 + j;
                tiles[index] = Instantiate(tile, new Vector3(j, 0, i), Quaternion.identity);
                Renderer mat = tiles[index].GetComponent<Renderer>();
                mat.material.color = (i + j) % 2 == 0 ? Color.white : Color.black;

                if ((board[index] & 7) != 0)
                {

                    gameObjectBoard[index] = Instantiate(pieces[board[index] & 7], new Vector3(j, 0, i), Quaternion.identity);
                    gameObjectBoard[index].transform.localScale = Vector3.one;
                    if (board[index] >> 3 == 1)
                        gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.white;
                    else
                        {gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.gray;
                    gameObjectBoard[index].transform.rotation = Quaternion.Euler(0, 180, 0);}

                }

            }

        }

    }
    //Player input
    public void onClick(InputAction.CallbackContext context)
    {

        int sideToMove = gameState.state.Peek().NextToMove;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (isMakingMove & selectedTile != null)
        {

            if (Physics.Raycast(ray, out RaycastHit hit2, 10000, 1 << 6))
            {

                Span<Move> playerMoves = stackalloc Move[32];
                bool isLegal = false;
                int selectedTileIndex = (int)(selectedTile.transform.position.x + selectedTile.transform.position.z * 8);
                int toTileIndex = (int)(hit2.transform.position.x + hit2.transform.position.z * 8);

                foreach (Move move in playerMoves)
                {

                    isLegal = move.TargetSquare == toTileIndex ? true : isLegal;

                }

                if (isLegal)
                {

                    isMakingMove = false;
                    Move currentMove = new Move(selectedTileIndex, toTileIndex, board[selectedTileIndex]);
                    gameState.updateBoardWithMove(currentMove);
                    updateBoardWithMove(currentMove);
                    selectedTile.GetComponent<Renderer>().material.color = lastTileColor;

                    foreach (Move move in playerMoves)
                    {

                        tiles[move.TargetSquare].GetComponent<Renderer>().material.color = (move.TargetSquare % 8 + move.TargetSquare / 8) % 2 == 0 ? Color.white : Color.black;

                    }


                    // Bot's Move
                    // Move optimalMove = findBotMove(3, false);
                    // gameState.updateBoardWithMove(optimalMove);
                    // updateBoardWithMove(optimalMove);

                }

                else
                {

                    isMakingMove = false;
                    selectedTile.GetComponent<Renderer>().material.color = lastTileColor;

                    foreach (Move move in playerMoves)
                    {
                        tiles[move.TargetSquare].GetComponent<Renderer>().material.color = (move.TargetSquare % 8 + move.TargetSquare / 8) % 2 == 0 ? Color.white : Color.black;
                    }

                }

            }
        }

        else if (Physics.Raycast(ray, out RaycastHit hit, 10000, 1 << 6) && gameObjectBoard[(int)(hit.transform.position.x + hit.transform.position.z * 8)] != null && board[(int)(hit.transform.position.x + hit.transform.position.z * 8)] >> 3 << 3 == (int)sideToMove)
        {

            int pos = (int)(hit.transform.position.x + hit.transform.position.z * 8);
            selectedTile = hit.transform;
            Renderer renderer = selectedTile.GetComponent<Renderer>();
            lastTileColor = renderer.material.color;
            renderer.material.color = Color.yellow;
            isMakingMove = true;
            Span<Move> moves = stackalloc Move[32];
            int count = 0;

            switch (board[pos] & 7)
            {

                case 1:
                    moveGenerator.createJumpingMove(gameState, pos,ref moves,ref count);
                    break;
                case 2:
                    moveGenerator.createSlidingMove(gameState, pos,ref moves,ref count);
                    break;
                case 3:
                    moveGenerator.createJumpingMove(gameState, pos,ref moves,ref count);
                    break;
                case 4:
                    moveGenerator.createSlidingMove(gameState, pos,ref moves,ref count);
                    break;
                case 5:
                    moveGenerator.createSlidingMove(gameState, pos,ref moves,ref count);
                    break;
                case 6:
                    moveGenerator.createKingMove(gameState, pos,ref moves,ref count);
                    break;

            }

            foreach (Move move in moves)
            {
                tiles[move.TargetSquare].GetComponent<Renderer>().material.color = Color.yellow;
            }

        }

    }
    //Bot search

    private Move findBotMove(int depth, bool isMax)
    {

        Move returno = new();
        float bestScore = isMax ? float.NegativeInfinity : float.PositiveInfinity;
        Span<Move> moves = stackalloc Move[218];
        moveGenerator.generateMoves(gameState,ref moves);

        foreach (Move move in moves)
        {
            gameState.updateBoardWithMove(move);
            float evalScore = botSearch(depth - 1, !isMax, gameState, float.NegativeInfinity, float.PositiveInfinity);
            if ((evalScore > bestScore && isMax) || (evalScore < bestScore && !isMax))
            {
                bestScore = evalScore;
                returno = move;
            }
            gameState.undoLastMove();
        }

        if (returno.SourceSquare != 0 && returno.TargetSquare != 0) return returno;
        throw new IndexOutOfRangeException();

    }
    private float botSearch(int depth, bool isMax, Position gameState, float alpha, float beta)
    {

        if (depth <= 0) return Evaluator.EvaluateBoard(gameState, moveGenerator);

        float score = isMax ? -Mathf.Infinity : Mathf.Infinity;
        int key = transpositionTable.zobristKey(gameState);

        // if (transpositionTable.hasKey(key))
        //     return transpositionTable.table[key];


        if (isMax)
        {

            Span<Move> moves = stackalloc Move[218];
            moveGenerator.generateMoves(gameState, ref moves);

            foreach (Move move in moves)
            {

                gameState.updateBoardWithMove(move);
                float y = botSearch(depth - 1, !isMax, gameState, alpha, beta);

                if (score < y)
                {
                    score = y;
                }

                alpha = Math.Max(alpha, score);

                transpositionTable.addEntery(key, score);

                gameState.undoLastMove();

                if (beta <= alpha)
                    break;

            }

            return score;

        }

        else
        {

            Span<Move> moves = stackalloc Move[218];
            moveGenerator.generateMoves(gameState, ref moves);

            foreach (Move move in moves)
            {
                gameState.updateBoardWithMove(move);
                float y = botSearch(depth - 1, !isMax, gameState, alpha, beta);

                if (score > y)
                {
                    score = y;
                }

                beta = Math.Min(beta, score);

                transpositionTable.addEntery(key, score);

                gameState.undoLastMove();

                if (beta <= alpha)
                    break;

            }

            return score;

        }

    }

    private void orderMoves(ref Stack<Move> moves, float alpha, float beta)
    {
        moveGenerator.updateAttackBitboard(gameState, gameState.state.Peek().NextToMove ^ 24);
        float[] moveScores = new float[218]; 
        foreach (Move move in moves)
        {

            float approxScore = 0;
            int targetedPiece = gameState.PieceAt(move.TargetSquare);
            PositionState state = gameState.state.Peek();
            bool isWhite = state.NextToMove == 8;
            int sideMulti = isWhite ? 1 : -1;

            //MVV-LVA

            if (targetedPiece > 0)
            {
                approxScore = sideMulti * (Evaluator.pieceValue(targetedPiece) - Evaluator.pieceValue(move.MovedPiece));
            }

            //pawn promotion

            if ((move.MovedPiece == 9 && ((BitBoard)move.TargetSquare & whitePromoteRow) > 0) || (move.MovedPiece == 17 && ((BitBoard)move.TargetSquare & blackPromoteRow) > 0))
            {
                approxScore += 900 * sideMulti;
            }

            //don't move into pawn attacks

            BitBoard enemyPawnBitboard = gameState.state.Peek().NextToMove == 16 ? moveGenerator.totalWhiteAttackBitboards[0] : moveGenerator.totalBlackAttackBitboards[0];
            if (((BitBoard)move.TargetSquare & enemyPawnBitboard) > 0)
                approxScore -= Evaluator.pieceValue(move.MovedPiece) * sideMulti;


        }
    }

    //Helper functions
    private void updateGameBoardtoGameState()
    {

        for (int i = 0; i < 8; i++)
        {

            for (int j = 0; j < 8; j++)
            {

                int index = i * 8 + j;
                int piece = gameState.PieceAt(index);
                board[index] = piece;

                if (gameObjectBoard[index] != null) Destroy(gameObjectBoard[index]);

                if (piece != 0)
                {

                    gameObjectBoard[index] = Instantiate(pieces[piece & 7], new Vector3(j, 0, i), Quaternion.identity);
                    gameObjectBoard[index].transform.localScale = Vector3.one;

                    if (piece >> 3 == 1)
                        gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.white;
                    else
                        {gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.gray;
                    gameObjectBoard[index].transform.rotation = Quaternion.Euler(0, 180, 0);}

                }

            }

        }

    }

    public void undoLastMove(InputAction.CallbackContext context)
    {

        PositionState lastState = gameState.state.Peek();
        gameState.undoLastMove();
        updateGameBoardtoGameState();

    }
    private void updateBoardWithMove(Move move)
    {

        board[move.TargetSquare] = board[move.SourceSquare];
        board[move.SourceSquare] = 0;

        updateGameBoardtoGameState();

    }

    public static void Quicksort(System.Span<Move> values, int[] scores, int low, int high)
    {
        if (low < high)
        {
            int pivotIndex = Partition(values, scores, low, high);
            Quicksort(values, scores, low, pivotIndex - 1);
            Quicksort(values, scores, pivotIndex + 1, high);
        }
    }

    static int Partition(System.Span<Move> values, int[] scores, int low, int high)
    {
        int pivotScore = scores[high];
        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (scores[j] > pivotScore)
            {
                i++;
                (values[i], values[j]) = (values[j], values[i]);
                (scores[i], scores[j]) = (scores[j], scores[i]);
            }
        }
        (values[i + 1], values[high]) = (values[high], values[i + 1]);
        (scores[i + 1], scores[high]) = (scores[high], scores[i + 1]);

        return i + 1;
    }

    //FEN parse
    private string[] splitFEN(string FENPosition)
    {
        try
        {
            string[] FENparts = new string[6];
            for (int i = 0; i < 5; i++)
            {
                FENparts[i] = FENPosition.Substring(0, FENPosition.IndexOf(' '));
                FENPosition = FENPosition.Substring(FENPosition.IndexOf(' ') + 1);
            }
            FENparts[5] = FENPosition;
            return FENparts;
        }
        catch (System.Exception)
        {
            Debug.Log("FEN must be 6 parts!");
            throw;
        }
    }
    private void placementParse(string FENPosition)
    {
        Dictionary<char, int> pieceLookup = new Dictionary<char, int>() { { 'p', 1 }, { 'b', 2 }, { 'n', 3 }, { 'r', 4 }, { 'q', 5 }, { 'k', 6 } };
        int rank = 7;
        int file = 0;
        foreach (char character in FENPosition)
        {
            if (character == '/')
            {
                rank -= 1;
                file = 0;
            }
            else
            {
                if (int.TryParse(character.ToString(), out int result))
                {
                    file += result;
                }
                else
                {
                    int piece = pieceLookup[char.ToLower(character)];
                    int color = char.IsUpper(character) ? 8 : 16;
                    board[rank * 8 + file] = piece | color;
                    file += 1;
                }
            }
        }
    }
    private int sideParse(string FENPosition)
    {
        if (FENPosition == "w")
        {
            return 8;
        }
        return 16;
    }
    private CastlingFlags[] castlingRightsParse(string FENPosition)
    {
        CastlingFlags[] flags = new CastlingFlags[2];
        flags[0] = CastlingFlags.None;
        flags[1] = CastlingFlags.None;
        for (int i = 0; i < FENPosition.Length; i++)
        {
            char[] right = FENPosition.ToCharArray();
            switch (right[i])
            {
                case 'K':
                    flags[1] = CastlingFlags.KingSide;
                    break;
                case 'Q':
                    if (flags[1] == CastlingFlags.KingSide) flags[1] = CastlingFlags.Both;
                    else flags[1] = CastlingFlags.QueenSide;
                    break;
                case 'k':
                    flags[0] = CastlingFlags.KingSide;
                    break;
                case 'q':
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
