#pragma once
#ifdef LOADERDLL_EXPORT
#define LOADERDLL_API __declspec(dllexport)
#else
#define LOADERDLL_API __declspec(dllimport)
#endif
#include "game.h"
#include "calendar.h"
#include "coordinate_conversions.h"
#include "creature.h"

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

struct IVector3 {
    int x, y, z;
};

struct IVector2 {
    int x, y;
};

struct Entity {
    int type;
    IVector2 loc;
    int isMonster;
    int isNpc;
    int hp;
    int maxHp;
    Creature::Attitude attitude;
};

struct EntityArray {
    int size;
    Entity* entities;
};

struct Tile {
    int ter;
    int furn;
    IVector3 loc;
    int seen;
};

struct Map {
    int width;
    int height;
    Tile* tiles;
};

struct GameData {
    IVector3 playerPosition;
    Calendar calendar;
    Weather weather;
    Map map;
};


struct CStringArray {
    char** stringArray;
    int len;
};

extern "C" {
    LOADERDLL_API void init(bool openMainMenu);
    LOADERDLL_API CStringArray* getWorldNames(void);
    LOADERDLL_API CStringArray* getWorldSaves(char* worldName);
    LOADERDLL_API int terId(char* str);
    LOADERDLL_API void deinit(void);
    LOADERDLL_API void loadGame(char* worldName); // loads first available game for the world
    LOADERDLL_API void loadSaveGame(char* worldName, char* saveName);
    LOADERDLL_API void doTurn(void);
    LOADERDLL_API int getTurn(void);
    LOADERDLL_API void doAction(char* action);
    LOADERDLL_API IVector3 playerPos(void);
    LOADERDLL_API Map* getTilesBetween(IVector2 from, IVector2 to);
    LOADERDLL_API EntityArray* getEntities(IVector2 from, IVector2 to);
    LOADERDLL_API GameData* getGameData(void);
}