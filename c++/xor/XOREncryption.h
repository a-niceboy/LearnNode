
/////////////////
#ifndef _XOR_ENCRYPTION_H_
#define _XOR_ENCRYPTION_H_

#include "Util.h"

class XOREncryption
{
public:
    XOREncryption(); virtual ~XOREncryption();
    const int* GetKeys() const;
    void Encode(const string& sIn, string& sOut) const;
    void Encode(const ContainerType& vIn, ContainerType& vOut) const;
    void Encode(const ContainerType& vIn, string& sOut) const;
    void Encode(const string& sIn, ContainerType& vOut) const;

    void Decode(const string& sIn, string& sOut) const;
    void Decode(const ContainerType& vIn, ContainerType& vOut) const;
    void Decode(const ContainerType& vIn, string& sOut) const;
    void Decode(const string& sIn, ContainerType& vOut) const;
    /*
    bool EncodeToChar(const ContainerType& vIn, string& sOut, const int inum = 7) const;
    bool DecodeFromChar(const string& sIn, ContainerType& vOut, const int inum = 1) const;

    string& HexToChar(string& s, const vector<uint8_t>& data) const;
    void CharToHex(vector<uint8_t>& data, const string& s) const;
    */
private:
    

    const int* m_keys;
};

#endif

////////////////////////////