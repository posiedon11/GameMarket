#pragma once
#ifndef APIMANAGER_LOCK
#define APIMANAGER_LOCK
#include"Tools.h"
#include"DataBaseManager.h"
#include"curl/curl.h"
using namespace DataBase_NameSpace;

namespace apimanager_namespace
{
	class APIManager
	{
	public:
		const std::string errorURLNoParam = "Error, missing paramater";
		const std::string errorURLCallDoesntExist = "Error, API call does not exist";
		const std::string curlFail = "requestFailed";
		const std::string checkHeaders = "CheckForHeaders";
		std::string managerName;


	protected:
		std::atomic<bool> running;
		std::thread mainThread;
		DataBaseManager& dbManager;
		Settings& settings;


	public:
		//Handles Threading
		void start();
		void stop();

		//Constructors
		virtual ~APIManager();
		APIManager(DataBaseManager &dbManager, Settings &settings);

		//Handles Converting to URL
		virtual std::string to_string(int apiCall) = 0;
		virtual std::string getURL(int apiCall, std::string paramater = "") = 0;
		bool validURL(std::string url);


		//Handles API calls
		std::string callAPI(int apiCall, std::string paramater = "");
		virtual bool parseJson(int apiCall, const std::string& json, CRUD operation = CRUD::Select) = 0;
		virtual void setupCurl(CURL* &curl, string paramater, string& responseBody, std::map<string, string>& responseHeaders) = 0;
		virtual void additionalHeaders(CURL*& curl, string paramater);
		virtual void handleCurlHeaders(std::map<string, string> responseHeaders);

	protected:
		void run();
		virtual void startCalls() = 0;


		virtual void createHeaders(curl_slist*& headers) = 0;
		void appendHeader(curl_slist*& headers, std::string header);
		void setHeaders(CURL*& curl, curl_slist* headers);
	};
}
#endif // !APIMANAGER_LOCK
