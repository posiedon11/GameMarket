#include"XboxAPIManager.h"



namespace XboxAPI_namespace
{
#pragma region XboxCallsURI

	std::string XblAPIManager::to_string(int apiCall) 
	{
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
		case APICalls::newGames: return "https://xbl.io/api/v2/marketplace/new";
		case APICalls::topGames: return "https://xbl.io/api/v2/marketplace/top-paid";
		case APICalls::bestGames: return "https://xbl.io/api/v2/marketplace/best-rated";
		case APICalls::comingSoonGames: return "https://xbl.io/api/v2/marketplace/coming-soon";
		case APICalls::freeGames: return "https://xbl.io/api/v2/marketplace/top-free";
		case APICalls::deals: return "https://xbl.io/api/v2/marketplace/deals";
		case APICalls::mostPlayedGames: return "https://xbl.io/api/v2/marketplace/most-played";
		case APICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";
		case APICalls::gameTitle: return "https://xbl.io/api/v2/marketplace/title";
		case APICalls::playerTitleHistory: return"https://xbl.io/api/v2/player/titleHistory";
		case APICalls::searchPlayer: return "https://xbl.io/api/v2/search";
		case APICalls::checkAPILimit: return "https://xbl.io/api/v2/account";
		default: return "";
		}
	}
	std::string XblAPIManager::getURL(int apiCall, std::string paramater)
	{
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
			//no possible paramaters
		case APICalls::newGames:
		case APICalls::topGames:
		case APICalls::bestGames:
		case APICalls::comingSoonGames:
		case APICalls::freeGames:
		case APICalls::deals:
		case APICalls::mostPlayedGames:
			return to_string(apiCall);

			//possible paramaters
		case APICalls::playerTitleHistory:return paramater == "" ? to_string(apiCall) : to_string(apiCall) + "/" + paramater;

			// necessary paramaters
		case APICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";

		case APICalls::gameTitle:
		case APICalls::searchPlayer:
			return paramater == "" ? errorURLNoParam : to_string(apiCall) + "/" + paramater;
		case APICalls::checkAPILimit:
			return to_string(apiCall);
		default:
			return errorURLCallDoesntExist;
		}
	}

#pragma endregion


	bool XblAPIManager::checkAPILimit()
	{
		string response = callAPI(static_cast<int>(APICalls::checkAPILimit), APIManager::checkHeaders);
		return parseJson(static_cast<int>(APICalls::checkAPILimit), response);
	}
	void XblAPIManager::clearTitleDataCache()
	{
		titleDataCache.clear();
	}

	XblAPIManager::XblAPIManager(DataBaseManager& dbManager, Settings& settings) : APIManager(dbManager, settings), xboxSettings(settings.xboxSettings)
	{
		
		if (xboxSettings.outputDebug)
			std::cout << "OpenXBL Started" << std::endl;
		xboxSettings.resetHourlyRequest();
	}

	
	void XblAPIManager::startCalls()
	{
		cout << "starting calls: " << endl;

		//xboxSettings.outputRemainingRequests();
		if (!checkAPILimit())
		{
			cout << "Out of calls" << endl;
			xboxSettings.setRemaingCalls(0);
		}
			
		xboxSettings.repartitionHourlyRequests();
		xboxSettings.outputRemainingRequests();



		/*mysqlx::Table tbl = dbManager.getTable(Tables::XboxUserProfiles);
		string response = callAPI(static_cast<int>(APICalls::playerTitleHistory), "2533274880644024");
		parseJson(static_cast<int>(APICalls::playerTitleHistory), response);
		tbl.update().set("lastScanned", mysqlx::expr("NOW()")).where("xuid = :xuid").bind("xuid", "2533274880644024").execute();
		for (auto& titleData : titleDataCache)
		{
			dbManager.queueInsert(Tables::XboxGameTitles, titleData.second);
		}
		dbManager.processQueue();*/


		//scanAllUserProfiles();
		scanAllXboxTitles();
		dbManager.processQueue();
		clearTitleDataCache();

		xboxSettings.outputRemainingRequests();
		cout << "Xbox Thread Sleeping" << endl;
		std::this_thread::sleep_for(std::chrono::minutes(30));
	}


