#include"SteamAPIManager.h"

namespace SteamAPI_namespace
{
	stmAPIManager::stmAPIManager(DataBaseManager& dbmanager, Settings& settings) : APIManager(dbmanager, settings), steamSettings(settings.steamSettings)
	{
		cout << "Steam API Manager started" << endl;
	}


	//Override functions
	std::string stmAPIManager::to_string(int apiCall)
	{
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
		case APICalls::getAppList: return "https://api.steampowered.com/ISteamApps/GetAppList/v2/?key=" + steamSettings.apiKey;
		case APICalls::getAppDetails:return "https://store.steampowered.com/api/appdetails?cc=us&appids=";
		
		default:
			return "";
		}
	}
	std::string stmAPIManager::getURL(int apiCall, std::string paramater)
	{
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
		case APICalls::getAppList: return to_string(apiCall);
		case APICalls::getAppDetails: return paramater == "" ? errorURLNoParam : to_string(apiCall) + paramater;
		default:
			break;
		}
	}
	void stmAPIManager::createHeaders(curl_slist*& headers)
	{

	}
	void stmAPIManager::startCalls()
	{
		string response = callAPI(static_cast<int>(APICalls::getAppList));
		parseJson(static_cast<int>(APICalls::getAppList),  response);
		dbManager.processQueue();


		cout << "Steam Thread Sleeping" << endl;
		std::this_thread::sleep_for(std::chrono::minutes(30));
	}
	bool stmAPIManager::parseJson(int apiCall, const std::string& json, CRUD operation)
	{
		rapidjson::Document document;
		document.Parse(json.c_str());
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
		case APICalls::getAppDetails: parseAppDetails(document, operation);
			break;
		case APICalls::getAppList: parseAppList(document, operation);
			break;
		}

		return false;
	}

	void stmAPIManager::parseAppList(rapidjson::Document &document, CRUD operation)
	{
		try {
			cout << "Parsing applist" << endl;
			const auto& applist = document.FindMember("applist");
			if (applist != document.MemberEnd() && applist->value.IsObject())
			{
				const auto& apps = applist->value.FindMember("apps");
				if (apps != applist->value.MemberEnd() && apps->value.IsArray())
				{
					for (const auto& app : apps->value.GetArray())
					{
						shared_ptr<SteamAPPListData> JSONdata = std::make_shared<SteamAPPListData>();
						if ((app.HasMember("appid") && app.HasMember("name")) &&
							(app["appid"].IsUint() && app["name"].IsString()))
						{
							JSONdata->appid = app["appid"].GetUint();
							JSONdata->name = app["name"].GetString();

							if (JSONdata->name != "")
							{
								dbManager.queueInsert(Tables::SteamAppIDs, JSONdata);
							}
						}

					}
				}
			}

		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Game Title" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}

	void stmAPIManager::parseAppDetails(rapidjson::Document& document, CRUD operation)
	{
		try
		{
			//For the current api/appdetails, you can only choose 1 app id at a time, so this loop is not necessary, but you never know.
			for (rapidjson::Value::ConstMemberIterator itr = document.MemberBegin(); itr != document.MemberEnd(); ++itr)
			{
				if (itr->value.IsObject())
				{
					const auto& success = itr->value.FindMember("success");
					if (success != itr->value.MemberEnd() && success->value.IsBool() && success->value.GetBool() == true)
					{
						const auto& data = itr->value.FindMember("data");
						if (data != itr->value.MemberEnd() && data->value.IsObject())
						{
							const auto& type = data->value.FindMember("type");
							if (type != data->value.MemberEnd() && type->value.IsString())
							{
								string typeString = type->value.GetString();
								if (typeString == "game")
								{
									cout << typeString << endl;
								}
								else if (typeString == "dlc")
								{
									cout << typeString << endl;
								}
							}
						}
					}
					cout << "CAll Fail" << endl;
				}
				else {
					cout << "Fail" << endl;
				}
			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Game Title" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}
	void stmAPIManager::setupCurl(CURL*& curl, string paramater, string& responseBody, std::map<string, string>& responseHeaders)
	{
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallBack);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &responseBody);

		//callback and response for headers
		if (paramater == checkHeaders)
		{
			curl_easy_setopt(curl, CURLOPT_HEADERDATA, &responseHeaders);
			curl_easy_setopt(curl, CURLOPT_HEADERFUNCTION, headerCallBack);
		}
	}

	void stmAPIManager::additionalHeaders(CURL*& curl, string paramater)
	{

	}
	void stmAPIManager::handleCurlHeaders(std::map<string, string> responseHeaders)
	{

	}
}