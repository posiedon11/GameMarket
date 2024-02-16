#pragma once
#ifndef DATABASEMANAGER_LOCK
#define DATABASEMANAGER_LOCK


#include<rapidjson/rapidjson.h>
#include<sstream>
#include<fstream>
#include<queue>
#include<mysqlx/xdevapi.h>
#include<iostream>


using namespace std;

namespace dataBaseManager_namespace
{
	class DataBaseManager
	{	
	private:
		static DataBaseManager* instance;

		queue<string> hi;
		mysqlx::Session* session;
		mysqlx::Schema *schema;
		//std::unique_ptr<mysqlx::Schema> schema1;

		string serverName = "127.0.0.1";
		int portNum = 33060;
		string username = "root";
		string password = "GamePasswordMarket11";

		DataBaseManager() : session(nullptr), schema(nullptr) {}
		~DataBaseManager() { if (session != nullptr) delete session; }

		DataBaseManager(const DataBaseManager&) = delete;
		DataBaseManager& operator=(const DataBaseManager&) = delete;

	public:
		void startDataBase();
		static DataBaseManager* getInstance();
		mysqlx::Schema* getSchema();
		void connectToServer();
	};
}
#endif