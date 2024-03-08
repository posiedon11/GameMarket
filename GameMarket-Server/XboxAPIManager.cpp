#include"XboxAPIManager.h"



namespace XboxAPI_namespace
{
#pragma region XboxCallsURI
	std::string toString(xblAPICalls call)
	{
		switch (call)
		{
		case xblAPICalls::newGames: return "https://xbl.io/api/v2/marketplace/new";
		case xblAPICalls::topGames: return "https://xbl.io/api/v2/marketplace/top-paid";
		case xblAPICalls::bestGames: return "https://xbl.io/api/v2/marketplace/best-rated";
		case xblAPICalls::comingSoonGames: return "https://xbl.io/api/v2/marketplace/coming-soon";
		case xblAPICalls::freeGames: return "https://xbl.io/api/v2/marketplace/top-free";
		case xblAPICalls::deals: return "https://xbl.io/api/v2/marketplace/deals";
		case xblAPICalls::mostPlayedGames: return "https://xbl.io/api/v2/marketplace/most-played";
		case xblAPICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";
		case xblAPICalls::gameTitle: return "https://xbl.io/api/v2/marketplace/title";
		case xblAPICalls::playerTitleHistory: return"https://xbl.io/api/v2/player/titleHistory";
		case xblAPICalls::searchPlayer: return "https://xbl.io/api/v2/search";
		case xblAPICalls::checkAPILimit: return "https://xbl.io/api/v2/account";
		default: return "";
		}
	}
	std::string getURL(xblAPICalls call, std::string paramater)
	{
		switch (call)
		{
			//no possible paramaters
		case xblAPICalls::newGames:
		case xblAPICalls::topGames:
		case xblAPICalls::bestGames:
		case xblAPICalls::comingSoonGames:
		case xblAPICalls::freeGames:
		case xblAPICalls::deals:
		case xblAPICalls::mostPlayedGames:
			return toString(call);

			//possible paramaters
		case xblAPICalls::playerTitleHistory:return paramater == "" ? toString(call) : toString(call) +"/" + paramater;

			// necessary paramaters
		case xblAPICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";

		case xblAPICalls::gameTitle:
		case xblAPICalls::searchPlayer:
			return paramater == "" ? errorNoParam : toString(call) +"/" + paramater;
		case xblAPICalls::checkAPILimit:
			return toString(call);
		default:
			return errorCallDoesntExist;
		}

	}
	bool validURL(std::string url)
	{
		if (url == "")
		{
			std::cout << "Empty URL" << std::endl;
			return false;
		}
		else if (url == errorNoParam) {
			std::cout << "A paramater was needed" << std::endl;
			return false;
		}
		else if (url == errorCallDoesntExist)
		{
			std::cout << "the given call does not exist";
				return false;
		}
		return true;
	}
#pragma endregion


	void XblAPIManager::checkAPILimit()
	{
		callAPI(xblAPICalls::checkAPILimit, checkHeaders);
	}
	void XblAPIManager::clearTitleDataCache()
	{
		titleDataCache.clear();
	}

	XblAPIManager::XblAPIManager(DataBaseManager& dbManager, XboxSettings& settings) : dbManager(dbManager), settings(settings)
	{
		if (settings.outputDebug)
			std::cout << "OpenXBL Started" << std::endl;
		settings.resetHourlyRequest();
	}

