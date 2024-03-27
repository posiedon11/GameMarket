#include"APIManager.h"


namespace apimanager_namespace
{
	void APIManager::start()
	{
		running = true;
		mainThread = std::thread(&APIManager::run, this);
	}
	void APIManager::stop()
	{
		running = false;
		if (mainThread.joinable())
			mainThread.join();

	}
	void APIManager::run()
	{
		while (running)
		{
			startCalls();
		}
	}

	APIManager::APIManager(DataBaseManager& dbManager, Settings& settings) :running(false), dbManager(dbManager), settings(settings) {};
	APIManager::~APIManager() { stop(); }


	bool APIManager::validURL(std::string url)
	{
		if (url == "")
		{
			std::cout << "Empty URL" << std::endl;
			return false;
		}
		else if (url == errorURLNoParam) {
			std::cout << "A paramater was needed" << std::endl;
			return false;
		}
		else if (url == errorURLCallDoesntExist)
		{
			std::cout << "the given call does not exist";
			return false;
		}
		return true;
	}



	std::string APIManager::callAPI(int callType, string paramater)
	{
		CURL* curl;
		CURLcode res;
		std::string responseBody;
		std::map<string, string> responseHeaders;

		curl = curl_easy_init();

		std::string url = getURL(callType, paramater);
		if (curl) {
			// Specify the URL to get

			if (!validURL(url))
			{
				return curlFail;
			}
			if (settings.outputCurl)
				std::cout << endl << url << std::endl;
			curl_easy_setopt(curl, CURLOPT_URL, url.c_str());


			//headers
			struct curl_slist* headers = NULL;
			createHeaders(headers);
			setHeaders(curl, headers);

			setupCurl(curl, paramater, responseBody, responseHeaders);
			// Perform the request, res will get the return code
			res = curl_easy_perform(curl);


			// Check for errors
			if (res != CURLE_OK) {
				if (settings.outputCurl)
				std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
				responseBody = curlFail;
			}
			else if (responseBody == "")
			{
				if (settings.outputCurl)
					std::cerr << "No response recieved" << std::endl;
				responseBody = curlFail;
			}
			else {
				//Response is valid
				handleCurlHeaders(responseHeaders);
			}

			// Clean up
			curl_slist_free_all(headers); // Free the header list
			curl_easy_cleanup(curl); // End a libcurl easy session
		}
		return responseBody;
	}


	
	void APIManager::appendHeader(curl_slist*& headers, std::string header)
	{
		headers = curl_slist_append(headers, header.c_str());
	}
	void APIManager::setHeaders(CURL*& curl, curl_slist* headers)
	{
		curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
	}

	void APIManager::additionalHeaders(CURL*& curl, string paramater) {};
	void APIManager::handleCurlHeaders(std::map<string, string> responseHeaders) {};
}