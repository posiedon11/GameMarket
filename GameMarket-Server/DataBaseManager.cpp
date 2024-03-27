#define _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS
#include"DataBaseManager.h"


namespace DataBase_NameSpace
{

	void DataBaseManager::taskTimer(std::atomic<bool> &running)
	{
		while (running.load())
		{
			processQueue();
			std::this_thread::sleep_for(std::chrono::minutes(1));
			if (!running.load())
			{
				cout << "Exitiing DBManager Thread" << endl;
			}
			
		}
	}

	void DataBaseManager::queueInsert(Tables table, shared_ptr<TableData> data, CRUD operation)
	{
		std::lock_guard<std::mutex> lock(queueMutex);
		insertQueue.push({ table, data, operation });
	}
	void DataBaseManager::processQueue()
	{
		while (!insertQueue.empty())
		{
			//cout << "Inserting into queue" << endl;
			std::lock_guard<std::mutex> lock(queueMutex);
			auto &task = insertQueue.front();
			insertQueue.pop();

			auto data = std::get<1>(task);
			auto table = std::get<0>(task);
			switch (std::get<2>(task))
			{
			case CRUD::Insert :
				insert(table, data);
				break;
			case CRUD::Update :
				update(table, data);
				break;
			case CRUD::UpSert:
				upsert(table, data);
			default:
				
				break;
			}
		}
	}

	void DataBaseManager::update(Tables table, shared_ptr<TableData> data)
	{
		mysqlx::Table tbl = getTable(table);
		std::string sqlQuery = "";
		try
		{

			switch (table)
			{
			case Tables::XboxUserProfiles:
			{
				if (auto updateData = dynamic_pointer_cast<XboxUpdateScannedData>(data))
				{
					tbl.update().set("lastScanned", mysqlx::expr("NOW()")).where("xuid = :xuid").bind("xuid", updateData->ID).execute();

				}
				break;
			}
			case Tables::XboxGameTitles:
			{
				/*if (auto titleData = dynamic_pointer_cast<XboxGameTitleData>(data))
				{
					
				}*/
				if (auto updateData = dynamic_pointer_cast<XboxUpdateScannedData>(data))
				{
					tbl.update().set("lastScanned", mysqlx::expr("NOW()")).where("modernTitleId = :modernTitleId").bind("modernTitleId", updateData->ID).execute();
				}
				else if (auto updateData = dynamic_pointer_cast<XboxProductGroupData>(data))
				{
					tbl.update().set("groupID",updateData->groupID).set("lastScanned", mysqlx::expr("NOW()")).where("modernTitleId = :modernTitleId").bind("modernTitleId", updateData->titleID).execute();
				}

				break;
			}
			case Tables::XboxGameBundles:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{

				}
				break;
			}
			case Tables::XboxProductIds:
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{

				}
				break;
			case Tables::XboxGameGenres:
				break;
			case Tables::XboxTitleDetails:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					tbl.update().set("productID", titleData->productID).where("modernTitleID = :modernTitleId").bind("modernTitleID", titleData->titleID);
				}
				/*else if (auto updateData = dynamic_pointer_cast<XboxProductGroupData>(data))
				{
					tbl.update().set("groupID", updateData->groupID).set("lastScanned", mysqlx::expr("NOW()")).where("modernTitleId = :modernTitleId").bind("modernTitleId", updateData->productID).execute();
				}*/
				break;
			}

