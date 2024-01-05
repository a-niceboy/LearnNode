#include "ByteShift.h"

ByteShift::ByteShift():m_key(rand() % 7 + 1)
{
} 

void ByteShift::Encode(const string& sIn, string& sOut) const {
    ContainerType vIn = s2v(sIn);
    ContainerType vOut;
    Encode(vIn, vOut);
    sOut = v2s(vOut);
}
void ByteShift::Encode(const ContainerType& vIn, ContainerType& vOut) const {
    vOut.clear();
    if (vIn.empty() == true)
    {
        return;
    }

    const size_t size = vIn.size();
    copy(vIn.begin(), vIn.end(), back_inserter(vOut));

    byte* pData = &vOut[0];
    byte temp = 0;
    int key = m_key;
    for (size_t i = 0; i < size; ++i)
    {
        temp = pData[i];
        pData[i] >>= key;
        temp <<= (8 - key);
        pData[i] = pData[i] | temp;

        key = (key + i * key) % 7 + 1;
    }
}
void ByteShift::Encode(const ContainerType& vIn, string& sOut) const {
    ContainerType vOut;
    Encode(vIn, vOut);
    sOut = v2s(vOut);
}
void ByteShift::Encode(const string& sIn, ContainerType& vOut) const {
    ContainerType vIn = s2v(sIn);
    Encode(vIn, vOut);
}
void ByteShift::Decode(const string& sIn, string& sOut) const {
    ContainerType vIn = s2v(sIn);
    ContainerType vOut;
    Decode(vIn, vOut);
    sOut = v2s(vOut);
}
void ByteShift::Decode(const ContainerType& vIn, ContainerType& vOut) const {
    vOut.clear();
    if (vIn.empty() == true)
    {
        return;
    }

    const size_t size = vIn.size();
    copy(vIn.begin(), vIn.end(), back_inserter(vOut));

    byte* pData = &vOut[0];
    byte temp = 0;
    int key = m_key;
    for (size_t i = 0; i < size; ++i)
    {
        int key2 = 8 - key;
        temp = pData[i];
        pData[i] >>= key2;
        temp <<= (8 - key2);
        pData[i] = pData[i] | temp;

        key = (key + i * key) % 7 + 1;
    }
}
void ByteShift::Decode(const ContainerType& vIn, string& sOut) const {
    ContainerType vOut;
    Decode(vIn, vOut);
    sOut = v2s(vOut);
}
void ByteShift::Decode(const string& sIn, ContainerType& vOut) const {
    ContainerType vIn = s2v(sIn);
    Decode(vIn, vOut);
}