#pragma once
//#ifdef LOADERDLL_EXPORT
//#define LOADERDLL_API __declspec(dllexport)
//#else
//#define LOADERDLL_API __declspec(dllimport)
//#endif
#include "game.h"

#define LOADERDLL_API __declspec(dllexport)

extern "C" {
    LOADERDLL_API game* loadGame(void);
    LOADERDLL_API int testSum(int a, int b);
}