#pragma region JSONParsing
	bool XblAPIManager::parseJson(int apiCall, const std::string& json, CRUD operation)
	{
		rapidjson::Document document;
		document.Parse(json.c_str());
		APICalls call = static_cast<APICalls>(apiCall);
		switch (call)
		{
		case APICalls::newGames:
		case APICalls::topGames:
		case APICalls::bestGames:
		case APICalls::comingSoonGames:
		case APICalls::freeGames:
		case APICalls::deals:
		case APICalls::mostPlayedGames:
			parseGeneric(document);
			break;
		case APICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";
		case APICalls::gameTitle:
			parseGameTitle(document, operation);
			break;
		case APICalls::playerTitleHistory:
			parsePlayerHistory(document, operation);
			break;
		case APICalls::searchPlayer: return "https://xbl.io/api/v2/search";
		case APICalls::checkAPILimit:
			return parsePlayerAccout(document, operation);
			break;
			
		}
	}

	void XblAPIManager::parseGeneric(rapidjson::Document &document)
	{
		try {
			if (xboxSettings.outputDebug)
				std::cout << "Parsing Generic" << std::endl;
			if (document.HasMember("Items") && document["Items"].IsArray())
			{
				if (xboxSettings.outputDebug)
					std::cout << "Found Items." << std::endl;
				const auto& items = document["Items"].GetArray();
				
				for (const auto& item : items)
				{
					shared_ptr<genericXboxData> jsonData = std::make_shared<genericXboxData>();
					jsonData->productID = item.HasMember("Id") ? item["Id"].GetString() : "";
					jsonData->ItemType = item.HasMember("ItemType") ? item["ItemType"].GetString() : "";
					if (xboxSettings.outputDebug)
						std::cout << "ID: " << jsonData->productID << std::endl << "ItemType: " << jsonData->ItemType << std::endl;
				}

			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Generic"<< std::endl;
			std::cout << ex.what() << std::endl;
		}
		
	}
	
	void XblAPIManager::parseGameTitle(rapidjson::Document& document, CRUD operation)
	{
		try
		{
			if (xboxSettings.outputDebug)
				cout << "Parsing Game Title" << endl;

			const auto& products = document.FindMember("Products");
			if (products != document.MemberEnd() && products->value.IsArray())
			{
				if (xboxSettings.outputDebug)
					std::cout << "Found Products." << std::endl;

				for (const auto& product : products->value.GetArray())
				{
					shared_ptr<XboxTitleDetailsData> jsonData = std::make_shared<XboxTitleDetailsData>();
					shared_ptr<XboxUpdateScannedData> updateData = std::make_shared<XboxUpdateScannedData>();
					shared_ptr<XboxProductGroupData> groupData = std::make_shared<XboxProductGroupData>();
					


					const auto& isSandBoxed = product.FindMember("IsSandboxedProduct");
					if (isSandBoxed == product.MemberEnd() || (isSandBoxed->value.IsBool() && isSandBoxed->value == false))
					{
						if (xboxSettings.outputDebug)
						cout << "Product is not sandBoxed" << endl;
						continue;
					}
					
					const auto& sandboxID = product.FindMember("SandboxId");
					if (sandboxID == product.MemberEnd() || (sandboxID->value.IsString() && sandboxID->value != "RETAIL"))
					{
						cout << "Product is not a retail Product" << endl;
						continue;
					}

					//gets all objects in relatedproduct
					auto relatedProducts = recursiveFind("MarketProperties/RelatedProducts", product, false);

					for (const auto* relatedProduct : relatedProducts)
					{
						if (relatedProduct->IsObject())
						{
							const auto& relationshipType = relatedProduct->FindMember("RelationshipType");
							if (relationshipType != relatedProduct->MemberEnd())
								if (relationshipType->value.IsString() && string(relationshipType->value.GetString()) == "Bundle")
								{
									const auto& relatedProductID = relatedProduct->FindMember("RelatedProductId");
									if (relatedProductID != relatedProduct->MemberEnd() && relatedProductID->value.IsString())
									{
										//cout << relatedProductID->value.GetString();
										jsonData->bundleIDs.push_back(relatedProductID->value.GetString());
									}

								}
						}
					}

					///find the productID
					const auto& productID = product.FindMember("ProductId");
					if (productID != product.MemberEnd() && productID->value.IsString())
					{
						jsonData->productID = productID->value.GetString();
						groupData->productID = productID->value.GetString();
					}

					//Find the titleID
					const auto& alternateIDs = product.FindMember("AlternateIds");
					if (alternateIDs != product.MemberEnd() && alternateIDs->value.IsArray())
					{
						for (const auto& alternateID : alternateIDs->value.GetArray())
						{
							const auto& idType = alternateID.FindMember("IdType");
							if (idType != alternateID.MemberEnd()
								&& idType->value.IsString() && string(idType->value.GetString()) == "XboxTitleId")
							{
								const auto& idValue = alternateID.FindMember("Value");
								if (idValue != alternateID.MemberEnd() && idValue->value.IsString())
								{
									jsonData->titleID = idValue->value.GetString();
									updateData->ID = idValue->value.GetString();
									groupData->titleID = idValue->value.GetString();
								}

							}
						}
					}



					const auto& productGroupID = nestedFind("Properties/ProductGroupId", product);
					const auto& productGroupName = nestedFind("Properties/ProductGroupName", product);

					if (productGroupID != nullptr && productGroupName != nullptr)
					{
						if (productGroupID->IsString() && productGroupName->IsString())
						{
							string groupID = productGroupID->GetString();
							string groupName = productGroupName->GetString();
							if (groupID != "" && groupName != "")
							{
								groupData->groupID = groupID;
								groupData->groupName = groupName;

								productGroupDataCache.insert(std::pair<string, shared_ptr<XboxProductGroupData>>(groupData->groupID, groupData));
							}

						}
					}

					dbManager.queueInsert(Tables::XboxProductIds, jsonData, CRUD::UpSert);
					dbManager.queueInsert(Tables::XboxTitleDetails, jsonData, operation);
					dbManager.queueInsert(Tables::XboxGameBundles, jsonData, CRUD::UpSert);
					//dbManager.queueInsert(Tables::XboxGameTitles, updateData, CRUD::Update);
					dbManager.queueInsert(Tables::XboxGameTitles, groupData, CRUD::Update);
					dbManager.processQueue();
				}

			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Game Title" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}


	void XblAPIManager::parsePlayerHistory(rapidjson::Document& document, CRUD operation)
	{
		try {
			if (xboxSettings.outputDebug)
				std::cout << "Parsing Player History" << std::endl;


			const auto& titles = document.FindMember("titles");
			if (titles != document.MemberEnd() && titles->value.IsArray())
			{
				for (const auto &title : titles->value.GetArray())
				{
					bool validTitle = true;
					shared_ptr<XboxGameTitleData> jsonData = std::make_shared<XboxGameTitleData>();

					//get titleid
					if (title.HasMember("titleId") && title["titleId"].IsString())
					{
						jsonData->titleID = title["titleId"].GetString();
					}
					else
						validTitle = false;
					//get title name
					if (title.HasMember("name") && title["name"].IsString())
					{
						jsonData->titleName = title["name"].GetString();
					}
					else
						validTitle = false;

					//get display image
					if (title.HasMember("displayImage") && title["displayImage"].IsString())
					{
						jsonData->displayImage = title["displayImage"].GetString();
					}
					else
						validTitle = false;

					//get modern title id
					if (title.HasMember("modernTitleId") && title["modernTitleId"].IsString())
					{
						jsonData->modernTitleID = title["modernTitleId"].GetString();
					}
					else
						validTitle = false;

					//get device list
					const auto& devices = title.FindMember("devices");
					if (devices != title.MemberEnd() && devices->value.IsArray())
					{
						for (const auto& device : devices->value.GetArray())
						{
							if (device.IsString())
							{
								string devicestr = device.GetString();
								if (device == "Win32")
								{
									validTitle = false;
									break;
								}
								jsonData->devices.push_back(devicestr);
							}
						}
					}
					else
						validTitle = false;

					//check if gamepass
					if (title.HasMember("gamePass") && title.IsObject())
					{
						if (title["gamePass"].HasMember("isGamePass") && title["gamePass"]["isGamePass"].IsBool())
						{
							jsonData->isGamePass = title["gamePass"]["isGamePass"].GetBool();
						}
						else
							validTitle = false;
					}
					else
						validTitle = false;


					//Insert into database
					if (validTitle)
					{
						titleDataCache.insert(std::pair<string, shared_ptr<XboxGameTitleData>>(jsonData->titleID, jsonData));
						//jsonData.outputData();
						//dbManager.queueInsert(Tables::XboxGameTitles, &jsonData);
						//dbManager.processQueue();
					}
				}


				
			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse player data" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}

	bool XblAPIManager::parsePlayerAccout(rapidjson::Document& document, CRUD operation)
	{
		try 
		{
			if (xboxSettings.outputDebug)
				cout << "Parsing Player Account" << endl;

			if (document.FindMember("error") != document.MemberEnd())
			{
				cout << "Player Account Error" << endl;
				return false;
			}
			return true;

		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse player Account" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}



	void XblAPIManager::outputCache()
	{
			for (auto &titleData : titleDataCache)
			{
				//cout << titleData.first << endl;
				//titleData.second.outputData();
				dbManager.queueInsert(Tables::XboxGameTitles, titleData.second);
				
			}
			
	}



	void XblAPIManager::scanAllXboxTitles()
	{
		if (!xboxSettings.canRequest(XboxSettings::hourlyAPICallsRemaining::title))
			return;


		mysqlx::Table tbl = dbManager.getTable(Tables::XboxGameTitles);


		auto now = std::chrono::system_clock::now();
		auto refreshTime = std::chrono::system_clock::to_time_t(now - xboxSettings.gameTitleUpdateFrequency);
		
		auto gameTitlesInsert = tbl.select("modernTitleId").where("lastScanned IS null")
			.orderBy("lastScanned").bind("refreshTime", refreshTime).execute();
		auto gameTitlesUpdate = tbl.select("modernTitleId").where("lastScanned < FROM_UNIXTIME(:refreshTime)")
			.orderBy("lastScanned").bind("refreshTime", refreshTime).execute();


		cout << "Scanning Xbox Titles" << endl;
		cout << "Game Titles needing Updates: " << gameTitlesUpdate.count() << endl;
		cout << "Game Titles needing Insertions: " << gameTitlesInsert.count() << endl;


		//Insertions first
		for (const auto row : gameTitlesInsert)
		{
			if (xboxSettings.canRequest(XboxSettings::hourlyAPICallsRemaining::title))
			{
				xboxSettings.makeRequest(XboxSettings::hourlyAPICallsRemaining::title);;


				string modernID  = row[0].get<std::string>();

				//update the scanned time
				string response = callAPI(static_cast<int>(APICalls::gameTitle), modernID);
				parseJson(static_cast<int>(APICalls::gameTitle), response, CRUD::Insert);


				
			}
			else
			{
				cout << "xbox game titles limit reached: " << endl;
				break;
			}
		}

		for (const auto row : gameTitlesUpdate)
		{
			if (xboxSettings.canRequest(XboxSettings::hourlyAPICallsRemaining::title))
			{
				xboxSettings.makeRequest(XboxSettings::hourlyAPICallsRemaining::title);
				shared_ptr<XboxUpdateScannedData> scannedData = make_shared<XboxUpdateScannedData>();
				scannedData->ID = row[0].get<std::string>();

				string response = callAPI(static_cast<int>(APICalls::gameTitle), scannedData->ID);

				parseJson(static_cast<int>(APICalls::gameTitle), response, CRUD::Update);


				//update the scanned time
				dbManager.queueInsert(Tables::XboxGameTitles, scannedData, CRUD::Update);
			}
			else
			{
				cout << "xbox game titles limit reached: " << endl;
				break;
			}
		}

		cout << "updating group data" << endl;
		for (const auto& groupData : productGroupDataCache)
		{
			dbManager.queueInsert(Tables::XboxGroupData, groupData.second, CRUD::UpSert);
		}
		
	}
	void XblAPIManager::scanAllUserProfiles()
	{
		mysqlx::Table tbl = dbManager.getTable(Tables::XboxUserProfiles);
		auto now = std::chrono::system_clock::now();
		auto refreshTime = std::chrono::system_clock::to_time_t(now - xboxSettings.userProfileUpdateFrequency);


		auto result = tbl.select("*").where("lastScanned < FROM_UNIXTIME(:refreshTime) or lastScanned IS null")
			.orderBy("lastScanned").bind("refreshTime", refreshTime).execute();

		cout << "User Profiles Needing updates: " <<result.count() << endl;

		for (auto row : result)
		{
			//cout << row[0] << endl;
			shared_ptr<XboxUpdateScannedData> scannedData = make_shared<XboxUpdateScannedData>();

			//get the XUID of the profile
			scannedData->ID = row[0].get<std::string>();


			string response = callAPI(static_cast<int>(APICalls::playerTitleHistory), scannedData->ID);
			parseJson(static_cast<int>(APICalls::playerTitleHistory), response, CRUD::UpSert);

			//update the profile with current time;
			dbManager.queueInsert(Tables::XboxUserProfiles, scannedData, CRUD::Update);
			
		}
		//parse the cache
		for (auto &titleData : titleDataCache)
		{
		
			dbManager.queueInsert(Tables::XboxGameTitles, titleData.second);
		}		
		dbManager.processQueue();
	}


#pragma endregion


#pragma region CurlStuff
	void XblAPIManager::createHeaders(curl_slist*& headers)
	{
		headers = NULL;
		headers = curl_slist_append(headers, "accept: */*");
		headers = curl_slist_append(headers, ("x-authorization: " + xboxSettings.xblAPIKey).c_str());
	}

	void XblAPIManager::setupCurl(CURL*& curl, string paramater, string& responseBody, std::map<string, string>& responseHeaders)
	{
		// Callback and resonse for body
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallBack);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &responseBody);

		//callback and response for headers
		if (paramater == checkHeaders)
		{
			curl_easy_setopt(curl, CURLOPT_HEADERDATA, &responseHeaders);
			curl_easy_setopt(curl, CURLOPT_HEADERFUNCTION, headerCallBack);
		}
	}

	void XblAPIManager::additionalHeaders(CURL*& curl, string paramater)
	{
		if (paramater == "hell0")
			cout << "hello";
	}
	void XblAPIManager::handleCurlHeaders(std::map<string, string> responseHeaders)
	{
		if (xboxSettings.outputDebug && settings.outputCurl)
			std::cout << "Curl Success" << std::endl;
		if (responseHeaders.find("X-RateLimit-Limit") != responseHeaders.end() && responseHeaders.find("X-RateLimit-Used") != responseHeaders.end())
		{
			xboxSettings.maxHourlyAPIRequests = stoi(responseHeaders["X-RateLimit-Limit"]);
			xboxSettings.remainingAPIRequests = xboxSettings.maxHourlyAPIRequests - stoi(responseHeaders["X-RateLimit-Used"]);
			
			//cout << xboxSettings.remainingAPIRequests << " / " << xboxSettings.maxHourlyAPIRequests << endl;
		}
	}
#pragma endregion

}