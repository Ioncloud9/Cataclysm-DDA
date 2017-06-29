#pragma once
#ifndef VOXMQCOMMAND
#define VOXMQCOMMAND

#include <vector>
#include <string>

using std::string;

class voxMQCommand {
    public:
        voxMQCommand();
        voxMQCommand(string id, string command);
        ~voxMQCommand();
        string ID;
        string Command;

        static voxMQCommand Parse(string message);
};

#endif