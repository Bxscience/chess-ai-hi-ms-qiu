using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class TranspositionTable
{

    public float[] table;
    private int[,] randomPieceValues = new int[64,22];
    private int blackToMove;
    private int[] castlingRights = new int[8];

    
    public void init(){

        table = new float[33554432];

        const int seed = 63912948;
        System.Random random = new(seed);

        for (int i = 0; i < 33554432; i++)
        {
            table[i] = float.NaN;
        }

        for (int j = 0; j < 64; j++)
        {

            for (int i = 0; i < 22; i++)
            {
                randomPieceValues[j,i] = random.Next(1,33554432);
            }
    
        }

        for (int i = 0; i < 8; i++)
        {
            castlingRights[i] = random.Next(1,33554432);
        }

        blackToMove = random.Next(1,33554432);

    }
    public int zobristKey(Position board){

        int returno = 0;
        PositionState state = board.state.Peek();

        BitBoard[] allposition = board.whitePositions.positions.Concat(board.blackPositions.positions).ToArray();

        for (int i = 0; i < allposition.Length; i++)
        {
            if ((allposition[i] & (BitBoard)i) > 0)
                returno ^= randomPieceValues[i,i >= 6 ? i - 5 | 16 : i + 1 | 8];
        }

        switch (state.WhiteCastlingRights)
        {

            case CastlingFlags.KingSide:
            returno ^= castlingRights[0];
            break;
            case CastlingFlags.QueenSide:
            returno ^= castlingRights[1];
            break;
            case CastlingFlags.Both:
            returno ^= castlingRights[2];
            break;
            case CastlingFlags.None:
            returno ^= castlingRights[3];
            break;

        }

        switch (state.BlackCastlingRights)
        {
            
            case CastlingFlags.KingSide:
            returno ^= castlingRights[4];
            break;
            case CastlingFlags.QueenSide:
            returno ^= castlingRights[5];
            break;
            case CastlingFlags.Both:
            returno ^= castlingRights[6];
            break;
            case CastlingFlags.None:
            returno ^= castlingRights[7];
            break;
            
        }
        
        if(state.NextToMove == 16) 
            returno ^= blackToMove;

        return returno;

    }

    public void addEntery(int key, float value){
        table[key] = value;
    }
    public bool hasKey(int key){
        if(!float.IsNaN(table[key])) 
            return true;
        return false;
    }

}
