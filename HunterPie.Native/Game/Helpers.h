#pragma once
#include <vector>

template <class T>
T* resolvePtrs(long long* base, std::vector<int> offsets)
{
    for (int offset : offsets)
        base = ((long long*)(*base + offset));

    return reinterpret_cast<T*>(base);
}

template <class T>
T* resolvePtrs(long long* base, int offset)
{
    base = (long long*)(*base + offset);
    return reinterpret_cast<T*>(base);
}

template <class T>
T* getFromCurrentSaveSlot(long long* base, int offset)
{
    auto saveBase = resolvePtrs<void*>(base, 0xa8);
    auto slot = *((unsigned int*)(saveBase - 8));
    return resolvePtrs<T>((long long*)saveBase + (slot * 0x27E9F0), offset);
}
