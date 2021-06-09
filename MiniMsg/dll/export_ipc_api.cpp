#include"export_ipc_api.h"
#include <thread>
#include <iostream>

std::size_t GetIpcId()
{
	return calc_unique_id();
}

void MapChannel(char const* name)
{
	std::lock_guard<std::mutex> guard(some_mutex);
	hmapreceiver[name] = new  ipc::channel{ name,ipc::receiver };
	hmapsender[name] = new  ipc::channel{ name,ipc::sender };
}

void Send(char* buf,int len)
{
	
	 sender__.send(buf, len);
}

 char* Rec(int* len)
 {
	 ipc::buff_t buf = receiver__.recv();
	 std::vector<ipc::byte_t> vdata = buf.to_vector();
	 char* buffer = new char[vdata.size()];
	 std::copy(vdata.begin(), vdata.end(), buffer);
	 *len = vdata.size();
	 return buffer;
 }

 void Close()
 {
	 receiver__.disconnect();
 }

 void SendName(char* name, char* buf, int len)
 {
	
		 if (hmapsender.find(name) != hmapsender.end()) { 
			 hmapsender[name]->send(buf, len);
		 }
 }

 char* RecName(char* name, int* len)
 {
	 if (hmapreceiver.find(name) != hmapreceiver.end()) {
		 ipc::buff_t buf = hmapreceiver[name]->recv();

		 std::vector<ipc::byte_t> vdata = buf.to_vector();
		 char* buffer = new char[vdata.size()];
		 std::copy(vdata.begin(), vdata.end(), buffer);
		 *len = vdata.size();
		 return buffer;
	 }
	 return nullptr;
	
 }
 
