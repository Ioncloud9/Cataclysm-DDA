#include "voxMQManager.h"
#include "game.h"
#include "map.h"
#include "player.h"
#include "mapdata.h"
#include "weather.h"
#include "weighted_list.h"
#include "cata_utility.h"
#include "zmq_utils.h"
#include "weather_gen.h"

#include <algorithm>
#include <fstream>
#include <stdlib.h>     /* srand, rand */
#include <sstream>
#include <windows.h> 
#include <stdio.h>  
#include <process.h>
#include <mutex>

std::mutex _current_mutex;
std::mutex _command_mutex;
voxMQCommand* _current;

zmq::context_t context(1);
zmq::socket_t publisher(context, ZMQ_REQ);
zmq::socket_t subscriber(context, ZMQ_REP);


voxMQManager::voxMQManager()
{
    publisher.bind(publisherURI);
    subscriber.bind(subscriberURI);
    hStopEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL);

    Start();
}

voxMQManager::~voxMQManager() {
    Stop();
    if (hStopEvent != NULL)
        ::CloseHandle(hStopEvent);
    
    if (hThread != NULL)
        ::CloseHandle(hThread);

    publisher.close();
    subscriber.close();
    context.close();
}

bool voxMQManager::HasCommand() {
    return _hasCommand;
}
bool voxMQManager::IsProcessing() {
    return _processing;
}

// Starts processing a command by aquiring a lock so that accidentally calling beginprocessing again for the same message doesn't hose things.
bool voxMQManager::BeginProcessing(string& command) {
    std::lock_guard<std::mutex> lock(_current_mutex);
    if (!_hasCommand) return false;
    if (_processing) return false;
    _processing = true;
    command = _current->Command;
    return true;
}

// Completes processing of a command such that it's ready to be sent off.
bool voxMQManager::EndProcessing() {
    std::lock_guard<std::mutex> lock(_current_mutex);
    if (!_hasCommand) return false;
    if (!_processing) return false;
    _processing = false;
    return true;
}

// Ends a command session by sending a response in reply to the current command, and releases the command lock
bool voxMQManager::EndCommand() {
    std::lock_guard<std::mutex> lock(_current_mutex);
    if (!_hasCommand) return false;
    if (_processing) return false;
    Send(_current->ID + ":" + FetchCommandData(_current->Command));        // send the response for the command and unlock
    _hasCommand = false;
    return true;
}

// Starts a command session by aquiring a lock to prevent other commands coming in before the current one is processed
bool voxMQManager::BeginCommand(voxMQCommand command) {
    std::lock_guard<std::mutex> lock(_current_mutex);
    if (_hasCommand) return false;
    _hasCommand = true;
    _current = new voxMQCommand(command.ID, command.Command);
    return true;
}

bool voxMQManager::GetNextCommand(voxMQCommand& command) {
    if (_requestQueue.size() > 0) {
        command = _requestQueue.front();
        _requestQueue.pop();
        _responseQueue.push(command);
        return true;
    }
    return false;
}
void voxMQManager::SendCommandResponse() {
    if (_responseQueue.empty()) return;
    voxMQCommand command = _responseQueue.front();
    _responseQueue.pop();
    Send(command.ID + ":" + GetMapData());
}

bool voxMQManager::Start()
{
    //check if thread is already running
    if (isRunning())
        return true;

    if (hStopEvent == NULL)
        return false;

    if (hThread != NULL)
        ::CloseHandle(hThread);

    hThread = (HANDLE)_beginthreadex(NULL, 0, &Listener, this, 0, &threadID);
    if (hThread == 0) {
        //failure
        threadID = -1;
        return false;
    }
    return true;
}

bool voxMQManager::Stop()
{
    //first check if the thread is running
    if (isRunning() == false)
        return true;

    if (hStopEvent != NULL) {
        //trigger the event to signal the thread to come to an end
        if (::SetEvent(hStopEvent) == 0) {
            return false;
        }
        ::Sleep(0); //give control to other threads
        ::WaitForSingleObject(hThread, INFINITE); //wait until thread has stopped
        threadID = -1;
    }
    else return false; //m_hStopEvent == NULL -> ERROR

    return true;
}

bool voxMQManager::isStopEventSignaled(DWORD dwTimeOut) const
{

    //check if the stop-event object is signaled using the
    //passed-in timeout value
    if (::WaitForSingleObject(hStopEvent, dwTimeOut) == WAIT_OBJECT_0)
        return true;
    else return false;
}

