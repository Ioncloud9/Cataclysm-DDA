#include "voxMQCommand.h"
#include "voxUtil.h"

voxMQCommand::voxMQCommand() {
}
voxMQCommand::voxMQCommand(string id, string command) {
    ID = id;
    Command = command;
}

voxMQCommand::~voxMQCommand() {
}


voxMQCommand voxMQCommand::Parse(string message) {
    vector<string> command = voxUtil::Split(message, ':');
    voxMQCommand* cmd = new voxMQCommand();
    cmd->ID = command[0];
    command.erase(command.begin());
    cmd->Command = voxUtil::Join(':', command);
    return *cmd;
}