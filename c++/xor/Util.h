#pragma once
#ifndef _UTIL_H_
#define _UTIL_H_

#include <vector>
#include <string>
#include <iostream>
#include <bitset>
using namespace std;

typedef unsigned char byte;
typedef vector<uint8_t> ContainerType;

inline ContainerType s2v(const string& s) {
    ContainerType ret;
    for (size_t i = 0; i < s.size(); i++)
    {
        ret.push_back(s[i]);
    }
    return ret;
}

inline string v2s(const ContainerType& v) {
    string ret;
    for (size_t i = 0; i < v.size(); i++)
    {
        ret.push_back(v[i]);
    }
    return ret;
}


/*
string& HexToChar(string& s, const vector<uint8_t>& data) const
{
    s = "";
    for (unsigned int i = 0; i < data.size(); ++i)
    {
        char szBuff[3] = "";
        sprintf_s(szBuff, "%02x", *reinterpret_cast<const unsigned char*>(&data[i]));
        s += szBuff[0];
        s += szBuff[1];
    }
    return s;
}

void CharToHex(vector<uint8_t>& data, const string& s) const
{
    data.clear();

    unsigned int ui = 0L;
    for (unsigned int i = 0; i < s.size(); ++i)
    {
        unsigned int localui = 0L;
        const char c = s[i];
        if ('0' <= c && c <= '9')
        {
            localui = c - '0';
        }
        else if ('A' <= c && c <= 'F')
        {
            localui = c - 'A' + 10;
        }
        else if ('a' <= c && c <= 'f')
        {
            localui = c - 'a' + 10;
        }

        if (i % 2 == 0)
        {
            ui = localui * 16L;
        }
        else
        {
            ui += localui;
            data.push_back(ui);
        }
    }
}
*/
#endif