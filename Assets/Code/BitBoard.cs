using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public struct BitBoard 
{
    private readonly ulong _squares;
    public BitBoard(ulong bitboard){
        _squares = bitboard;
    }
    public static BitBoard operator &(BitBoard a, BitBoard b) 
    => new BitBoard(a._squares & b._squares);
    public static BitBoard operator |(BitBoard a, BitBoard b)
    => new BitBoard(a._squares | b._squares);
    public static BitBoard operator ^(BitBoard a, BitBoard b) 
    => new BitBoard(a._squares ^ b._squares);
    public static BitBoard operator >>(BitBoard a, int b) 
    => new BitBoard(b > 0 ? a._squares >> b : a._squares << -b);
    public static BitBoard operator <<(BitBoard a, int b) 
    => new BitBoard(b > 0 ? a._squares << b : a._squares >> -b);
    public static bool operator ==(BitBoard a, ulong b)
    => a._squares == b;
    public static bool operator !=(BitBoard a, ulong b)
    => a._squares != b;
    public static bool operator <(BitBoard a, ulong b)
    => a._squares < b;
    public static bool operator >(BitBoard a, ulong b)
    => a._squares > b;
    public static bool operator <=(BitBoard a, ulong b)
    => a._squares <= b;
    public static bool operator >= (BitBoard a, ulong b)    
    => a._squares >= b;
    public static BitBoard operator -(BitBoard a, BitBoard b)
    => new BitBoard(a._squares - b._squares);
    public static BitBoard operator +(BitBoard a, BitBoard b)
    => new BitBoard(a._squares + b._squares);
    public static BitBoard operator *(BitBoard a, BitBoard b)
    => new BitBoard(a._squares * b._squares);
    public static BitBoard operator ~(BitBoard a)
    => new BitBoard(~a._squares);
    public static explicit operator BitBoard(ulong a) 
    => new BitBoard(a);
    public static BitBoard zero => new BitBoard(0);
     public static explicit operator BitBoard(int a){
        if(a > 63){
            Debug.Log("not in range");
            throw new IndexOutOfRangeException();
        }
        ulong returno = 1ul<<(a);
        return new BitBoard(returno);
    }
    public static explicit operator ulong(BitBoard a)
    => a._squares;
    public override string ToString()
    {
        return _squares.ToString();//Convert.ToString((long)_squares, 2);
    }
    static readonly private ulong deBruijn = 0x03f79d71b4cb0a89L;
    static readonly private int[] magicTable = {
      0, 1,48, 2,57,49,28, 3,
     61,58,50,42,38,29,17, 4,
     62,55,59,36,53,51,43,22,
     45,39,33,30,24,18,12, 5,
     63,47,56,27,60,41,37,16,
     54,35,52,21,44,32,23,11,
     46,26,40,15,34,20,31,10,
     25,14,19, 9,13, 8, 7, 6,
    };

    static public int bitscan(BitBoard b) {
       int idx = (int)(((ulong)b * deBruijn) >> 58);
       return magicTable[idx];
    }
    
    public BitBoard NEshift(){return new BitBoard(_squares << 9);}
    public BitBoard NWshift(){return new BitBoard(_squares << 7);}
    public BitBoard Nshift(){return new BitBoard(_squares << 8);}
    public BitBoard SEshift( ){return new BitBoard(_squares >> 9);}
    public BitBoard SWshift( ){return new BitBoard(_squares >> 7);}
    public BitBoard Sshift( ){return new BitBoard(_squares >> 8);}
    public BitBoard Eshift( ){return new BitBoard(_squares >> 1);}
    public BitBoard Wshift( ){return new BitBoard(_squares << 1);}
public static BitBoard flipVertical(BitBoard a) {
   const ulong k1 = (0x00FF00FF00FF00FF);
   const ulong k2 = (0x0000FFFF0000FFFF);
   ulong x = a._squares;
   x = ((x >>  8) & k1) | ((x & k1) <<  8);
   x = ((x >> 16) & k2) | ((x & k2) << 16);
   x = ( x >> 32)       | ( x       << 32);
   return new BitBoard(x);
}
public static BitBoard mirrorHorizontal (BitBoard a) {
   const ulong k1 = (0x5555555555555555);
   const ulong k2 = (0x3333333333333333);
   const ulong k4 = (0x0f0f0f0f0f0f0f0f);
   ulong x = a._squares;
   x = ((x >> 1) & k1) | ((x & k1) << 1);
   x = ((x >> 2) & k2) | ((x & k2) << 2);
   x = ((x >> 4) & k4) | ((x & k4) << 4);
   return new BitBoard(x);
}
public static BitBoard flipDiagA1H8(BitBoard a) {
   ulong t;
   const ulong k1 = (0x5500550055005500);
   const ulong k2 = (0x3333000033330000);
   const ulong k4 = (0x0f0f0f0f00000000);
   ulong x = a._squares;
   t  = k4 & (x ^ (x << 28));
   x ^=       t ^ (t >> 28) ;
   t  = k2 & (x ^ (x << 14));
   x ^=       t ^ (t >> 14) ;
   t  = k1 & (x ^ (x <<  7));
   x ^=       t ^ (t >>  7) ;
   return new BitBoard(x);
}
public static BitBoard bitReversal(BitBoard a){
    return mirrorHorizontal(flipVertical(a));
}
}