	std::string XblAPIManager::callAPI(xblAPICalls call, std::string paramater)
	{
		CURL* xboxCurl;
		CURLcode res;
		std::string responseBody;
		std::map<string, string> responseHeaders;

		xboxCurl = curl_easy_init();

		std::string url = getURL(call, paramater);
		if (xboxCurl) {
			// Specify the URL to get

			if (!validURL)
			{
				return curlFail;
			}
			std::cout << url << std::endl<< std::endl;
			curl_easy_setopt(xboxCurl, CURLOPT_URL, url.c_str());


			//headers
			struct curl_slist* headers = NULL;
			createHeaders(headers);
			setHeaders(xboxCurl, headers);


			// Callback and resonse for body
			curl_easy_setopt(xboxCurl, CURLOPT_WRITEFUNCTION, writeCallBack);
			curl_easy_setopt(xboxCurl, CURLOPT_WRITEDATA, &responseBody);

			//callback and response for headers
			if (paramater == checkHeaders )
			{
				curl_easy_setopt(xboxCurl, CURLOPT_HEADERDATA, &responseHeaders);
				curl_easy_setopt(xboxCurl, CURLOPT_HEADERFUNCTION, headerCallBack);
			}


			

			// Perform the request, res will get the return code
			res = curl_easy_perform(xboxCurl);


			// Check for errors
			if (res != CURLE_OK) {
				std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
				responseBody = curlFail;
			}
			else if (responseBody == "")
			{
				std::cerr << "No response recieved"<< std::endl;
				responseBody = curlFail;
			}
			else {
				//Response is valid
			    //std::cout << "Response Data: " << readBuffer << std::endl;
				if (settings.outputDebug)
					std::cout << "Curl Success" << std::endl <<endl;
				if (responseHeaders.find("X-RateLimit-Limit") != responseHeaders.end() && responseHeaders.find("X-RateLimit-Used") != responseHeaders.end())
				{
					settings.maxHourlyAPIRequests = stoi(responseHeaders["X-RateLimit-Limit"]);
					settings.remainingAPIRequests = settings.maxHourlyAPIRequests - stoi(responseHeaders["X-RateLimit-Used"]);
					cout << settings.remainingAPIRequests << " / " << settings.maxHourlyAPIRequests << endl;
				}
					
				
			}

			// Clean up
			curl_slist_free_all(headers); // Free the header list
			curl_easy_cleanup(xboxCurl); // End a libcurl easy session
		}
		return responseBody;
	}

	
	void XblAPIManager::startCalls()
	{
		cout << "starting calls: " << endl;

		settings.outputRemainingRequests();
		checkAPILimit();
		settings.repartitionHourlyRequests();
		settings.outputRemainingRequests();

		scanAllXboxTitles();
		scanAllUserProfiles();
	}

	void XblAPIManager::taskTimer(std::atomic<bool>&running)
	{
		while (running.load())
		{
			std::this_thread::sleep_for(chrono::hours(1));
			if (!running.load())
			{
				cout << "Xbox time exit: " << endl;
				break;
			}
			startCalls();
		}
		
	}

#pragma region JSONParsing
	bool XblAPIManager::parseJson(xblAPICalls call, const std::string& json)
	{
		rapidjson::Document document;
		document.Parse(json.c_str());

		switch (call)
		{
		case xblAPICalls::newGames:
		case xblAPICalls::topGames:
		case xblAPICalls::bestGames:
		case xblAPICalls::comingSoonGames:
		case xblAPICalls::freeGames:
		case xblAPICalls::deals:
		case xblAPICalls::mostPlayedGames:
			parseGeneric(document);
			break;
		case xblAPICalls::gameDetails: return "https://xbl.io/api/v2/marketplace/details";
		case xblAPICalls::gameTitle:
			parseGameTitle(document);
			break;
		case xblAPICalls::playerTitleHistory:
			parsePlayerHistory(document);
			break;
		case xblAPICalls::searchPlayer: return "https://xbl.io/api/v2/search";
		}
	}

