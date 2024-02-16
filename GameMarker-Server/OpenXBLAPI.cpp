#include"OpenXBLAPI.h"

using namespace std;

namespace OpenXBL_namespace
{
    
    XblAPI* XblAPI::getInstance()
    {
        if (instance == NULL)
            instance = new XblAPI();
        return instance;
    }
    void XblAPI::startXBLAPI()
    {
        dbManager = DataBaseManager::getInstance();

        //collection = dbManager->getSchema().createCollection("XboxGames",true);
    }

   size_t XblAPI::WriteCallback(void* contents, size_t size, size_t nmemb, std::string* userp) {
        userp->append((char*)contents, size * nmemb);
        return size * nmemb;
    }

    // Set the custom header
    void XblAPI::createHeaders(curl_slist* &headers)
    {
        headers = NULL;
        headers = curl_slist_append(headers, "accept: */*");
        headers = curl_slist_append(headers, ("x-authorization: " + xblAPIkey).c_str()); // Replace with your actual x-authorization value
    }
    void XblAPI::appendHeadder(curl_slist* &headers, string header)
    {
        headers = curl_slist_append(headers, header.c_str());
    }
    void XblAPI::setHeaders(CURL*& curl, curl_slist* headers)
    {
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    }

    string XblAPI::apiCall(string url)
    {
        CURL* xboxCurl;
        CURLcode res;
        string readBuffer;
        
        xboxCurl = curl_easy_init();

        if (xboxCurl) {
            // Specify the URL to get
            
            if (url == xblAPI)
            {
                cout << "api call not found";
                return 0;
            }
            cout << url << endl;
            curl_easy_setopt(xboxCurl, CURLOPT_URL, url.c_str());


            //headers
            struct curl_slist* headers = NULL;
            createHeaders(headers);
            setHeaders(xboxCurl, headers);


            // Set the callback function to receive the response
            curl_easy_setopt(xboxCurl, CURLOPT_WRITEFUNCTION, WriteCallback);

            // Set the data to pass to the callback function
            curl_easy_setopt(xboxCurl, CURLOPT_WRITEDATA, &readBuffer);

            // Perform the request, res will get the return code
            res = curl_easy_perform(xboxCurl);


            // Check for errors
            if (res != CURLE_OK) {
                std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
                readBuffer = curlFail;
            }
            else {
                // Use the content
               // std::cout << "Response Data: " << readBuffer << std::endl;
                cout << "Curl Success";
            }

            // Clean up
            curl_slist_free_all(headers); // Free the header list
            curl_easy_cleanup(xboxCurl); // End a libcurl easy session
        }
        return readBuffer;
    }



    
}