#pragma once
#ifndef VOXMQMANAGER
#define VOXMQMANAGER

#include "voxMQCommand.h"
#include "voxUtil.h"
#include "json.h"
#include "cqueue.h"
#include "weather.h"
#include "monster.h"

#include <zmq.hpp>
#include <list>
#include <vector>
#include <string>
#include <unordered_map>

using std::string;
using zmq::message_t;

class JsonObject;
class JsonOut;

class voxMQManager {
public:
    DWORD getExitCode();

    voxMQManager();
    ~voxMQManager();
    string GetMapData();
    void Send(string message);
    void SendResponse(string id, string message);
    std::vector<string> ReadCommand(message_t* message);
    voxMQCommand CurrentCommand;
    bool Start();
    bool Stop();
    bool isRunning(DWORD dwTimeOut = 50) const;
    DWORD getThreadID() const;
    bool IsProcessing();
    bool HasCommand();
    
    bool BeginProcessing(string& command);
    bool EndProcessing();

    bool BeginCommand(voxMQCommand command);
    bool EndCommand();

    string FetchCommandData(string command);
    bool GetNextCommand(voxMQCommand& command);
    void SendCommandResponse();
protected:
    const int size = 20;
protected:
    static unsigned __stdcall Listener(void* PtrToInstance);
    bool isStopEventSignaled(DWORD dwTimeOut = 10) const;
private:
    const string subscriberURI = "tcp://*:3333";
    const string publisherURI = "tcp://*:3332";
    HANDLE hThread;
    HANDLE hStopEvent;
    unsigned threadID;
    bool stop = false;
    bool _processing = false;
    bool _hasCommand = false;
    bool ProcessSpecialCommand(voxMQCommand command);
    std::queue<voxMQCommand> _requestQueue;
    std::queue<voxMQCommand> _responseQueue;
};

#endif