using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Game : MonoBehaviour
{
    public GameObject tile;
    public GameObject pawn;
    public GameObject bishop;
    public GameObject knight;
    public GameObject rook;
    public GameObject queen;
    public GameObject king;
    Position gameState;
    private const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    void Start()
    {
        initalizeBoard();
        gameState = new Position(startFEN);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void startNewGame(){
        gameState = new Position(startFEN);
    }
    void initalizeBoard(){
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject newTile = Instantiate(tile,new Vector3(i,0,j), Quaternion.identity);
                Material mat = newTile.GetComponent<Material>();
                mat.color =  (i + j * 8)%2 == 0? Color.white: Color.black;
            }
        }
    } 
}
