#define _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS
#include"DataBaseManager.h"


namespace DataBase_NameSpace
{

	void DataBaseManager::taskTimer(std::atomic<bool> running)
	{
		while (running.load())
		{
			std::this_thread::sleep_for(std::chrono::minutes(1));
			if (!running.load())
			{
				cout << "Exitiing DBManager Thread" << endl;
			}
			processQueue();
		}
	}

	void DataBaseManager::queueInsert(Tables table, TableData* data)
	{
		std::lock_guard<std::mutex> lock(queueMutex);
		insertQueue.push({ table, data });
	}
	void DataBaseManager::processQueue()
	{
		while (!insertQueue.empty())
		{
			//cout << "Inserting into queue" << endl;
			std::lock_guard<std::mutex> lock(queueMutex);
			auto task = insertQueue.front();
			insertQueue.pop();
			upsert(task.first, task.second);
		}
	}

	void DataBaseManager::upsert(Tables table, TableData* data)
	{
		mysqlx::Table tbl = getTable(table);
		std::string sqlQuery="";
		try
		{

			switch (table)
			{
			case Tables::XboxUserProfiles:
			{
				std::string sqlQuery = "(INSERT INTO " + toString(table) +
					" (xuid, gamertag, lastScanned) " +
					"VALUES(?, ?) " +
					"ON DUPLICATE KEY UPDATE gamertag = VALUES(gamertag), lastScanned=VALUES(NOW());)";
				break;
			}
			case Tables::XboxGameTitles:
			{
				if (auto titleData = dynamic_cast<XboxGameTitleData*>(data))
				{
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (titleID, titleName, displayImage, modernTitleID, isGamePass) " +
						"VALUES(?, ?, ?, ?, ?) " +
						"ON DUPLICATE KEY UPDATE titleName = VALUES(titleName), displayImage = VALUES(displayImage), modernTitleId=VALUES(modernTitleId), isGamePass = VALUES(isGamePass)";

					//cout << sqlQuery << endl;
					session->sql(sqlQuery)
						.bind(titleData->titleID,titleData->titleName, titleData->displayImage,titleData->modernTitleID, titleData->isGamePass)
						.execute();
				}
				
				break;
			}
			case Tables::XboxGameBundles:
			{
				if (auto titleData = dynamic_cast<XboxTitleDetailsData*>(data))
				{
					tbl.remove().where("productID = :productID").bind("productID", titleData->productID).execute();
					auto tblInsert = tbl.insert("productID", "relatedProductId");
					for (auto bundleId : titleData->bundleIDs)
					{
						tblInsert = tblInsert.values(titleData->productID, bundleId);
					}
					tblInsert.execute();					
				}
				break;
			}
			case Tables::XboxGameDetails:

				break;
			case Tables::XboxGameGenres:
				break;
			case Tables::XboxTitleDetails:
			{
				if (auto titleData = dynamic_cast<XboxTitleDetailsData*>(data))
				{
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (modernTitleID, productID) " +
						"VALUES(?, ?) " +
						"ON DUPLICATE KEY UPDATE modernTitleID = VALUES(modernTitleID), productID = VALUES(productID)";

					//cout << sqlQuery << endl;
					/*session->sql(sqlQuery)
						.bind(titleData->titleID, titleData->productID)
						.execute();*/
					tbl.insert("modernTitleID", "productID").values(titleData->titleID, titleData->productID).execute();
				}
			}
			case Tables::SteamGames:
				break;
			case Tables::SteamGameGenres:
				break;
			}
		
			}
		catch (std::exception& ex) {
			std::cout << endl << "STD EXCEPTION: " << ex.what() << std::endl <<endl;
		}
		catch (...) {
			std::cout << "Unknown exception occurred" << std::endl;
		}
	}

	mysqlx::Collection& DataBaseManager::createCollection(Tables name)
	{

			auto collection = std::make_shared<mysqlx::Collection>(this->getSchema().createCollection(toString(name), true));
			collectionsMap[name] = collection;
			std::cout << "Creating collection" << std::endl;
			return *collection;

	}
	mysqlx::Collection& DataBaseManager::getCollection(Tables name)
	{
		auto colleciton = collectionsMap.find(name);
		if (colleciton == collectionsMap.end())
		{
			return createCollection(name);
		}
		std::cout << "Collection Exists" << std::endl;
		return *colleciton->second;
	}

