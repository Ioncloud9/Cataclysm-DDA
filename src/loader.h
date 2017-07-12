#pragma once
#ifdef LOADERDLL_EXPORT
#define LOADERDLL_API __declspec(dllexport)
#else
#define LOADERDLL_API __declspec(dllimport)
#endif
#include "game.h"
#include "calendar.h"

struct Weather {
    weather_type type;
    double temperature;
    double humidity;
    double wind;
    double pressure;
    bool acidic;
};

struct Calendar {
    season_type season;
    char* time;
    int years;
    int days;
    moon_phase moon;
    bool isNight;
};

struct Tile {
    char* ter;
    char* furn;
};

struct Map {
    int width;
    int height;
    Tile* tiles;
};

struct MapData {
    Calendar calendar;
    Weather weather;
    Map map;
};


extern "C" {
    LOADERDLL_API void init(void);
    LOADERDLL_API void getWorldNames(/*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver);
    LOADERDLL_API void getWorldSaves(char* worldName, /*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver);
    LOADERDLL_API void deinit(void);
    LOADERDLL_API void loadGame(char* worldName); // loads first available game for the world
    LOADERDLL_API void getTer(/*out*/ char** ter);
    LOADERDLL_API void doTurn(void);
    LOADERDLL_API int getTurn(void);
    LOADERDLL_API void doAction(char* action);
    LOADERDLL_API int playerX(void);
    LOADERDLL_API int playerY(void);
    LOADERDLL_API void getMap(/*[out]*/ MapData** data);
}