bool voxMQManager::isRunning(DWORD dwTimeOut) const
{
    //INFO: thread-handle state is set to nonsignaled while the thread is running
    //and set to signaled when the thread terminates
    if (::WaitForSingleObject(hThread, dwTimeOut/*ms*/) == WAIT_TIMEOUT)
        return true; //the wait timed out -> thread handle is not signaled -> thread is running
    else
        return false;
}

DWORD voxMQManager::getThreadID() const
{
    return threadID;
}

DWORD voxMQManager::getExitCode()
{
    DWORD dwExitCode;
    BOOL bSuc = ::GetExitCodeThread(hThread, &dwExitCode);
    return dwExitCode;
}

std::vector<string> voxMQManager::ReadCommand(message_t* message) {
    string rpl = voxUtil::ReadMessage(message);
    std::vector<string> command = voxUtil::Split(rpl, ':');
    return command;
}

void voxMQManager::Send(string message) {
    try 
    {
        message_t request = voxUtil::CreateMessage(message);
        publisher.send(request);
        message_t response;
        publisher.recv(&response);
    }
    catch (std::exception& ex) {
        popup(_("Send failed with: %s"), ex.what());
        message = "FAIL";
    }
}
void voxMQManager::SendResponse(string id, string message) {
    Send(id + message);
}

unsigned __stdcall voxMQManager::Listener(void* PtrToInstance)
{
    voxMQManager *pInstance = reinterpret_cast<voxMQManager*>(PtrToInstance);
    while (!pInstance->stop) {
        message_t msg;
        if (subscriber.recv(&msg)) {
            string command = voxUtil::ReadMessage(&msg);
            voxMQCommand pCmd = voxMQCommand::Parse(command);
            if (!pInstance->ProcessSpecialCommand(pCmd)) {
                pInstance->_requestQueue.push(pCmd);
                subscriber.send(voxUtil::CreateMessage(pCmd.ID + ":PROC"));
                /*
                if (!pInstance->BeginCommand(pCmd)) {
                    subscriber.send(voxUtil::CreateMessage(pCmd.ID + ":BUSY"));
                }
                else {
                    subscriber.send(voxUtil::CreateMessage(pCmd.ID + ":PROC"));
                }
                */
            }
        }

        Sleep(100);
    }
    _endthreadex(0);
    return 0;
}

bool voxMQManager::ProcessSpecialCommand(voxMQCommand command) {
    if (command.Command == "MapData") {
        subscriber.send(voxUtil::CreateMessage(command.ID + ":" + GetMapData()));
        return true;
    }
    return false;
}

string voxMQManager::FetchCommandData(string command) {
    if (command == "MapData") return GetMapData(); // as an example... that's all we have right now
    return GetMapData();
}
string voxMQManager::GetMapData() {
    const tripoint ppos = g->u.pos();
    std::stringstream ss;
    w_point* w = g->weather_precise.get();
    weather_generator gen = g->get_cur_weather_gen();
    
    JsonOut json(ss);
    json.start_object();

    json.member("calendar");
    json.start_object();
        json.member("season", calendar::turn.name_season(calendar::turn.get_season()));
        json.member("time", calendar::turn.print_time());
        json.member("isNight", calendar::turn.is_night());
    json.end_object();

    json.member("weather");
    json.start_object();
        json.member("type", gen.get_weather_conditions(*w));
        json.member("temprature", w->temperature);
        json.member("humidity", w->humidity);
        json.member("wind", w->windpower);
        json.member("pressure", w->pressure);
        json.member("acidic", w->acidic);
    json.end_object();

    json.member("map");
    json.start_object();
        json.member("width", size * 2);
        json.member("height", size * 2);
        json.member("tiles");
        json.start_array();
            for (int dx = -size; dx < size; dx++) {
                for (int dy = -size; dy < size; dy++) {
                    const tripoint p(ppos.x + dx, ppos.y + dy, ppos.z);
                    json.start_object();
                    if (g->u.sees(p, true)) {
                        json.member("ter", g->m.ter(p)->id);
                        if (g->m.has_furn(p)) {
                            json.member("furn", g->m.furn(p)->id);
                        }
                    }
                    json.end_object();
                }
            }
        json.end_array();
    json.end_object();

    json.end_object();

    return (ss.str().c_str());
}