	mysqlx::Table& DataBaseManager::createTable(Tables name)
	{
		auto table = std::make_shared<mysqlx::Table>(this->getSchema().getTable(toString(name), false));
		if (!table->existsInDatabase())
		{
			std::string sqlFilePath = "";
			std::cout << "Creating table" << std::endl;
			switch (name)
			{
			case Tables::XboxUserProfiles:
				sqlFilePath = "sqlQueries/create xboxuserprofiles.sql";
				break;
			case Tables::XboxGameTitles:
				sqlFilePath = "sqlQueries/create xboxgametitles.sql";
				break;
			case Tables::XboxGameBundles:
				sqlFilePath = "sqlQueries/create xboxgamebundles.sql";
				break;
			case Tables::XboxGameDetails:
				sqlFilePath = "sqlQueries/create xboxgamedetails";
				break;
			case Tables::XboxGameGenres:
				sqlFilePath = "sqlQueries/create xboxgamegenres.sql";
				break;
			case Tables::XboxTitleDetails:
				sqlFilePath = "sqlQueries/create xboxtitledetails.sql";
				break;
			case Tables::XboxMarketDetails:
				sqlFilePath = "sqlQueries/create xboxmarketdetails.sql";
				break;
			case Tables::SteamGames:
				break;
			case Tables::SteamGameGenres:
				break;
			default:
				sqlFilePath = "";
				break;
			}
			if (sqlFilePath != "")
			{
				executeSQLFile(sqlFilePath);
				table = std::make_shared<mysqlx::Table>(this->getSchema().getTable(toString(name), true));
			}
			else
			{
				std::cout << "Invalid sqlfilepath" << std::endl;
			}
		}
		else
		{
			std::cout << toString(name) << " exists in db." << std::endl << endl;
		}
		tablesMap[name] = table;
		return *table;

	}
	mysqlx::Table& DataBaseManager::getTable(Tables name)
	{
		auto table = tablesMap.find(name);
		if (table == tablesMap.end())
		{
			std::cout << toString(name) << " not in program. " << std::endl;
			return createTable(name);
		}
		return *table->second;
	}


	bool DataBaseManager::addToCollection(Tables name, std::string data)
	{
		try
		{
			auto& collection = getCollection(name);
			collection.add(data).execute();
			return true;
		}
		catch (const std::exception& ex)
		{
			std::cerr << ex.what() << std::endl;
			return false;
		}
	}
	mysqlx::DocResult DataBaseManager::getDoc(Tables name)
	{
		auto collection = getCollection(name);
		if (collectionsMap.find(name) == collectionsMap.end())
		{
			std::cout << "Collection does not exist";
		}

		return collection.find().execute();
	}

	void DataBaseManager::executeSQLFile(std::string filePath)
	{
		try
		{
			size_t strPos = 0;
			std::string sqlString = readFromFile(filePath);
			while (std::string::npos != (strPos = sqlString.find(';')))
			{
				std::string substring = sqlString.substr(0, strPos);
				session->sql(substring).execute();
				sqlString.erase(0, strPos + 1);
			}
			//session->sql( readSqlFile(filePath)).execute();
			if (settings.outputDebug)
				std::cout << "Sql Ran Successfully" << std::endl << std::endl;
		}
		catch (const std::exception& ex)
		{
			std::cerr << "Error executing sql" << ex.what() << std::endl;
		}
		
	}

	DataBaseManager::DataBaseManager(SQLServerSettings &settings, std::string schemaName) : settings(settings)
	{
		try
		{
			//connect to session
			session = std::make_unique<mysqlx::Session>(mysqlx::Session(settings.serverName, settings.serverPort, settings.serverUserName, settings.serverPassword));
			std::cout << "DataBase Connected" << std::endl;


			//connect to schema
			schema = std::make_unique<mysqlx::Schema>(session->createSchema(schemaName, true));
			std::cout << "Schema Connected" << std::endl;
			session->sql("Use gamemarket;").execute();

		}
		catch (mysqlx::Error& error)
		{
			std::cerr << "Error connecting to DB" << error.what() << std::endl;
			throw;
		}
	}
}