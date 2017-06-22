#pragma once
#ifndef ZMQ_MANAGER_H
#define ZMQ_MANAGER_H

#include "animation.h"
#include "map.h"
#include "weather.h"
#include "tile_id_data.h"
#include "enums.h"
#include "weighted_list.h"
#include "json.h"

#include <zmq.hpp>
#include <list>
#include <map>
#include <vector>
#include <string>
#include <unordered_map>

using std::string;
using zmq::message_t;

class JsonObject;
class JsonOut;

class zmq_manager {
public:
    DWORD getExitCode();

    zmq_manager();
    ~zmq_manager();
    string GetMapData();
    string MovePlayer(string dir);

    bool Start();
    bool isRunning(DWORD dwTimeOut = 50) const;
    bool Stop();
    DWORD getThreadID() const; 
    string Send(string command, string message);
    template<typename Out>
    void split(const string &s, char delim, Out result);
    std::vector<string> split(const string &s, char delim);
    zmq::message_t CreateMessage(string str);
    string ReadMessage(message_t* message);
    std::vector<string> ReadCommand(message_t* message);
protected:
    static unsigned __stdcall zmqListener(void* PtrToInstance);
    bool isStopEventSignaled(DWORD dwTimeOut = 10) const;
protected:
    const int size = 20;
    HANDLE hThread;
    HANDLE hStopEvent;
    unsigned threadID;
    bool stop = false;
};

#endif