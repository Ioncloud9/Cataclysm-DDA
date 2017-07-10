#pragma once
#ifdef LOADERDLL_EXPORT
#define LOADERDLL_API __declspec(dllexport)
#else
#define LOADERDLL_API __declspec(dllimport)
#endif
#include "game.h"

extern "C" {
    LOADERDLL_API void init(void);
    LOADERDLL_API void getWorldNames(/*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver);
    LOADERDLL_API void getWorldSaves(char* worldName, /*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver);
    LOADERDLL_API void deinit(void);
}