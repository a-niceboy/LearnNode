/////////////////
#ifndef _BIT_SHIFT_H_
#define _BIT_SHIFT_H_

#include "Util.h"

class ByteShift
{
public:
    ByteShift();
    void Encode(const string& sIn, string& sOut) const;
    void Encode(const ContainerType& vIn, ContainerType& vOut) const;
    void Encode(const ContainerType& vIn, string& sOut) const;
    void Encode(const string& sIn, ContainerType& vOut) const;

    void Decode(const string& sIn, string& sOut) const;
    void Decode(const ContainerType& vIn, ContainerType& vOut) const;
    void Decode(const ContainerType& vIn, string& sOut) const;
    void Decode(const string& sIn, ContainerType& vOut) const;
private:
    const byte m_key;
};

#endif

////////////////////////////