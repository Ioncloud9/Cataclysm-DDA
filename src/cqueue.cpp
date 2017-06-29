#include "cqueue.h"

template <class T>
cqueue<T>::cqueue() {
    pthread_mutex_init(&push_mutex, NULL);
    pthread_mutex_init(&pop_mutex, NULL);
    pthread_cond_init(&cond, NULL);
}

template <class T>
void cqueue<T>::Push(T data) {
    pthread_mutex_lock(&push_mutex);

    _queue.push(data);

    pthread_cond_signal(&cond);
    pthread_mutex_unlock(&push_mutex);
}

template <class T>
T cqueue<T>::Pop() {
    pthread_mutex_lock(&pop_mutex);

    while (_queue.empty() == true)
    {
        pthread_cond_wait(&cond, &pop_mutex);
    }

    T data = _queue.pop();

    pthread_mutex_unlock(&pop_mutex);
    return data;
}

template <class T>
int cqueue<T>::Count() {
    return _queue.size();
}