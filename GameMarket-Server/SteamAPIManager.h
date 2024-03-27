#pragma once
#ifndef STEAMAPI_LOCK
#define STEAMAPI_LOCK
#include"Tools.h"
#include"DataBaseManager.h"
#include"APIManager.h"


using namespace DataBase_NameSpace;
using namespace apimanager_namespace;
using namespace GameMarketSettings_NameSpace;

namespace SteamAPI_namespace
{

	class stmAPIManager : public APIManager {
	public:
		enum class APICalls {
			getAppList,
			getAppDetails
		};
	private:
		SteamSettings& steamSettings;
		string checkHeaders = "checkHeaders";

	public:
		stmAPIManager(DataBaseManager& dbmanager, Settings &settings);


		//Override functions
		std::string to_string(int apiCall) override;
		std::string getURL(int apiCall, std::string paramater = "") override;
		void createHeaders(curl_slist*& headers) override;
		void startCalls() override;
		bool parseJson(int apiCall, const std::string& json, CRUD operation = CRUD::Select) override;
		void setupCurl(CURL*& curl, string paramater, string& responseBody, std::map<string, string>& responseHeaders) override;

		void additionalHeaders(CURL*& curl, string paramater) override;
		void handleCurlHeaders(std::map<string, string> responseHeaders) override;


		void parseAppDetails(rapidjson::Document& document, CRUD operation);
		void parseAppList(rapidjson::Document &document, CRUD operation);
	private:
	};
}


#endif // !STEAMAPI_LOCK
