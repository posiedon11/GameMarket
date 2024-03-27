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
		enum class Schemas {
			xbox,
			steam
		};
		std::unique_ptr<mysqlx::Session> session;
		std::unique_ptr<mysqlx::Schema> schema;
		SQLServerSettings& serverSettings;
		Settings& settings;

		std::map<Tables, std::shared_ptr<mysqlx::Collection>> collectionsMap;
		std::map<Tables, std::shared_ptr<mysqlx::Table>> tablesMap;
		std::map < Schemas, std::unique_ptr<mysqlx::Schema>> schemaMap;
		std::mutex queueMutex;
		std::queue < std::tuple<Tables, shared_ptr<TableData>, CRUD>> insertQueue;



	public:
		std::string schemaName;
		

		//functions
	private:
		void upsert(Tables table, shared_ptr<TableData>data);
		void update(Tables table, shared_ptr<TableData> data);
		void insert(Tables table, shared_ptr<TableData> data);
	public:
#pragma region Constructors
		DataBaseManager(Settings &settings, std::string schemaName = "GameMarket");
#pragma endregion
		mysqlx::Schema& getSchema(Schemas name);
		mysqlx::Session& getSession() const;

		void taskTimer(std::atomic<bool> &running);


		mysqlx::Table& createTable(Tables name);
		mysqlx::Table& getTable(Tables name);



		void queueInsert(Tables table, shared_ptr<TableData> data, CRUD operation = CRUD::UpSert);
		void processQueue();
		
		

		void executeSQLFile(std::string filePath);

	};
}

#endif // !DATABASEMANAGER_LOCK
