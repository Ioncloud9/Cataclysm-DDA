#include "zmq_sender.h"
#include "game.h"
#include "map.h"
#include "player.h"
#include "mapdata.h"
#include "weather.h"
#include "weighted_list.h"
#include "cata_utility.h"

#include <algorithm>
#include <fstream>
#include <stdlib.h>     /* srand, rand */
#include <sstream>

void* zmqContext = NULL;
void* zmqPublisher = NULL;

zmq_sender::zmq_sender() {
    zmqContext = zmq_ctx_new();
    zmqPublisher = zmq_socket(zmqContext, ZMQ_PUB);
    int rc = zmq_bind(zmqPublisher, "tcp://*:3332");
    assert(rc == 0);
}

zmq_sender::~zmq_sender() {
    zmq_close(zmqPublisher);
    zmq_ctx_destroy(zmqContext);
}

void zmq_sender::SendMapData() {
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

    zmq_send(zmqPublisher, ss.str().c_str(), (size_t)ss.str().length(), 0);
}