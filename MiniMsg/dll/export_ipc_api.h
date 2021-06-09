#pragma once
#include <libipc/ipc.h>
#include <map>
#include <unordered_map>
#include <mutex>
#ifndef _EXPORT_LIBXLCRACK_DLL_

#define EXPORT_LIBXLCRACK  _declspec(dllimport)
#else
#define EXPORT_LIBXLCRACK  _declspec(dllexport)
#endif

constexpr char const name__[] = "ipcmsg";
constexpr char const quit__[] = "q";
constexpr char const id__[] = "c";

ipc::channel sender__{ name__, ipc::sender };
ipc::channel receiver__{ name__, ipc::receiver };

std::unordered_map<std::string, ipc::channel*> hmapsender;
std::unordered_map<std::string, ipc::channel*> hmapreceiver;

std::mutex some_mutex;

inline std::size_t calc_unique_id() {
    static ipc::shm::handle g_shm{ "__CHAT_ACC_STORAGE__", sizeof(std::atomic<std::size_t>) };
    return static_cast<std::atomic<std::size_t>*>(g_shm.get())->fetch_add(1, std::memory_order_relaxed);
}

extern "C" EXPORT_LIBXLCRACK std::size_t GetIpcId();

extern "C" EXPORT_LIBXLCRACK void MapChannel(char const* name);
extern "C" EXPORT_LIBXLCRACK  void Send(char* buf,int len);
extern "C" EXPORT_LIBXLCRACK  char* Rec(int* len);
extern "C" EXPORT_LIBXLCRACK   void Close();

extern "C" EXPORT_LIBXLCRACK  void SendName(char*name, char* buf, int len);

extern "C" EXPORT_LIBXLCRACK  char* RecName(char* name,int* len);