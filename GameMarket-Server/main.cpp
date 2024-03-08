#include"Tables.h"
#include"GameMarketSettings.h"
#include"DataBaseManager.h"
#include<iostream>
#include"XboxAPIManager.h"
#include"Tools.h"


using namespace tables_namespace;
using namespace GameMarketSettings_NameSpace;
using namespace DataBase_NameSpace;
using namespace XboxAPI_namespace;

int main(void)
{
	SQLServerSettings &ss = SQLServerSettings::getInstance();
	XboxSettings& xS = XboxSettings::getInstance();


	//DataBaseManager dbManager(ss.serverName, ss.serverPort,ss.serverUserName,ss.serverPassword, "GameMarket");
	DataBaseManager dbManager(ss);

	XblAPIManager xbManager(dbManager, xS);
	std::atomic<bool> running(true);
	try
	{
		std::thread xboxManagerThread(&XblAPIManager::taskTimer, std::ref(xbManager), std::ref(running));
		std::thread dbManagerThread(&DataBaseManager::taskTimer, std::ref(dbManager), std::ref(running));
		auto startTime = std::chrono::system_clock::now();
		while (startTime < startTime + std::chrono::minutes(2))
		{
			
		}
		cout << "Thread timer is over" << endl;
		running.store(false);

		if (xboxManagerThread.joinable())
			xboxManagerThread.join();
		if (dbManagerThread.joinable())
			dbManagerThread.join();

		std::string response;
		
		/*xS.outputRemainingRequests();
		xbManager.checkAPILimit();
		xS.repartitionHourlyRequests();
		xS.outputRemainingRequests();
		

		xbManager.scanAllXboxTitles();
		xbManager.scanAllUserProfiles();
		dbManager.processQueue();
		xbManager.clearTitleDataCache();
		
		dbManager.processQueue();


		xS.outputRemainingRequests();*/




		/*response = xbManager.callAPI(xblAPICalls::gameTitle, "609700427");
		xbManager.parseJson(xblAPICalls::gameTitle, response);*/
		/*response = xbManager.callAPI(xblAPICalls::gameTitle, "1007715169");
		xbManager.parseJson(xblAPICalls::gameTitle, response);*/
		//dbManager.processQueue();
		//xbManager.parseJson(xblAPICalls::newGames, xbManager.callAPI(xblAPICalls::newGames));

		//cout << "Parsed: " <<toString(xblAPICalls::gameTitle) << endl;



		//dbManager.getTable(Tables::XboxGameDetails);
		/*mysqlx::Table uP = dbManager.getTable(Tables::XboxUserProfiles);
		
		dbManager.executeSQLFile("sqlQueries/insert userprofiles.sql");
		mysqlx::RowResult result = uP.select("*").execute();
		auto colCount = result.getColumnCount();
		for (auto curColl = 0; curColl < colCount; ++curColl)
		{
			cout << result.getColumn(curColl).getColumnName() << "                    ";
		}
		cout << endl;
		for (mysqlx::Row row : result)
		{
			for (auto curColl = 0; curColl < colCount; ++curColl)
			{
				cout << row.get(curColl) << "                 ";
			}
			cout << endl;
		}*/


	}
	catch (const std::exception& e)
	{
		std::cerr << "Error:  " << e.what() << std::endl;
	}
	
	return 0;
}