#pragma once
#ifndef VOXUTIL
#define VOXUTIL

#include <zmq.hpp>
#include <stdlib.h>

using std::string;
using std::vector;
using std::stringstream;
using zmq::message_t;

class voxUtil {
    public:
        template<typename Out>
        static void Split(const string &s, char delim, Out result);
        static vector<string> Split(const string &s, char delim);
        static string Join(char delim, const vector<string> &v);
        static message_t CreateMessage(string str);
        static string ReadMessage(message_t* message);
};
#endif