	void XblAPIManager::parseGeneric(rapidjson::Document &document)
	{
		try {
			if (settings.outputDebug)
				std::cout << "Parsing Generic" << std::endl;
			if (document.HasMember("Items") && document["Items"].IsArray())
			{
				if (settings.outputDebug)
					std::cout << "Found Items." << std::endl;
				const auto& items = document["Items"].GetArray();
				genericXboxData jsonData;
				for (const auto& item : items)
				{
					jsonData.productID = item.HasMember("Id") ? item["Id"].GetString() : "";
					jsonData.ItemType = item.HasMember("ItemType") ? item["ItemType"].GetString() : "";
					if (settings.outputDebug)
						std::cout << "ID: " << jsonData.productID << std::endl << "ItemType: " << jsonData.ItemType << std::endl;
				}

			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Generic"<< std::endl;
			std::cout << ex.what() << std::endl;
		}
		
	}
	
	void XblAPIManager::parseGameTitle(rapidjson::Document& document)
	{
		try
		{
			if (settings.outputDebug)
				cout << "Parsing Game Title" << endl;

			const auto& products = document.FindMember("Products");
			if (products != document.MemberEnd() && products->value.IsArray())
			{
				if (settings.outputDebug)
					std::cout << "Found Products." << std::endl;

				for (const auto& product : products->value.GetArray())
				{
					XboxTitleDetailsData JSONData;

					const auto& isSandBoxed = product.FindMember("IsSandboxedProduct");
					if (isSandBoxed == product.MemberEnd() || (isSandBoxed->value.IsBool() && isSandBoxed->value == false))
					{
						if (settings.outputDebug)
						cout << "Product is not sandBoxed" << endl;
						continue;
					}
					
					const auto& sandboxID = product.FindMember("SandboxId");
					if (sandboxID == product.MemberEnd() || (sandboxID->value.IsString() && sandboxID->value != "RETAIL"))
					{
						cout << "Product is not a retail Product" << endl;
						continue;
					}

					//Mainly to find bundles;
					const auto& marketproperties = product.FindMember("MarketProperties");
					if (marketproperties != product.MemberEnd() && marketproperties->value.IsArray())
					{
						for (const auto& marketproperty : marketproperties->value.GetArray())
						{
							const auto& relatedproducts = marketproperty.FindMember("RelatedProducts");
							if (relatedproducts != marketproperty.MemberEnd() && relatedproducts->value.IsArray())
							{
								for (const auto& relatedproduct : relatedproducts->value.GetArray())
								{
									const auto& relationshipType = relatedproduct.FindMember("RelationshipType");
									//Check to see if relationship is a bundle
									if (relationshipType != relatedproduct.MemberEnd()
										&& relationshipType->value.IsString() && string(relationshipType->value.GetString()) == "Bundle")
									{
										const auto& relatedProductID = relatedproduct.FindMember("RelatedProductId");
										if (relatedProductID != relatedproduct.MemberEnd() && relatedProductID->value.IsString())
										{
											JSONData.bundleIDs.push_back(relatedProductID->value.GetString());
										}

									}
								}
							}
						}
					}


					const auto& productID = product.FindMember("ProductId");
					if (productID != product.MemberEnd() && productID->value.IsString())
					{
						JSONData.productID = productID->value.GetString();
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
									JSONData.titleID = idValue->value.GetString();
								}

							}
						}
					}
					//cout << "outputtind data for: " << JSONData.titleID << endl;
					//JSONData.outputData();
					dbManager.queueInsert(Tables::XboxTitleDetails, &JSONData);
					dbManager.queueInsert(Tables::XboxGameBundles, &JSONData);
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

	void XblAPIManager::parseGameTitle2(rapidjson::Document& document)
	{
		try {
			if (settings.outputDebug)
				std::cout << "Parsing Game Title" << std::endl;

			const auto& products = document.FindMember("Products");
			if (products != document.MemberEnd() && products->value.IsArray())
			{
				if (settings.outputDebug)
					std::cout << "Found Products." << std::endl;
				XboxGameMarketData jsonData;

				//There should only be a single product for game title
				for (const auto& product : products->value.GetArray())
				{
					
					localizedProperties(product, jsonData);

					MarketProperties(product, jsonData);
					//find the product id
					const auto& pID = product.FindMember("ProductId");
					if (pID != product.MemberEnd() && pID->value.IsString())
					{
						jsonData.productID = pID->value.GetString();
					}

					
					//Find DisplaySkuAvailabilities
					displaySkuAvailabilities(product, jsonData);
					jsonData.outputData();
				}

			}
		}
		catch (const std::exception& ex)
		{
			std::cout << "Cound not parse Game Title" << std::endl;
			std::cout << ex.what() << std::endl;
		}
	}

	void XblAPIManager::parsePlayerHistory(rapidjson::Document& document)
	{
		try {
			if (settings.outputDebug)
				std::cout << "Parsing Player History" << std::endl;


			const auto& titles = document.FindMember("titles");
			if (titles != document.MemberEnd() && titles->value.IsArray())
			{
				for (const auto &title : titles->value.GetArray())
				{
					bool validTitle = true;
					XboxGameTitleData jsonData;

					//get titleid
					if (title.HasMember("titleId") && title["titleId"].IsString())
					{
						jsonData.titleID = title["titleId"].GetString();
					}
					else
						validTitle = false;
					//get title name
					if (title.HasMember("name") && title["name"].IsString())
					{
						jsonData.titleName = title["name"].GetString();
					}
					else
						validTitle = false;

					//get display image
					if (title.HasMember("displayImage") && title["displayImage"].IsString())
					{
						jsonData.displayImage = title["displayImage"].GetString();
					}
					else
						validTitle = false;

					//get modern title id
					if (title.HasMember("modernTitleId") && title["modernTitleId"].IsString())
					{
						jsonData.modernTitleID = title["modernTitleId"].GetString();
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
								jsonData.devices.push_back(devicestr);
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
							jsonData.isGamePass = title["gamePass"]["isGamePass"].GetBool();
						}
						else
							validTitle = false;
					}
					else
						validTitle = false;


					//Insert into database
					if (validTitle)
					{
						titleDataCache.insert(std::pair<string, XboxGameTitleData>(jsonData.titleID, jsonData));
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



	void XblAPIManager::outputCache()
	{
			for (auto &titleData : titleDataCache)
			{
				//cout << titleData.first << endl;
				//titleData.second.outputData();
				dbManager.queueInsert(Tables::XboxGameTitles, &titleData.second);
				
			}
			
	}

	void XblAPIManager::scanAllXboxTitles()
	{
		if (!settings.canRequest(XboxSettings::hourlyAPICallsRemaining::title))
			return;
		mysqlx::Table tbl = dbManager.getTable(Tables::XboxGameTitles);
		auto now = std::chrono::system_clock::now();
		auto refreshTime = std::chrono::system_clock::to_time_t(now - settings.gameTitleUpdateFrequency);
		
		auto gameTitles = tbl.select("modernTitleId").where("lastScanned < FROM_UNIXTIME(:refreshTime) or lastScanned IS null")
			.orderBy("lastScanned").bind("refreshTime", refreshTime).execute();
		cout << "Game Titles needing Updates: " << gameTitles.count() << endl;
		for (const auto row : gameTitles)
		{
			if (settings.canRequest(XboxSettings::hourlyAPICallsRemaining::title))
			{
				settings.makeRequest(XboxSettings::hourlyAPICallsRemaining::title);
				string response = callAPI(xblAPICalls::gameTitle, row[0].get<std::string>());
				parseJson(xblAPICalls::gameTitle, response);
				tbl.update().set("lastScanned", mysqlx::expr("NOW()")).where("modernTitleId = :modernTitleId").bind("modernTitleId", row[0].get<std::string>()).execute();
			}
			else
			{
				cout << "xbox game titles limit reached: " << endl;
				break;
			}
		}
		
	}

	void XblAPIManager::scanAllUserProfiles()
	{
		mysqlx::Table tbl = dbManager.getTable(Tables::XboxUserProfiles);
		auto now = std::chrono::system_clock::now();
		auto refreshTime = std::chrono::system_clock::to_time_t(now - settings.userProfileUpdateFrequency);
		//cout << std::time(&refreshTime) << endl;
		//cout << refreshTime << endl;

		auto result = tbl.select("*").where("lastScanned < FROM_UNIXTIME(:refreshTime) or lastScanned IS null")
			.orderBy("lastScanned").bind("refreshTime", refreshTime).execute();
		//auto result = tbl.select("*").where("lastScanned == NULL").execute();
		//auto result = tbl.select("*").execute();
		cout << "User Profiles Needing updates: " <<result.count() << endl;

		int i =0;
		for (auto row : result)
		{
			//cout << row[0] << endl;
			string response = callAPI(xblAPICalls::playerTitleHistory, row[0].get<std::string>());
			parseJson(xblAPICalls::playerTitleHistory, response);
			tbl.update().set("lastScanned", mysqlx::expr("NOW()")).where("xuid = :xuid").bind("xuid", row[0].get<std::string>()).execute();
			if (++i > 2)
				break;
		}

		for (auto &titleData : titleDataCache)
		{
			dbManager.queueInsert(Tables::XboxGameTitles, &titleData.second);
		}		
		
	}

	void XblAPIManager::localizedProperties(const rapidjson::Value& product, XboxGameMarketData &jsonData)
	{
		//Local Section for game info
		const auto& localProp = product.FindMember("LocalizedProperties");
		if (localProp != product.MemberEnd() && localProp->value.IsArray())
		{
			std::cout << "found Local" << std::endl;

			for (auto& elem : localProp->value.GetArray())
			{
				if (elem.IsObject())
				{
					jsonData.devName = elem.FindMember("DeveloperName")->value.GetString();
					jsonData.pubName = elem.FindMember("PublisherName")->value.GetString();
					jsonData.productDesc = elem.FindMember("ProductDescription")->value.GetString();
					jsonData.productTitle = elem.FindMember("ProductTitle")->value.GetString();
				}
			}
		}

	}
	void XblAPIManager::displaySkuAvailabilities(const rapidjson::Value &product, XboxGameMarketData &jsonData)
	{
		int displaySkuNum = 0;
		const auto& displaySkuAvail = product.FindMember("DisplaySkuAvailabilities");
		if (displaySkuAvail != product.MemberEnd() && displaySkuAvail->value.IsArray())
		{
			bool foundPrice = false;
			cout << displaySkuAvail->value.Size() << endl;
			//for each display sku in availab
			for (auto& elemSkuAvail : displaySkuAvail->value.GetArray())
			{
				//There should only be one.
				++displaySkuNum;
				//find Sku
				const auto& bundledSkus = nestedFind("Sku/Properties/BundledSkus", elemSkuAvail);
				if (bundledSkus != nullptr)
				{
					if (bundledSkus->IsArray())
					{
						for (auto& bundle : bundledSkus->GetArray())
						{
							const auto& bigID = bundle.FindMember("BigId");
							const auto& isPrimary = bundle.FindMember("IsPrimary");

							if (bigID != bundle.MemberEnd() && bigID->value.IsString())
							{
								if (isPrimary != bundle.MemberEnd() && isPrimary->value.IsBool() && isPrimary->value.GetBool() == true)
								{
									jsonData.primarySku = bigID->value.GetString();
									std::cout << "Found primary title" << std::endl;
								}
							}
						}
					}

				}

				//Find Availabilities
				const auto& avail = elemSkuAvail.FindMember("Availabilities");
				if (avail != elemSkuAvail.MemberEnd() && avail->value.IsArray() && !foundPrice)
				{
					//For each avail
					bool purchasable = false;
					int availNum = 0;
					for (auto& elemAvail : avail->value.GetArray())
					{
						cout << "Avail Size: " << avail->value.Size() << endl;
						++availNum;
						const auto& actions = elemAvail.FindMember("Actions");

						if (actions != elemAvail.MemberEnd() && actions->value.IsArray())
						{
							for (auto& action : actions->value.GetArray())
							{
								if (action.IsString())
								{
									string actstr = action.GetString();
									cout << action.GetString() << endl;
									if (actstr == "Purchase")
									{
										cout << "Found purchase match" << endl;
										purchasable = true;
									}
								}
							}
							std::cout << "[" << displaySkuNum << "][" << availNum << "]" << ":  ";
							if (!purchasable)
								std::cout << "sku not purchasable" << std::endl;
							else
								std::cout << "Sku purchasable" << std::endl;
						}

						if (!purchasable)
							continue;

						//Check for Current list price date range. Can find platforms as well
						const auto& conditions = elemAvail.FindMember("Conditions");
						if (conditions != elemAvail.MemberEnd() && conditions->value.IsObject())
						{
							auto endDate = conditions->value.FindMember("EndDate");
							auto startDate = conditions->value.FindMember("StartDate");

							if ((endDate != conditions->value.MemberEnd() && startDate != conditions->value.MemberEnd()) &&
								startDate->value.IsString() && endDate->value.IsString())
							{
								cout << "Found Dates" << endl;
								jsonData.startDate = startDate->value.GetString();
								jsonData.endDate = endDate->value.GetString();
								if (inDateRange(jsonData.startDate, jsonData.endDate))
								{
									cout << "in range" << endl;
								}
							}
						}

						//find orderManagement data
						const auto& price = nestedFind("OrderManagementData/Price", elemAvail);
						if (price != nullptr)
						{
							auto listPrice = price->FindMember("ListPrice");
							auto msrp = price->FindMember("MSRP");
							if ((listPrice != price->MemberEnd() && msrp != price->MemberEnd())
								&& (listPrice->value.IsNumber() && msrp->value.IsNumber()))
							{
								jsonData.listPrice = listPrice->value.GetFloat();
								jsonData.MSRP = msrp->value.GetFloat();
								foundPrice = true;
								if (settings.outputDebug)
									std::cout << "Found Price" << std::endl;
								break;
							}
						}
					}
				}
			}
		}
	}

	void XblAPIManager::MarketProperties(const rapidjson::Value& product, XboxGameMarketData& jsonData)
	{

	}
#pragma endregion


#pragma region Headers
	void XblAPIManager::createHeaders(curl_slist*& headers)
	{
		headers = NULL;
		headers = curl_slist_append(headers, "accept: */*");
		headers = curl_slist_append(headers, ("x-authorization: " + settings.xblAPIKey).c_str()); // Replace with your actual x-authorization value
	}
	void XblAPIManager::appendHeadder(curl_slist*& headers, std::string header)
	{
		headers = curl_slist_append(headers, header.c_str());
	}
	void XblAPIManager::setHeaders(CURL*& curl, curl_slist* headers)
	{
		curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
	}
#pragma endregion

}