			case Tables::SteamAppIDs:
			{
				if (auto titleData = dynamic_pointer_cast<SteamAPPListData>(data))
				{

				}
				break;
			}
			}

		}
		catch (std::exception& ex) {
			std::cout << endl << "STD EXCEPTION: " << ex.what() << std::endl << endl;
		}
		catch (...) {
			std::cout << "Unknown exception occurred" << std::endl;
		}
	}
	void DataBaseManager::insert(Tables table, shared_ptr<TableData> data)
	{
		mysqlx::Table tbl = getTable(table);
		std::string sqlQuery = "";
		try
		{

			switch (table)
			{
			case Tables::XboxGameTitles:
			{
				if (auto titleData = dynamic_pointer_cast<XboxGameTitleData>(data))
				{
				}

				break;
			}
			case Tables::XboxGameBundles:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					auto tblInsert = tbl.insert("productID", "relatedProductId");
					for (auto &bundleId : titleData->bundleIDs)
					{
						tbl.remove().where("relatedProductId = :relatedProductId").bind("relatedProductId", bundleId).execute();
						tblInsert = tblInsert.values(titleData->productID, bundleId);
					}
					tblInsert.execute();
				}
				break;
			}
			case Tables::XboxProductIds:
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					auto tblInsert = tbl.insert("productID");
					tblInsert = tblInsert.values(titleData->productID);
					for (auto bundleId : titleData->bundleIDs)
					{
						tblInsert = tblInsert.values(bundleId);
					}
					tblInsert.execute();
				}
				break;
			case Tables::XboxGameGenres:
				break;
			case Tables::XboxTitleDetails:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					tbl.insert("modernTitleID", "productID").values(titleData->titleID, titleData->productID).execute();
				}
				break;
			}

			case Tables::SteamAppIDs:
			{
				if (auto titleData = dynamic_pointer_cast<SteamAPPListData>(data))
				{
					tbl.insert("appID").values(titleData->appid).execute();
				}
				break;
			}
			}

		}
		catch (std::exception& ex) {
			std::cout << "Error Inserting" << endl;
			std::cout << endl << "STD EXCEPTION: " << ex.what() << std::endl << endl;
		}
		catch (...) {
			std::cout << "Unknown exception occurred" << std::endl;
		}
	}
	void DataBaseManager::upsert(Tables table, shared_ptr<TableData> data)
	{
		mysqlx::Table tbl = getTable(table);
		std::string sqlQuery="";
		try
		{

			switch (table)
			{
			case Tables::XboxUserProfiles:
			{
				if (auto profileData = dynamic_pointer_cast<XboxProfileData>(data))
				{
				}
				else
				{
					cout << "not profile data" << endl;
				}
				
				break;
			}
			case Tables::XboxGameTitles:
			{
				if (auto titleData = dynamic_pointer_cast<XboxGameTitleData>(data))
				{
					session->sql("Use Xbox").execute();
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
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{		
					/*session->sql("Use Xbox").execute();
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (relatedProductID, productID) " +
						"VALUES(?, ?) " +
						"ON DUPLICATE KEY UPDATE productID = VALUES(productID)";
					session->sql(sqlQuery).bind(titleData->titleID).execute();

					for (auto& bundleId : titleData->bundleIDs)
					{
						session->sql(sqlQuery).bind(bundleId).execute();
					}*/

					auto tblInsert = tbl.insert("relatedProductId", "productID");
					for (auto& bundleId : titleData->bundleIDs)
					{
						tbl.remove().where("relatedProductId = :relatedProductId").bind("relatedProductId", bundleId).execute();
						tblInsert = tblInsert.values(bundleId, titleData->productID);
					}
					tblInsert.execute();
				}
				break;
			}
			case Tables::XboxProductIds:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					session->sql("Use Xbox").execute();
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (productID) " +
						"VALUES(?) " +
						"ON DUPLICATE KEY UPDATE productID = VALUES(productID)";
					session->sql(sqlQuery).bind(titleData->productID).execute();

					for (auto& bundleId : titleData->bundleIDs)
					{
						session->sql(sqlQuery).bind(bundleId).execute();
					}
				}
				break;
			}
			case Tables::XboxGameGenres:
			{
				break;
			}
			case Tables::XboxTitleDetails:
			{
				if (auto titleData = dynamic_pointer_cast<XboxTitleDetailsData>(data))
				{
					session->sql("use xbox;").execute();
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (modernTitleID, productID) " +
						"VALUES(?, ?) " +
						"ON DUPLICATE KEY UPDATE modernTitleID = VALUES(modernTitleID), productID = VALUES(productID)";

					session->sql(sqlQuery)
						.bind(titleData->titleID, titleData->productID)
						.execute();
				}
				break;
			}
			case Tables::XboxGroupData:
			{
				if (auto groupData = dynamic_pointer_cast<XboxProductGroupData>(data))
				{
					session->sql("use xbox;").execute();
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (groupID, groupName) " +
						"VALUES(?, ?) " +
						"ON DUPLICATE KEY UPDATE groupName = VALUES(groupName)";

					session->sql(sqlQuery)
						.bind(groupData->groupID, groupData->groupName)
						.execute();
				}
				break;
			}




			case Tables::SteamAppIDs:
			{
				if (auto titleData = dynamic_pointer_cast<SteamAPPListData>(data))
				{
					std::string sqlQuery = "INSERT INTO " + toString(table) +
						" (modernTitleID, productID) " +
						"VALUES(?, ?) " +
						"ON DUPLICATE KEY UPDATE modernTitleID = VALUES(modernTitleID), productID = VALUES(productID)";
				}
				break;
			}
			}
		
			}
		catch (std::exception& ex) {
			std::cout << endl << "STD EXCEPTION: " << ex.what() << std::endl <<endl;
		}
		catch (...) {
			std::cout << "Unknown exception occurred" << std::endl;
		}
	}


	mysqlx::Table& DataBaseManager::createTable(Tables name)
	{
		shared_ptr<mysqlx::Table> table;
		int schemasNum = -1;
		switch (name)
		{
		case tables_namespace::Tables::XboxUserProfiles:
		case tables_namespace::Tables::XboxGameTitles:
		case tables_namespace::Tables::XboxGameBundles:
		case tables_namespace::Tables::XboxProductIds:
		case tables_namespace::Tables::XboxTitleDetails:
		case tables_namespace::Tables::XboxMarketDetails:
		case tables_namespace::Tables::XboxGameGenres:
		case tables_namespace::Tables::XboxGroupData:
			table = std::make_shared<mysqlx::Table>(this->getSchema(Schemas::xbox).getTable(toString(name), false));
			schemasNum =static_cast<int>( Schemas::xbox);
			break;
		case tables_namespace::Tables::SteamAppIDs:
		case tables_namespace::Tables::SteamAppGenres:
		case tables_namespace::Tables::SteamAppDetails:
		case tables_namespace::Tables::SteamAppDevelopers:
		case tables_namespace::Tables::SteamAppPublishers:
		case tables_namespace::Tables::SteamAppPlatforms:
		case tables_namespace::Tables::SteamPackageDetails:
		case tables_namespace::Tables::SteamPackageIDs:
		case tables_namespace::Tables::SteamPackages:
			table = std::make_shared<mysqlx::Table>(this->getSchema(Schemas::steam).getTable(toString(name), false));
			schemasNum = static_cast<int>(Schemas::steam);
			break;
		default:
			break;
		}
		
		//auto table = std::make_shared<mysqlx::Table>(this->getSchema().getTable(toString(name), false));
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
			case Tables::XboxProductIds:
				sqlFilePath = "sqlQueries/create xboxproductID.sql";
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
			case Tables::XboxGroupData:
				sqlFilePath = "sqlQueries/create xboxgroupdata.sql";
				break;


				//steam
			case tables_namespace::Tables::SteamAppIDs:
				sqlFilePath = "sqlQueries/create steamappids.sql";
				break;
			case tables_namespace::Tables::SteamAppGenres:
				sqlFilePath = "sqlQueries/create steamappgenres.sql";
				break;
			case tables_namespace::Tables::SteamAppDetails:
				sqlFilePath = "sqlQueries/create steamappdetails.sql";
				break;
			case tables_namespace::Tables::SteamAppDevelopers:
			case tables_namespace::Tables::SteamAppPublishers:
				sqlFilePath = "sqlQueries/create steamappdev-pub.sql";
				break;
			case tables_namespace::Tables::SteamAppPlatforms:
				sqlFilePath = "sqlQueries/create steamappplatforms.sql";
				break;
			case tables_namespace::Tables::SteamPackageDetails:
				sqlFilePath = "sqlQueries/create steampackegedetails.sql";
				break;
			case tables_namespace::Tables::SteamPackageIDs:
				sqlFilePath = "sqlQueries/create steampackageids.sql";
				break;
			case tables_namespace::Tables::SteamPackages:
				sqlFilePath = "sqlQueries/create steampackages.sql";
				break;
			default:
				sqlFilePath = "";
				break;
			}
			if (sqlFilePath != "")
			{
				executeSQLFile(sqlFilePath);
				switch (schemasNum)
				{
				case static_cast<int>(Schemas::xbox):
					table = std::make_shared<mysqlx::Table>(this->getSchema(Schemas::xbox).getTable(toString(name), true));
					break;
				case static_cast<int>(Schemas::steam):
					table = std::make_shared<mysqlx::Table>(this->getSchema(Schemas::steam).getTable(toString(name), true));
					break;
				}
				
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


	void DataBaseManager::executeSQLFile(std::string filePath)
	{
		try
		{
			size_t strPos = 0;
			vector<std::string> sqlQueries = readFromFile(filePath);
			for (auto querie : sqlQueries)
			{
				session->sql(querie).execute();
			}
			if (serverSettings.outputDebug)
				std::cout << "Sql Ran Successfully" << std::endl << std::endl;
		}
		catch (const std::exception& ex)
		{
			std::cerr << "Error executing sql" << ex.what() << std::endl;
		}
		
	}

	DataBaseManager::DataBaseManager(Settings &settings, std::string schemaName) : settings(settings), serverSettings(settings.sqlServerSettings)
	{
		try
		{
			//connect to session
			session = std::make_unique<mysqlx::Session>(mysqlx::Session(serverSettings.serverName, serverSettings.serverPort, serverSettings.serverUserName, serverSettings.serverPassword));
			std::cout << "DataBase Connected" << std::endl;


			//connect to schema
			schema = std::make_unique<mysqlx::Schema>(session->createSchema(schemaName, true));
			std::cout << "Schema Connected" << std::endl;


			schemaMap[Schemas::xbox] = std::make_unique<mysqlx::Schema>(session->createSchema(settings.sqlServerSettings.xboxSchema, true));
			schemaMap[Schemas::steam] = std::make_unique<mysqlx::Schema>(session->createSchema(settings.sqlServerSettings.steamSchema, true));
			//session->sql("Use gamemarket;").execute();

		}
		catch (mysqlx::Error& error)
		{
			std::cerr << "Error connecting to DB" << error.what() << std::endl;
			throw;
		}
	}


	mysqlx::Schema& DataBaseManager::getSchema(Schemas name)
	{
		return *schemaMap[name];
	}
	mysqlx::Session& DataBaseManager::getSession() const
	{
		return *session;
	}
}