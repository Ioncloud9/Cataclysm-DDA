#include "zmq_manager.h"
#include "game.h"
#include "map.h"
#include "player.h"
#include "mapdata.h"
#include "weather.h"
#include "weighted_list.h"
#include "cata_utility.h"
#include "zmq_utils.h"

#include <algorithm>
#include <fstream>
#include <stdlib.h>     /* srand, rand */
#include <sstream>
#include <windows.h> 
#include <stdio.h>  
#include <process.h>


zmq::context_t zmqContext (1);
zmq::socket_t zmq_PUB_Socket(zmqContext, ZMQ_REQ);
zmq::socket_t zmq_SUB_Socket(zmqContext, ZMQ_REP);

zmq_manager::zmq_manager()
{
    zmq_PUB_Socket.bind("tcp://*:3332");
    zmq_SUB_Socket.bind("tcp://*:3333");
    //create the event-object
    hStopEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL);
    Start();
}

zmq_manager::~zmq_manager() {
    Stop();

    //cleanup the event-object
    if (hStopEvent != NULL)
        ::CloseHandle(hStopEvent);

    if (hThread != NULL)
        ::CloseHandle(hThread);
    zmq_PUB_Socket.close();
    zmq_SUB_Socket.close();

    zmqContext.close();
}
 
bool zmq_manager::Start()
{
    //check if thread is already running
    if (isRunning())
        return true;

    if (hStopEvent == NULL)
        return false;

    if (hThread != NULL)
        ::CloseHandle(hThread);

    hThread = (HANDLE)_beginthreadex(NULL, 0, &zmqListener, this, 0, &threadID);
    if (hThread == 0) {
        //failure
        threadID = -1;
        return false;
    }
    return true;
}

bool zmq_manager::Stop()
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

bool zmq_manager::isStopEventSignaled(DWORD dwTimeOut) const
{
   
    //check if the stop-event object is signaled using the
    //passed-in timeout value
    if (::WaitForSingleObject(hStopEvent, dwTimeOut) == WAIT_OBJECT_0)
        return true;
    else return false;
}

bool zmq_manager::isRunning(DWORD dwTimeOut) const
{
    //INFO: thread-handle state is set to nonsignaled while the thread is running
    //and set to signaled when the thread terminates
    if (::WaitForSingleObject(hThread, dwTimeOut/*ms*/) == WAIT_TIMEOUT)
        return true; //the wait timed out -> thread handle is not signaled -> thread is running
    else
        return false;
}

DWORD zmq_manager::getThreadID() const
{
    return threadID;
}

DWORD zmq_manager::getExitCode()
{
    DWORD dwExitCode;
    BOOL bSuc = ::GetExitCodeThread(hThread, &dwExitCode);
    return dwExitCode;
}

message_t zmq_manager::CreateMessage(string str) {
    message_t resp(str.size());
    memcpy(resp.data(), str.data(), str.size());
    return resp;
}
string zmq_manager::ReadMessage(message_t* message) {
    string rpl = string(static_cast<char*>(message->data()), message->size());
    return rpl;
}
std::vector<string> zmq_manager::ReadCommand(message_t* message) {
    string rpl = ReadMessage(message);
    std::vector<string> command = split(rpl, ':');
    return command;
}

string zmq_manager::Send(string command, string message) {
    message_t request = CreateMessage(message);
    zmq_PUB_Socket.send(request);
    message_t resp; 
    zmq_PUB_Socket.recv(&resp);
    return ReadMessage(&resp);
}

unsigned __stdcall zmq_manager::zmqListener(void* PtrToInstance)
{
    zmq_manager *pInstance = reinterpret_cast<zmq_manager*>(PtrToInstance);
    while (!pInstance->stop) {
        message_t msg;
        if (zmq_SUB_Socket.recv(&msg)) {
            std::vector<string> command = pInstance->ReadCommand(&msg);
            string strResponse;
            if (command[0] == "MapData") {
                strResponse = pInstance->GetMapData();
            }
            else if (command[0] == "Move") {
                strResponse = pInstance->MovePlayer(command[1]);
            }
            else {
                strResponse = "OK";
            }

            message_t resp = pInstance->CreateMessage(strResponse);
            zmq_SUB_Socket.send(resp);
        }
        
        Sleep(100);
    }
    _endthreadex(0);
    return 0;
}

template<typename Out>
void zmq_manager::split(const string &s, char delim, Out result) {
    std::stringstream ss;
    ss.str(s);
    string item;
    while (std::getline(ss, item, delim)) {
        *(result++) = item;
    }
}


std::vector<string> zmq_manager::split(const string &s, char delim) {
    std::vector<string> elems;
    split(s, delim, std::back_inserter(elems));
    return elems;
}

string zmq_manager::MovePlayer(string dir) {
    WORD key;

    if (dir == "N") {
        key = 0x38;
    }
    else if (dir == "S") {
        key = 0x32;
    }
    else if (dir == "E") {
        key = 0x36;
    }
    else if (dir == "W") {
        key = 0x34;
    }
    else if (dir == "NE") {
        key = 0x39;
    }
    else if (dir == "NW") {
        key = 0x37;
    }
    else if (dir == "SE") {
        key = 0x33;
    }
    else if (dir == "SW") {
        key = 0x31;
    }

    INPUT ip;
    ip.type = INPUT_KEYBOARD;
    ip.ki.wScan = 0;
    ip.ki.time = 0;
    ip.ki.dwExtraInfo = 0;

    ip.ki.wVk = key;
    ip.ki.dwFlags = 0;
    SendInput(1, &ip, sizeof(INPUT));

    ip.ki.dwFlags = KEYEVENTF_KEYUP; // KEYEVENTF_KEYUP for key release
    SendInput(1, &ip, sizeof(INPUT));

    return GetMapData();
}

string zmq_manager::GetMapData() {
    const tripoint ppos = g->u.pos();
    std::stringstream ss;
    JsonOut json(ss);

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

    return (ss.str().c_str());
}