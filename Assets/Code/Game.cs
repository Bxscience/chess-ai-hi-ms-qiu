using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    public GameObject tile;
    public GameObject[] pieces;
    private Position gameState;
    private const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private GameObject[] gameObjectBoard = new GameObject[64];
    private int[] board = new int[64];
    private Transform selectedTile;
    private bool isMakingMove = false;
    private Color lastTileColor;
    private PlayerInput mouseInputAction;
    Dictionary<char, byte> pieceLookup = new Dictionary<char, byte>() { { 'p', 1 }, { 'b', 2 }, { 'n', 3 }, { 'r', 4 }, { 'q', 5 }, { 'k', 6 } };
    void Awake()
    {
        mouseInputAction = new PlayerInput();
    }
    private void OnEnable()
    {
        mouseInputAction.PlayerInputs.Click.performed += dosomething;
        mouseInputAction.Enable();
    }

    void OnDisable()
    {
        mouseInputAction.Disable();
        mouseInputAction.PlayerInputs.Click.performed -= dosomething;
    }

    private void dosomething(InputAction.CallbackContext context)
    {
        Debug.Log("please work");
    }

    void Start()
    {
        initalizeGame(startFEN );
        initalizeBoard();
        //gameState = new Position(startFEN);
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            onClick();
        }
    }
    //For initalizing the internal movement and bitboards
    void initalizeGame(string FENString)
    {
        string[] FEN = splitFEN(FENString);
        placementParse(FEN[0]);
        /*sideParse(FEN[1]);
        castlingRightsParse(FEN[2]);
        enPassantTargetParse(FEN[3]);
        halfMoveClockParse(FEN[4]);
        fullMoveCountParse(FEN[5]);
        gameState = new Position();*/
    }
    //Player input
    public void onClick()
    {
        Debug.Log("clicked");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (isMakingMove & selectedTile != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit2, 10000, 1 << 6))
            {
                isMakingMove = false;
                int selectedTileIndex = (int)(selectedTile.transform.position.x + selectedTile.transform.position.z * 8);
                int toTileIndex = (int)(hit2.transform.position.x + hit2.transform.position.z * 8);
                Debug.Log(toTileIndex);
                Move currentMove = new Move(selectedTileIndex, toTileIndex,board[selectedTileIndex], null);
                selectedTile.GetComponent<Renderer>().material.color = lastTileColor;
                updateBoardWithMove(currentMove);
            }
        }
        else if (Physics.Raycast(ray, out RaycastHit hit, 10000, 1 << 6) && gameObjectBoard[(int)(hit.transform.position.x + hit.transform.position.z * 8)] != null)
        {
            selectedTile = hit.transform;
            Renderer renderer = selectedTile.GetComponent<Renderer>();
            lastTileColor = renderer.material.color;
            renderer.material.color = Color.yellow;
            isMakingMove = true;
        }
    }
    //For initalizing game objects
    void initalizeBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int index = i * 8 + j;
                GameObject newTile = Instantiate(tile, new Vector3(i, 0, j), Quaternion.identity);
                Renderer mat = newTile.GetComponent<Renderer>();
                mat.material.color = (i + j) % 2 == 0 ? Color.white : Color.black;
                Debug.Log("index: " + index + " piece: " + pieces[board[index] & 7] + " int: " + board[index]);
                if ((board[index] & 7) != 0)
                {
                    gameObjectBoard[index] = Instantiate(pieces[board[index] & 7], new Vector3(j, 0, i), Quaternion.identity);
                    gameObjectBoard[index].transform.localScale = Vector3.one;
                    if (board[index] >> 3 == 1) gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.white;
                    else { gameObjectBoard[index].GetComponent<Renderer>().material.color = Color.black; gameObjectBoard[index].transform.rotation = Quaternion.Euler(0, 180, 0); }
                    ;
                }
            }
        }
    }
    //Helper functions
    void updateBoardWithMove(Move move){
        board[move.TargetSquare] = board[move.SourceSquare];
        board[move.SourceSquare] = 0;
        if(gameObjectBoard[move.TargetSquare]) Destroy(gameObjectBoard[move.TargetSquare]);
        gameObjectBoard[move.SourceSquare].transform.position = new Vector3(move.TargetSquare % 8, 0 ,move.TargetSquare/8);
        gameObjectBoard[move.TargetSquare] = gameObjectBoard[move.SourceSquare];
        gameObjectBoard[move.SourceSquare] = null;
    }
    //FEN parse
    private readonly Dictionary<string, short> posLookup = new Dictionary<string, short> { { "A", 1 }, { "B", 2 }, { "C", 3 }, { "D", 4 }, { "E", 5 }, { "F", 6 }, { "G", 7 }, { "H", 8 } };
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
