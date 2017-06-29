#include "voxUtil.h"

message_t voxUtil::CreateMessage(string str) {
    message_t resp(str.size());
    memcpy(resp.data(), str.data(), str.size());
    return resp;
}
string voxUtil::ReadMessage(message_t* message) {
    string rpl = string(static_cast<char*>(message->data()), message->size());
    return rpl;
}

template<typename Out>
void voxUtil::Split(const string &s, char delim, Out result) {
    stringstream ss;
    ss.str(s);
    string item;
    while (getline(ss, item, delim)) {
        *(result++) = item;
    }
}

vector<string> voxUtil::Split(const string &s, char delim) {
    vector<string> elems;
    Split(s, delim, std::back_inserter(elems));
    return elems;
}

string voxUtil::Join(char delim, const vector<string> &v) {
    stringstream ss;
    for (size_t i = 0; i < v.size(); i++) {
        if (i != 0) {
            ss << ",";
        }
        ss << v[i];
    }
    return ss.str();
}