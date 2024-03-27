#include"Tables.h"
#include"GameMarketSettings.h"
#include"DataBaseManager.h"
#include<iostream>
#include"XboxAPIManager.h"
#include"Tools.h"
#include"SteamAPIManager.h"

using namespace tables_namespace;
using namespace GameMarketSettings_NameSpace;
using namespace DataBase_NameSpace;
using namespace XboxAPI_namespace;
using namespace SteamAPI_namespace;

int main(void)
{
	Settings& settings = Settings::getInstance();


	//DataBaseManager dbManager(ss.serverName, ss.serverPort,ss.serverUserName,ss.serverPassword, "GameMarket");
	DataBaseManager dbManager(settings);

	XblAPIManager xbManager(dbManager, settings);
	stmAPIManager stmManager(dbManager, settings);
	std::atomic<bool> running(true);
	try
	{
		//std::thread xboxManagerThread(&XblAPIManager::taskTimer, std::ref(xbManager), std::ref(running));
		//std::thread dbManagerThread(&DataBaseManager::taskTimer, std::ref(dbManager), std::ref(running));
		
		// 
		// 
		/*string response = xbManager.callAPI(static_cast<int>(XblAPIManager::APICalls::gameTitle), "609700427");
		xbManager.parseJson(static_cast<int>(XblAPIManager::APICalls::gameTitle), response, CRUD::Update);*/
		xbManager.start();
		//stmManager.start();
		auto startTime = std::chrono::system_clock::now();
		auto currentTime = startTime;
		while (currentTime < startTime + std::chrono::hours(6))
		{
			/*currentTime = std::chrono::system_clock::now();
			if (std::chrono::duration_cast<std::chrono::seconds>(currentTime - startTime) % 20)
				cout << std::chrono::duration_cast<std::chrono::minutes>(currentTime - startTime).count() << endl;*/
		}
		cout << "Thread timer is over" << endl;
		running.store(false);
		xbManager.stop();
		stmManager.stop();
		

		//std::string response;

	}
	catch (const std::exception& e)
	{
		std::cerr << "Error:  " << e.what() << std::endl;
	}
	
	return 0;
}