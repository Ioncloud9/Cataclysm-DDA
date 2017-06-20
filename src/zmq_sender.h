#pragma once
#ifndef ZMQ_SENDER_H
#define ZMQ_SENDER_H

#include "animation.h"
#include "map.h"
#include "weather.h"
#include "tile_id_data.h"
#include "enums.h"
#include "weighted_list.h"
#include "json.h"
#include "zmq.h"

#include <list>
#include <map>
#include <vector>
#include <string>
#include <unordered_map>


extern void *zmqPublisher, *zmqContext;

class JsonObject;
class JsonOut;

class zmq_sender {
public:
    zmq_sender();
    ~zmq_sender();
    void SendMapData();
};

#endif