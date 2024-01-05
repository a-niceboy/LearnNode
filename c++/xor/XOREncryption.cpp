
#include "XOREncryption.h"

XOREncryption::XOREncryption()
    : m_keys(new int [4] {rand() % 256, rand() % 256, rand() % 256, rand() % 256 })
{
    //srand(time(0));
}

XOREncryption::~XOREncryption() {
    delete []m_keys;
}

const int* XOREncryption::GetKeys() const
{
    return m_keys;
}

void XOREncryption::Encode(const string& sIn, string& sOut) const {
    ContainerType vIn = s2v(sIn);
    ContainerType vOut;
    Encode(vIn, vOut);
    sOut = v2s(vOut);
}

void XOREncryption::Encode(const ContainerType& vIn, ContainerType& vOut) const {
    vOut.clear();
    if (vIn.empty() == true)
    {
        return;
    }

    const size_t size = vIn.size();
    copy(vIn.begin(), vIn.end(), back_inserter(vOut));

    byte* pData = &vOut[0];

    int ixor = m_keys[0];
    for (size_t i = 0; i < size; ++i)
    {
        ixor = abs((ixor + m_keys[1]) * (ixor - m_keys[2]) + m_keys[3]) % 256;
        pData[i] ^= ixor;
    }
}

void XOREncryption::Encode(const ContainerType& vIn, string& sOut) const {
    ContainerType vOut;
    Encode(vIn, vOut);
    sOut = v2s(vOut);
}

void XOREncryption::Encode(const string& sIn, ContainerType& vOut) const {
    ContainerType vIn = s2v(sIn);
    Encode(vIn, vOut);
}


void XOREncryption::Decode(const string& sIn, string& sOut) const {
    ContainerType vIn = s2v(sIn);
    ContainerType vOut;
    Decode(vIn, vOut);
    sOut = v2s(vOut);
}
void XOREncryption::Decode(const ContainerType& vIn, ContainerType& vOut) const {
    vOut.clear();
    if (vIn.empty() == true)
    {
        return ;
    }
    const size_t size = vIn.size();

    copy(vIn.begin(), vIn.end(), back_inserter(vOut));
    byte* pData = &vOut[0];

    int ixor = m_keys[0];
    for (size_t i = 0; i < size; ++i)
    {
        ixor = abs((ixor + m_keys[1]) * (ixor - m_keys[2]) + m_keys[3]) % 256;
        pData[i] ^= ixor;
    }
}
void XOREncryption::Decode(const ContainerType& vIn, string& sOut) const {
    ContainerType vOut;
    Decode(vIn, vOut);
    sOut = v2s(vOut);
}
void XOREncryption::Decode(const string& sIn, ContainerType& vOut) const {
    ContainerType vIn = s2v(sIn);
    Decode(vIn, vOut);
}