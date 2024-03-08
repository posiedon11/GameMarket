#pragma once
#ifndef DATABASEMANAGER_LOCK
#define DATABASEMANAGER_LOCK
#define _SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING
#define _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS
#include<mysqlx/xdevapi.h>
#include<string>
#include<iostream>
#include"Tables.h"
#include<vector>
#include<map>
#include"GameMarketSettings.h"
#include<queue>
#include<memory>
#include<mutex>

using namespace tables_namespace;
using namespace GameMarketSettings_NameSpace;

namespace DataBase_NameSpace
{
	class DataBaseManager {
	private:
		std::unique_ptr<mysqlx::Session> session;
		std::unique_ptr<mysqlx::Schema> schema;
		SQLServerSettings& settings;

		std::map<Tables, std::shared_ptr<mysqlx::Collection>> collectionsMap;
		std::map<Tables, std::shared_ptr<mysqlx::Table>> tablesMap;
		std::mutex queueMutex;
		std::queue < std::pair<Tables, TableData*>> insertQueue;



	public:
		std::string schemaName;
		

		//functions
	private:

	public:
#pragma region Constructors
		DataBaseManager(SQLServerSettings &settings, std::string schemaName = "GameMarket");
#pragma endregion
		mysqlx::Schema& getSchema() const
		{
			return *schema;
		}
		mysqlx::Session& getSession() const
		{
			return *session;
		}

		void taskTimer(std::atomic<bool> running);

		mysqlx::Collection& createCollection(Tables name);
		bool addToCollection(Tables name, std::string data);
		mysqlx::Collection& getCollection(Tables name);


		mysqlx::Table& createTable(Tables name);
		mysqlx::Table& getTable(Tables name);



		void queueInsert(Tables table, TableData* data);
		void processQueue();
		void upsert(Tables table,TableData *data);
		mysqlx::DocResult getDoc(Tables name);

		void executeSQLFile(std::string filePath);

	};
}

#endif // !DATABASEMANAGER_LOCK
