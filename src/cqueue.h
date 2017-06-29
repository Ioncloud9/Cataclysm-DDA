#pragma once
#ifndef CQUEUE
#define CQUEUE

#include <queue>
#include <pthread.h>

template <class T>
class cqueue
{

private:
    std::queue<T> _queue;
    pthread_mutex_t push_mutex;
    pthread_mutex_t pop_mutex;
    pthread_cond_t cond;

public:
    cqueue();
    void Push(T data);
    T Pop();
    int Count();
};
#endif