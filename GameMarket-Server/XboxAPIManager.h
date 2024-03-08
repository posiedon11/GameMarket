#pragma once
#ifndef XBOXAPI_LOCK
#define XBOXAPI_LOCK

#include"Tables.h"
#include"DataBaseManager.h"
#include"GameMarketSettings.h"
#include<curl/curl.h>
#include<rapidjson/rapidjson.h>
#include<rapidjson/document.h>
#include<queue>
#include<vector>


using namespace DataBase_NameSpace;
using namespace GameMarketSettings_NameSpace;

namespace XboxAPI_namespace
{
#pragma region xblAPICallsURI
	const std::string errorNoParam = "Error, missing paramater";
	const std::string errorCallDoesntExist = "Error, xbl API call does not exist";

	enum class xblAPICalls
	{
		checkAPILimit,
		newGames,
		topGames,
		bestGames,
		comingSoonGames,
		freeGames,
		deals,
		mostPlayedGames,
		gameDetails,
		gameTitle,
		playerTitleHistory,
		searchPlayer
	};

	std::string toString(xblAPICalls call);

	std::string getURL(xblAPICalls call, std::string paramater = "");

#pragma endregion

	
	
#pragma region MyRegion
	class XblAPIManager
	{
	public:
		XboxSettings& settings;
	private:
		DataBaseManager& dbManager;
		map<string, XboxGameTitleData> titleDataCache;
		const std::string curlFail = "requestFailed";
		const std::string checkHeaders = "CheckForHeaders";
		std::thread mainXboxThread;
		std::atomic<bool> running;

	public:
#pragma region Constructor/destruct
		XblAPIManager(DataBaseManager& dbManager, XboxSettings& settings);
#pragma endregion

		void startCalls();
		void taskTimer(std::atomic<bool>&running);
		void start();
		void stop();
		void checkAPILimit();
		void createHeaders(curl_slist*& headers);
		void appendHeadder(curl_slist*& headers, std::string header);
		void setHeaders(CURL*& curl, curl_slist* headers);
		void outputCache();

		std::string callAPI(xblAPICalls call, std::string paramater = "");


		bool parseJson(xblAPICalls call, const std::string& json);
		void parseGeneric(rapidjson::Document& document);
		void parseGameTitle(rapidjson::Document& document);
		void parseGameTitle2(rapidjson::Document& document);
		void parsePlayerHistory(rapidjson::Document& document);


		void scanAllXboxTitles();

		void scanAllUserProfiles();

		void scanAllMarketDetails();

		void clearTitleDataCache();
		void localizedProperties(const rapidjson::Value& product, XboxGameMarketData& jsonData);
		void displaySkuAvailabilities(const rapidjson::Value& product, XboxGameMarketData& jsonData);
		void MarketProperties(const rapidjson::Value& product, XboxGameMarketData& jsonData);
	};
#pragma endregion

}

#endif // !XBOXAPI_LOCK

