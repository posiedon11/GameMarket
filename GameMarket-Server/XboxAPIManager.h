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
#include"APIManager.h"


using namespace DataBase_NameSpace;
using namespace GameMarketSettings_NameSpace;
using namespace apimanager_namespace;

namespace XboxAPI_namespace
{		
#pragma region MyRegion
	class XblAPIManager : public APIManager
	{
	public:
		enum class APICalls
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


	private:
		map<string, shared_ptr<XboxGameTitleData>> titleDataCache;
		map<string, shared_ptr<XboxProductGroupData>> productGroupDataCache;

		XboxSettings& xboxSettings;
	public:
#pragma region Constructor/destruct
		XblAPIManager(DataBaseManager& dbManager, Settings& settings);
#pragma endregion

		//Override functions
		std::string to_string(int apiCall) override;
		std::string getURL(int apiCall, std::string paramater = "") override;
		void createHeaders(curl_slist*& headers) override;
		void startCalls() override;
		bool parseJson(int apiCall, const std::string& json, CRUD operation = CRUD::Select) override;
		void setupCurl(CURL*& curl,string paramater, string& responseBody, std::map<string, string>& responseHeaders) override;

		void additionalHeaders(CURL*& curl, string paramater) override;
		void handleCurlHeaders(std::map<string, string> responseHeaders) override;

		//Uses a single API call to check the remaining calls
		bool checkAPILimit();

		void outputCache();


		//All Json Parsing
		void parseGeneric(rapidjson::Document& document);
		void parseGameTitle(rapidjson::Document& document, CRUD operation);
		void parsePlayerHistory(rapidjson::Document& document, CRUD operation);
		bool parsePlayerAccout(rapidjson::Document& document, CRUD operation);

		//All Scanning
		void scanAllXboxTitles();
		void scanAllUserProfiles();
		void scanAllMarketDetails();

		void clearTitleDataCache();

		//Used to clear up code a bit
	};
#pragma endregion

}

#endif // !XBOXAPI_LOCK

