#include <vector>
#include <iostream>
#include "XOREncryption.h"
#include "ByteShift.h"
#include <chrono>

using namespace std;

int main()
{
    ByteShift byteShift;
    XOREncryption encryption;
    string str = "code";
    string str2 = "";
    string str3 = "";
    byteShift.Encode(str, str2);
    encryption.Encode(str2, str3);
    encryption.Decode(str3, str2);
    byteShift.Decode(str2, str);
    cout << "str = " << str << endl;
    cout << "str2 = " << str2 << endl;

    vector<uint8_t> vIn(str.begin(), str.end());
    vector<uint8_t> vOut;
    vector<uint8_t> vOut2;
    byteShift.Encode(vIn, vOut);
    encryption.Encode(vOut, vOut2);
    encryption.Decode(vOut2, vOut);
    byteShift.Decode(vOut, vIn);
    string s1(vIn.begin(), vIn.end());
    cout << "vIn = " << s1 << endl;
    string s2(vOut2.begin(), vOut2.end());
    cout << "vOut = " << s2 << endl;
    return 0;
}